using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
namespace MyAvaloniaApp;

public static class SaveManager
{
    public static async Task SaveAsync(ProjectData data, string filePath)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        
        await File.WriteAllTextAsync(filePath, json);
    }

    public static async Task<ProjectData?> LoadAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<ProjectData>(json);
    }
}