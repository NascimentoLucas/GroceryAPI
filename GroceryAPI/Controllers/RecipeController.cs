

using System.Text.Json;
using GroceryAPI.Models;

namespace GroceryAPI.Controllers;


public static class RecipeFactory
{
    internal static Recipe? GetRecipe(string respText)
    {
        UpstreamResponse? upstream;

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        upstream = JsonSerializer.Deserialize<UpstreamResponse>(respText, options);
        if (upstream is null)
            throw new InvalidOperationException("Falha ao desserializar resposta do upstream.");

        if (upstream.Output.Count < 1)
        {
            throw new Exception("Output is empty");
        }

        string? output;
        try
        {
            output = upstream.Output[0].Content[0].Text;
        }
        catch (Exception)
        {
            throw;
        }

        if (string.IsNullOrEmpty(output))
            throw new Exception("Output is null");

        return JsonSerializer.Deserialize<Recipe>(output, options);
    }
}