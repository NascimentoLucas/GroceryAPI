using Microsoft.AspNetCore.Mvc;
using GroceryAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace GroceryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroceryController : ControllerBase
{
    private readonly HttpClient _http;
    private readonly AppDbContext _db;
    private readonly ExtractionOptions _options;
    private PromptOptions _prompt;


    public GroceryController(IHttpClientFactory factory, AppDbContext db, IOptions<ExtractionOptions> opts, IOptions<PromptOptions> prompt)
    {
        _db = db;
        _http = factory.CreateClient("Extraction");
        _options = opts.Value;
        _prompt = prompt.Value;
    }

    [HttpPost("extract")]
    [Consumes("application/json")]
    public async Task<IActionResult> ExtractAsync(
        [FromBody] string text,
        [FromQuery] bool save = true,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return BadRequest(new { error = "Body must be a non-empty string." });


        var payload = new
        {
            model = _prompt.Model,
            input = _prompt.BuildInput(text)
        };

        using var resp = await _http.PostAsJsonAsync("", payload);
        var respText = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
        {
            return StatusCode((int)resp.StatusCode, new
            {
                error = "Upstream extraction failed.",
                status = resp.StatusCode.ToString(),
                body = Truncate(respText, 600)
            });
        }


        Recipe? recipe;
        try
        {

            recipe = RecipeFactory.GetRecipe(respText);
        }
        catch (Exception e)
        {
            return UnprocessableEntity(e.Message);
        }

        if (recipe is null)
        {
            return UnprocessableEntity("Recipe could not be deserialized");
        }

        if (!save)
            return Ok(recipe);

        // ---- Persist (upsert) ----
        // FOOD (unique by name due to CITEXT index)
        var food = await _db.Foods
            .FirstOrDefaultAsync(f => f.Name == recipe.Food, ct);

        if (food is null)
        {
            food = new Food { Id = Guid.NewGuid(), Name = recipe.Food };
            _db.Foods.Add(food);
            await _db.SaveChangesAsync(ct);
        }

        // INGREDIENTS + JOIN (always create a new row)
        var createdJoin = new List<object>();
        if (recipe.Ingredients is not null)
        {
            foreach (var it in recipe.Ingredients.Where(i => !string.IsNullOrWhiteSpace(i.Name)))
            {
                // upsert ingredient by name
                var ing = await _db.Ingredients.FirstOrDefaultAsync(i => i.Name == it.Name, ct);
                if (ing is null)
                {
                    ing = new Ingredient { Id = Guid.NewGuid(), Name = it.Name };
                    _db.Ingredients.Add(ing);
                    await _db.SaveChangesAsync(ct);
                }

                // ALWAYS create a new FoodIngredient row
                var join = new FoodIngredient
                {
                    Id = Guid.NewGuid(),
                    FoodId = food.Id,
                    IngredientId = ing.Id,
                    Quantity = it.Quantity
                };
                _db.FoodIngredients.Add(join);
                await _db.SaveChangesAsync(ct);

                createdJoin.Add(new
                {
                    foodIngredientId = join.Id,
                    ingredientId = ing.Id,
                    ingredientName = ing.Name,
                    quantity = join.Quantity
                });
            }
        }


        return Ok(new
        {
            saved = true,
            food = new { id = food.Id, name = food.Name },
            items = createdJoin
        });
    }

    private static string Truncate(string s, int max)
        => s.Length <= max ? s : s[..max] + "…";

    // --------- minimal response types ----------
    private record ExtractResponse(string Food, List<ExtractIngredient>? Ingredients);
    private record ExtractIngredient(string Name, string? Quantity);
}
