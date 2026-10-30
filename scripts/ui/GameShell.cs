using Godot;
using System;

public partial class GameShell : CanvasLayer
{
    public event Action PlayRequested;
    public event Action NewGameRequested;
    public event Action ResumeRequested;
    public event Action ReturnToMenuRequested;
    public event Action<Vector2> JoystickMoved;
    public event Action JumpRequested;
    public event Action AttackRequested;
    public event Action PauseRequested;

    private const string SettingsPath = "user://settings.cfg";
    private Control _root;
    private VBoxContainer _content;
    private Label _message;
    private string _returnPage = "main";
    private float _masterVolume = 0.85f;
    private float _musicVolume = 0.65f;
    private float _sfxVolume = 0.85f;
    private bool _showTouch = true;
    private bool _fullscreen;
    private bool _reducedMotion;
    private string _currentPage = "main";
    public bool IsMenuOpen { get; private set; } = true;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Layer = 20;
        LoadSettings();
        BuildRoot();
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        _returnPage = "main";
        _currentPage = "main";
        BuildPage("PLATFORMER GAME PIXEL", "A little adventure through the Loveglow forest.");
        AddActionButton("CONTINUE", () => { PlayRequested?.Invoke(); HideShell(); });
        AddActionButton("NEW GAME", () => ConfirmNewGame());
        AddActionButton("SAVE SLOTS", ShowSaveSlots);
        AddActionButton("SETTINGS", () => ShowSettings("main"));
        AddActionButton("ABOUT & VERSION", ShowAbout);
        AddActionButton("CREDITS", ShowCredits);
        AddFooter();
    }

    public void ShowPauseMenu()
    {
        _returnPage = "pause";
        _currentPage = "pause";
        BuildPage("PAUSED", "Take a breather. The forest can wait.");
        AddActionButton("RESUME", () => { ResumeRequested?.Invoke(); HideShell(); });
        AddActionButton("SETTINGS", () => ShowSettings("pause"));
        AddActionButton("SAVE NOW", () => { SaveSystem.Instance?.Save(); SetMessage("Game saved."); });
        AddActionButton("MAIN MENU", () => { ReturnToMenuRequested?.Invoke(); ShowMainMenu(); });
        AddFooter();
    }

    public void ShowSettings(string returnPage)
    {
        _returnPage = returnPage;
        _currentPage = "settings";
        BuildPage("EXTENDED SETTINGS", "Your preferences are saved on this device.");
        AddSlider("MASTER VOLUME", _masterVolume, value => { _masterVolume = value; ApplyAudio(); SaveSettings(); });
        AddSlider("MUSIC VOLUME", _musicVolume, value => { _musicVolume = value; ApplyAudio(); SaveSettings(); });
        AddSlider("SFX VOLUME", _sfxVolume, value => { _sfxVolume = value; ApplyAudio(); SaveSettings(); });
        AddToggle("SHOW TOUCH CONTROLS", _showTouch, value => { _showTouch = value; SetTouchVisible(value); SaveSettings(); });
        AddToggle("FULLSCREEN", _fullscreen, value => { _fullscreen = value; ApplyDisplay(); SaveSettings(); });
        AddToggle("REDUCED MOTION", _reducedMotion, value => { _reducedMotion = value; SaveSettings(); });
        AddActionButton("BACK", () => { if (_returnPage == "pause") ShowPauseMenu(); else ShowMainMenu(); });
        AddFooter();
    }

    public void ShowAbout()
    {
        _returnPage = "main";
        _currentPage = "about";
        BuildPage("ABOUT THE GAME", "A cozy pixel-art platform adventure.");
        AddText(GameInfo.VersionLabel);
        AddText("Engine: Godot 4.7 .NET");
        AddText("Runtime audio: Loveglow, synthesized with C# DSP");
        AddText("Controls: keyboard, gamepad and Android touch");
        AddText("Changelog");
        AddText(GameInfo.Changelog);
        AddActionButton("BACK", ShowMainMenu);
        AddFooter();
    }

    public void ShowCredits()
    {
        _currentPage = "credits";
        BuildPage("CREDITS", "Made with care, pixels and procedural sound.");
        AddText("Game & code: Platformer Game Pixel");
        AddText("Music & sound: Loveglow DSP");
        AddText("Engine: Godot Engine");
        AddText("Third-party asset credits are listed in the repository README.");
        AddActionButton("BACK", ShowMainMenu);
        AddFooter();
    }

    public void SetTouchVisible(bool visible)
    {
        if (_root == null) return;
        var controls = _root.GetNodeOrNull<Control>("TouchControls");
        if (controls != null) controls.Visible = visible && _showTouch;
    }


    public override void _UnhandledInput(InputEvent @event)
    {
        if (!@event.IsActionPressed("ui_cancel")) return;
        if (!IsMenuOpen)
        {
            PauseRequested?.Invoke();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (_currentPage == "settings")
        {
            if (_returnPage == "pause") ShowPauseMenu();
            else ShowMainMenu();
            GetViewport().SetInputAsHandled();
        }
        else if (_currentPage == "pause")
        {
            ResumeRequested?.Invoke();
            HideShell();
            GetViewport().SetInputAsHandled();
        }
        else if (_currentPage != "main")
        {
            ShowMainMenu();
            GetViewport().SetInputAsHandled();
        }
    }

    public void HideShell()
    {
        foreach (var child in _root.GetChildren())
        {
            if (child.Name.ToString() != "TouchControls") child.QueueFree();
        }
        IsMenuOpen = false;
    }

    public void ShowShell()
    {
        ShowMainMenu();
    }


    private void ShowSaveSlots()
    {
        _currentPage = "saves";
        BuildPage("SAVE SLOTS", "Choose which adventure to continue.");
        for (var slot = 1; slot <= 3; slot++)
        {
            var selectedSlot = slot;
            var summary = SaveSystem.Instance?.GetSlotSummary(slot) ?? "Unavailable";
            AddActionButton($"SLOT {slot}  •  {summary}", () =>
            {
                SaveSystem.Instance?.SelectSlot(selectedSlot);
                ShowMainMenu();
            });
        }
        AddActionButton("DELETE CURRENT SLOT", () =>
        {
            _currentPage = "delete";
            BuildPage("DELETE SAVE?", "This permanently clears the selected slot.");
            AddActionButton("DELETE SLOT", () =>
            {
                SaveSystem.Instance?.DeleteSlot(SaveSystem.Instance.CurrentSlot);
                ShowSaveSlots();
            });
            AddActionButton("CANCEL", ShowSaveSlots);
        });
        AddActionButton("BACK", ShowMainMenu);
        AddFooter();
    }

    private void ConfirmNewGame()
    {
        _currentPage = "confirm";
        BuildPage("START A NEW GAME?", "Your selected save slot will be reset.");
        AddActionButton("START NEW GAME", () => { NewGameRequested?.Invoke(); HideShell(); });
        AddActionButton("CANCEL", ShowMainMenu);
    }

    private void BuildRoot()
    {
        _root = new Control { Name = "ShellRoot" };
        _root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        _root.MouseFilter = Control.MouseFilterEnum.Stop;
        AddChild(_root);
        BuildTouchControls();
    }

    private void BuildTouchControls()
    {
        var controls = new Control { Name = "TouchControls", MouseFilter = Control.MouseFilterEnum.Pass };
        controls.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        _root.AddChild(controls);

        var joystick = new VirtualJoystick { Name = "MoveJoystick", AnchorLeft = 0, AnchorTop = 1, AnchorRight = 0, AnchorBottom = 1, OffsetLeft = 22, OffsetTop = -158, OffsetRight = 154, OffsetBottom = -26 };
        joystick.DirectionChanged += direction => JoystickMoved?.Invoke(direction);
        controls.AddChild(joystick);

        var jump = MakeTouchButton(controls, "JUMP", new Vector2(1, 1), new Vector2(-118, -106), new Vector2(-24, -34), new Vector2(94, 72));
        jump.Pressed += () => JumpRequested?.Invoke();
        var attack = MakeTouchButton(controls, "ATK", new Vector2(1, 1), new Vector2(-202, -74), new Vector2(-128, -24), new Vector2(74, 50));
        attack.Pressed += () => AttackRequested?.Invoke();
        var pause = MakeTouchButton(controls, "Ⅱ", new Vector2(1, 0), new Vector2(-58, 12), new Vector2(-14, 50), new Vector2(44, 38));
        pause.Pressed += () => PauseRequested?.Invoke();
    }

    private static Button MakeTouchButton(Control parent, string label, Vector2 anchors, Vector2 topLeft, Vector2 bottomRight, Vector2 size)
    {
        var button = new Button
        {
            Text = label,
            AnchorLeft = anchors.X,
            AnchorTop = anchors.Y,
            AnchorRight = anchors.X,
            AnchorBottom = anchors.Y,
            OffsetLeft = topLeft.X,
            OffsetTop = topLeft.Y,
            OffsetRight = bottomRight.X,
            OffsetBottom = bottomRight.Y,
            CustomMinimumSize = size,
            FocusMode = Control.FocusModeEnum.None
        };
        button.AddThemeFontSizeOverride("font_size", 16);
        button.Modulate = new Color(1f, 1f, 1f, 0.88f);
        parent.AddChild(button);
        return button;
    }

    private void BuildPage(string title, string subtitle)
    {
        foreach (var child in _root.GetChildren())
        {
            if (child.Name.ToString() != "TouchControls") child.QueueFree();
        }

        var shade = new ColorRect { Name = "MenuShade", Color = new Color(0.035f, 0.075f, 0.10f, 0.90f), MouseFilter = Control.MouseFilterEnum.Stop };
        shade.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        _root.AddChild(shade);

        var panel = new PanelContainer
        {
            Name = "MenuPanel",
            AnchorLeft = 0.5f,
            AnchorTop = 0.5f,
            AnchorRight = 0.5f,
            AnchorBottom = 0.5f,
            OffsetLeft = -190,
            OffsetTop = -132,
            OffsetRight = 190,
            OffsetBottom = 132
        };
        panel.AddThemeStyleboxOverride("panel", MakePanelStyle());
        _root.AddChild(panel);

        IsMenuOpen = true;
        var scroll = new ScrollContainer { HorizontalScrollMode = ScrollContainer.ScrollModeEnum.Disabled };
        scroll.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        panel.AddChild(scroll);
        _content = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, Alignment = BoxContainer.AlignmentMode.Center };
        _content.AddThemeConstantOverride("separation", 8);
        scroll.AddChild(_content);

        var heading = new Label { Text = title, HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart };
        heading.AddThemeFontSizeOverride("font_size", 22);
        heading.AddThemeColorOverride("font_color", new Color("f4e6b9"));
        _content.AddChild(heading);
        var caption = new Label { Text = subtitle, HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart };
        caption.AddThemeFontSizeOverride("font_size", 12);
        caption.AddThemeColorOverride("font_color", new Color("c4d7cf"));
        _content.AddChild(caption);
        _message = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        _message.AddThemeFontSizeOverride("font_size", 11);
        _content.AddChild(_message);
    }

    private void AddActionButton(string label, Action action)
    {
        var button = new Button { Text = label, CustomMinimumSize = new Vector2(230, 36), SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter, FocusMode = Control.FocusModeEnum.None };
        button.AddThemeFontSizeOverride("font_size", 14);
        button.Pressed += action;
        _content.AddChild(button);
    }

    private void AddText(string text)
    {
        var label = new Label { Text = text, AutowrapMode = TextServer.AutowrapMode.WordSmart, HorizontalAlignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2(260, 0) };
        label.AddThemeFontSizeOverride("font_size", 12);
        label.AddThemeColorOverride("font_color", new Color("e0e8d9"));
        _content.AddChild(label);
    }

    private void AddSlider(string label, float value, Action<float> changed)
    {
        AddText($"{label}: {Mathf.RoundToInt(value * 100)}%");
        var slider = new HSlider { MinValue = 0, MaxValue = 1, Step = 0.01, Value = value, CustomMinimumSize = new Vector2(250, 24), SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter };
        slider.ValueChanged += number => changed((float)number);
        _content.AddChild(slider);
    }

    private void AddToggle(string label, bool value, Action<bool> changed)
    {
        var toggle = new CheckButton { Text = label, ButtonPressed = value, SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter };
        toggle.Toggled += changed;
        _content.AddChild(toggle);
    }

    private void AddFooter()
    {
        AddText(GameInfo.VersionLabel);
    }

    private void SetMessage(string text)
    {
        if (_message != null) _message.Text = text;
    }

    private static StyleBoxFlat MakePanelStyle()
    {
        var style = new StyleBoxFlat
        {
            BgColor = new Color("183b43"),
            BorderColor = new Color("d4bd7a"),
            CornerRadiusTopLeft = 12,
            CornerRadiusTopRight = 12,
            CornerRadiusBottomLeft = 12,
            CornerRadiusBottomRight = 12,
            BorderWidthBottom = 2,
            BorderWidthTop = 2,
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
            ContentMarginLeft = 18,
            ContentMarginRight = 18,
            ContentMarginTop = 14,
            ContentMarginBottom = 14
        };
        return style;
    }

    private void ApplyAudio()
    {
        var masterBus = AudioServer.GetBusIndex("Master");
        if (masterBus >= 0) AudioServer.SetBusVolumeDb(masterBus, Mathf.LinearToDb(Mathf.Max(0.001f, _masterVolume)));
        DspSoundtrack.Instance?.SetMusicVolume(_musicVolume);
        DspSoundtrack.Instance?.SetSfxVolume(_sfxVolume);
    }

    private void ApplyDisplay()
    {
        DisplayServer.WindowSetMode(_fullscreen ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
    }

    private void LoadSettings()
    {
        var config = new ConfigFile();
        if (config.Load(SettingsPath) != Error.Ok) return;
        _masterVolume = (float)config.GetValue("audio", "master", 0.85f);
        _musicVolume = (float)config.GetValue("audio", "music", 0.65f);
        _sfxVolume = (float)config.GetValue("audio", "sfx", 0.85f);
        _showTouch = (bool)config.GetValue("mobile", "show_touch", true);
        _fullscreen = (bool)config.GetValue("display", "fullscreen", false);
        _reducedMotion = (bool)config.GetValue("accessibility", "reduced_motion", false);
        ApplyAudio();
        ApplyDisplay();
    }

    private void SaveSettings()
    {
        var config = new ConfigFile();
        config.SetValue("audio", "master", _masterVolume);
        config.SetValue("audio", "music", _musicVolume);
        config.SetValue("audio", "sfx", _sfxVolume);
        config.SetValue("mobile", "show_touch", _showTouch);
        config.SetValue("display", "fullscreen", _fullscreen);
        config.SetValue("accessibility", "reduced_motion", _reducedMotion);
        config.Save(SettingsPath);
    }
}
