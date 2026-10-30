# Platformer Game Pixel

Production-mode 2D pixel platformer built with **Godot 4.7 + C#/.NET**, with an Android Kotlin bridge and a cloud-first build pipeline.

## Current foundation

1. **Production player controller** — acceleration, air control, coyote time, jump buffering, facing, attack and damage.
2. **World/level structure** — reusable `Platform` scene, camera limits and a real level layout.
3. **Pixel presentation** — crisp pixel-style procedural characters/backgrounds with nearest-neighbor filtering.
4. **Enemies + combat** — patrol walkers with health and player attack range.
5. **Collectibles + progression** — persistent crystal count and best score state.
6. **Health/death/respawn** — health, invulnerability, fall death and checkpoint respawn.
7. **Mobile controls** — touch movement, jump and attack controls on Android.
8. **Save system** — versioned JSON save data in `user://save.json`.
9. **Audio-ready architecture** — gameplay systems are separated so SFX/music can be added without rewriting gameplay.
10. **HUD/settings-ready structure** — HUD is a separate CanvasLayer and Android-safe UI hooks are isolated.
11. **Android build pipeline** — GitHub Actions builds a debug APK on merged `main` pushes and keeps the artifact for 7 days.
12. **Kotlin integration** — `PlatformerBridge` is a Godot Android plugin written in Kotlin and exposed to the C# game layer.

## C#

Yes. This repo is now C#/.NET-first. Godot supports C# on Android, but Android export is still marked experimental by Godot. Android exports require .NET 9 or newer, so CI uses .NET 9. citehttps://docs.godotengine.org/en/4.7/tutorials/export/exporting_for_android.html

## Kotlin bridge

`android/platformer_bridge/` contains the Kotlin Android library. It exposes:

- `vibrate(milliseconds)`
- `toast(message)`

The C# wrapper stays safe on desktop/editor builds and talks to the Android singleton only when the plugin exists.

Godot 4.2+ uses the v2 Android plugin architecture, which packages Kotlin/Java Android libraries through Gradle. citehttps://docs.godotengine.org/en/4.7/tutorials/platform/android/android_plugin.html

## Cloud build

- `Godot Production Check` runs C# compilation and headless startup on pushes and pull requests.
- `Android Debug APK` runs only on `main` pushes or manual dispatch.
- Android APK artifacts expire after **7 days**.

## Local run

Use the **Godot .NET 4.7 editor**, not the standard GDScript-only editor.

```sh
dotnet build PlatformerGamePixel.csproj
godot --path .
```

For Android, the Gradle export path is enabled in `export_presets.cfg`. The Kotlin bridge requires the Android Gradle build flow.
