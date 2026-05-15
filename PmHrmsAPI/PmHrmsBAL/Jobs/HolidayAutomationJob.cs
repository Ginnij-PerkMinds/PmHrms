using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PmHrmsAPI.PmHrmsBAL.Jobs
{
    public class HolidayAutomationJob
    {
        private readonly PmHrmsContext _context;
        private readonly ILogger<HolidayAutomationJob> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        private static readonly JsonSerializerOptions _jsonOpts =
            new() { PropertyNameCaseInsensitive = true };

        public HolidayAutomationJob(
            PmHrmsContext context,
            ILogger<HolidayAutomationJob> logger,
            HttpClient httpClient,
            IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _httpClient = httpClient;
            _config = config;
        }

        [AutomaticRetry(Attempts = 2)]
         public async Task RunAsync(int? year = null)
        {
             var targetYear = year ?? DateTime.Now.Year + 1;
            _logger.LogInformation("[HolidayJob] Starting for {Year}. Updating Master Catalog.", targetYear);

             await CopyRecurringSystemHolidays(targetYear);
             await CopyRecurringCustomHolidays(targetYear);
 
            var fetched = await FetchFromCalendarific(targetYear, "IN");
            if (!fetched)
            {
                _logger.LogWarning("[HolidayJob] Calendarific failed, falling back to Nager.at");
                await FetchFromNager(targetYear, "IN");
            }

            // MIGRATION NOTE: 
            // SyncSystemHolidaysToOrgs() is REMOVED. 
            // In the new architecture, Orgs will create a 'HolidayGroup' and map these Master holidays themselves via UI.

            _logger.LogInformation("[HolidayJob] Completed updating Master Catalog for {Year}", targetYear);
        }

         // ──────────────────────────────────────────────────────────────
        // PRIMARY: Calendarific
        // ──────────────────────────────────────────────────────────────
        private async Task<bool> FetchFromCalendarific(int year, string countryCode)
        {
            try
            {
                var apiKey = _config["Calendarific:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogWarning("[HolidayJob] Calendarific API key not configured");
                    return false;
                }
 
                var url = $"https://calendarific.com/api/v2/holidays" +
                          $"?api_key={apiKey}&country={countryCode}&year={year}&type=national";
 
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("[HolidayJob] Calendarific HTTP {Status}", response.StatusCode);
                    return false;
                }
 
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<CalendarificResponse>(content, _jsonOpts);
 
                var holidays = result?.Response?.Holidays;
                if (holidays == null || holidays.Count == 0)
                {
                    _logger.LogWarning("[HolidayJob] Calendarific returned no holidays");
                    return false;
                }
 
                await PersistSystemHolidays(
                    year,
                    countryCode,
                    holidays.Select(h => (
                        Name: h.Name,
                        Date: new DateOnly(h.Date.Datetime.Year, h.Date.Datetime.Month, h.Date.Datetime.Day)
                    )),
                    source: "Calendarific"
                );
 
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HolidayJob] Calendarific fetch threw");
                return false;
            }
        }
 

       // ──────────────────────────────────────────────────────────────
        // FALLBACK: Nager.at
        // ──────────────────────────────────────────────────────────────
        private async Task FetchFromNager(int year, string countryCode)
        {
            try
            {
                var url = $"https://date.nager.at/api/v3/PublicHolidays/{year}/{countryCode}";
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return;
 
                var content = await response.Content.ReadAsStringAsync();
                var apiHolidays = JsonSerializer.Deserialize<List<NagerHolidayDto>>(content, _jsonOpts)
                                  ?? new List<NagerHolidayDto>();
 
                await PersistSystemHolidays(
                    year,
                    countryCode,
                    apiHolidays.Select(h => (
                        Name: string.IsNullOrWhiteSpace(h.LocalName) ? h.Name : h.LocalName,
                        Date: DateOnly.FromDateTime(h.Date)
                    )),
                    source: "Nager"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HolidayJob] Nager.at fetch threw");
            }
        }

        // ──────────────────────────────────────────────────────────────
        // Persist — deduplicates by date
        // ──────────────────────────────────────────────────────────────
        private async Task PersistSystemHolidays(
            int year,
            string countryCode,
            IEnumerable<(string Name, DateOnly Date)> incoming,
            string source)
        {
            // CHANGED: Use !IsCustom instead of CountryCode != "CSTM"
            var existingDates = (await _context.SystemHolidays
                .Where(x => x.Year == year && !x.IsCustom) 
                .Select(x => x.HolidayDate)
                .ToListAsync())
                .ToHashSet();
 
            var toAdd = new List<SystemHoliday>();
 
            foreach (var (name, date) in incoming)
            {
                if (existingDates.Contains(date)) continue;
 
                toAdd.Add(new SystemHoliday
                {
                    HolidayName = name,
                    HolidayDate = date,
                    Year = year,
                    CountryCode = countryCode,
                    IsRecurring = false,
                    IsCustom = false, // Explicitly set to false for Global API holidays
                    CreatedByOrgId = null, // Global holiday, no specific owner
                    CreatedAt = DateTime.UtcNow
                });
                existingDates.Add(date);
            }
 
            if (toAdd.Count > 0)
            {
                await _context.SystemHolidays.AddRangeAsync(toAdd);
                await _context.SaveChangesAsync();
                _logger.LogInformation("[HolidayJob][{Source}] Inserted {Count} holidays for {Year}",
                    source, toAdd.Count, year);
            }
        }

         // ──────────────────────────────────────────────────────────────
        // Copy recurring holidays from previous year
        // ──────────────────────────────────────────────────────────────
        private async Task CopyRecurringSystemHolidays(int targetYear)
        {
            var sourceYear = targetYear - 1;
 
            // CHANGED: Use !IsCustom instead of CountryCode != "CSTM"
            var recurring = await _context.SystemHolidays
                .Where(h =>
                    h.IsRecurring &&
                    h.Year == sourceYear &&
                    !h.IsCustom)
                .ToListAsync();
 
            var existingNames = (await _context.SystemHolidays
                .Where(x => x.Year == targetYear && !x.IsCustom)
                .Select(x => x.HolidayName)
                .ToListAsync())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
 
            var toAdd = recurring
                .Where(h => !existingNames.Contains(h.HolidayName))
                .Select(h => new SystemHoliday
                {
                    HolidayName = h.HolidayName,
                    HolidayDate = BuildRecurringDate(h.HolidayDate, targetYear),
                    Year = targetYear,
                    CountryCode = h.CountryCode,
                    IsRecurring = true,
                    IsCustom = false,
                    CreatedByOrgId = null,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();
 
            if (toAdd.Count > 0)
            {
                await _context.SystemHolidays.AddRangeAsync(toAdd);
                await _context.SaveChangesAsync();
                _logger.LogInformation("[HolidayJob] Copied {Count} recurring holidays → {Year}",
                    toAdd.Count, targetYear);
            }
        }

        private async Task CopyRecurringCustomHolidays(int targetYear)
        {
            var sourceYear = targetYear - 1;

            var recurringCustoms = await _context.SystemHolidays
                .Where(h =>
                    h.IsRecurring &&
                    h.IsCustom &&
                    h.Year == sourceYear &&
                    h.CreatedByOrgId != null)
                .ToListAsync();

            var existingKeys = (await _context.SystemHolidays
                .Where(x =>
                    x.Year == targetYear &&
                    x.IsCustom &&
                    x.CreatedByOrgId != null)
                .Select(x => new
                {
                    x.CreatedByOrgId,
                    x.HolidayName,
                    x.HolidayDate
                })
                .ToListAsync())
                .Select(x => BuildCustomHolidayKey(
                    x.CreatedByOrgId!.Value,
                    x.HolidayName,
                    x.HolidayDate))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var toAdd = new List<SystemHoliday>();

            foreach (var holiday in recurringCustoms)
            {
                var targetDate = BuildRecurringDate(holiday.HolidayDate, targetYear);
                var targetKey = BuildCustomHolidayKey(
                    holiday.CreatedByOrgId!.Value,
                    holiday.HolidayName,
                    targetDate);

                if (existingKeys.Contains(targetKey))
                {
                    continue;
                }

                toAdd.Add(new SystemHoliday
                {
                    HolidayName = holiday.HolidayName,
                    HolidayDate = targetDate,
                    Year = targetYear,
                    CountryCode = holiday.CountryCode,
                    IsRecurring = true,
                    IsCustom = true,
                    CreatedByOrgId = holiday.CreatedByOrgId,
                    CreatedAt = DateTime.UtcNow
                });

                existingKeys.Add(targetKey);
            }

            if (toAdd.Count > 0)
            {
                await _context.SystemHolidays.AddRangeAsync(toAdd);
                await _context.SaveChangesAsync();
                _logger.LogInformation("[HolidayJob] Copied {Count} recurring custom holidays â†’ {Year}",
                    toAdd.Count, targetYear);
            }
        }

        private static DateOnly BuildRecurringDate(DateOnly sourceDate, int targetYear)
        {
            var safeDay = Math.Min(sourceDate.Day, DateTime.DaysInMonth(targetYear, sourceDate.Month));
            return new DateOnly(targetYear, sourceDate.Month, safeDay);
        }

        private static string BuildCustomHolidayKey(int orgId, string holidayName, DateOnly holidayDate)
        {
            return $"{orgId}|{holidayName.Trim().ToLowerInvariant()}|{holidayDate:yyyy-MM-dd}";
        }
    }
 
    // ── DTOs ──────────────────────────────────────────────────────────
    public class NagerHolidayDto { public DateTime Date { get; set; } public string LocalName { get; set; } = ""; public string Name { get; set; } = ""; }
    public class CalendarificResponse { public CalendarificResponseBody Response { get; set; } = new(); }
    public class CalendarificResponseBody { public List<CalendarificHoliday> Holidays { get; set; } = new(); }
    public class CalendarificHoliday { public string Name { get; set; } = ""; public CalendarificDate Date { get; set; } = new(); }
    public class CalendarificDate { public CalendarificDatetime Datetime { get; set; } = new(); }
    public class CalendarificDatetime { public int Year { get; set; } public int Month { get; set; } public int Day { get; set; } }
}
