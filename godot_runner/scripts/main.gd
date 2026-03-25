extends Node2D

const SCREEN_SIZE := Vector2(1920, 1080)
const GROUND_Y := 820.0
const CHAR_SHEET_PATH := "res://assets/characters/custom_runner_sheet.png"

enum GameState { SPLASH, MENU, CHARACTER_SELECT, PLAYING, PAUSED, GAME_OVER }

var state: GameState = GameState.SPLASH
var selected_character := 0
var distance := 0.0
var score := 0
var coins := 0
var best_score := 0
var run_speed := 560.0
var speed_ramp := 22.0
var spawn_timer := 0.0
var spawn_interval := 1.15
var rng := RandomNumberGenerator.new()

var world: Node2D
var beach_props: Node2D
var entities: Node2D
var ui_layer: CanvasLayer
var audio_bus: AudioBus
var overlay: ColorRect
var player: Area2D
var player_sprite: Sprite2D
var player_shadow: ColorRect
var hit_flash_timer := 0.0

var vertical_velocity := 0.0
var gravity := 2700.0
var jump_velocity := -1180.0
var is_sliding := false
var slide_timer := 0.0
var shield_timer := 0.0

var splash_panel: Control
var menu_panel: Control
var select_panel: Control
var hud_panel: Control
var pause_panel: Control
var game_over_panel: Control
var jump_button: Button
var slide_button: Button
var distance_label: Label
var coin_label: Label
var score_label: Label
var best_label: Label
var game_over_score: Label

var character_frames: Array[Texture2D] = []

func _ready() -> void:
	rng.randomize()
	best_score = safe_int_from_file("user://best_score.txt")
	selected_character = safe_int_from_file("user://selected_character.txt")
	build_world()
	audio_bus = AudioBus.new()
	add_child(audio_bus)
	build_ui()
	build_player()
	load_character_frames()
	apply_character_visual(selected_character)
	switch_state(GameState.SPLASH)
	var splash_timer := get_tree().create_timer(1.0)
	splash_timer.timeout.connect(func(): switch_state(GameState.MENU))

func build_world() -> void:
	world = Node2D.new()
	add_child(world)

	var sky := ColorRect.new()
	sky.color = Color("#6fd3ff")
	sky.size = SCREEN_SIZE
	world.add_child(sky)

	var sea := ColorRect.new()
	sea.color = Color("#3da2d6")
	sea.position = Vector2(0, 460)
	sea.size = Vector2(SCREEN_SIZE.x, 290)
	world.add_child(sea)

	var sand := ColorRect.new()
	sand.color = Color("#e9cb86")
	sand.position = Vector2(0, 750)
	sand.size = Vector2(SCREEN_SIZE.x, 330)
	world.add_child(sand)

	beach_props = Node2D.new()
	world.add_child(beach_props)
	for i in 24:
		spawn_background_prop(i * 180.0)

	entities = Node2D.new()
	world.add_child(entities)

func spawn_background_prop(x_pos: float) -> void:
	var prop := ColorRect.new()
	var h := rng.randi_range(56, 170)
	prop.color = [Color("#6d8d48"), Color("#d8614f"), Color("#f6f3d7")][rng.randi_range(0, 2)]
	prop.position = Vector2(x_pos, GROUND_Y - h - rng.randi_range(40, 180))
	prop.size = Vector2(rng.randi_range(18, 44), h)
	beach_props.add_child(prop)

func build_player() -> void:
	player = Area2D.new()
	player.position = Vector2(460, GROUND_Y)
	entities.add_child(player)

	var col := CollisionShape2D.new()
	var shape := RectangleShape2D.new()
	shape.size = Vector2(74, 108)
	col.shape = shape
	player.add_child(col)

	player_sprite = Sprite2D.new()
	player_sprite.texture_filter = CanvasItem.TEXTURE_FILTER_NEAREST
	player_sprite.position = Vector2(0, -54)
	player.add_child(player_sprite)

	player_shadow = ColorRect.new()
	player_shadow.color = Color(0, 0, 0, 0.25)
	player_shadow.size = Vector2(96, 18)
	player_shadow.position = Vector2(-48, 10)
	player.add_child(player_shadow)

