using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PmHrmsAPI.PmHrmsBAL;
using PmHrmsAPI.PmHrmsBAL.Exceptions;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.Repositories;
using PmHrmsAPI.PmHrmsBAL.Services;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Repositories;
using PmHrmsAPI.PmHrmsDAL.Utility;
using PmHrmsAPI.PmHrmsFAL;
using PmHrmsAPI.PmHrmsFAL.IRepositories;
using PmHrmsAPI.PmHrmsFAL.IRespositories;
using PmHrmsAPI.PmHrmsFAL.Repositories;
using System.Text;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authorization;
using Hangfire.Dashboard;
using PmHrmsAPI.PmHrmsBAL.Jobs;



namespace PmHrmsAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var aspEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (aspEnv != "Production")
            {
                LoadDotEnvIfPresent();
            }

            var builder = WebApplication.CreateBuilder(args);

            Console.WriteLine($"[Startup] Environment : {builder.Environment.EnvironmentName}");
            Console.WriteLine($"[Startup] ContentRoot : {builder.Environment.ContentRootPath}");


            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Missing connection string. Set ConnectionStrings__DefaultConnection.");

            var jwtKey = builder.Configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new InvalidOperationException("Missing JWT key. Set Jwt__Key.");

            // FIX: Added CommandTimeout(30) — prevents infinite SQL hangs (e.g. lock contention
            // during migration job SaveChangesAsync). Any SQL command > 30s will now throw a
            // SqlException instead of blocking the background thread forever.
            builder.Services.AddDbContext<PmHrmsContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.CommandTimeout(300);
                }));



              

            builder.Services.AddHttpContextAccessor();

           
            builder.Services.AddControllers()
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.Converters.Add(
                            new System.Text.Json.Serialization.JsonStringEnumConverter()
                        );
                    });


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular",
                    policy =>
                    {
                        policy
                            .AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });


            builder.Services.AddHangfire(config =>
            config.UseSqlServerStorage(
                connectionString
            ));

            builder.Services.AddHangfireServer();



            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ITenantService, TenantService>();
            builder.Services.AddTransient<IEmailService, EmailService>();
            builder.Services.AddScoped<IPermissionService, PermissionService>();
            builder.Services.AddScoped<IMigrationService, MigrationService>();
            builder.Services.AddScoped<IBulkEmployeeService, BulkEmployeeService>();
            builder.Services.AddScoped<IGeoService, GeoService>();
            builder.Services.AddScoped<IEmployeeOnboardingService, EmployeeOnboardingService>();
            builder.Services.AddScoped<IPayrollCalculator, PayrollCalculator>();
            builder.Services.AddScoped<PmHrmsAPI.PmHrmsBAL.Services.Interfaces.IPayrollService, PmHrmsAPI.PmHrmsBAL.Services.PayrollService>();

            builder.Services.AddScoped<OrganizationDAL>();
            builder.Services.AddScoped<DepartmentDAL>(); 
            builder.Services.AddScoped<DesignationDAL>();
            builder.Services.AddScoped<EmployeeDAL>();
            builder.Services.AddScoped<EmployeeDetailDAL>();
            builder.Services.AddScoped<EmployeeDocumentDAL>();
            builder.Services.AddScoped<GlobalSearchDAL>();
            builder.Services.AddScoped<RoleLayoutAccessDAL>();
            builder.Services.AddScoped<OrgRoleDAL>();
            builder.Services.AddScoped<RolePermissionDAL>();
            builder.Services.AddScoped<WorkPolicyDAL>();
            builder.Services.AddScoped<AttendanceDAL>();
            builder.Services.AddScoped<OfficeLocationDAL>();
            builder.Services.AddScoped<RemoteLocationDAL>();
            builder.Services.AddScoped<LeaveDAL>();
            builder.Services.AddScoped<PostDAL>();
            builder.Services.AddScoped<TaskDAL>();

            builder.Services.AddScoped<HolidayDAL>();

            builder.Services.AddScoped<DesignationWorkPolicyDAL>();
            builder.Services.AddScoped<SalaryStructureDAL>();
            builder.Services.AddScoped<EmployeeBankAccountDAL>();
            





            builder.Services.AddScoped<IOrganizationBAL, OrganizationBAL>();
            builder.Services.AddScoped<IDepartmentBAL, DepartmentBAL>();
            builder.Services.AddScoped<IDesignationBAL, DesignationBAL>();
            builder.Services.AddScoped<IEmployeeBAL, EmployeeBAL>();
            builder.Services.AddScoped<IEmployeeDetailBAL, EmployeeDetailBAL>();
            builder.Services.AddScoped<IEmployeeDocumentBAL, EmployeeDocumentBAL>();
            builder.Services.AddScoped<IGlobalSearchBAL, GlobalSearchBAL>();
            builder.Services.AddScoped<IRoleLayoutAccessBAL, RoleLayoutAccessBAL>();
            builder.Services.AddScoped<IRolePermissionBAL, RolePermissionBAL>();
            builder.Services.AddScoped<IOrgRoleBAL, OrgRoleBAL>();
            builder.Services.AddScoped<IWorkPolicyBAL, WorkPolicyBAL>();
            builder.Services.AddScoped<IAttendanceBAL, PmHrmsBAL.Repositories.AttendanceBAL>();
            builder.Services.AddScoped<IOfficeLocationBAL, OfficeLocationBAL>();
            builder.Services.AddScoped<ILeaveBAL, LeaveBAL>();
            builder.Services.AddScoped<IPostBAL , PostBAL>();
            builder.Services.AddScoped<ITaskBAL , TaskBAL>();
            builder.Services.AddScoped<IHolidayBAL, HolidayBAL>();
            builder.Services.AddScoped<ISalaryStructureBAL,SalaryStructureBAL>();
            builder.Services.AddScoped<IEmployeeBankAccountBAL, EmployeeBankAccountBAL>();



            builder.Services.AddScoped<IImageFAL, ImageFAL>();
            builder.Services.AddScoped<IDocumentFAL, DocumentFAL>();

            builder.Services.AddScoped<LeaveAccrualJob>();
            builder.Services.AddScoped<HolidayAutomationJob>();
            builder.Services.AddScoped<PayrollJob>();

            builder.Services.AddHttpContextAccessor(); 

            builder.Services.AddScoped<ExpenseDAL>();
            builder.Services.AddScoped<IExpenseBAL, ExpenseBAL>();

            //builder.Logging.ClearProviders();   
            builder.Logging.AddConsole();  
            builder.Logging.AddDebug();       
            builder.Logging.AddLog4Net("log4net.config");   


            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>  
                {
                    options.Events = new JwtBearerEvents        
                    {
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine("JWT AUTH FAILED:");
                            Console.WriteLine(context.Exception.GetType().FullName);
                            Console.WriteLine(context.Exception.Message);
                            return Task.CompletedTask;         
                        },
                        OnChallenge = context =>
                        {
                            Console.WriteLine("JWT CHALLENGE ERROR:");
                            Console.WriteLine(context.Error);
                            Console.WriteLine(context.ErrorDescription);   
                            context.HandleResponse();
                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/json";      
                            return context.Response.WriteAsync("{\"message\":\"You are not authorized\"}");
                        
                         },  
                    };

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero,

                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtKey)
                        )
                    };

                });


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHttpClient<HolidayAutomationJob>();


            builder.Services.AddHttpClient<HolidayAutomationJob>();  

            var app = builder.Build();    


             var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();

            

           
           
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }  

            app.Use(async (context, next) => 
            {
                try       
                {
                    await next();             
                }
                catch (ForbiddenException ex)          
                {      
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";

                    await context.Response.WriteAsJsonAsync(new
                    {   
                        type = "PERMISSION_DENIED",
                        message = ex.Message
                    });
                }
                catch (ArgumentException ex)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    context.Response.ContentType = "application/json";

                    await context.Response.WriteAsJsonAsync(new
                    {
                        type = "VALIDATION_ERROR",
                        message = ex.Message
                    });
                }
                catch (Exception ex)
                {
                    var logger = context.RequestServices
                        .GetRequiredService<ILogger<Program>>();

                    logger.LogError(ex, "Unhandled exception occurred");

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";

                    var message = app.Environment.IsDevelopment()
                        ? ex.Message
                        : "An internal server error occurred.";

                    await context.Response.WriteAsJsonAsync(new
                    {
                        type    = "SERVER_ERROR",
                        message = message
                    });
                }
            });


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseCors("AllowAngular");   

            app.UseAuthentication(); 
            
            app.UseAuthorization();

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
                {
                    Authorization = app.Environment.IsDevelopment()
                        ? new[] { new AllowAllDashboardAuthorizationFilter() }  
                        : new[] { new AuthenticatedDashboardAuthorizationFilter() }
                });

            RecurringJob.AddOrUpdate<LeaveAccrualJob>(
                "monthly-leave-accrual",
                job => job.RunAsync(),
                    Cron.Monthly(1, 0, 0));

            
                RecurringJob.AddOrUpdate<PayrollJob>(
                    "enqueue-due-payroll-runs",           
                    job => job.EnqueueDuePayrollRunsAsync(),
                    Cron.Daily,                          
                    new RecurringJobOptions
                    {
                        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")
                    });


            app.MapControllers();


            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<PmHrmsContext>();  

                var dbKeys = db.PermissionMasters         
                    .Select(p => p.PermissionKey)
                    .ToHashSet();      

                var codeKeys = typeof(PermissionKeys)
                    .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                    .Select(f => f.GetValue(null)?.ToString())                      
                    .ToHashSet();             

                var missingInDb = codeKeys.Except(dbKeys).ToList();
                var extraInDb = dbKeys.Except(codeKeys).ToList();

                if (missingInDb.Any() || extraInDb.Any())
                {
                    var logger = scope.ServiceProvider
                        .GetRequiredService<ILogger<Program>>();

                    logger.LogCritical(
                        "Permission mismatch detected. Missing: {Missing}, Extra: {Extra}",
                        string.Join(",", missingInDb),
                        string.Join(",", extraInDb)
                    );
                }
            }

               

            app.Run();
        }

        private static void LoadDotEnvIfPresent()
        {
            var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            if (!File.Exists(envPath))
                return;

            foreach (var rawLine in File.ReadAllLines(envPath))
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var separatorIndex = line.IndexOf('=');
                if (separatorIndex <= 0)
                    continue;

                var key = line[..separatorIndex].Trim();
                var value = line[(separatorIndex + 1)..].Trim().Trim('"');
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                var existing = Environment.GetEnvironmentVariable(key);
                if (string.IsNullOrWhiteSpace(existing))
                {
                    Environment.SetEnvironmentVariable(key, value);
                }
            }
        }
    }

public sealed class AllowAllDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}
    public sealed class AuthenticatedDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            return httpContext.User.Identity?.IsAuthenticated == true;
        }
    }
}
