using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace GroceryAPI.Configuration;

/// <summary>
/// Reads secrets from a directory (e.g., /run/secrets) where each file name is a key
/// and the file content is the value. Double underscores in file names map to nested keys.
/// Example: a file named "Services__Extraction__ApiKey" becomes "Services:Extraction:ApiKey".
/// </summary>
public sealed class FileSecretsConfigurationSource : IConfigurationSource
{
    public string DirectoryPath { get; }
    public bool Optional { get; }
    public string? KeyPrefix { get; }

    public FileSecretsConfigurationSource(string directoryPath, bool optional = true, string? keyPrefix = null)
    {
        DirectoryPath = directoryPath;
        Optional = optional;
        KeyPrefix = keyPrefix;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
        => new FileSecretsConfigurationProvider(DirectoryPath, Optional, KeyPrefix);
}

public sealed class FileSecretsConfigurationProvider : ConfigurationProvider
{
    private readonly string _directoryPath;
    private readonly bool _optional;
    private readonly string? _keyPrefix;

    public FileSecretsConfigurationProvider(string directoryPath, bool optional, string? keyPrefix)
    {
        _directoryPath = directoryPath;
        _optional = optional;
        _keyPrefix = keyPrefix;
    }

    public override void Load()
    {
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        if (!Directory.Exists(_directoryPath))
        {
            if (_optional) return;
            throw new DirectoryNotFoundException($"Secrets directory not found: {_directoryPath}");
        }

        foreach (var file in Directory.EnumerateFiles(_directoryPath))
        {
            // fileName now has no extension
            var fileName = Path.GetFileNameWithoutExtension(file);

            // ignore dotfiles and empty names
            if (string.IsNullOrWhiteSpace(fileName) || fileName.StartsWith('.')) continue;

            // map "A__B__C" => "A:B:C"
            var key = fileName.Replace("__", ":", StringComparison.Ordinal);

            if (!string.IsNullOrEmpty(_keyPrefix))
                key = $"{_keyPrefix}:{key}";

            // read/trim trailing newlines
            var value = File.ReadAllText(file).TrimEnd('\r', '\n');

            data[key] = value;
        }

        Data = data;
    }
}

public static class FileSecretsConfigurationExtensions
{
    /// <summary>
    /// Add secrets from a directory (default: /run/secrets).
    /// Each file name is used as the configuration key; file content is the value.
    /// </summary>
    public static IConfigurationBuilder AddFileSecrets(
        this IConfigurationBuilder builder,
        string directoryPath = "/run/secrets",
        bool optional = true,
        string? keyPrefix = null)
    {
        return builder.Add(new FileSecretsConfigurationSource(directoryPath, optional, keyPrefix));
    }
}
