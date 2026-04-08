using System;
using System.Linq;
using System.Text.Json.Serialization;
using Avalonia.Media;
using Ical.Net;

namespace GoDoIt;

public record Category(string Name = "", [property: JsonConverter(typeof(ColorJsonConverter))] Color Color = default)
{
    [JsonInclude]
    public Guid Id { get; init; } = Guid.NewGuid();

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

    public static Category FromCalendarProperty(ICalendarProperty value)
    {
        if (value.Name == PROPERTY_NAME)
        {
            string name = value.Parameters.First(p => p.Name == NAME_PARAM)?.Value ?? string.Empty;
            _ = Color.TryParse(value.Parameters.First(p => p.Name == COLOR_PARAM).Value, out var color); // returns default color if none found
            var id = value.Value as Guid? ?? Guid.NewGuid();
            return new(name, color) { Id = id };
        }
        else
        {
            return new();
        }
    }
}