func build_ui() -> void:
	ui_layer = CanvasLayer.new()
	add_child(ui_layer)

	overlay = ColorRect.new()
	overlay.size = SCREEN_SIZE
	overlay.color = Color(0, 0, 0, 0)
	ui_layer.add_child(overlay)

	splash_panel = panel_with_title("A%A", "Beach Runner")
	ui_layer.add_child(splash_panel)

	menu_panel = panel_with_title("A%A", "Modern Pixel Beach Rush")
	var play_btn := styled_button("Play")
	play_btn.pressed.connect(func(): switch_state(GameState.CHARACTER_SELECT))
	menu_panel.add_child(play_btn)
	play_btn.position = Vector2(870, 580)
	ui_layer.add_child(menu_panel)

	select_panel = panel_with_title("Select Runner", "Choose your hero")
	var male_btn := styled_button("Male Runner")
	male_btn.position = Vector2(680, 510)
	male_btn.pressed.connect(func(): select_character_and_start(0))
	select_panel.add_child(male_btn)
	var female_btn := styled_button("Female Runner")
	female_btn.position = Vector2(1080, 510)
	female_btn.pressed.connect(func(): select_character_and_start(1))
	select_panel.add_child(female_btn)
	ui_layer.add_child(select_panel)

	hud_panel = Control.new()
	hud_panel.size = SCREEN_SIZE
	distance_label = hud_text("DIST 0m", Vector2(46, 32))
	coin_label = hud_text("COIN 0", Vector2(46, 82))
	score_label = hud_text("SCORE 0", Vector2(46, 132))
	hud_panel.add_child(distance_label)
	hud_panel.add_child(coin_label)
	hud_panel.add_child(score_label)
	var pause_btn := styled_button("Pause")
	pause_btn.position = Vector2(1700, 38)
	pause_btn.pressed.connect(func(): switch_state(GameState.PAUSED))
	hud_panel.add_child(pause_btn)
	jump_button = styled_button("JUMP")
	jump_button.position = Vector2(1470, 870)
	jump_button.size = Vector2(180, 128)
	jump_button.pressed.connect(jump)
	hud_panel.add_child(jump_button)
	slide_button = styled_button("SLIDE")
	slide_button.position = Vector2(1680, 870)
	slide_button.size = Vector2(180, 128)
	slide_button.pressed.connect(slide)
	hud_panel.add_child(slide_button)
	ui_layer.add_child(hud_panel)

	pause_panel = panel_with_title("Paused", "Take a breath")
	var resume_btn := styled_button("Resume")
	resume_btn.position = Vector2(860, 530)
	resume_btn.pressed.connect(func(): switch_state(GameState.PLAYING))
	pause_panel.add_child(resume_btn)
	ui_layer.add_child(pause_panel)

	game_over_panel = panel_with_title("Game Over", "Beach run ended")
	game_over_score = hud_text("Score 0", Vector2(840, 470))
	game_over_panel.add_child(game_over_score)
	best_label = hud_text("Best 0", Vector2(840, 520))
	game_over_panel.add_child(best_label)
	var restart_btn := styled_button("Restart")
	restart_btn.position = Vector2(760, 590)
	restart_btn.pressed.connect(start_game)
	game_over_panel.add_child(restart_btn)
	var menu_btn := styled_button("Menu")
	menu_btn.position = Vector2(1010, 590)
	menu_btn.pressed.connect(func(): switch_state(GameState.MENU))
	game_over_panel.add_child(menu_btn)
	ui_layer.add_child(game_over_panel)

func panel_with_title(title: String, subtitle: String) -> Control:
	var panel := Control.new()
	panel.size = SCREEN_SIZE
	var bg := ColorRect.new()
	bg.color = Color(0, 0, 0, 0.34)
	bg.size = SCREEN_SIZE
	panel.add_child(bg)
	var title_label := hud_text(title, Vector2(720, 210), 78)
	panel.add_child(title_label)
	var sub_label := hud_text(subtitle, Vector2(710, 300), 38)
	sub_label.modulate = Color("#ffe7ab")
	panel.add_child(sub_label)
	return panel

func styled_button(text: String) -> Button:
	var btn := Button.new()
	btn.text = text
	btn.size = Vector2(300, 110)
	btn.modulate = Color("#fff4d2")
	btn.add_theme_font_size_override("font_size", 34)
	return btn

func hud_text(text: String, pos: Vector2, size := 42) -> Label:
	var label := Label.new()
	label.text = text
	label.position = pos
	label.add_theme_font_size_override("font_size", size)
	label.modulate = Color("#1d1b1a")
	return label

