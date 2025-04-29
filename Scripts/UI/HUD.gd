extends Control

# UI elements
@onready var health_bar = $BottomHUD/StatsContainer/HealthBar
@onready var health_label = $BottomHUD/StatsContainer/HealthBar/Label
@onready var mana_bar = $BottomHUD/StatsContainer/ManaBar
@onready var mana_label = $BottomHUD/StatsContainer/ManaBar/Label
@onready var motes_counter = $TopHUD/MotesCounter
@onready var weapon_display = $BottomHUD/WeaponDisplay
@onready var spell_display = $BottomHUD/SpellDisplay
@onready var tarot_hand = $RightHUD/TarotHand
@onready var minimap = $TopHUD/Minimap
@onready var level_display = $TopHUD/LevelDisplay
@onready var boss_health_bar = $BossHealthBar
@onready var mobile_controls = $MobileControls
@onready var pause_button = $TopHUD/PauseButton
@onready var notifications = $Notifications
@onready var damage_flash = $DamageFlash
@onready var pickup_notification = $PickupNotification

# For mobile controls
@onready var left_stick = $MobileControls/LeftStick
@onready var right_stick = $MobileControls/RightStick
@onready var spell_button = $MobileControls/SpellButton
@onready var interact_button = $MobileControls/InteractButton

# Audio elements
@onready var pickup_sound = $PickupSound
@onready var damage_sound = $DamageSound
@onready var heal_sound = $HealSound

# Player reference
var player_node: Node = null

# Class-specific styling
var class_colors = {
	"OccultDetective": Color(0.2, 0.4, 0.7, 1.0),
	"ApostateMedium": Color(0.5, 0.2, 0.7, 1.0),
	"IrredeemableDebtor": Color(0.7, 0.2, 0.2, 1.0),
	"ReclusiveArchivist": Color(0.2, 0.7, 0.4, 1.0),
	"SeditiousOrator": Color(0.7, 0.7, 0.2, 1.0),
	"PhantomConstable": Color(0.5, 0.5, 0.5, 1.0)
}

func _ready():
	# Connect signals
	pause_button.pressed.connect(_on_pause_button_pressed)
	
	# Initialize boss health bar as hidden
	boss_health_bar.visible = false
	
	# Initialize damage flash as transparent
	damage_flash.modulate.a = 0
	
	# Initialize pickup notification as hidden
	pickup_notification.visible = false
	
	# Detect if mobile and show/hide mobile controls accordingly
	_check_platform()
	
	# Connect to game manager signals
	var game_manager = get_node("/root/GameManager")
	if game_manager:
		game_manager.connect("motes_collected", _on_motes_collected)
		game_manager.connect("game_state_changed", _on_game_state_changed)

func _check_platform():
	# Check if we're on mobile
	var is_mobile = false
	
	if OS.get_name() == "Android" or OS.get_name() == "iOS":
		is_mobile = true
	
	# Show/hide mobile controls based on platform
	mobile_controls.visible = is_mobile
	
	# If on mobile, connect mobile control signals
	if is_mobile:
		left_stick.connect("stick_moved", _on_left_stick_moved)
		right_stick.connect("stick_moved", _on_right_stick_moved)
		spell_button.pressed.connect(_on_spell_button_pressed)
		interact_button.pressed.connect(_on_interact_button_pressed)

func set_player(node):
	player_node = node
	
	if player_node:
		# Connect to player signals
		player_node.connect("health_changed", _on_health_changed)
		player_node.connect("mana_changed", _on_mana_changed)
		player_node.connect("weapon_changed", _on_weapon_changed)
		player_node.connect("spell_changed", _on_spell_changed)
		
		# Initialize HUD with current player values
		_update_health_display(player_node.get_current_health(), player_node.get_max_health())
		_update_mana_display(player_node.get_current_mana(), player_node.get_max_mana())
		
		# Apply class-specific styling if available
		var player_class = player_node.get("_playerClass")
		if player_class and player_class.get("ClassName") in class_colors:
			_apply_class_styling(player_class.get("ClassName"))

func _apply_class_styling(class_name):
	if class_name in class_colors:
		var color = class_colors[class_name]
		
		# Apply color to various HUD elements
		health_bar.modulate = color.lightened(0.3)
		weapon_display.modulate = color.lightened(0.1)
		spell_display.modulate = color.lightened(0.2)
	else:
		# Default color if class not recognized
		var default_color = Color(0.4, 0.4, 0.4, 1.0)
		health_bar.modulate = default_color
		weapon_display.modulate = default_color
		spell_display.modulate = default_color

