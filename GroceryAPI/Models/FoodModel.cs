
using System.ComponentModel.DataAnnotations;

namespace GroceryAPI.Models
{
    public class Food
    {
        public Guid Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        // Navigation
        public ICollection<FoodIngredient> FoodIngredients { get; set; } = [];
    }
}


namespace GroceryAPI.DTOs
{
    public sealed record FoodDto(Guid Id, string Name, DateTime CreatedAt);
    public sealed record CreateFoodDto(string Name);
    public sealed record UpdateFoodDto(string Name);

}