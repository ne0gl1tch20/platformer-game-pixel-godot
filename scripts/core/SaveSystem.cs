
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

	public Vector2 GetCheckpoint()
	{
		return new Vector2(CheckpointX, CheckpointY);
	}

	public string GetSlotSummary(int slot)
	{
		if (!IsValidSlot(slot))
			return "INVALID SLOT";

		string path = GetSlotPath(slot);

		if (!FileAccess.FileExists(path))
			return "EMPTY";

		var data = ReadSaveData(path);

		if (data == null)
			return "CORRUPTED SAVE";

		int coins = GetInt(data, "coins", 0);
		int best = GetInt(data, "best_score", 0);

		return $"COINS {coins}  •  BEST {best}";
	}

	public void SelectSlot(int slot)
	{
		if (!IsValidSlot(slot))
		{
			GD.PushWarning($"[Save] Invalid slot selection: {slot}");
			return;
		}

		if (_currentSlot == slot)
			return;

		Save();
		_currentSlot = slot;
		LoadSlot(slot);
	}

	public void DeleteSlot(int slot)
	{
		if (!IsValidSlot(slot))
		{
			GD.PushWarning($"[Save] Invalid slot deletion: {slot}");
			return;
		}

		string path = GetSlotPath(slot);

		if (FileAccess.FileExists(path))
		{
			Error error = DirAccess.RemoveAbsolute(
				ProjectSettings.GlobalizePath(path)
			);

			if (error != Error.Ok)
			{
				GD.PushWarning($"[Save] Could not delete slot {slot}: {error}");
				return;
			}
		}

		if (_currentSlot == slot)
			ResetProgress();
	}

	public void NewGame()
	{
		ResetProgress();
		Save();
	}

	public void Save()
	{
		if (!IsValidSlot(_currentSlot))
		{
			GD.PushWarning($"[Save] Invalid active slot: {_currentSlot}");
			return;
		}

		string path = GetSlotPath(_currentSlot);

		using var file = FileAccess.Open(
			path,
			FileAccess.ModeFlags.Write
		);

		if (file == null)
		{
			GD.PushWarning(
				$"[Save] Could not open slot {_currentSlot}: {FileAccess.GetOpenError()}"
			);
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

		// Migrate the old single-file save into slot 1.
		if (FileAccess.FileExists(LegacySavePath))
		{
			var legacyData = ReadSaveData(LegacySavePath);

			if (legacyData != null)
			{
				ApplySaveData(legacyData);
				Save();
				return;
			}

			GD.PushWarning("[Save] Legacy save is invalid.");
		}

		ResetProgress();
	}

	private void LoadSlot(int slot)
	{
		if (!IsValidSlot(slot))
		{
			GD.PushWarning($"[Save] Invalid slot load: {slot}");
			return;
		}

		string path = GetSlotPath(slot);

		if (!FileAccess.FileExists(path))
		{
			ResetProgress();
			return;
		}

		var data = ReadSaveData(path);

		if (data == null)
		{
			GD.PushWarning($"[Save] Slot {slot} contains invalid data.");
			ResetProgress();
			return;
		}

		ApplySaveData(data);
	}

	private static Godot.Collections.Dictionary ReadSaveData(string path)
	{
		using var file = FileAccess.Open(
			path,
			FileAccess.ModeFlags.Read
		);

		if (file == null)
		{
			GD.PushWarning(
				$"[Save] Could not read {path}: {FileAccess.GetOpenError()}"
			);
			return null;
		}

		Variant parsed = Json.ParseString(file.GetAsText());

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

	private static string GetSlotPath(int slot)
	{
		return $"user://save_slot_{slot}.json";
	}

	private static bool IsValidSlot(int slot)
	{
		return slot >= 1 && slot <= SlotCount;
	}

	private static int GetInt(
		Godot.Collections.Dictionary data,
		string key,
		int fallback
	)
	{
		if (!data.ContainsKey(key))
			return fallback;

		Variant value = data[key];

		return value.VariantType switch
		{
			Variant.Type.Int => (int)value,
			Variant.Type.Float => Mathf.RoundToInt((float)value),
			_ => fallback
		};
	}
}
