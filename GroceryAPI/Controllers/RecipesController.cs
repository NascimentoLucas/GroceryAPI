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

    public RecipesController(IConfiguration config, IHttpClientFactory httpFactory)
    {
        _apiKey = config["API_KEY"];
        _endpoint = config["API_URL"];

        if (_apiKey == null || _endpoint == null)
        {
            Console.WriteLine("ENV IS NOT LOAD");
        }

        _http = httpFactory.CreateClient();
    }
}
