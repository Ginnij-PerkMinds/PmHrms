
namespace PmHrmsAPI.PmHrmsDAL.DbEntities;
public class SystemMailSetting
    {
        public int Id { get; set; }
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Mail { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? UserName { get; set; }
        public string Password { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }