using Godot;

public partial class Enemy : CharacterBody2D
{
    [Export] public float PatrolDistance { get; set; } = 80f;
    [Export] public float Speed { get; set; } = 45f;
    [Export] public int Health { get; set; } = 2;
    private float _left;
    private float _right;
    private float _direction = 1f;
    private bool _dying;
    private Sprite2D _sprite;

    public override void _Ready()
    {
        AddToGroup("enemies");
        _sprite = new Sprite2D
        {
            Texture = GD.Load<Texture2D>("res://assets/sprites/enemies/walker.svg"),
            Position = new Vector2(0, -10),
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest
        };
        AddChild(_sprite);
        _sprite = new Sprite2D
        {
            Texture = GD.Load<Texture2D>("res://assets/sprites/enemies/walker.svg"),
            Position = new Vector2(0, -10),
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest
        };
        AddChild(_sprite);
        var collision = new CollisionShape2D { Position = new Vector2(0, -7) };
        collision.Shape = new RectangleShape2D { Size = new Vector2(18, 14) };
        AddChild(collision);
        _left = GlobalPosition.X - PatrolDistance;
        _right = GlobalPosition.X + PatrolDistance;
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        if (_sprite == null) return;
        _sprite.FlipH = _direction < 0f;
        _sprite.Position = new Vector2(0, -10 + Mathf.Sin((float)Time.GetTicksMsec() * 0.008f) * 1.5f);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_dying) return;
        Velocity = new Vector2(_direction * Speed, 0f);
        if (GlobalPosition.X <= _left) _direction = 1f;
        if (GlobalPosition.X >= _right) _direction = -1f;
        MoveAndSlide();
        var players = GetTree().GetNodesInGroup("player");
        if (players.Count > 0 && players[0] is PlayerController player && GlobalPosition.DistanceTo(player.GlobalPosition) < 18f)
            player.TakeDamage(1);
    }

    public void TakeDamage(int amount)
    {
        if (_dying) return;
        Health -= amount;
        if (Health <= 0)
        {
            _dying = true;
            QueueFree();
        }
        QueueRedraw();
    }

}
