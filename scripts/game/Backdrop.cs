using Godot;

public partial class Backdrop : Node2D
{
    public override void _Draw()
    {
        DrawRect(new Rect2(-2000, -1000, 5000, 1300), new Color("83c9e8"));
        DrawRect(new Rect2(-2000, 190, 5000, 80), new Color("a8dfc2"));
        for (var i = 0; i < 24; i++)
        {
            var x = i * 125f - 100f;
            var y = 42f + (i % 3) * 13f;
            DrawRect(new Rect2(x, y, 34, 7), new Color("e9f6ef"));
            DrawRect(new Rect2(x + 8, y - 7, 20, 7), new Color("e9f6ef"));
        }
        for (var i = 0; i < 15; i++)
        {
            var x = i * 190f - 120f;
            DrawColoredPolygon(new[] { new Vector2(x, 245), new Vector2(x + 80, 170), new Vector2(x + 160, 245) }, new Color("72b89a"));
        }
    }
}
