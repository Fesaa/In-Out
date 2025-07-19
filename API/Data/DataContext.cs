using System.Text.Json;
using API.Entities;
using API.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace API.Data;

public class DataContext: DbContext
{
    public DataContext(DbContextOptions<DataContext> options): base(options)
    {
        ChangeTracker.Tracked += OnEntityTracked;
        ChangeTracker.StateChanged += OnEntityStateChanged;
    }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Delivery>()
            .Property(d => d.SystemMessages)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                v => JsonSerializer.Deserialize<List<SystemMessage>>(v, JsonSerializerOptions.Default) ?? new List<SystemMessage>()
            );
    }
    
    public DbSet<ServerSetting> ServerSettings { get; set; }
    public DbSet<User> Users { get; set; }
    
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Delivery> Deliveries { get; set; }
    public DbSet<DeliveryLine> DeliveryLines { get; set; }
    
    public DbSet<Client> Clients { get; set; }
    
    private static void OnEntityTracked(object? sender, EntityTrackedEventArgs e)
    {
        if (e.FromQuery || e.Entry.State != EntityState.Added || e.Entry.Entity is not IEntityDate entity) return;

        entity.LastModifiedUtc = DateTime.UtcNow;

        // This allows for mocking
        if (entity.CreatedUtc == DateTime.MinValue)
        {
            entity.CreatedUtc = DateTime.UtcNow;
        }
    }
    
    private static void OnEntityStateChanged(object? sender, EntityStateChangedEventArgs e)
    {
        if (e.NewState != EntityState.Modified || e.Entry.Entity is not IEntityDate entity) return;
        entity.LastModifiedUtc = DateTime.UtcNow;
    }
    
    private void OnSaveChanges()
    {
        foreach (var saveEntity in ChangeTracker.Entries()
                     .Where(e => e.State == EntityState.Modified)
                     .Select(entry => entry.Entity)
                     .OfType<IHasConcurrencyToken>())
        {
            saveEntity.OnSavingChanges();
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        OnSaveChanges();

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        OnSaveChanges();

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
    
}