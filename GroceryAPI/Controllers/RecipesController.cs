using Microsoft.AspNetCore.Mvc;
using GroceryAPI.Models;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;


namespace GroceryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipesController : ControllerBase
{
    public readonly string? _endpoint;
    private readonly string? _apiKey;
    private readonly HttpClient _http;
    private readonly FoodsController _foods;

    public RecipesController(IConfiguration config, IHttpClientFactory httpFactory, AppDbContext db)
    {
        _apiKey = config["API_KEY"];
        _endpoint = config["API_URL"];
        if (_apiKey == null || _endpoint == null)
        {
            throw new Exception("ENV IS NOT LOAD");
        }

        _foods = new FoodsController(db);
        _http = httpFactory.CreateClient();
    }
}
