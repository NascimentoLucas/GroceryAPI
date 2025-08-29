using DotNetEnv;
using GroceryAPI.Configuration;
using GroceryAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
#region Env
Env.Load();
builder.Configuration.AddEnvironmentVariables();
builder.Configuration
   .AddFileSecrets("/etc/secrets", optional: true);

const string dbString = "Default";
var connectionString = builder.Configuration.GetConnectionString(dbString);

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Connection string '" + dbString + "' is required.");

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(connectionString));

var apiKey = builder.Configuration["Services:Extraction:ApiKey"];
var apiUrl = builder.Configuration["Services:Extraction:Url"];

builder.Services.AddOptions<ExtractionOptions>()
    .Bind(builder.Configuration.GetSection(ExtractionOptions.Section))
    .Validate(o => !string.IsNullOrWhiteSpace(o.ApiUrl), "Extraction Url is required")
    .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "Extraction ApiKey is required")
    .ValidateOnStart(); // fail fast at startup

builder.Services.AddHttpClient("Extraction", (sp, http) =>
{
    var cfg = sp.GetRequiredService<IOptions<ExtractionOptions>>().Value;

    if (string.IsNullOrWhiteSpace(cfg.ApiUrl))
        throw new InvalidOperationException("Missing Services:Extraction:Url configuration.");
    if (string.IsNullOrWhiteSpace(cfg.ApiKey))
        throw new InvalidOperationException("Missing Services:Extraction:ApiKey configuration.");

    http.BaseAddress = new Uri(cfg.ApiUrl);
    http.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", cfg.ApiKey);
});



builder.Services.AddOptions<PromptOptions>()
    .Bind(builder.Configuration.GetSection(PromptOptions.Section))
    .Validate(o => !string.IsNullOrWhiteSpace(o.Prompt), "Prompt is required")
    .Validate(o => !string.IsNullOrWhiteSpace(o.Model), "Model is required")
    .ValidateOnStart();
#endregion

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen();
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });
}

builder.Services.AddControllers();
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors();
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
