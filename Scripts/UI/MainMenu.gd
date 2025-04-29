extends Control

signal start_game(is_host, player_name, player_class)
signal join_game(address, port, player_name, player_class)

# UI elements
@onready var host_button = $PanelContainer/MarginContainer/VBoxContainer/ButtonsContainer/HostButton
@onready var join_button = $PanelContainer/MarginContainer/VBoxContainer/ButtonsContainer/JoinButton
@onready var settings_button = $PanelContainer/MarginContainer/VBoxContainer/ButtonsContainer/SettingsButton
@onready var quit_button = $PanelContainer/MarginContainer/VBoxContainer/ButtonsContainer/QuitButton
@onready var player_name_input = $PanelContainer/MarginContainer/VBoxContainer/PlayerNameContainer/PlayerNameInput
@onready var class_option_button = $PanelContainer/MarginContainer/VBoxContainer/ClassSelectionContainer/ClassOptionButton
@onready var class_description_label = $PanelContainer/MarginContainer/VBoxContainer/ClassSelectionContainer/ClassDescriptionLabel
@onready var ip_address_input = $JoinGameDialog/VBoxContainer/IPContainer/IPAddressInput
@onready var port_input = $JoinGameDialog/VBoxContainer/PortContainer/PortInput
@onready var join_game_dialog = $JoinGameDialog
@onready var settings_dialog = $SettingsDialog
@onready var version_label = $VersionLabel
@onready var loading_panel = $LoadingPanel

# Audio
@onready var button_sound = $ButtonSound
@onready var menu_music = $MenuMusic

# Class data
var classes = {
	"OccultDetective": {
		"description": "Volition-driven detective with marksman rifle, rally aura, and interrupt shot.",
		"color": Color.LIGHT_BLUE
	},
	"ApostateMedium": {
		"description": "Inland Empire-based medium with shotgun, astral projection blink, and spirit shield.",
		"color": Color.LIGHT_PURPLE
	},
	"IrredeemableDebtor": {
		"description": "Authority-driven debtor with gatling cannon, fear shout, and berserk swap-HP-for-damage.",
		"color": Color.DARK_RED
	},
	"ReclusiveArchivist": {
		"description": "Empathy-centered archivist with beam staff, slow-time field, and combo tracker.",
		"color": Color.LIGHT_GREEN
	},
	"SeditiousOrator": {
		"description": "Suggestion-based orator with dual pistols, charm grenade, and critical trick-shot.",
		"color": Color.YELLOW
	},
	"PhantomConstable": {
		"description": "Esprit de Corps-based constable with lawgiver revolver, badge-ward totem, and revive tether.",
		"color": Color.LIGHT_GRAY
	}
}

func _ready():
	# Connect signals
	host_button.pressed.connect(_on_host_button_pressed)
	join_button.pressed.connect(_on_join_button_pressed)
	settings_button.pressed.connect(_on_settings_button_pressed)
	quit_button.pressed.connect(_on_quit_button_pressed)
	
	join_game_dialog.get_node("VBoxContainer/ButtonContainer/ConnectButton").pressed.connect(_on_connect_button_pressed)
	join_game_dialog.get_node("VBoxContainer/ButtonContainer/CancelButton").pressed.connect(_on_join_cancel_button_pressed)
	
	settings_dialog.get_node("VBoxContainer/ButtonContainer/SaveButton").pressed.connect(_on_settings_save_button_pressed)
	settings_dialog.get_node("VBoxContainer/ButtonContainer/CancelButton").pressed.connect(_on_settings_cancel_button_pressed)
	
	class_option_button.item_selected.connect(_on_class_option_selected)
	
	# Set version label
	version_label.text = "v0.1.0 Alpha"
	
	# Initialize UI elements
	_populate_class_dropdown()
	_load_player_settings()
	
	# Start menu music
	menu_music.play()
	
	# Hide dialogs and loading panel
	join_game_dialog.visible = false
	settings_dialog.visible = false
	loading_panel.visible = false

func _populate_class_dropdown():
	class_option_button.clear()
	
	var index = 0
	for class_name in classes.keys():
		# Format class name for display (convert CamelCase to "Camel Case")
		var display_name = ""
		for i in range(class_name.length()):
			if i > 0 and class_name[i].is_uppercase() and not class_name[i-1].is_uppercase():
				display_name += " "
			display_name += class_name[i]
		
		class_option_button.add_item(display_name, index)
		index += 1
	
	# Select first item by default
	if class_option_button.item_count > 0:
		class_option_button.select(0)
		_update_class_description(0)

