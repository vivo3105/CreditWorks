using Microsoft.EntityFrameworkCore;
using VehicleManagement.Application.Interfaces;
using VehicleManagement.Domain.Entities;
using VehicleManagement.Domain.Services;

namespace VehicleManagement.Infrastructure.Persistence;

public class AppDbContext : DbContext, IUnitOfWork
{
    private static readonly CategoryConfigurationValidator CategoryValidator = new();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    public DbSet<VehicleCategory> VehicleCategories => Set<VehicleCategory>();

    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        if (CategoriesChanged())
        {
            // Bring every stored category into the tracker. Ones already tracked
            // keep their pending changes (identity resolution), so the tracker then
            // holds the set as it will be after saving.
            await VehicleCategories.LoadAsync(cancellationToken);
            ValidateTrackedCategories();
        }

        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        if (CategoriesChanged())
        {
            VehicleCategories.Load();
            ValidateTrackedCategories();
        }

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    // Last line of defence: whatever code path adds, edits or deletes a
    // category, no two weight ranges in the resulting set may clash.
    // (VehicleCategoryService also validates first, to fail before any changes.)
    private bool CategoriesChanged() =>
        ChangeTracker
            .Entries<VehicleCategory>()
            .Any(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);

    private void ValidateTrackedCategories()
    {
        var resultingSet = ChangeTracker
            .Entries<VehicleCategory>()
            .Where(e => e.State is not (EntityState.Deleted or EntityState.Detached))
            .Select(e => e.Entity)
            .ToList();

        CategoryValidator.Validate(resultingSet);
    }
}
