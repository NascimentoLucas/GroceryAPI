namespace GroceryAPI.Models;

public sealed class ExtractionOptions
{
    public const string Section = "Services:Extraction";
    public string ApiUrl { get; init; } = default!;
    public string ApiKey { get; init; } = default!;
}

public sealed class PromptOptions
{
    public const string Section = "Services:Prompt";
    public string Model { get; init; } = default!;
    public string Prompt { get; init; } = default!;

    public string BuildInput(string text)
        => $"{Prompt}\n{text}";
}
