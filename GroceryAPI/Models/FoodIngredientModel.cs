#nullable enable
using System.ComponentModel.DataAnnotations;

namespace GroceryAPI.Models
{
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
}


namespace GroceryAPI.Dtos
{
    public record FoodIngredientDto(
        Guid Id,
        Guid FoodId,
        Guid IngredientId,
        string? Quantity,
        DateTime CreatedAt
    );

    public class FoodIngredientCreateDto
    {
        [Required] public Guid FoodId { get; set; }
        [Required] public Guid IngredientId { get; set; }
        [MaxLength(200)] public string? Quantity { get; set; }
    }

    public class FoodIngredientUpdateDto
    {
        [MaxLength(200)] public string? Quantity { get; set; }
    }

    public class FoodIngredientUpdateDtoWithIngredient : FoodIngredientUpdateDto
    {
        [Required] public Guid IngredientId { get; set; }
    }

}
