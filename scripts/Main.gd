extends Node2D

const ADVENTURER_SCENE: PackedScene = preload("res://scenes/Adventurer.tscn")
var adventurer: CharacterBody2D

func _ready() -> void:
	print("[Game] Booting Platformer Game Pixel...")
	_build_level()
	_spawn_adventurer()
	_build_hud()
	print("[Game] Startup complete. Move: A/D or arrows | Jump: Space/W/Up")

func _draw() -> void:
	draw_rect(Rect2(-2000, -1000, 5000, 1300), Color("#83c9e8"))
	draw_rect(Rect2(-2000, 190, 5000, 80), Color("#a8dfc2"))
	for i in range(18):
		var x := float(i * 125 - 100)
		draw_rect(Rect2(x, 42 + (i % 3) * 13, 34, 7), Color("#e9f6ef"))
		draw_rect(Rect2(x + 8, 35 + (i % 3) * 13, 20, 7), Color("#e9f6ef"))
	for i in range(12):
		var x := float(i * 190 - 120)
		draw_colored_polygon(PackedVector2Array([Vector2(x, 245), Vector2(x + 80, 170), Vector2(x + 160, 245)]), Color("#72b89a"))

func _build_level() -> void:
	print("[Level] Building platforms and ground...")
	_create_platform(Vector2(0, 258), Vector2(1800, 24), Color("#6caa58"))
	_create_platform(Vector2(170, 211), Vector2(76, 12), Color("#9b7048"))
	_create_platform(Vector2(300, 174), Vector2(76, 12), Color("#9b7048"))
	_create_platform(Vector2(430, 137), Vector2(76, 12), Color("#9b7048"))
	_create_platform(Vector2(570, 195), Vector2(90, 12), Color("#9b7048"))

func _create_platform(at: Vector2, size: Vector2, color: Color) -> void:
	var platform := StaticBody2D.new()
	platform.position = at
	platform.name = "Platform"
	var collision := CollisionShape2D.new()
	var rectangle := RectangleShape2D.new()
	rectangle.size = size
	collision.shape = rectangle
	platform.add_child(collision)
	var visual := Polygon2D.new()
	visual.polygon = PackedVector2Array([Vector2(-size.x / 2.0, -size.y / 2.0), Vector2(size.x / 2.0, -size.y / 2.0), Vector2(size.x / 2.0, size.y / 2.0), Vector2(-size.x / 2.0, size.y / 2.0)])
	visual.color = color
	platform.add_child(visual)
	add_child(platform)

func _spawn_adventurer() -> void:
	adventurer = ADVENTURER_SCENE.instantiate()
	adventurer.name = "Adventurer"
	adventurer.position = Vector2(64, 230)
	add_child(adventurer)
	print("[Game] Adventurer spawned at ", adventurer.position)

func _build_hud() -> void:
	var layer := CanvasLayer.new()
	layer.name = "HUD"
	add_child(layer)
	var hint := Label.new()
	hint.text = "PIXEL TRAIL  |  MOVE: A/D or arrows  |  JUMP: SPACE / W / UP"
	hint.position = Vector2(8, 7)
	hint.add_theme_font_size_override("font_size", 10)
	hint.add_theme_color_override("font_color", Color("#26394b"))
	layer.add_child(hint)
