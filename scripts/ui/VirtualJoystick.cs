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
        var center = (Size * 0.5f).Round();
        var radius = Mathf.Floor(Mathf.Min(Size.X, Size.Y) * 0.40f);
        var outer = new Rect2(center - Vector2.One * radius, Vector2.One * radius * 2f);
        var border = outer.Grow(-4f);
        var inner = outer.Grow(-7f);

        // Layered rectangles keep the control crisp at every display scale.
        DrawRect(outer.Grow(3f), new Color("10252b", 0.62f));
        DrawRect(outer, new Color("d6b875", 0.88f));
        DrawRect(border, new Color("203d3b", 0.94f));
        DrawRect(inner, new Color("31584d", 0.82f));

        var knobCenter = (center + _stick * radius * 0.62f).Round();
        var knobHalf = Mathf.Floor(radius * 0.34f);
        var knob = new Rect2(knobCenter - Vector2.One * knobHalf, Vector2.One * knobHalf * 2f);
        DrawRect(knob.Grow(3f), new Color("152b2d", 0.9f));
        DrawRect(knob, new Color("f0d58e", 0.96f));
        DrawRect(knob.Grow(-4f), new Color("fff0bd", 0.92f));

        var tick = new Color("e4d5a4", 0.48f);
        DrawRect(new Rect2(center.X - 1f, outer.Position.Y + 9f, 2f, 5f), tick);
        DrawRect(new Rect2(center.X - 1f, outer.End.Y - 14f, 2f, 5f), tick);
        DrawRect(new Rect2(outer.Position.X + 9f, center.Y - 1f, 5f, 2f), tick);
        DrawRect(new Rect2(outer.End.X - 14f, center.Y - 1f, 5f, 2f), tick);
    }
}
