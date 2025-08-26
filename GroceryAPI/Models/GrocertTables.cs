#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroceryAPI.Models;

public class Ingredient
{
    public long Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    // Navigation
    public ICollection<FoodIngredient> FoodIngredients { get; set; } = new List<FoodIngredient>();
}

public class FoodIngredient
{
    public long Id { get; set; } // Surrogate PK (easier for updates); you can switch to composite if you prefer.

    // FKs
    public long FoodId { get; set; }
    public long IngredientId { get; set; }

    // Payload
    [MaxLength(200)]
    public string? Quantity { get; set; }

    // Navigation
    public Food Food { get; set; } = null!;
    public Ingredient Ingredient { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
