#nullable enable
using System.Net.Mime;
using GroceryAPI.Models;
using GroceryAPI.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GroceryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class FoodIngredientsController : ControllerBase
{
    private readonly AppDbContext _db;

    public FoodIngredientsController(AppDbContext db) => _db = db;

    // GET: /api/foodingredients?foodId=...&ingredientId=...&page=1&pageSize=20
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FoodIngredientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FoodIngredientDto>>> List(
        [FromQuery] Guid? foodId,
        [FromQuery] Guid? ingredientId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0 || pageSize > 200) pageSize = 20;

        var query = _db.FoodIngredients.AsNoTracking().AsQueryable();

        if (foodId is not null) query = query.Where(fi => fi.FoodId == foodId);
        if (ingredientId is not null) query = query.Where(fi => fi.IngredientId == ingredientId);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(fi => fi.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(fi => new FoodIngredientDto(
                fi.Id, fi.FoodId, fi.IngredientId, fi.Quantity, fi.CreatedAt))
            .ToListAsync();

        Response.Headers["X-Total-Count"] = total.ToString();
        Response.Headers["X-Page"] = page.ToString();
        Response.Headers["X-Page-Size"] = pageSize.ToString();

        return Ok(items);
    }

    // GET: /api/foodingredients/{id}
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FoodIngredientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FoodIngredientDto>> Get(Guid id)
    {
        var fi = await _db.FoodIngredients.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new FoodIngredientDto(x.Id, x.FoodId, x.IngredientId, x.Quantity, x.CreatedAt))
            .FirstOrDefaultAsync();

        return fi is null ? NotFound() : Ok(fi);
    }

    // POST: /api/foodingredients
    [HttpPost]
    [ProducesResponseType(typeof(FoodIngredientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FoodIngredientDto>> Create([FromBody] FoodIngredientCreateDto body)
    {
        // Optional: ensure FKs exist (avoids FK violations surfacing as 500)
        var foodExists = await _db.Foods.AnyAsync(f => f.Id == body.FoodId);
        var ingExists = await _db.Ingredients.AnyAsync(i => i.Id == body.IngredientId);
        if (!foodExists || !ingExists) return BadRequest(new { error = "FoodId or IngredientId not found." });

        var entity = new FoodIngredient
        {
            Id = Guid.NewGuid(),
            FoodId = body.FoodId,
            IngredientId = body.IngredientId,
            Quantity = body.Quantity
        };

        _db.FoodIngredients.Add(entity);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // Violates unique index (FoodId, IngredientId)
            return Conflict(new { error = "This ingredient is already linked to the specified food." });
        }

        var dto = new FoodIngredientDto(entity.Id, entity.FoodId, entity.IngredientId, entity.Quantity, entity.CreatedAt);
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, dto);
    }

    // PUT: /api/foodingredients/{id}
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(FoodIngredientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FoodIngredientDto>> Update(Guid id, [FromBody] FoodIngredientUpdateDto body)
    {
        var entity = await _db.FoodIngredients.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();

        entity.Quantity = body.Quantity;

        await _db.SaveChangesAsync();

        var dto = new FoodIngredientDto(entity.Id, entity.FoodId, entity.IngredientId, entity.Quantity, entity.CreatedAt);
        return Ok(dto);
    }

    // DELETE: /api/foodingredients/{id}
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _db.FoodIngredients.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NoContent();

        _db.FoodIngredients.Remove(entity);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // -------- Optional nested helpers (nice DX) --------
    // POST /api/foodingredients/foods/{foodId}/add
    // Body: { "ingredientId": "...", "quantity": "2 tbsp" }
    [HttpPost("foods/{foodId:guid}/add")]
    [ProducesResponseType(typeof(FoodIngredientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FoodIngredientDto>> AddToFood(
        Guid foodId,
        [FromBody] FoodIngredientUpdateDtoWithIngredient body)
    {
        var foodExists = await _db.Foods.AnyAsync(f => f.Id == foodId);
        var ingExists = await _db.Ingredients.AnyAsync(i => i.Id == body.IngredientId);
        if (!foodExists || !ingExists) return BadRequest(new { error = "FoodId or IngredientId not found." });

        var entity = new FoodIngredient
        {
            Id = Guid.NewGuid(),
            FoodId = foodId,
            IngredientId = body.IngredientId,
            Quantity = body.Quantity
        };

        _db.FoodIngredients.Add(entity);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return Conflict(new { error = "This ingredient is already linked to the specified food." });
        }

        var dto = new FoodIngredientDto(entity.Id, entity.FoodId, entity.IngredientId, entity.Quantity, entity.CreatedAt);
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, dto);
    }

    // PUT /api/foodingredients/foods/{foodId}/ingredients/{ingredientId}
    // Body: { "quantity": "to taste" }
    [HttpPut("foods/{foodId:guid}/ingredients/{ingredientId:guid}")]
    [ProducesResponseType(typeof(FoodIngredientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FoodIngredientDto>> UpdateQuantityForFoodIngredient(
        Guid foodId, Guid ingredientId, [FromBody] FoodIngredientUpdateDto body)
    {
        var entity = await _db.FoodIngredients
            .FirstOrDefaultAsync(x => x.FoodId == foodId && x.IngredientId == ingredientId);

        if (entity is null) return NotFound();

        entity.Quantity = body.Quantity;
        await _db.SaveChangesAsync();

        var dto = new FoodIngredientDto(entity.Id, entity.FoodId, entity.IngredientId, entity.Quantity, entity.CreatedAt);
        return Ok(dto);
    }
    private static bool IsUniqueViolation(DbUpdateException ex)
            => ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation;

}
