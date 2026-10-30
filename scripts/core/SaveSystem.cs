using Godot;

public partial class SaveSystem : Node
{
    public static SaveSystem Instance { get; private set; }

    private const int SlotCount = 3;
    private int _currentSlot = 1;
    private string SavePath => $"user://save_slot_{_currentSlot}.json";

    public int CurrentSlot => _currentSlot;
    public int BestScore { get; private set; }
    public int Coins { get; private set; }
    public int CheckpointX { get; private set; } = 64;
    public int CheckpointY { get; private set; } = 220;

    public override void _Ready()
    {
        Instance = this;
        var slotConfig = new ConfigFile();
        if (slotConfig.Load("user://active_slot.cfg") == Error.Ok)
            _currentSlot = Mathf.Clamp((int)slotConfig.GetValue("save", "active_slot", 1), 1, SlotCount);
        MigrateLegacySave();
        Load();
    }

    public bool HasSave(int slot)
    {
        if (slot < 1 || slot > SlotCount) return false;
        return FileAccess.FileExists($"user://save_slot_{slot}.json");
    }

    public string GetSlotSummary(int slot)
    {
        if (slot < 1 || slot > SlotCount) return "Invalid slot";
        if (!HasSave(slot)) return "Empty";
        if (slot == _currentSlot) return $"Selected • {Coins} crystals";
        using var file = FileAccess.Open($"user://save_slot_{slot}.json", FileAccess.ModeFlags.Read);
        if (file == null) return "Unreadable save";
        var parsed = Json.ParseString(file.GetAsText());
        if (parsed.VariantType != Variant.Type.Dictionary) return "Damaged save";
        var data = parsed.AsGodotDictionary();
        var coins = data.ContainsKey("coins") ? (int)data["coins"] : 0;
        return $"{coins} crystals";
    }

    public void SelectSlot(int slot)
    {
        if (slot < 1 || slot > SlotCount) return;
        _currentSlot = slot;
        var config = new ConfigFile();
        config.SetValue("save", "active_slot", _currentSlot);
        config.Save("user://active_slot.cfg");
        Load();
    }

    public void DeleteSlot(int slot)
    {
        if (slot < 1 || slot > SlotCount) return;
        var path = $"user://save_slot_{slot}.json";
        if (FileAccess.FileExists(path)) DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(path));
        if (slot == _currentSlot)
        {
            BestScore = 0;
            Coins = 0;
            CheckpointX = 64;
            CheckpointY = 220;
        }
    }

    public void NewGame()
    {
        BestScore = 0;
        Coins = 0;
        CheckpointX = 64;
        CheckpointY = 220;
        Save();
    }

    public void AddCoins(int amount)
    {
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

    public void Save()
    {
        using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
        if (file == null)
        {
            GD.PushWarning("[Save] Could not open save file: " + FileAccess.GetOpenError());
            return;
        }

        var data = new Godot.Collections.Dictionary
        {
            ["version"] = 2,
            ["slot"] = _currentSlot,
            ["best_score"] = BestScore,
            ["coins"] = Coins,
            ["checkpoint_x"] = CheckpointX,
            ["checkpoint_y"] = CheckpointY
        };
        file.StoreString(Json.Stringify(data));
    }

    private void Load()
    {
        BestScore = 0;
        Coins = 0;
        CheckpointX = 64;
        CheckpointY = 220;
        if (!FileAccess.FileExists(SavePath)) return;
        using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
        if (file == null) return;

        var parsed = Json.ParseString(file.GetAsText());
        if (parsed.VariantType != Variant.Type.Dictionary) return;
        var data = parsed.AsGodotDictionary();
        BestScore = GetInt(data, "best_score", 0);
        Coins = GetInt(data, "coins", 0);
        CheckpointX = GetInt(data, "checkpoint_x", 64);
        CheckpointY = GetInt(data, "checkpoint_y", 220);
    }

    private void MigrateLegacySave()
    {
        if (FileAccess.FileExists("user://save_slot_1.json") || !FileAccess.FileExists("user://save.json")) return;
        using var legacy = FileAccess.Open("user://save.json", FileAccess.ModeFlags.Read);
        if (legacy == null) return;
        using var migrated = FileAccess.Open("user://save_slot_1.json", FileAccess.ModeFlags.Write);
        if (migrated != null) migrated.StoreString(legacy.GetAsText());
    }

    private static int GetInt(Godot.Collections.Dictionary data, string key, int fallback)
    {
        return data.ContainsKey(key) ? (int)data[key] : fallback;
    }
}
