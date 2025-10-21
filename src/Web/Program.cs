using CleanArchitectureBase.Application;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Infrastructure;
using CleanArchitectureBase.Infrastructure.Data;
using CleanArchitectureBase.Infrastructure.Hangfire;
using CleanArchitectureBase.Web;
using CleanArchitectureBase.Web.Attributes;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MySql;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
   
    //Đây là thời gian tối đa Kestrel chờ client gửi toàn bộ request headers
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);    
});

DotNetEnv.Env.Load("../../.env");
builder.Configuration.AddEnvironmentVariables();
// Add services to the container.
builder.Services.AddKeyVaultIfConfigured(builder.Configuration);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebServices();
builder.Services.AddScoped<PaymentAuthEndpointFilter>();

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseStorage(
        new MySqlStorage(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            new MySqlStorageOptions
            {
                TransactionIsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                QueuePollInterval = TimeSpan.FromSeconds(15),
                JobExpirationCheckInterval = TimeSpan.FromHours(1),
                CountersAggregateInterval = TimeSpan.FromMinutes(5),
                PrepareSchemaIfNecessary = true,
                DashboardJobListLimit = 50000,
                TransactionTimeout = TimeSpan.FromMinutes(1),
                TablesPrefix = "Hangfire_"
            }
        )
    ));

builder.Services.AddHangfireServer(options =>
{
    options.Queues = new[] { "stock" };
});



builder.Services.AddScoped<IHangfireService, HangfireService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    // app.UseHsts();
}

app.UseHealthChecks("/health");
// app.UseHttpsRedirection();


// CORS MIDDLEWARE
// if (app.Environment.IsDevelopment())
//     app.UseCors("AllowAll");
// else
app.Use(async (context, next) =>
{
    var origin = context.Request.Headers["Origin"].ToString();
    if (!string.IsNullOrEmpty(origin))
        Console.WriteLine($"🌐 Incoming Origin: {origin}");
    await next();
});
    app.UseCors("AllowSpecificOrigins");
    // TODO: Bỏ các option bên trong rồi fetch thử lại
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            var allowedOrigins = ctx.Context.RequestServices
                .GetRequiredService<IConfiguration>()
                .GetSection("AllowedOrigins")
                .Get<string[]>() ?? new[]
            {
                "http://127.0.0.1:3000",
                "http://localhost:3000",
                "https://localhost:3000",
                "http://192.168.1.100:8080", // Thêm các origin khác
                "https://example.com",
                "http://36.50.135.207:5000",
                "http://36.50.135.207:5555"
            };

            var requestOrigin = ctx.Context.Request.Headers.Origin.FirstOrDefault();
            Console.WriteLine("REQUEST_ORIGIN: " + requestOrigin);
            if (!string.IsNullOrEmpty(requestOrigin) && allowedOrigins.Contains(requestOrigin))
            {
                ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", requestOrigin);
                ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, OPTIONS");
                ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "*");
                ctx.Context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
            }
            // else
            // {
            //     // Nếu origin không được phép, có thể trả về lỗi hoặc không thêm header CORS
            //     ctx.Context.Response.StatusCode = 403; // Forbidden
            // }
        }
    });
app.UseAuthentication();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = []
});

app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
    settings.DocumentPath = "/api/specification.json";
    settings.DocExpansion = "list"; //none/list/full
});


app.MapScalarApiReference("/api-docs",options =>
{
    options.Title = "CleanArchitectureBase API Docs";
    options.WithOpenApiRoutePattern("/api/specification.json");
});




app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.UseExceptionHandler(options => { });    

app.Map("/", () => Results.Redirect("/api"));


app.MapEndpoints();

app.Run();


namespace CleanArchitectureBase.Web
{
    public partial class Program
    {
    }
}
