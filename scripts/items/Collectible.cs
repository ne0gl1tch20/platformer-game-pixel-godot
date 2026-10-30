using Godot;

public partial class Collectible : Area2D
{
    [Export] public int Value { get; set; } = 1;

    public override void _Ready()
    {
        var collision = new CollisionShape2D();
        collision.Shape = new RectangleShape2D { Size = new Vector2(10, 14) };
        AddChild(collision);
        BodyEntered += OnBodyEntered;
        QueueRedraw();
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is not PlayerController) return;
        SaveSystem.Instance?.AddCoins(Value);
        AndroidBridge.Instance?.Vibrate(18);
        QueueFree();
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(-5, -7, 10, 14), new Color("f8e6a0"));
        DrawRect(new Rect2(-3, -9, 6, 18), new Color("f4a340"));
        DrawRect(new Rect2(-1, -7, 2, 14), new Color("fff4c2"));
    }
}
