using Godot;

public partial class Game : Node2D
{
    private PlayerController _player;
    private Label _status;
    private Label _score;
    private GameShell _shell;
    private bool _gameStarted;

    private readonly PackedScene _playerScene = GD.Load<PackedScene>("res://scenes/player/Player.tscn");
    private readonly PackedScene _platformScene = GD.Load<PackedScene>("res://scenes/world/Platform.tscn");
    private readonly PackedScene _enemyScene = GD.Load<PackedScene>("res://scenes/enemies/Walker.tscn");
    private readonly PackedScene _collectibleScene = GD.Load<PackedScene>("res://scenes/items/Crystal.tscn");

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        ConfigureInput();
        GD.Print("[Game] Booting production game...");
        BuildWorld();
        SpawnPlayer();
        BuildHud();
        StartDspSoundtrack();
        BuildGameShell();
        (GetNodeOrNull<AndroidBridge>("/root/AndroidBridge"))?.LogBridgeStatus();
        GetTree().Paused = true;
        GD.Print("[Game] Game shell ready. Waiting at main menu.");
    }

    public override void _Process(double delta)
    {
        if (_player == null || _status == null || _score == null) return;
        _status.Text = $"HP {new string('♥', _player.Health)}{new string('♡', _player.MaxHealthValue - _player.Health)}   |   MOVE   JUMP   ATTACK";
        var coins = SaveSystem.Instance?.Coins ?? 0;
        _score.Text = $"CRYSTALS {coins}   BEST {SaveSystem.Instance?.BestScore ?? 0}";
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!_gameStarted || _shell == null || _shell.Visible) return;
        if (@event.IsActionPressed("ui_cancel") && !@event.IsEcho())
        {
            GetTree().Paused = true;
            _shell.ShowPauseMenu();
            GetViewport().SetInputAsHandled();
        }
    }

    private static void ConfigureInput()
    {
        AddKeyAction("move_left", Key.A, Key.Left);
        AddKeyAction("move_right", Key.D, Key.Right);
        AddKeyAction("jump", Key.Space, Key.W, Key.Up);
        AddKeyAction("attack", Key.F, Key.J);
    }

    private static void AddKeyAction(string action, params Key[] keys)
    {
        if (!InputMap.HasAction(action)) InputMap.AddAction(action);
        foreach (var key in keys)
        {
            var ev = new InputEventKey { PhysicalKeycode = key };
            if (!InputMap.ActionHasEvent(action, ev)) InputMap.ActionAddEvent(action, ev);
        }
    }

    private void BuildWorld()
    {
        _createBackdrop();
        CreatePlatform(new Vector2(0, 258), new Vector2(1800, 24), new Color("6caa58"));
        CreatePlatform(new Vector2(170, 211), new Vector2(76, 12));
        CreatePlatform(new Vector2(300, 174), new Vector2(76, 12));
        CreatePlatform(new Vector2(430, 137), new Vector2(76, 12));
        CreatePlatform(new Vector2(570, 195), new Vector2(90, 12));
        CreatePlatform(new Vector2(735, 155), new Vector2(90, 12));
        CreatePlatform(new Vector2(910, 205), new Vector2(120, 12));
        CreatePlatform(new Vector2(1120, 170), new Vector2(110, 12));
        CreatePlatform(new Vector2(1320, 215), new Vector2(120, 12));

        SpawnEnemy(new Vector2(350, 150));
        SpawnEnemy(new Vector2(760, 125));
        SpawnEnemy(new Vector2(1140, 140));

        SpawnCrystal(new Vector2(175, 190));
        SpawnCrystal(new Vector2(305, 153));
        SpawnCrystal(new Vector2(435, 116));
        SpawnCrystal(new Vector2(775, 134));
        SpawnCrystal(new Vector2(1160, 149));
        SpawnCrystal(new Vector2(1360, 194));
    }

    private void _createBackdrop()
    {
        AddChild(new WorldParallax { Name = "WorldParallax" });
        AddTree(new Vector2(95, 171), 0.9f);
        AddTree(new Vector2(520, 92), 0.72f);
        AddTree(new Vector2(1010, 159), 1.05f);
        AddTree(new Vector2(1470, 168), 0.88f);
    }

    private void SpawnPlayer()
    {
        _player = _playerScene.Instantiate<PlayerController>();
        _player.Name = "Adventurer";
        _player.GlobalPosition = SaveSystem.Instance?.GetCheckpoint() ?? new Vector2(64, 220);
        AddChild(_player);
        var camera = new Camera2D { PositionSmoothingEnabled = true, PositionSmoothingSpeed = 8f };
        camera.LimitLeft = 0;
        camera.LimitRight = 1500;
        camera.LimitTop = 0;
        camera.LimitBottom = 270;
        _player.AddChild(camera);
    }

    private void CreatePlatform(Vector2 position, Vector2 size, Color? color = null)
    {
        var platform = _platformScene.Instantiate<Platform>();
        platform.Position = position;
        platform.Size = size;
        if (color.HasValue) platform.PlatformColor = color.Value;
        AddChild(platform);
    }

    private void SpawnEnemy(Vector2 position)
    {
        var enemy = _enemyScene.Instantiate<Enemy>();
        enemy.Position = position;
        AddChild(enemy);
    }

    private void SpawnCrystal(Vector2 position)
    {
        var crystal = _collectibleScene.Instantiate<Collectible>();
        crystal.Position = position;
        AddChild(crystal);
    }

    private void AddTree(Vector2 position, float scale)
    {
        var tree = new Sprite2D
        {
            Texture = GD.Load<Texture2D>("res://assets/sprites/environment/tree.svg"),
            Position = position,
            Centered = false,
            Scale = Vector2.One * scale,
            ZIndex = -1,
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest
        };
        AddChild(tree);
    }

    private void StartDspSoundtrack()
    {
        var music = new DspSoundtrack { Name = "LoveglowDSP" };
        AddChild(music);
    }

    private void BuildHud()
    {
        var layer = new CanvasLayer { Name = "HUD" };
        AddChild(layer);
        _status = new Label { Text = "LOADING...", Position = new Vector2(8, 7) };
        _status.AddThemeFontSizeOverride("font_size", 10);
        _status.AddThemeColorOverride("font_color", new Color("26394b"));
        layer.AddChild(_status);
        _score = new Label { Position = new Vector2(8, 22) };
        _score.AddThemeFontSizeOverride("font_size", 10);
        _score.AddThemeColorOverride("font_color", new Color("26394b"));
        layer.AddChild(_score);
    }

    private void BuildGameShell()
    {
        _shell = new GameShell { Name = "GameShell" };
        AddChild(_shell);
        _shell.PlayRequested += StartGame;
        _shell.ResumeRequested += ResumeGame;
        _shell.NewGameRequested += StartNewGame;
        _shell.ReturnToMenuRequested += ReturnToMenu;
        _shell.JoystickMoved += direction => _player?.SetTouchDirection(direction.X);
        _shell.JumpRequested += () => _player?.PressTouchJump();
        _shell.AttackRequested += () => _player?.PerformTouchAttack();
    }

    private void StartGame()
    {
        _gameStarted = true;
        GetTree().Paused = false;
    }

    private void ResumeGame()
    {
        _gameStarted = true;
        GetTree().Paused = false;
    }

    private void StartNewGame()
    {
        SaveSystem.Instance?.NewGame();
        if (_player != null)
        {
            _player.GlobalPosition = new Vector2(64, 220);
            _player.ResetForNewGame();
        }
        _gameStarted = true;
        GetTree().Paused = false;
    }

    private void ReturnToMenu()
    {
        _gameStarted = false;
        GetTree().Paused = true;
    }
}
