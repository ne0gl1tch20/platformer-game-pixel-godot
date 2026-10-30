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
    private AnimatedSprite2D _sprite;
    private string _currentAnimation = "";

    public int Health => _health;
    public int MaxHealthValue => MaxHealth;

    public override void _Ready()
    {
        AddToGroup("player");
        var collision = new CollisionShape2D { Position = new Vector2(0, -12) };
        collision.Shape = new RectangleShape2D { Size = new Vector2(12, 24) };
        AddChild(collision);
        _sprite = CreateAnimatedSprite();
        AddChild(_sprite);
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
            DspSoundtrack.Instance?.PlayJump();
        }

        if (Input.IsActionJustPressed("attack")) Attack();
        _attackTimer = Mathf.Max(0f, _attackTimer - dt);
        _invulnerabilityTimer = Mathf.Max(0f, _invulnerabilityTimer - dt);
        MoveAndSlide();
        UpdateAnimation();
        if (GlobalPosition.Y > 520f) Die();
        QueueRedraw();
    }

    public void SetTouchDirection(float direction) => _touchDirection = Mathf.Clamp(direction, -1f, 1f);

    public void ResetForNewGame()
    {
        _health = MaxHealth;
        _dead = false;
        _coyoteTimer = 0f;
        _jumpBufferTimer = 0f;
        _touchDirection = 0f;
        _touchJump = false;
        _attackTimer = 0f;
        _invulnerabilityTimer = 0f;
        Velocity = Vector2.Zero;
        QueueRedraw();
    }
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
        DspSoundtrack.Instance?.PlayAttack();
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

    private AnimatedSprite2D CreateAnimatedSprite()
    {
        var texture = GD.Load<Texture2D>("res://assets/sprites/player/adventurer_strip.svg");
        var frames = new SpriteFrames();
        frames.AddAnimation("idle");
        frames.SetAnimationSpeed("idle", 1f);
        frames.AddAnimation("run");
        frames.SetAnimationSpeed("run", 9f);
        frames.AddAnimation("jump");
        frames.SetAnimationSpeed("jump", 1f);

        AddAtlasFrame(frames, "idle", texture, 0);
        AddAtlasFrame(frames, "run", texture, 1);
        AddAtlasFrame(frames, "run", texture, 2);
        AddAtlasFrame(frames, "jump", texture, 3);

        var animated = new AnimatedSprite2D
        {
            Name = "AnimatedSprite",
            SpriteFrames = frames,
            Position = new Vector2(0, -16),
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest
        };
        animated.Play("idle");
        return animated;
    }

    private static void AddAtlasFrame(SpriteFrames frames, string animation, Texture2D texture, int index)
    {
        var atlas = new AtlasTexture
        {
            Atlas = texture,
            Region = new Rect2(index * 32, 0, 32, 32),
            FilterClip = true
        };
        frames.AddFrame(animation, atlas);
    }

    private void UpdateAnimation()
    {
        if (_sprite == null) return;
        var next = !IsOnFloor() ? "jump" : Mathf.Abs(Velocity.X) > 12f ? "run" : "idle";
        if (next != _currentAnimation)
        {
            _currentAnimation = next;
            _sprite.Play(next);
        }
        _sprite.FlipH = _facing < 0f;
        _sprite.Modulate = _invulnerabilityTimer > 0f
            ? new Color(1f, 0.72f, 0.72f)
            : Colors.White;
        _sprite.Scale = _attackTimer > 0f ? new Vector2(1.12f, 0.92f) : Vector2.One;
    }

    public override void _Draw()
    {
        if (_attackTimer > 0f)
        {
            DrawRect(new Rect2(_facing > 0 ? 12 : -25, -13, 13, 3), new Color("f8e6a0"));
            DrawRect(new Rect2(_facing > 0 ? 16 : -25, -16, 7, 2), new Color("fff4c2"));
        }
    }
}
