using Godot;

public partial class Game : Node2D
{
    private PlayerController _player;
    private Label _status;
    private Label _score;
    private Control _touchUi;

    private readonly PackedScene _playerScene = GD.Load<PackedScene>("res://scenes/player/Player.tscn");
    private readonly PackedScene _platformScene = GD.Load<PackedScene>("res://scenes/world/Platform.tscn");
    private readonly PackedScene _enemyScene = GD.Load<PackedScene>("res://scenes/enemies/Walker.tscn");
    private readonly PackedScene _collectibleScene = GD.Load<PackedScene>("res://scenes/items/Crystal.tscn");

    public override void _Ready()
    {
        ConfigureInput();
        GD.Print("[Game] Booting production game...");
        BuildWorld();
        SpawnPlayer();
        BuildHud();
        BuildTouchControls();
        (GetNodeOrNull<AndroidBridge>("/root/AndroidBridge"))?.LogBridgeStatus();
        GD.Print("[Game] Production systems online.");
    }

    public override void _Process(double delta)
    {
        if (_player == null) return;
        _status.Text = $"HP {new string('♥', _player.Health)}{new string('♡', _player.MaxHealthValue - _player.Health)}   |   MOVE   JUMP   ATTACK";
        var coins = SaveSystem.Instance?.Coins ?? 0;
        _score.Text = $"CRYSTALS {coins}   BEST {SaveSystem.Instance?.BestScore ?? 0}";
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
        var backdrop = new Backdrop { ZIndex = -10 };
        AddChild(backdrop);
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

    private void BuildTouchControls()
    {
        if (!OS.HasFeature("mobile")) return;
        _touchUi = new Control { Name = "TouchControls" };
        _touchUi.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        GetTree().Root.AddChild(_touchUi);
        var left = MakeButton("◀", new Vector2(24, 190));
        var right = MakeButton("▶", new Vector2(92, 190));
        var jump = MakeButton("JUMP", new Vector2(360, 176), new Vector2(96, 50));
        var attack = MakeButton("ATK", new Vector2(292, 210), new Vector2(58, 42));
        left.ButtonDown += () => _player?.SetTouchDirection(-1f);
        left.ButtonUp += () => _player?.SetTouchDirection(0f);
        right.ButtonDown += () => _player?.SetTouchDirection(1f);
        right.ButtonUp += () => _player?.SetTouchDirection(0f);
        jump.Pressed += () => _player?.PressTouchJump();
        attack.Pressed += () => _player?.PerformTouchAttack();
    }

    private Button MakeButton(string text, Vector2 position, Vector2 size = default)
    {
        if (size == default) size = new Vector2(58, 50);
        var button = new Button { Text = text, Position = position, Size = size };
        button.AddThemeFontSizeOverride("font_size", 14);
        _touchUi.AddChild(button);
        return button;
    }
}
