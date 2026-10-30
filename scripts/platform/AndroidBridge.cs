using Godot;

public partial class AndroidBridge : Node
{
    public static AndroidBridge Instance { get; private set; }
    private const string PluginName = "PlatformerBridge";

    public override void _Ready() => Instance = this;

    public bool IsAvailable() => OS.HasFeature("android") && Engine.HasSingleton(PluginName);

    public void Vibrate(long milliseconds = 35)
    {
        if (!IsAvailable()) return;
        var plugin = Engine.GetSingleton(PluginName);
        plugin.Call("vibrate", milliseconds);
    }

    public void Toast(string message)
    {
        if (!IsAvailable()) return;
        var plugin = Engine.GetSingleton(PluginName);
        plugin.Call("toast", message);
    }

    public void LogBridgeStatus()
    {
        GD.Print(IsAvailable()
            ? "[AndroidBridge] Kotlin plugin connected."
            : "[AndroidBridge] Running without Kotlin plugin (editor/desktop-safe).");
    }
}
