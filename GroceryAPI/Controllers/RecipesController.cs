using Microsoft.AspNetCore.Mvc;
using GroceryAPI.Models;

namespace GroceryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipesController : ControllerBase
{
    // simulate a simple in-memory “database”
    private static readonly List<(Guid Id, RecipeDto Data)> _db = new();

    // POST: api/recipes
    [HttpPost]
    public IActionResult CreateRecipe([FromBody] RecipeDto recipe)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var id = Guid.NewGuid();
        _db.Add((id, recipe));

        return CreatedAtAction(
            nameof(GetRecipeById),
            new { id },
            new { id, food = recipe.Food, ingredients = recipe.Ingredients }
        );
    }

    // GET: api/recipes/{id}
    [HttpGet("{id:guid}")]
    public IActionResult GetRecipeById(Guid id)
    {
        var item = _db.FirstOrDefault(x => x.Id == id);
        if (item == default) return NotFound();

        return Ok(new { id = item.Id, food = item.Data.Food, ingredients = item.Data.Ingredients });
    }

    // GET: api/recipes
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_db.Select(r => new { id = r.Id, r.Data.Food, r.Data.Ingredients }));
    }
}
