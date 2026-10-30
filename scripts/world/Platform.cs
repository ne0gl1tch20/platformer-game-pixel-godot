using Godot;

public partial class Platform : StaticBody2D
{
    [Export] public Vector2 Size { get; set; } = new(80, 12);
    [Export] public Color PlatformColor { get; set; } = new("9b7048");

    public override void _Ready()
    {
        var collision = new CollisionShape2D();
        var shape = new RectangleShape2D { Size = Size };
        collision.Shape = shape;
        AddChild(collision);
        QueueRedraw();
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(-Size / 2f, Size), PlatformColor);
        DrawRect(new Rect2(-Size.X / 2f, -Size.Y / 2f, Size.X, 3), new Color("b98555"));
    }
}