func load_character_frames() -> void:
	character_frames.clear()
	if ResourceLoader.exists(CHAR_SHEET_PATH):
		var sheet := load(CHAR_SHEET_PATH) as Texture2D
		var frame_w := int(sheet.get_width() / 4)
		var frame_h := int(sheet.get_height() / 2)
		for row in 2:
			var img := Image.create(frame_w, frame_h, false, Image.FORMAT_RGBA8)
			img.blit_rect(sheet.get_image(), Rect2i(0, row * frame_h, frame_w, frame_h), Vector2i.ZERO)
			character_frames.append(ImageTexture.create_from_image(img))
	else:
		character_frames.append(make_runner_texture(Color("#2f64ff"), Color("#f8d8b3")))
		character_frames.append(make_runner_texture(Color("#ff4fb2"), Color("#f6cfab")))

func make_runner_texture(body: Color, skin: Color) -> Texture2D:
	var img := Image.create(64, 96, false, Image.FORMAT_RGBA8)
	img.fill(Color(0, 0, 0, 0))
	img.fill_rect(Rect2i(20, 6, 24, 20), skin)
	img.fill_rect(Rect2i(14, 26, 36, 50), body)
	img.fill_rect(Rect2i(18, 76, 12, 18), Color("#402f25"))
	img.fill_rect(Rect2i(34, 76, 12, 18), Color("#402f25"))
	return ImageTexture.create_from_image(img)

func apply_character_visual(idx: int) -> void:
	selected_character = clamp(idx, 0, max(0, character_frames.size() - 1))
	player_sprite.texture = character_frames[selected_character]

func select_character_and_start(idx: int) -> void:
	apply_character_visual(idx)
	FileAccess.open("user://selected_character.txt", FileAccess.WRITE).store_string(str(idx))
	start_game()

func start_game() -> void:
	distance = 0
	score = 0
	coins = 0
	run_speed = 560.0
	spawn_interval = 1.15
	spawn_timer = 0.0
	shield_timer = 0.0
	player.position = Vector2(460, GROUND_Y)
	vertical_velocity = 0
	is_sliding = false
	for child in entities.get_children():
		if child != player:
			child.queue_free()
	switch_state(GameState.PLAYING)

func switch_state(new_state: GameState) -> void:
	state = new_state
	splash_panel.visible = state == GameState.SPLASH
	menu_panel.visible = state == GameState.MENU
	select_panel.visible = state == GameState.CHARACTER_SELECT
	hud_panel.visible = state == GameState.PLAYING
	pause_panel.visible = state == GameState.PAUSED
	game_over_panel.visible = state == GameState.GAME_OVER
	if state == GameState.GAME_OVER:
		game_over_score.text = "Score %d" % score
		best_label.text = "Best %d" % best_score
	fade_overlay(state in [GameState.PAUSED, GameState.GAME_OVER])

func fade_overlay(dim: bool) -> void:
	var tween := create_tween()
	var alpha := 0.32 if dim else 0.0
	tween.tween_property(overlay, "color", Color(0, 0, 0, alpha), 0.22)

func _process(delta: float) -> void:
	move_background(delta)
	if state == GameState.PLAYING:
		run_gameplay(delta)
	if state == GameState.PLAYING and Input.is_action_just_pressed("pause"):
		switch_state(GameState.PAUSED)
	elif state == GameState.PAUSED and Input.is_action_just_pressed("pause"):
		switch_state(GameState.PLAYING)

func move_background(delta: float) -> void:
	for prop in beach_props.get_children():
		prop.position.x -= run_speed * 0.20 * delta
		if prop.position.x < -100:
			prop.position.x = SCREEN_SIZE.x + rng.randi_range(60, 280)
			prop.position.y = GROUND_Y - prop.size.y - rng.randi_range(40, 180)

func run_gameplay(delta: float) -> void:
	if Input.is_action_just_pressed("jump"):
		jump()
	if Input.is_action_just_pressed("slide"):
		slide()

	distance += run_speed * 0.08 * delta
	run_speed += speed_ramp * delta
	spawn_interval = max(0.48, spawn_interval - delta * 0.012)
	score = int(distance) + coins * 12
	distance_label.text = "DIST %dm" % int(distance)
	coin_label.text = "COIN %d" % coins
	score_label.text = "SCORE %d" % score

	update_player_physics(delta)
	update_entities(delta)
	spawn_timer -= delta
	if spawn_timer <= 0:
		spawn_timer = spawn_interval
		spawn_hazard_or_pickup()

	if shield_timer > 0:
		shield_timer -= delta
		player_shadow.color = Color("#58f0ff") if int(Time.get_ticks_msec() / 90) % 2 == 0 else Color(0, 0, 0, 0.2)
	else:
		player_shadow.color = Color(0, 0, 0, 0.25)

