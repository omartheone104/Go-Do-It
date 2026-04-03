using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GoDoIt;

file record AppData(
    [property: JsonPropertyName("tasks")] List<Event> Tasks,
    [property: JsonPropertyName("categories")] List<Category> Categories
);

public static class StorageService
{
    // ~/.local/share/GoDoIt/data.json (Linux)
    // %APPDATA%\GoDoIt\data.json (Windows)
    // ~/Library/Application Support/GoDoIt/data.json (macOS)
    private static readonly string DataDir =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GoDoIt");

    private static readonly string DataFile = Path.Combine(DataDir, "data.json");
    private static readonly string BackupFile = Path.Combine(DataDir, "data.backup.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters =
        {
            new EventJsonConverter(),
            new RepeatIntervalJsonConverter(),
            new ColorJsonConverter(),
        }
    };

    public static (List<Event> Tasks, List<Category> Categories) Load()
    {
        if (!File.Exists(DataFile))
            return ([], []);

        try
        {
            var json = File.ReadAllText(DataFile);
            var data = JsonSerializer.Deserialize<AppData>(json, JsonOptions);
            return (data?.Tasks ?? [], data?.Categories ?? []);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[GoDoIt] Failed to load data.json: {ex.Message}");
            return TryLoadBackup();
        }
    }

    public static void Save(IEnumerable<Event> tasks, IEnumerable<Category> categories)
    {
        try
        {
            Directory.CreateDirectory(DataDir);

            if (File.Exists(DataFile))
                File.Copy(DataFile, BackupFile, overwrite: true);

            var data = new AppData([.. tasks], [.. categories]);
            var json = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(DataFile, json);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[GoDoIt] Failed to save data.json: {ex.Message}");
        }
    }

    private static (List<Event> Tasks, List<Category> Categories) TryLoadBackup()
    {
        if (!File.Exists(BackupFile))
            return ([], []);

        try
        {
            Console.Error.WriteLine("[GoDoIt] Attempting to restore from backup...");
            var json = File.ReadAllText(BackupFile);
            var data = JsonSerializer.Deserialize<AppData>(json, JsonOptions);
            return (data?.Tasks ?? [], data?.Categories ?? []);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[GoDoIt] Backup restore also failed: {ex.Message}");
            return ([], []);
        }
    }
}
