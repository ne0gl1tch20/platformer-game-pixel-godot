public static class GameInfo
{
    public const string Title = "Platformer Game Pixel";
    public const string Version = "0.2.0";
    public const string Channel = "Development";
    public const int Build = 1;
    public const string Changelog = "0.2.0 - Game shell, responsive mobile controls, settings and save slots.\n0.1.0 - Pixel overworld, adventurer, enemies, collectibles and Loveglow DSP audio.";

    public static string VersionLabel => $"v{Version} • {Channel} • Build {Build}";
}
