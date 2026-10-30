# Platformer Game Pixel

A small, runnable 2D pixel-platformer prototype built with **Godot 4.7**.

## Run it

1. Open this folder in Godot 4.7.
2. Press **F5** to run the project.
3. Move with **A/D** or the **Left/Right arrow keys**.
4. Jump with **Space**, **W**, or **Up**.

The main scene builds a simple platform course and spawns the Adventurer automatically. The character is drawn with crisp rectangles in GDScript, so no external sprite import is needed for this prototype.

## Project layout

- `scenes/Main.tscn` — game entry scene
- `scenes/Adventurer.tscn` — player scene and collision
- `scripts/Main.gd` — level setup, player spawning and HUD
- `scripts/Adventurer.gd` — movement, jumping and pixel-art drawing

## Headless validation

```sh
godot --headless --editor --path . --quit
godot --headless --path . --quit-after 120
```

GitHub Actions runs the import and startup checks on pushes and pull requests.