func _on_health_changed(current, max_val):
	_update_health_display(current, max_val)
	
	# Play damage or heal sound based on whether health decreased or increased
	if current < _previous_health:
		damage_sound.play()
		_show_damage_flash()
	elif current > _previous_health:
		heal_sound.play()
	
	_previous_health = current

var _previous_health = 0

func _update_health_display(current, max_val):
	health_bar.value = (current / max_val) * 100
	health_label.text = "%d/%d" % [int(current), int(max_val)]
	_previous_health = current

func _on_mana_changed(current, max_val):
	_update_mana_display(current, max_val)

func _update_mana_display(current, max_val):
	mana_bar.value = (current / max_val) * 100
	mana_label.text = "%d/%d" % [int(current), int(max_val)]

func _on_motes_collected(amount):
	var current_motes = int(motes_counter.text.split(": ")[1]) + amount
	motes_counter.text = "Motes: %d" % current_motes
	
	# Show pickup notification
	_show_pickup_notification("Motes +%d" % amount, Color(0.5, 0.8, 1.0, 1.0))
	
	# Play pickup sound
	pickup_sound.play()

func _on_weapon_changed(weapon):
	if weapon:
		var weapon_name = weapon.get("WeaponName")
		var weapon_rarity = weapon.get("Rarity")
		var display_name = weapon.call("GetDisplayName")
		
		# Update weapon display with name and rarity color
		weapon_display.get_node("WeaponName").text = display_name
		
		# Set color based on rarity
		var color = Color.WHITE
		match weapon_rarity:
			0: # Common
				color = Color(0.7, 0.7, 0.7, 1.0)
			1: # Uncommon
				color = Color(0.3, 0.7, 0.3, 1.0)
			2: # Rare
				color = Color(0.3, 0.5, 0.9, 1.0)
			3: # Epic
				color = Color(0.7, 0.3, 0.9, 1.0)
			4: # Mythic
				color = Color(1.0, 0.7, 0.2, 1.0)
		
		weapon_display.get_node("WeaponName").add_theme_color_override("font_color", color)
		
		# Update weapon icon if available
		var weapon_model = weapon.get("WeaponModel")
		if weapon_model and ResourceLoader.exists(weapon_model):
			weapon_display.get_node("WeaponIcon").texture = load(weapon_model)

func _on_spell_changed(spell):
	if spell:
		var spell_name = spell.get("name")
		
		# Update spell display
		spell_display.get_node("SpellName").text = spell_name
		
		# Update spell icon if available
		var spell_icon = spell.get("icon")
		if spell_icon and ResourceLoader.exists(spell_icon):
			spell_display.get_node("SpellIcon").texture = load(spell_icon)

func _on_game_state_changed(new_state):
	match new_state:
		0: # MainMenu
			visible = false
		1: # Connecting
			visible = false
		2: # Playing
			visible = true
		3: # Paused
			# HUD stays visible during pause
			pass
		4: # GameOver
			# HUD stays visible during game over
			pass
		5: # Victory
			# HUD stays visible during victory
			pass

