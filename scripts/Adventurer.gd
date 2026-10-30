extends CharacterBody2D

@export var move_speed: float = 150.0
@export var jump_velocity: float = -360.0
@export var gravity: float = 900.0

var facing: float = 1.0

func _ready() -> void:
	print("[Adventurer] Ready at ", global_position)

func _physics_process(delta: float) -> void:
	var direction := Input.get_axis("ui_left", "ui_right")
	if Input.is_key_pressed(KEY_A):
		direction -= 1.0
	if Input.is_key_pressed(KEY_D):
		direction += 1.0
	direction = clampf(direction, -1.0, 1.0)
	if direction != 0.0:
		facing = signf(direction)
		velocity.x = direction * move_speed
	else:
		velocity.x = move_toward(velocity.x, 0.0, move_speed * 8.0 * delta)

	if not is_on_floor():
		velocity.y += gravity * delta
	elif Input.is_action_just_pressed("ui_accept") or Input.is_key_pressed(KEY_W) or Input.is_key_pressed(KEY_UP):
		velocity.y = jump_velocity
		print("[Adventurer] Jump!")

	move_and_slide()
	queue_redraw()

func _draw() -> void:
	draw_set_transform(Vector2.ZERO, 0.0, Vector2(facing, 1.0))
	draw_rect(Rect2(-7, -27, 14, 8), Color("#34233d"))
	draw_rect(Rect2(-8, -23, 4, 6), Color("#34233d"))
	draw_rect(Rect2(-5, -20, 10, 9), Color("#f2bd86"))
	draw_rect(Rect2(2, -17, 2, 2), Color("#29243a"))
	draw_rect(Rect2(-6, -12, 12, 3), Color("#f4a340"))
	draw_rect(Rect2(-7, -10, 14, 8), Color("#3d78bd"))
	draw_rect(Rect2(-9, -9, 3, 7), Color("#3d78bd"))
	draw_rect(Rect2(6, -9, 3, 7), Color("#3d78bd"))
	draw_rect(Rect2(-7, -3, 14, 3), Color("#5b3b2c"))
	draw_rect(Rect2(-6, 0, 5, 3), Color("#34233d"))
	draw_rect(Rect2(2, 0, 5, 3), Color("#34233d"))
