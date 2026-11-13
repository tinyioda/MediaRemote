using System.Text.Json;

namespace MediaRemote;

/// <summary>Persists the user's button mapping across restarts as a small JSON file.</summary>
internal static class MappingStore
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MediaRemote",
        "mapping.json");

    public static ButtonMapping Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                var assignments = JsonSerializer.Deserialize<Dictionary<RemoteAction, RemoteButtons>>(json);
                if (assignments is not null)
                {
                    return ButtonMapping.FromAssignments(assignments);
                }
            }
        }
        catch
        {
            // Missing, unreadable, or invalid (e.g. hand-edited) file: fall back
            // to defaults rather than fail to start.
        }

        return ButtonMapping.CreateDefault();
    }

    public static void Save(ButtonMapping mapping)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(mapping.Assignments));
        }
        catch
        {
            // Best-effort: losing a save just means the remap doesn't survive a
            // restart, it shouldn't take the app down.
        }
    }
}
