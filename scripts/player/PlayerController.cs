using Godot;

public partial class PlayerController : CharacterBody2D
{
    [Export] public float MoveSpeed { get; set; } = 165f;
    [Export] public float Acceleration { get; set; } = 1100f;
    [Export] public float AirAcceleration { get; set; } = 800f;
    [Export] public float JumpVelocity { get; set; } = -365f;
    [Export] public float Gravity { get; set; } = 950f;
    [Export] public float CoyoteTime { get; set; } = 0.10f;
    [Export] public float JumpBuffer { get; set; } = 0.12f;
    [Export] public int MaxHealth { get; set; } = 5;

    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private float _touchDirection;
    private bool _touchJump;
    private float _facing = 1f;
    private float _attackTimer;
    private float _invulnerabilityTimer;
    private int _health;
    private Vector2 _spawnPoint;
    private bool _dead;

    public int Health => _health;
    public int MaxHealthValue => MaxHealth;

    public override void _Ready()
    {
        AddToGroup("player");
        var collision = new CollisionShape2D { Position = new Vector2(0, -12) };
        collision.Shape = new RectangleShape2D { Size = new Vector2(12, 24) };
        AddChild(collision);
        _health = MaxHealth;
        _spawnPoint = SaveSystem.Instance?.GetCheckpoint() ?? GlobalPosition;
        GD.Print("[Player] Adventurer online at ", GlobalPosition);
        QueueRedraw();
    }

    public override void _PhysicsProcess(double delta)
    {
        var dt = (float)delta;
        if (_dead)
        {
            HandleDeath(dt);
            return;
        }

        var direction = Mathf.Clamp(Input.GetAxis("move_left", "move_right") + _touchDirection, -1f, 1f);
        if (Mathf.Abs(direction) > 0.01f)
        {
            _facing = Mathf.Sign(direction);
            var accel = IsOnFloor() ? Acceleration : AirAcceleration;
            Velocity = new Vector2(Mathf.MoveToward(Velocity.X, direction * MoveSpeed, accel * dt), Velocity.Y);
        }
        else
        {
            Velocity = new Vector2(Mathf.MoveToward(Velocity.X, 0f, Acceleration * dt), Velocity.Y);
        }

        _coyoteTimer = IsOnFloor() ? CoyoteTime : _coyoteTimer - dt;
        _jumpBufferTimer = (Input.IsActionJustPressed("jump") || _touchJump) ? JumpBuffer : _jumpBufferTimer - dt;
        _touchJump = false;

        if (!IsOnFloor()) Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity * dt);
        if (_jumpBufferTimer > 0f && _coyoteTimer > 0f)
        {
            Velocity = new Vector2(Velocity.X, JumpVelocity);
            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;
        }

        if (Input.IsActionJustPressed("attack")) Attack();
        _attackTimer = Mathf.Max(0f, _attackTimer - dt);
        _invulnerabilityTimer = Mathf.Max(0f, _invulnerabilityTimer - dt);
        MoveAndSlide();
        if (GlobalPosition.Y > 520f) Die();
        QueueRedraw();
    }

    public void SetTouchDirection(float direction) => _touchDirection = Mathf.Clamp(direction, -1f, 1f);
    public void PressTouchJump() => _touchJump = true;
    public void PerformTouchAttack() => Attack();

    public void TakeDamage(int amount)
    {
        if (_dead || _invulnerabilityTimer > 0f) return;
        _health = Mathf.Max(0, _health - amount);
        _invulnerabilityTimer = 0.75f;
        Velocity = new Vector2(-_facing * 90f, -180f);
        AndroidBridge.Instance?.Vibrate(30);
        if (_health == 0) Die();
    }

    private void Attack()
    {
        if (_attackTimer > 0f) return;
        _attackTimer = 0.28f;
        foreach (var node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is Enemy enemy && GlobalPosition.DistanceTo(enemy.GlobalPosition) < 44f && Mathf.Sign(enemy.GlobalPosition.X - GlobalPosition.X) == _facing)
                enemy.TakeDamage(1);
        }
    }

    private void Die()
    {
        if (_dead) return;
        _dead = true;
        Velocity = Vector2.Zero;
        AndroidBridge.Instance?.Vibrate(70);
    }

    private void HandleDeath(float delta)
    {
        _invulnerabilityTimer += delta;
        if (_invulnerabilityTimer < 0.65f) return;
        _dead = false;
        _health = MaxHealth;
        GlobalPosition = SaveSystem.Instance?.GetCheckpoint() ?? _spawnPoint;
        Velocity = Vector2.Zero;
        _invulnerabilityTimer = 0.5f;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_invulnerabilityTimer > 0f && Mathf.FloorToInt(_invulnerabilityTimer * 14f) % 2 == 0) return;
        DrawSetTransform(Vector2.Zero, 0f, new Vector2(_facing, 1f));
        DrawRect(new Rect2(-7, -27, 14, 8), new Color("34233d"));
        DrawRect(new Rect2(-8, -23, 4, 6), new Color("34233d"));
        DrawRect(new Rect2(-5, -20, 10, 9), new Color("f2bd86"));
        DrawRect(new Rect2(2, -17, 2, 2), new Color("29243a"));
        DrawRect(new Rect2(-6, -12, 12, 3), new Color("f4a340"));
        DrawRect(new Rect2(-7, -10, 14, 8), new Color("3d78bd"));
        DrawRect(new Rect2(-9, -9, 3, 7), new Color("3d78bd"));
        DrawRect(new Rect2(6, -9, 3, 7), new Color("3d78bd"));
        DrawRect(new Rect2(-7, -3, 14, 3), new Color("5b3b2c"));
        DrawRect(new Rect2(-6, 0, 5, 3), new Color("34233d"));
        DrawRect(new Rect2(2, 0, 5, 3), new Color("34233d"));
        if (_attackTimer > 0f) DrawRect(new Rect2(12, -13, 13, 3), new Color("f8e6a0"));
    }
}
