using System.Threading.RateLimiting;
using VehicleManagement.Api;
using VehicleManagement.Application.Interfaces;
using VehicleManagement.Application.Services;
using VehicleManagement.Domain.Services;
using VehicleManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddInfrastructure(
    builder.Configuration,
    builder.Environment.ContentRootPath);

builder.Services.AddSingleton<VehicleCategoryResolver>();
builder.Services.AddSingleton<CategoryConfigurationValidator>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IManufacturerService, ManufacturerService>();
builder.Services.AddScoped<IVehicleCategoryService, VehicleCategoryService>();
builder.Services.AddSingleton<ICategoryIconProvider, ConfigurationCategoryIconProvider>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

// Per-IP rate limit so the public endpoints can't be scraped or flooded.
// Behind a reverse proxy, add UseForwardedHeaders so RemoteIpAddress is the client's IP.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = builder.Configuration.GetValue("RateLimiting:PermitLimit", 100),
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

// Only the website's own origin may call the API from a browser.
const string WebCorsPolicy = "Web";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(WebCorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase);
                }

                return false;
            })
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Resolve now so an invalid "CategoryIcons" config stops startup with a clear error.
app.Services.GetRequiredService<ICategoryIconProvider>();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseCors(WebCorsPolicy);
app.UseCors("AllowLocalhost");
app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

app.Run();
