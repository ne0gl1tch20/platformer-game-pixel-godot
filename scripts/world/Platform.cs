using Godot;

public partial class Platform : StaticBody2D
{
    [Export] public Vector2 Size { get; set; } = new(80, 12);
    [Export] public Color PlatformColor { get; set; } = new("9b7048");

    public override void _Ready()
    {
        var collision = new CollisionShape2D();
        collision.Shape = new RectangleShape2D { Size = Size };
        AddChild(collision);

        var grassFrames = new SpriteFrames();
        grassFrames.AddAnimation("sway");
        grassFrames.SetAnimationSpeed("sway", 5f);
        var strip = GD.Load<Texture2D>("res://assets/sprites/environment/grass_strip.svg");
        for (var i = 0; i < 4; i++)
        {
            grassFrames.AddFrame("sway", new AtlasTexture
            {
                Atlas = strip,
                Region = new Rect2(i * 16, 0, 16, 16),
                FilterClip = true
            });
        }

        var count = Mathf.Max(1, Mathf.CeilToInt(Size.X / 28f));
        for (var i = 0; i < count; i++)
        {
            var grass = new AnimatedSprite2D
            {
                SpriteFrames = grassFrames,
                Animation = "sway",
                Position = new Vector2(-Size.X / 2f + 14f + i * 28f, -Size.Y / 2f - 8f),
                TextureFilter = CanvasItem.TextureFilterEnum.Nearest
            };
            AddChild(grass);
            grass.Frame = (int)GD.RandRange(0, 3);
            grass.Play("sway");
        }
        QueueRedraw();
    }

    public override void _Draw()
    {
        var top = -Size.Y / 2f;
        DrawRect(new Rect2(-Size.X / 2f, top, Size.X, Size.Y), PlatformColor);
        DrawRect(new Rect2(-Size.X / 2f, top + 3f, Size.X, 2f), new Color("80523a"));
        for (var x = 0f; x < Size.X; x += 16f)
        {
            var px = -Size.X / 2f + x;
            DrawRect(new Rect2(px + 3f, top + 8f, 4f, 2f), new Color("704632"));
            DrawRect(new Rect2(px + 10f, top + 13f, 3f, 2f), new Color("ad7650"));
            DrawRect(new Rect2(px + 1f, top, 7f, 2f), new Color("b7e47d"));
        }
    }
}