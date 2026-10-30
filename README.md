# Platformer Game Pixel

A cozy 2D pixel-art platformer built with **Godot 4.7 + C#/.NET**, with a Kotlin Android bridge and cloud APK builds.

## Game shell

- Main menu: Continue, New Game, Save Slots, Settings, About & Version, Credits.
- Pause menu: Resume, Settings, Save Now and return to the main menu.
- Three persistent save slots with legacy save migration and delete/reset support.
- Version label and in-game changelog sourced from `scripts/game/GameInfo.cs`.
- Keyboard, gamepad and Android touch input.
- Responsive menu panels and anchored touch controls for different display sizes.

## Settings

Settings persist locally in `user://settings.cfg`:

- Master, music and sound-effect volume.
- Android touch-control visibility.
- Desktop fullscreen option.
- Reduced-motion option for parallax layers.

## Android controls

Landscape-first platform layout with a virtual analog joystick, jump, attack and pause controls. Touch controls are anchored to the display edges instead of using fixed screen coordinates. The UI uses a scrollable, adaptive menu panel for smaller displays.

## Gameplay and presentation

- C# player controller with acceleration, air control, coyote time and jump buffering.
- Pixel overworld tiles, animated grass, trees and parallax scenery.
- Walker enemies, combat, health, collectibles and respawning.
- Loveglow ambient soundtrack and jump/attack/collect sounds synthesized as PCM at runtime with C# DSP.

## Kotlin bridge

`android/platformer_bridge/` contains the Kotlin Android plugin. The C# wrapper safely exposes vibration and toast support when the plugin is available.

## Build and run

Use the **Godot .NET 4.7 editor** and .NET 9.

```sh
dotnet build PlatformerGamePixel.csproj --configuration Release
godot --headless --path . --quit-after 120
```

GitHub Actions runs the Godot C# validation workflow on pushes and pull requests. The Android debug APK workflow runs on pushes to `main` and manual dispatch; APK artifacts expire after 7 days.
