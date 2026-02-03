using System.Text.Json;
using System.Text.Json.Serialization;

namespace AssetFox.Core.Analysis.Testing.CharacterizationTesting;

public static class Serialization
{
    public static JsonSerializerOptions Options { get; } = new()
    {
        Converters = { new JsonStringEnumConverter() },
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
    };
}
