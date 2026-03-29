using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Media;

namespace GoDoIt;

class EventJsonConverter : JsonConverter<Event>
{
    public override Event? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Guid id = Guid.Empty;
        string title = "";
        string description = "";
        DateTime dueDate = DateTime.MinValue;
        Guid categoryId = Guid.Empty;
        Guid? parentId = null;
        bool isComplete = false;
        RepeatInterval repeat = RepeatInterval.None;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject) break;
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            string? prop = reader.GetString();
            reader.Read(); // Move to value

            switch (prop)
            {
                case nameof(Event.Id):
                    id = reader.GetGuid();
                    break;
                case nameof(Event.Title):
                    title = reader.GetString() ?? "";
                    break;
                case nameof(Event.Description):
                    description = reader.GetString() ?? "";
                    break;
                case nameof(Event.DueDate):
                    dueDate = reader.GetDateTime();
                    break;
                case nameof(Event.CategoryId):
                    categoryId = reader.GetGuid();
                    break;
                case nameof(Event.ParentId):
                    parentId = reader.TokenType == JsonTokenType.Null ? null : reader.GetGuid();
                    break;
                case nameof(Event.IsComplete):
                    isComplete = reader.GetBoolean();
                    break;
                case nameof(Event.RepeatInterval):
                    repeat = JsonSerializer.Deserialize<RepeatInterval>(ref reader, options);
                    break;
            }
        }

        var @event = new Event(title, description, dueDate, categoryId, parentId, isComplete, repeat)
        {
            Id = id
        };

        return @event;
    }

    public override void Write(Utf8JsonWriter writer, Event value, JsonSerializerOptions options)
    {
        // options.Converters.Add(new RepeatIntervalJsonConverter());
        writer.WriteStartObject();
        // throw new NotImplementedException();
        writer.WriteString(nameof(value.Id), value.Id);
        writer.WriteString(nameof(value.Title), value.Title);
        writer.WriteString(nameof(value.Description), value.Description);
        writer.WriteString(nameof(value.DueDate), value.DueDate);
        writer.WriteString(nameof(value.CategoryId), value.CategoryId);
        if (value.ParentId is Guid pid)
        {
            writer.WriteString(nameof(value.ParentId), pid);
        }
        else
        {
            writer.WriteNull(nameof(value.ParentId));
        }

        writer.WriteBoolean(nameof(value.IsComplete), value.IsComplete);
        writer.WritePropertyName(nameof(value.RepeatInterval));
        JsonSerializer.Serialize(writer, value.RepeatInterval, options);

        writer.WriteEndObject();
    }
}

class RepeatIntervalJsonConverter : JsonConverter<RepeatInterval>
{
    public override RepeatInterval Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
    reader.GetString()?.Normalize().ToLowerInvariant() switch
    {
        "daily" => RepeatInterval.Daily,
        "weekly" => RepeatInterval.Weekly,
        "monthly" => RepeatInterval.Monthly,
        "yearly" => RepeatInterval.Yearly,
        _ => RepeatInterval.None
    };

    public override void Write(Utf8JsonWriter writer, RepeatInterval value, JsonSerializerOptions options) =>
    writer.WriteStringValue((value switch
    {
        RepeatInterval.Daily => nameof(RepeatInterval.Daily),
        RepeatInterval.Weekly => nameof(RepeatInterval.Weekly),
        RepeatInterval.Monthly => nameof(RepeatInterval.Monthly),
        RepeatInterval.Yearly => nameof(RepeatInterval.Yearly),
        _ => nameof(RepeatInterval.None),
    }).Normalize().ToLowerInvariant());
}

class ColorJsonConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var colorString = reader.GetString() ?? "";
        try
        {
            return Color.Parse(colorString);
        }
        catch
        {
            return Color.FromRgb(0xff, 0xff, 0xff);
        }
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        // throw new NotImplementedException();
        writer.WriteStringValue(value.ToString());
    }
}