func update_minimap(room_data, current_room_id, connections):
	# Clear existing minimap
	var minimap_grid = minimap.get_node("MinimapGrid")
	for child in minimap_grid.get_children():
		child.queue_free()
	
	# Create minimap based on room data
	# This is a simplified implementation that would be expanded in the real game
	for i in range(len(room_data)):
		var room = room_data[i]
		var room_position = room.get("Position")
		
		var room_tile = TextureRect.new()
		room_tile.texture = preload("res://Assets/UI/minimap_room.png") # Placeholder
		room_tile.custom_minimum_size = Vector2(10, 10)
		
		# Position the room tile on the grid
		var grid_position = Vector2(
			room_position.x * 12 + 50, 
			room_position.y * 12 + 50
		)
		room_tile.position = grid_position
		
		# Highlight current room
		if i == current_room_id:
			room_tile.modulate = Color(1.0, 0.8, 0.2, 1.0)
		else:
			# Color based on room type
			match room.get("Type"):
				0: # Starting
					room_tile.modulate = Color(0.2, 0.7, 0.2, 1.0)
				5: # Treasure
					room_tile.modulate = Color(0.9, 0.7, 0.2, 1.0)
				6: # Boss
					room_tile.modulate = Color(0.9, 0.2, 0.2, 1.0)
				_:
					room_tile.modulate = Color(0.7, 0.7, 0.7, 1.0)
		
		# Show connections between rooms
		if i in connections:
			for connected_room_id in connections[i]:
				if connected_room_id < len(room_data):
					var connected_room = room_data[connected_room_id]
					var connected_position = connected_room.get("Position")
					
					var line = Line2D.new()
					line.width = 2.0
					line.default_color = Color(0.5, 0.5, 0.5, 0.7)
					line.add_point(grid_position + Vector2(5, 5))
					
					var connected_grid_position = Vector2(
						connected_position.x * 12 + 50,
						connected_position.y * 12 + 50
					)
					line.add_point(connected_grid_position + Vector2(5, 5))
					
					minimap_grid.add_child(line)
		
		minimap_grid.add_child(room_tile)

func update_level_display(level_number):
	level_display.text = "Level: %d" % level_number

func show_boss_health(boss_name, current_health, max_health):
	boss_health_bar.visible = true
	boss_health_bar.get_node("BossName").text = boss_name
	boss_health_bar.get_node("HealthBar").value = (current_health / max_health) * 100

func hide_boss_health():
	boss_health_bar.visible = false

func add_tarot_card(card):
	var card_slot = TextureRect.new()
	card_slot.texture = load("res://Assets/UI/tarot_card_slot.png") # Placeholder
	card_slot.custom_minimum_size = Vector2(50, 80)
	
	var card_name = Label.new()
	card_name.text = card.get("CardName")
	card_name.autowrap_mode = Label.AUTOWRAP_WORD
	card_name.custom_minimum_size = Vector2(45, 0)
	card_name.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	
	var card_color = card.get("CardColor")
	if card_color:
		card_name.add_theme_color_override("font_color", card_color)
	
	card_slot.add_child(card_name)
	
	tarot_hand.add_child(card_slot)
	
	# Show pickup notification
	_show_pickup_notification("New Tarot: " + card.get("CardName"), card_color if card_color else Color(0.8, 0.6, 0.9, 1.0))

func _show_damage_flash():
	damage_flash.modulate.a = 0.5
	
	# Create tween to fade out damage flash
	var tween = create_tween()
	tween.tween_property(damage_flash, "modulate:a", 0.0, 0.5)

func _show_pickup_notification(text, color):
	pickup_notification.get_node("Label").text = text
	pickup_notification.get_node("Label").add_theme_color_override("font_color", color)
	pickup_notification.visible = true
	
	# Create tween to fade in and out
	var tween = create_tween()
	tween.tween_property(pickup_notification, "modulate:a", 1.0, 0.2)
	tween.tween_interval(1.5)
	tween.tween_property(pickup_notification, "modulate:a", 0.0, 0.5)
	tween.tween_callback(func(): pickup_notification.visible = false)

func _on_pause_button_pressed():
	var game_manager = get_node("/root/GameManager")
	if game_manager:
		game_manager.pause_game()

# Mobile control methods
func _on_left_stick_moved(direction):
	if player_node:
		# Pass movement input to player
		player_node.set("_inputDirection", direction)

func _on_right_stick_moved(direction):
	if player_node:
		# Pass aim input to player
		player_node.set("_aimDirection", direction)

func _on_spell_button_pressed():
	if player_node and player_node.has_method("CastSpell"):
		player_node.call("CastSpell")

func _on_interact_button_pressed():
	if player_node and player_node.has_method("Interact"):
		player_node.call("Interact")

# Show notification message
func show_notification(message, duration = 3.0):
	var notification_label = Label.new()
	notification_label.text = message
	notification_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	notification_label.add_theme_font_size_override("font_size", 20)
	notification_label.modulate.a = 0
	
	notifications.add_child(notification_label)
	
	# Create fade in/out tween
	var tween = create_tween()
	tween.tween_property(notification_label, "modulate:a", 1.0, 0.5)
	tween.tween_interval(duration)
	tween.tween_property(notification_label, "modulate:a", 0.0, 0.5)
	tween.tween_callback(func(): notification_label.queue_free())
