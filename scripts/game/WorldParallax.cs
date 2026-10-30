using Godot;

public partial class WorldParallax : ParallaxBackground
{
    public void SetReducedMotion(bool reduced)
    {
        foreach (var child in GetChildren())
        {
            if (child is not ParallaxLayer layer) continue;
            var name = layer.Name.ToString();
            var speed = name == "CloudLayer" ? 0.12f : name == "MountainLayer" ? 0.28f : 0.52f;
            layer.MotionScale = reduced ? Vector2.Zero : new Vector2(speed, 1f);
        }
    }

    public override void _Ready()
    {
        var sky = new ColorRect
        {
            Name = "Sky",
            Color = new Color("83c9e8"),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        sky.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        AddChild(sky);

        AddLayer("CloudLayer", "res://assets/sprites/environment/clouds.svg", 0.12f, 18f, 512, 96, 3);
        AddLayer("MountainLayer", "res://assets/sprites/environment/mountains.svg", 0.28f, 102f, 960, 180, 3);
        AddLayer("ForestLayer", "res://assets/sprites/environment/forest.svg", 0.52f, 106f, 960, 180, 3);
    }

    private void AddLayer(string layerName, string texturePath, float speed, float y, int width, int height, int copies)
    {
        var layer = new ParallaxLayer
        {
            Name = layerName,
            MotionScale = new Vector2(speed, 1f),
            MotionMirroring = new Vector2(width, 0f)
        };
        AddChild(layer);

        var texture = GD.Load<Texture2D>(texturePath);
        for (var i = -1; i < copies; i++)
        {
            layer.AddChild(new Sprite2D
            {
                Texture = texture,
                Centered = false,
                Position = new Vector2(i * width, y),
                TextureFilter = CanvasItem.TextureFilterEnum.Nearest
            });
        }
    }
}