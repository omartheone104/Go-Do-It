using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using Avalonia.Media;
using Ical.Net;

namespace GoDoIt;

public record Category(string Name = "", [property: JsonConverter(typeof(ColorJsonConverter))] Color Color = default)
{
    [JsonInclude]
    public Guid Id { get; internal init; } = Guid.NewGuid();

    public static readonly string NAME_PARAM = "X-NAME".ToUpperInvariant();
    public static readonly string COLOR_PARAM = "X-COLOR".ToUpperInvariant();
    public static readonly string PROPERTY_NAME = "X-CATEGORY".ToUpperInvariant();
    public CalendarProperty AsCalendarProperty()
    {
        var prop = new CalendarProperty("X-CATEGORY", Id.ToString());
        prop.AddParameter(NAME_PARAM, Name);
        prop.AddParameter(COLOR_PARAM, Color.ToString());

        return prop;
    }

    public static bool TryFromCalendarProperty([NotNullWhen(true)] ICalendarProperty? value, [NotNullWhen(true)] out Category? category)
    {
        if (value is null || value.Name != PROPERTY_NAME)
        {
            category = null;
            return false;
        }

        if (value.Parameters.First(p => p.Name == NAME_PARAM).Value is string name &&
        Color.TryParse(value.Parameters.First(p => p.Name == COLOR_PARAM).Value, out var color) &&
        Guid.TryParse(value.Value?.ToString(), out var id))
        {
            category = new(name, color) { Id = id };
            return true;
        }

        category = null;
        return false;
    }

    public static Category? FromCalendarProperty(ICalendarProperty? value)
    {
        if (TryFromCalendarProperty(value, out var cat))
        {
            return cat;
        }
        return null;
    }
}
