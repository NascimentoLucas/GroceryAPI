#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroceryAPI.Models;


public class FoodIngredient
{
    public Guid Id { get; set; } // Surrogate PK (easier for updates); you can switch to composite if you prefer.

    // FKs
    public Guid FoodId { get; set; }
    public Guid IngredientId { get; set; }

    // Payload
    [MaxLength(200)]
    public string? Quantity { get; set; }

    // Navigation
    public Food Food { get; set; } = null!;
    public Ingredient Ingredient { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