func _update_class_description(index):
	var class_key = classes.keys()[index]
	var description = classes[class_key]["description"]
	var color = classes[class_key]["color"]
	
	class_description_label.text = description
	class_description_label.add_theme_color_override("font_color", color)

func _load_player_settings():
	# Load saved player name if exists
	if PlayerSettings.has_setting("player_name"):
		player_name_input.text = PlayerSettings.get_setting("player_name")
	else:
		player_name_input.text = "Player" + str(randi() % 1000)
	
	# Load saved class if exists
	if PlayerSettings.has_setting("player_class_index"):
		var saved_index = PlayerSettings.get_setting("player_class_index")
		if saved_index < class_option_button.item_count:
			class_option_button.select(saved_index)
			_update_class_description(saved_index)

func _save_player_settings():
	PlayerSettings.set_setting("player_name", player_name_input.text)
	PlayerSettings.set_setting("player_class_index", class_option_button.selected)
	PlayerSettings.save_settings()

func _on_host_button_pressed():
	button_sound.play()
	
	var player_name = player_name_input.text
	if player_name.strip_edges() == "":
		player_name = "Host" + str(randi() % 1000)
		player_name_input.text = player_name
	
	var player_class = classes.keys()[class_option_button.selected]
	
	_save_player_settings()
	
	# Show loading panel
	loading_panel.get_node("Label").text = "Starting Game..."
	loading_panel.visible = true
	
	# Use a timer to allow the UI to update before starting the game
	var timer = Timer.new()
	timer.one_shot = true
	timer.wait_time = 0.5
	timer.timeout.connect(func(): emit_signal("start_game", true, player_name, player_class))
	add_child(timer)
	timer.start()

func _on_join_button_pressed():
	button_sound.play()
	join_game_dialog.visible = true

func _on_settings_button_pressed():
	button_sound.play()
	settings_dialog.visible = true

func _on_quit_button_pressed():
	button_sound.play()
	get_tree().quit()

func _on_connect_button_pressed():
	button_sound.play()
	
	var player_name = player_name_input.text
	if player_name.strip_edges() == "":
		player_name = "Player" + str(randi() % 1000)
		player_name_input.text = player_name
	
	var player_class = classes.keys()[class_option_button.selected]
	var address = ip_address_input.text
	var port = int(port_input.text)
	
	_save_player_settings()
	
	# Hide dialog
	join_game_dialog.visible = false
	
	# Show loading panel
	loading_panel.get_node("Label").text = "Connecting..."
	loading_panel.visible = true
	
	# Use a timer to allow the UI to update before joining the game
	var timer = Timer.new()
	timer.one_shot = true
	timer.wait_time = 0.5
	timer.timeout.connect(func(): emit_signal("join_game", address, port, player_name, player_class))
	add_child(timer)
	timer.start()

func _on_join_cancel_button_pressed():
	button_sound.play()
	join_game_dialog.visible = false

func _on_settings_save_button_pressed():
	button_sound.play()
	
	# Save settings logic would go here
	# For now, just close the dialog
	settings_dialog.visible = false

func _on_settings_cancel_button_pressed():
	button_sound.play()
	settings_dialog.visible = false

func _on_class_option_selected(index):
	button_sound.play()
	_update_class_description(index)

func show_connection_error(error_message):
	# Hide loading panel
	loading_panel.visible = false
	
	# Show error dialog
	var error_dialog = $ErrorDialog
	error_dialog.get_node("VBoxContainer/ErrorLabel").text = error_message
	error_dialog.visible = true
	
	# Add a timer to auto-hide after 5 seconds
	var timer = Timer.new()
	timer.one_shot = true
	timer.wait_time = 5.0
	timer.timeout.connect(func(): error_dialog.visible = false)
	add_child(timer)
	timer.start()

# This is a placeholder class for the PlayerSettings singleton
# In a real implementation, this would be an autoload script
class PlayerSettings:
	static var settings = {}
	
	static func has_setting(key):
		return key in settings
	
	static func get_setting(key):
		return settings.get(key)
	
	static func set_setting(key, value):
		settings[key] = value
	
	static func save_settings():
		# In a real implementation, this would save to disk
		pass
