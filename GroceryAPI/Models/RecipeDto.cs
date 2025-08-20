using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GroceryAPI.Models;
public class RecipeDto
{
    // maps the "food" field from JSON
    [Required]
    [JsonPropertyName("food")]
    public string Food { get; set; } = string.Empty;

    // maps the "ingredients" array from JSON
    [Required]
    [MinLength(1)]
    [JsonPropertyName("ingredients")]
    public List<IngredientDto> Ingredients { get; set; } = new();
}

public class IngredientDto
{
    // maps "name"
    [Required]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    // maps "quantity"
    [Required]
    [JsonPropertyName("quantity")]
    public string Quantity { get; set; } = string.Empty;
}
