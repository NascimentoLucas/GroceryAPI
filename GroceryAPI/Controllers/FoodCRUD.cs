#nullable enable
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GroceryAPI.Models;
using GroceryAPI.DTOs;

// If you're on Npgsql, this helps catch unique/FK violations:
using Npgsql;

namespace GroceryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class FoodsController : ControllerBase
{
    private readonly AppDbContext _db;

    public FoodsController(AppDbContext db) => _db = db;

    // GET /api/foods?query=rice&page=1&pageSize=20
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<FoodDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<FoodDto>>> GetAll(
        [FromQuery] string? query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 200 ? 20 : pageSize;

        var foods = _db.Foods.AsNoTracking();

        // Case-insensitive search (works great with CITEXT; ILIKE is extra-friendly for partials)
        if (!string.IsNullOrWhiteSpace(query))
        {
            foods = foods.Where(f => EF.Functions.ILike(f.Name, $"%{query.Trim()}%"));
        }

        var total = await foods.CountAsync();
        var items = await foods
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FoodDto(f.Id, f.Name, f.CreatedAt))
            .ToListAsync();

        return Ok(new PagedResult<FoodDto>(items, page, pageSize, total));
    }

    // GET /api/foods/{id}
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FoodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FoodDto>> GetById([FromRoute] Guid id)
    {
        var f = await _db.Foods.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new FoodDto(x.Id, x.Name, x.CreatedAt))
            .FirstOrDefaultAsync();

        if (f is null) return NotFound();
        return Ok(f);
    }

    // POST /api/foods
    [HttpPost]
    [ProducesResponseType(typeof(FoodDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FoodDto>> Create([FromBody] CreateFoodDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { error = "Name is required." });

        var entity = new Food { Name = dto.Name.Trim() };

        _db.Foods.Add(entity);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return Conflict(new { error = "A food with this name already exists." });
        }

        // Reload to capture DB defaults (CreatedAt)
        await _db.Entry(entity).ReloadAsync();

        var result = new FoodDto(entity.Id, entity.Name, entity.CreatedAt);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
    }

    // PUT /api/foods/{id}
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(FoodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FoodDto>> Update([FromRoute] Guid id, [FromBody] UpdateFoodDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { error = "Name is required." });

        var entity = await _db.Foods.FirstOrDefaultAsync(f => f.Id == id);
        if (entity is null) return NotFound();

        entity.Name = dto.Name.Trim();

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return Conflict(new { error = "A food with this name already exists." });
        }

        var result = new FoodDto(entity.Id, entity.Name, entity.CreatedAt);
        return Ok(result);
    }

    // DELETE /api/foods/{id}
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var entity = await _db.Foods.FirstOrDefaultAsync(f => f.Id == id);
        if (entity is null) return NotFound();

        _db.Foods.Remove(entity);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsForeignKeyRestriction(ex))
        {
            // Your model uses Restrict on FoodIngredients => surface a clear message
            return Conflict(new { error = "Cannot delete this food because it has ingredients linked." });
        }

        return NoContent();
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation;

    private static bool IsForeignKeyRestriction(DbUpdateException ex)
        => ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.ForeignKeyViolation;
}
