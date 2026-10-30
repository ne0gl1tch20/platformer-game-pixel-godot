using Godot;

public partial class Backdrop : Node2D
{
    public override void _Draw()
    {
        DrawRect(new Rect2(-2400, -600, 6000, 1000), new Color("83c9e8"));
        DrawRect(new Rect2(-2400, 190, 6000, 100), new Color("a8dfc2"));
        DrawCircle(new Vector2(390, 48), 18, new Color("fff0b3"));
        DrawCircle(new Vector2(390, 48), 23, new Color(1f, 0.94f, 0.7f, 0.16f));
    }
}