func update_player_physics(delta: float) -> void:
	vertical_velocity += gravity * delta
	player.position.y += vertical_velocity * delta
	if player.position.y >= GROUND_Y:
		player.position.y = GROUND_Y
		vertical_velocity = 0

	if is_sliding:
		slide_timer -= delta
		player.scale.y = 0.64
		if slide_timer <= 0:
			is_sliding = false
			player.scale.y = 1.0

func spawn_hazard_or_pickup() -> void:
	if rng.randf() < 0.34:
		spawn_pickup(["coin", "coin", "coin", "shield", "speed"][rng.randi_range(0, 4)])
	else:
		spawn_obstacle(["rock", "crab", "debris", "umbrella"][rng.randi_range(0, 3)])

func spawn_obstacle(kind: String) -> void:
	var obj := Area2D.new()
	obj.set_meta("type", kind)
	obj.position = Vector2(SCREEN_SIZE.x + 140, GROUND_Y)
	var body := ColorRect.new()
	body.position = Vector2(-42, -74)
	body.size = Vector2(84, 74)
	body.color = {
		"rock": Color("#60565a"),
		"crab": Color("#d54933"),
		"debris": Color("#7a5a2c"),
		"umbrella": Color("#db2f83")
	}[kind]
	obj.add_child(body)
	var col := CollisionShape2D.new()
	var shape := RectangleShape2D.new()
	shape.size = body.size
	col.shape = shape
	col.position = Vector2(0, -36)
	obj.add_child(col)
	entities.add_child(obj)

func spawn_pickup(kind: String) -> void:
	var obj := Area2D.new()
	obj.set_meta("type", kind)
	obj.position = Vector2(SCREEN_SIZE.x + 120, GROUND_Y - rng.randi_range(60, 180))
	var body := ColorRect.new()
	body.position = Vector2(-24, -24)
	body.size = Vector2(48, 48)
	body.color = {
		"coin": Color("#ffd24d"),
		"shield": Color("#58f0ff"),
		"speed": Color("#92ff61")
	}[kind]
	obj.add_child(body)
	var col := CollisionShape2D.new()
	var shape := CircleShape2D.new()
	shape.radius = 24
	col.shape = shape
	obj.add_child(col)
	entities.add_child(obj)

func update_entities(delta: float) -> void:
	for obj in entities.get_children():
		if obj == player:
			continue
		obj.position.x -= run_speed * delta
		if obj.position.x < -120:
			obj.queue_free()
			continue
		if overlaps_player(obj):
			resolve_overlap(obj)

func overlaps_player(obj: Node2D) -> bool:
	var dx := abs(obj.position.x - player.position.x)
	var dy := abs((obj.position.y - 30) - (player.position.y - 50))
	return dx < 70 and dy < (40 if is_sliding else 70)

func resolve_overlap(obj: Area2D) -> void:
	var kind := str(obj.get_meta("type"))
	if kind in ["coin", "shield", "speed"]:
		if kind == "coin":
			coins += 1
		elif kind == "shield":
			shield_timer = 6.0
		else:
			run_speed += 120
		obj.queue_free()
		return

	if shield_timer > 0:
		shield_timer = 0
		obj.queue_free()
		shake_camera()
		return

	best_score = max(best_score, score)
	FileAccess.open("user://best_score.txt", FileAccess.WRITE).store_string(str(best_score))
	switch_state(GameState.GAME_OVER)

func jump() -> void:
	if state != GameState.PLAYING:
		return
	if player.position.y >= GROUND_Y - 0.1:
		vertical_velocity = jump_velocity

func slide() -> void:
	if state != GameState.PLAYING:
		return
	if player.position.y >= GROUND_Y - 0.1:
		is_sliding = true
		slide_timer = 0.55

func shake_camera() -> void:
	var tween := create_tween()
	for i in 3:
		tween.tween_property(world, "position", Vector2(rng.randf_range(-10, 10), rng.randf_range(-8, 8)), 0.03)
	tween.tween_property(world, "position", Vector2.ZERO, 0.06)

func safe_int_from_file(path: String) -> int:
	if not FileAccess.file_exists(path):
		return 0
	var value := FileAccess.get_file_as_string(path).strip_edges()
	return int(value) if value.is_valid_int() else 0
