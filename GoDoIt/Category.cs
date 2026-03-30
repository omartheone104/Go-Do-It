using System;
using System.Text.Json.Serialization;
using Avalonia.Media;

namespace GoDoIt;

public record Category(string Name, [property: JsonConverter(typeof(ColorJsonConverter))] Color Color)
{
    [JsonInclude]
    public Guid Id { get; init; } = Guid.NewGuid();
}
