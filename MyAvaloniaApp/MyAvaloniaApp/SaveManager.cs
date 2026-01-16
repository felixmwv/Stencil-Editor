using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MyAvaloniaApp.Shapes;

namespace MyAvaloniaApp
{
    public static class SaveManager
    {
        private static JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters =
            {
                new ShapeBaseConverter()
            }
        };

        public static async Task SaveAsync(ProjectData data, string filePath)
        {
            var json = JsonSerializer.Serialize(data, options);
            await File.WriteAllTextAsync(filePath, json);
        }

        public static async Task<ProjectData?> LoadAsync(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<ProjectData>(json, options);
        }
    }

    public class ShapeBaseConverter : JsonConverter<ShapeBase>
    {
        public override ShapeBase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;

                if (!root.TryGetProperty("ShapeType", out var typeProp))
                    throw new JsonException("Missing ShapeType");

                string type = typeProp.GetString()!;

                return type switch
                {
                    "Circle" => JsonSerializer.Deserialize<CircleShape>(root.GetRawText(), options),
                    "Rectangle" => JsonSerializer.Deserialize<RectangleShape>(root.GetRawText(), options),
                    "Polygon" => JsonSerializer.Deserialize<PolygonShape>(root.GetRawText(), options),
                    _ => throw new JsonException($"Unknown shape type: {type}")
                };
            }
        }

        public override void Write(Utf8JsonWriter writer, ShapeBase value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (value is CircleShape c)
            {
                writer.WriteString("ShapeType", "Circle");
                writer.WriteNumber("X", c.X);
                writer.WriteNumber("Y", c.Y);
                writer.WriteNumber("Scale", c.Scale);
                writer.WriteNumber("Rotation", c.Rotation);
                writer.WriteNumber("Radius", c.Radius);
            }
            else if (value is RectangleShape r)
            {
                writer.WriteString("ShapeType", "Rectangle");
                writer.WriteNumber("X", r.X);
                writer.WriteNumber("Y", r.Y);
                writer.WriteNumber("Scale", r.Scale);
                writer.WriteNumber("Rotation", r.Rotation);
                writer.WriteNumber("Radius", r.Radius);
            }
            else if (value is PolygonShape p)
            {
                writer.WriteString("ShapeType", "Polygon");
                writer.WriteNumber("X", p.X);
                writer.WriteNumber("Y", p.Y);
                writer.WriteNumber("Scale", p.Scale);
                writer.WriteNumber("Rotation", p.Rotation);

                writer.WriteStartArray("Points");
                foreach (var pt in p.Points)
                {
                    writer.WriteStartObject();
                    writer.WriteNumber("X", pt.X);
                    writer.WriteNumber("Y", pt.Y);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
    }
}
