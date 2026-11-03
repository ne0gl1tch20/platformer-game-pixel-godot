using Godot;

public partial class SaveSystem : Node
{
	private const int SlotCount = 3;
	private const int DefaultSlot = 1;
	private const string LegacySavePath = "user://save.json";

	public static SaveSystem Instance { get; private set; }

	private int _currentSlot = DefaultSlot;

	public int CurrentSlot => _currentSlot;
	public int BestScore { get; private set; }
	public int Coins { get; private set; }
	public int CheckpointX { get; private set; } = 64;
	public int CheckpointY { get; private set; } = 220;

	public override void _Ready()
	{
		Instance = this;
		LoadInitialSave();
	}

	public override void _ExitTree()
	{
		if (Instance == this)
			Instance = null;
	}

	public void AddCoins(int amount)
	{
		if (amount <= 0)
			return;

		Coins += amount;
		Save();
	}

	public void SetBestScore(int score)
	{
		if (score <= BestScore)
			return;

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
		if (!IsValidSlot(slot))
			return "INVALID SLOT";

		var path = GetSlotPath(slot);
		if (!FileAccess.FileExists(path))
			return "EMPTY";

		var data = ReadSaveData(path);
		if (data == null)
			return "CORRUPTED SAVE";

		var coins = GetInt(data, "coins", 0);
		var best = GetInt(data, "best_score", 0);
		return $"COINS {coins}  •  BEST {best}";
	}

	/// <summary>Changes the active save slot and loads its progress.</summary>
	public void SelectSlot(int slot)
	{
		if (!IsValidSlot(slot))
		{
			GD.PushWarning($"[Save] Ignoring invalid slot selection: {slot}");
			return;
		}

		if (_currentSlot == slot)
			return;

		Save();
		_currentSlot = slot;
		LoadSlot(slot);
	}

	/// <summary>Deletes a slot. Deleting the active slot resets its in-memory progress.</summary>
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
			ResetProgress();
	}

	/// <summary>Starts a fresh adventure in the currently selected slot.</summary>
	public void NewGame()
	{
		ResetProgress();
		Save();
	}

	/// <summary>Writes the current progress to the active slot.</summary>
	public void Save()
	{
		if (!IsValidSlot(_currentSlot))
		{
			GD.PushWarning($"[Save] Refusing to write invalid slot: {_currentSlot}");
			return;
		}

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
		file.Flush();
	}

	private void LoadInitialSave()
	{
		_currentSlot = DefaultSlot;

		if (FileAccess.FileExists(GetSlotPath(_currentSlot)))
		{
			LoadSlot(_currentSlot);
			return;
		}

		// Migrate the original single-file save without deleting the source file.
		if (FileAccess.FileExists(LegacySavePath))
		{
			var legacyData = ReadSaveData(LegacySavePath);
			if (legacyData != null)
			{
				ApplySaveData(legacyData);
				Save();
				return;
			}

			GD.PushWarning("[Save] The legacy save file is invalid. Starting with fresh progress.");
		}

		ResetProgress();
	}

	/// <summary>Loads the requested slot, using fresh defaults when it is empty or invalid.</summary>
	private void LoadSlot(int slot)
	{
		if (!IsValidSlot(slot))
		{
			GD.PushWarning($"[Save] Ignoring invalid slot load: {slot}");
			return;
		}

		var path = GetSlotPath(slot);
		if (!FileAccess.FileExists(path))
		{
			ResetProgress();
			return;
		}

		var data = ReadSaveData(path);
		if (data == null)
		{
			GD.PushWarning($"[Save] Slot {slot} contains invalid JSON. Starting with fresh progress.");
			ResetProgress();
			return;
		}

		ApplySaveData(data);
	}

	private static Godot.Collections.Dictionary ReadSaveData(string path)
	{
		using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.PushWarning($"[Save] Could not read {path}: {FileAccess.GetOpenError()}");
			return null;
		}

		var parsed = Json.ParseString(file.GetAsText());
		if (parsed.VariantType != Variant.Type.Dictionary)
			return null;

		return parsed.AsGodotDictionary();
	}

	private void ApplySaveData(Godot.Collections.Dictionary data)
	{
		BestScore = Mathf.Max(0, GetInt(data, "best_score", 0));
		Coins = Mathf.Max(0, GetInt(data, "coins", 0));
		CheckpointX = GetInt(data, "checkpoint_x", 64);
		CheckpointY = GetInt(data, "checkpoint_y", 220);
	}

	private void ResetProgress()
	{
		BestScore = 0;
		Coins = 0;
		CheckpointX = 64;
		CheckpointY = 220;
	}

	private static string GetSlotPath(int slot) => $"user://save_slot_{slot}.json";

	private static bool IsValidSlot(int slot) => slot >= 1 && slot <= SlotCount;

	private static int GetInt(Godot.Collections.Dictionary data, string key, int fallback)
	{
		if (!data.ContainsKey(key))
			return fallback;

		var value = data[key];
		return value.VariantType switch
		{
			Variant.Type.Int => (int)value,
			Variant.Type.Float => Mathf.RoundToInt((float)value),
			_ => fallback
		};
	}
}
