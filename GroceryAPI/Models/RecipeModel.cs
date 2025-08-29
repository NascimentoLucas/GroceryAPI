using System.Text.Json.Serialization;

namespace GroceryAPI.Models;

public sealed class Recipe
{
    [JsonPropertyName("food")]
    public string Food { get; set; } = "";
    [JsonPropertyName("ingredients")]
    public List<RecipeIngredients> Ingredients { get; set; } = new();
}

public sealed class RecipeIngredients
{

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("quantity")]
    public string Quantity { get; set; } = "";
}