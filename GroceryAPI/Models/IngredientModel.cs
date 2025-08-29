#nullable enable
using System.ComponentModel.DataAnnotations;

namespace GroceryAPI.Models
{

    public class Ingredient
    {
        public Guid Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        // Navigation
        public ICollection<FoodIngredient> FoodIngredients { get; set; } = new List<FoodIngredient>();
    }
}

namespace GroceryAPI.DTOs
{
    // Simple list/read DTO
    public sealed record IngredientDto(
        Guid Id,
        string Name,
        DateTime CreatedAt
    );

    // Create payload
    public sealed class CreateIngredientDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;
    }

    // Update payload
    public sealed class UpdateIngredientDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;
    }
}