using System.Text;
using CleanArchitectureBase.Application.Common.FileServices;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Mappings;
using CleanArchitectureBase.Application.Common.Settings;
using CleanArchitectureBase.Application.Orders.Interfaces;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Infrastructure.Data;
using CleanArchitectureBase.Infrastructure.Data.Interceptors;
using CleanArchitectureBase.Infrastructure.FileServices;
using CleanArchitectureBase.Infrastructure.Google;
using CleanArchitectureBase.Infrastructure.Hangfire;
using CleanArchitectureBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using CleanArchitectureBase.Infrastructure.Redis;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CleanArchitectureBase.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        //Lấy CNS từ appsetting
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        Guard.Against.Null(connectionString, message: "Connection string 'DefaultConnection' not found.");

        //Cấu hình Interceptor cho Database
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        //Cấu hình Interceptor cho xử lý Event
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.EnableSensitiveDataLogging(); 
            options.EnableDetailedErrors(); 
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0, 36))
            );
        });


        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ApplicationDbContextInitialiser>();
        services.AddTransient<IEmailService, EmailService>();

        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.Configure<GoogleSettings>(configuration.GetSection("GoogleSettings"));
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.Configure<PaymentSettings>(configuration.GetSection("PaymentSettings"));
        services.Configure<GeminiSettings>(configuration.GetSection("GeminiSettings"));

        var a = configuration.GetSection("JwtSettings").Get<JwtSettings>();
        if (a == null) throw new Exception("Lỗi");
        Console.WriteLine(a.SecretKey);

        #region Lưu ý AddIdentity

        //TODO: Không nên dùng AddIdentity, vì nó sẽ mặc định đăng ký AddAuthentication của nó
        //Nên dùng IDentity
        //Nếu dùng thì phải ghi đè AddAuthentication và đặt hàm AddAuthentication ở sau AddIdentity, tránh bị Identity ghi đè

        #endregion

        services
            .AddIdentity<UserAccount, ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddErrorDescriber<CustomIdentityErrorDescriber>()
            .AddDefaultTokenProviders();
        ;

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
                Guard.Against.Null(jwtSettings, message: "JwtSetting not found.");
                Guard.Against.NullOrEmpty(jwtSettings.SecretKey, message: "Secret key not found.");

                var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false, // Bật kiểm tra Issuer
                    ValidateAudience = false, // Bật kiểm tra Audience
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ClockSkew = TimeSpan.Zero
                };

                //Bổ sung cơ chế đọc token từ cookie      
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                        if (string.IsNullOrEmpty(token))
                        {
                            token = context.Request.Cookies["access_token"];
                        }

                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorizationBuilder();
        services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigins", policy =>
            {
                var allowedOrigins = 
                // configuration.GetSection("AllowedOrigins").Get<string[]>() ??
                                     new[]
                                     {
                                         "http://localhost:3000", "https://localhost:3000", "http://36.50.135.207:5000", "http://36.50.135.207:5555"
                                     };

                policy.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders("Authorization");
            });
        });

        services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 2;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(60);
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            }
        );


        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IIdentityService, IdentityService>();
        // services.AddScoped<IHangFireService, HangFireService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IPlayGroundService, PlayGroundService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddSingleton<IRedisService, RedisService>();

        services.AddSingleton<IGoogleAccessTokenProvider>(provider =>
        {
            var json = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS_JSON");
            return new GoogleAccessTokenProvider(json!);
        });
        services.AddHttpClient<IGoogleAuthService, GoogleAuthService>();


        services.AddAuthorization(options =>
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));

        return services;
    }
}
