using Godot;

public partial class SaveSystem : Node
{
    public static SaveSystem Instance { get; private set; }

    private const string LegacySavePath = "user://save.json";
    private const int SlotCount = 3;
    private int _currentSlot = 1;

    public int CurrentSlot => _currentSlot;
    public int BestScore { get; private set; }
    public int Coins { get; private set; }
    public int CheckpointX { get; private set; } = 64;
    public int CheckpointY { get; private set; } = 220;

    public override void _Ready()
    {
        Instance = this;
        LoadSlot(_currentSlot, migrateLegacySave: true);
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        Coins += amount;
        Save();
    }

    public void SetBestScore(int score)
    {
        if (score <= BestScore) return;
        BestScore = score;
        Save();
    }

    public void SetCheckpoint(Vector2 position)
    {
        CheckpointX = Mathf.RoundToInt(position.X);
        CheckpointY = Mathf.RoundToInt(position.Y);
        Save();
    }

    public Vector2 GetCheckpoint() => new(CheckpointX, CheckpointY);

    /// <summary>Returns a short UI-friendly description of a save slot.</summary>
    public string GetSlotSummary(int slot)
    {
        if (!IsValidSlot(slot)) return "INVALID SLOT";

        var path = GetSlotPath(slot);
        if (!FileAccess.FileExists(path)) return "EMPTY";

        var data = ReadSaveData(path);
        if (data == null) return "CORRUPTED SAVE";

        var coins = GetInt(data, "coins", 0);
        var best = GetInt(data, "best_score", 0);
        return $"CRYSTALS {coins}  •  BEST {best}";
    }

    /// <summary>Changes the active save slot and loads its progress.</summary>
    public void SelectSlot(int slot)
    {
        if (!IsValidSlot(slot))
        {
            GD.PushWarning($"[Save] Ignoring invalid slot selection: {slot}");
            return;
        }

        if (_currentSlot == slot) return;
        Save();
        _currentSlot = slot;
        LoadSlot(slot);
    }

    /// <summary>Deletes a slot. Deleting the active slot resets it to a fresh save.</summary>
    public void DeleteSlot(int slot)
    {
        if (!IsValidSlot(slot))
        {
            GD.PushWarning($"[Save] Ignoring invalid slot deletion: {slot}");
            return;
        }

        var path = GetSlotPath(slot);
        if (FileAccess.FileExists(path))
        {
            var error = DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(path));
            if (error != Error.Ok)
            {
                GD.PushWarning($"[Save] Could not delete slot {slot}: {error}");
                return;
            }
        }

        if (_currentSlot == slot)
        {
            ResetProgress();
        }
    }

    /// <summary>Starts a fresh adventure in the currently selected slot.</summary>
    public void NewGame()
    {
        ResetProgress();
        Save();
    }

    public void Save()
    {
        var path = GetSlotPath(_currentSlot);
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        if (file == null)
        {
            GD.PushWarning($"[Save] Could not open slot {_currentSlot}: {FileAccess.GetOpenError()}");
            return;
        }

        var data = new Godot.Collections.Dictionary
        {
            ["version"] = 1,
            ["best_score"] = BestScore,
            ["coins"] = Coins,
            ["checkpoint_x"] = CheckpointX,
            ["checkpoint_y"] = CheckpointY
        };
        file.StoreString(Json.Stringify(data));
    }

    private void LoadSlot(int slot, bool migrateLegacySave = false)
    {
        ResetProgress();

        var path = GetSlotPath(slot);
        if (FileAccess.FileExists(path))
        {
            LoadFromPath(path);
            return;
        }

        // Preserve progress created by the original single-file save system.
        if (migrateLegacySave && FileAccess.FileExists(LegacySavePath))
        {
            LoadFromPath(LegacySavePath);
            Save();
            GD.Print("[Save] Migrated legacy save.json into slot 1.");
        }
    }

    private void LoadFromPath(string path)
    {
        var data = ReadSaveData(path);
        if (data == null)
        {
            GD.PushWarning($"[Save] Save data at {path} is invalid. Starting with default progress.");
            return;
        }

        BestScore = Mathf.Max(0, GetInt(data, "best_score", 0));
        Coins = Mathf.Max(0, GetInt(data, "coins", 0));
        CheckpointX = GetInt(data, "checkpoint_x", 64);
        CheckpointY = GetInt(data, "checkpoint_y", 220);
    }

    private static Godot.Collections.Dictionary ReadSaveData(string path)
    {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null) return null;

        var parsed = Json.ParseString(file.GetAsText());
        if (parsed.VariantType != Variant.Type.Dictionary) return null;
        return parsed.AsGodotDictionary();
    }

    private void ResetProgress()
    {
        BestScore = 0;
        Coins = 0;
        CheckpointX = 64;
        CheckpointY = 220;
    }

    private string GetSlotPath(int slot) => $"user://save_slot_{slot}.json";

    private static bool IsValidSlot(int slot) => slot >= 1 && slot <= SlotCount;

    private static int GetInt(Godot.Collections.Dictionary data, string key, int fallback)
    {
        if (!data.ContainsKey(key)) return fallback;

        var value = data[key];
        return value.VariantType switch
        {
            Variant.Type.Int => (int)value,
            Variant.Type.Float => Mathf.RoundToInt((float)value),
            _ => fallback
        };
    }
}
