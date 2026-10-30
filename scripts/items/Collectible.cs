using Godot;

public partial class Collectible : Area2D
{
    private Sprite2D _sprite;
    [Export] public int Value { get; set; } = 1;

    public override void _Ready()
    {
        _sprite = new Sprite2D
        {
            Texture = GD.Load<Texture2D>("res://assets/sprites/items/crystal.svg"),
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest
        };
        AddChild(_sprite);
        var collision = new CollisionShape2D();
        collision.Shape = new RectangleShape2D { Size = new Vector2(10, 14) };
        AddChild(collision);
        BodyEntered += OnBodyEntered;
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        if (_sprite == null) return;
        var time = (float)Time.GetTicksMsec() * 0.003f;
        _sprite.Position = new Vector2(0, Mathf.Sin(time * 1.8f) * 2.5f);
        _sprite.Rotation = Mathf.Sin(time) * 0.12f;
        _sprite.Scale = Vector2.One * (1f + Mathf.Sin(time * 2f) * 0.045f);
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is not PlayerController) return;
        SaveSystem.Instance?.AddCoins(Value);
        AndroidBridge.Instance?.Vibrate(18);
        QueueFree();
    }

}
