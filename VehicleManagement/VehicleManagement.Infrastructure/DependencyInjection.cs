using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VehicleManagement.Application.Interfaces;
using VehicleManagement.Infrastructure.Persistence;
using VehicleManagement.Infrastructure.Repositories;

namespace VehicleManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string basePath)
    {
        var connectionString = ResolveAttachDbFilename(
            configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' is not configured."),
            basePath);

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IVehicleCategoryRepository, VehicleCategoryRepository>();
        services.AddScoped<IManufacturerRepository, ManufacturerRepository>();

        return services;
    }

    // SqlClient requires AttachDbFilename to be absolute, so a relative path
    // in configuration is resolved against the host's content root.
    private static string ResolveAttachDbFilename(string connectionString, string basePath)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);

        if (string.IsNullOrEmpty(builder.AttachDBFilename) ||
            Path.IsPathRooted(builder.AttachDBFilename) ||
            builder.AttachDBFilename.StartsWith("|DataDirectory|", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        builder.AttachDBFilename = Path.GetFullPath(
            Path.Combine(basePath, builder.AttachDBFilename));

        return builder.ConnectionString;
    }
}
