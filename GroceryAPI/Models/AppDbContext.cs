using Microsoft.EntityFrameworkCore;

namespace GroceryAPI.Models;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Food> Foods => Set<Food>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<FoodIngredient> FoodIngredients => Set<FoodIngredient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- foods ---
        modelBuilder.Entity<Food>(e =>
        {
            e.ToTable("foods");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")                 // PostgreSQL
            .HasDefaultValueSql("gen_random_uuid()");

            e.Property(x => x.Name)
                .HasColumnName("name")
                .IsRequired();

            e.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd();

            e.HasMany(x => x.FoodIngredients)
                .WithOne(fi => fi.Food)
                .HasForeignKey(fi => fi.FoodId)
                .OnDelete(DeleteBehavior.Restrict); // avoid cascading deletes across the join
        });

        // --- ingredients ---
        modelBuilder.Entity<Ingredient>(e =>
        {
            e.ToTable("ingredients");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")                 // PostgreSQL
            .HasDefaultValueSql("gen_random_uuid()");

            e.Property(x => x.Name)
                .HasColumnName("name")
                .HasColumnType("citext")
                .IsRequired();

            e.HasIndex(x => x.Name).IsUnique();

            e.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd();

            e.HasMany(x => x.FoodIngredients)
              .WithOne(fi => fi.Ingredient)
              .HasForeignKey(fi => fi.IngredientId)
              .OnDelete(DeleteBehavior.Restrict);
        });

        // --- food_ingredients (join with payload) ---
        modelBuilder.Entity<FoodIngredient>(e =>
        {
            e.ToTable("food_ingredients");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("id");

            e.Property(x => x.FoodId)
            .HasColumnName("food_id")
            .HasColumnType("uuid");

            e.Property(x => x.IngredientId)
            .HasColumnName("ingredient_id")
            .HasColumnType("uuid");

            e.Property(x => x.Quantity)
                .HasColumnName("quantity")
                .HasColumnType("text");

            // avoid duplicate ingredient entries per food
            e.HasIndex(x => new { x.FoodId, x.IngredientId }).IsUnique();

            e.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd();
        });
    }
}
