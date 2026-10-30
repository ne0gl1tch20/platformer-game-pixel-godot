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

    public override void _Ready()
    {
        AddToGroup("enemies");
        var collision = new CollisionShape2D { Position = new Vector2(0, -7) };
        collision.Shape = new RectangleShape2D { Size = new Vector2(18, 14) };
        AddChild(collision);
        _left = GlobalPosition.X - PatrolDistance;
        _right = GlobalPosition.X + PatrolDistance;
        QueueRedraw();
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

    public override void _Draw()
    {
        DrawRect(new Rect2(-10, -14, 20, 14), new Color("713b53"));
        DrawRect(new Rect2(-7, -19, 14, 7), new Color("8f5368"));
        DrawRect(new Rect2(-5, -16, 3, 3), new Color("f8e6a0"));
        DrawRect(new Rect2(2, -16, 3, 3), new Color("f8e6a0"));
        DrawRect(new Rect2(-9, 0, 7, 3), new Color("3b3048"));
        DrawRect(new Rect2(2, 0, 7, 3), new Color("3b3048"));
    }
}
