using Godot;

public partial class SaveSystem : Node
{
	public static SaveSystem Instance { get; private set; }

    private const string SavePath = "user://save.json";
    public int BestScore { get; private set; }
    public int Coins { get; private set; }
    public int CheckpointX { get; private set; } = 64;
    public int CheckpointY { get; private set; } = 220;

    public override void _Ready()
    {
        Instance = this;
        Load();
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
            ["version"] = 1,
            ["best_score"] = BestScore,
            ["coins"] = Coins,
            ["checkpoint_x"] = CheckpointX,
            ["checkpoint_y"] = CheckpointY
        };
        file.StoreString(Json.Stringify(data));
    }

    private void Load()
    {
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

	private static int GetInt(Godot.Collections.Dictionary data, string key, int fallback)
	{
		return data.ContainsKey(key) ? (int)data[key] : fallback;
	}
}
