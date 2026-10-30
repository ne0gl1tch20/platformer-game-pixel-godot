using Godot;
using System;

public partial class VirtualJoystick : Control
{
    [Export] public float DeadZone { get; set; } = 0.16f;
    public event Action<Vector2> DirectionChanged;

    private Vector2 _stick = Vector2.Zero;
    private int _touchIndex = -1;
    private bool _mouseHeld;

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(132, 132);
        MouseFilter = MouseFilterEnum.Stop;
        QueueRedraw();
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventScreenTouch touch)
        {
            if (touch.Pressed && _touchIndex == -1)
            {
                _touchIndex = touch.Index;
                UpdateStick(touch.Position);
            }
            else if (!touch.Pressed && touch.Index == _touchIndex)
            {
                _touchIndex = -1;
                ResetStick();
            }
            AcceptEvent();
        }
        else if (@event is InputEventScreenDrag drag && drag.Index == _touchIndex)
        {
            UpdateStick(drag.Position);
            AcceptEvent();
        }
        else if (@event is InputEventMouseButton mouse && mouse.ButtonIndex == MouseButton.Left)
        {
            _mouseHeld = mouse.Pressed;
            if (_mouseHeld) UpdateStick(mouse.Position);
            else ResetStick();
            AcceptEvent();
        }
        else if (@event is InputEventMouseMotion motion && _mouseHeld)
        {
            UpdateStick(motion.Position);
            AcceptEvent();
        }
    }

    private void UpdateStick(Vector2 pointer)
    {
        var radius = Mathf.Min(Size.X, Size.Y) * 0.34f;
        var offset = pointer - Size * 0.5f;
        _stick = offset.Length() > radius ? offset.Normalized() : offset / Mathf.Max(radius, 1f);
        if (_stick.Length() < DeadZone) _stick = Vector2.Zero;
        DirectionChanged?.Invoke(_stick);
        QueueRedraw();
    }

    private void ResetStick()
    {
        _stick = Vector2.Zero;
        DirectionChanged?.Invoke(Vector2.Zero);
        QueueRedraw();
    }

    public override void _Draw()
    {
        var center = Size * 0.5f;
        var radius = Mathf.Min(Size.X, Size.Y) * 0.43f;
        DrawCircle(center, radius, new Color(0.07f, 0.12f, 0.16f, 0.42f));
        DrawArc(center, radius, 0, Mathf.Tau, 48, new Color(0.86f, 0.94f, 0.91f, 0.68f), 2f);
        DrawCircle(center + _stick * radius * 0.68f, radius * 0.43f, new Color(0.91f, 0.83f, 0.59f, 0.88f));
        DrawArc(center + _stick * radius * 0.68f, radius * 0.43f, 0, Mathf.Tau, 32, new Color(1f, 1f, 1f, 0.8f), 2f);
    }
}
