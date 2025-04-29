extends Control

signal card_selected(card_resource)

# UI elements
@onready var card_container = $CardsContainer
@onready var title_label = $TitleLabel
@onready var description_label = $DescriptionLabel
@onready var timer = $Timer

# Card options
var card_options = []
var selected_card = null

# Max selection time in seconds
export var selection_time = 15.0

func _ready():
	timer.wait_time = selection_time
	timer.one_shot = true
	timer.timeout.connect(_on_timer_timeout)
	
	# Hide by default
	visible = false

func show_card_selection(cards, title = "Choose a Tarot Card"):
	# Clear previous cards
	for child in card_container.get_children():
		child.queue_free()
	
	# Store card options
	card_options = cards
	
	# Set title
	title_label.text = title
	
	# Reset description
	description_label.text = "Select a card to add to your hand"
	
	# Create card UI elements
	for i in range(len(cards)):
		var card = cards[i]
		var card_button = create_card_button(card, i)
		card_container.add_child(card_button)
	
	# Show selection UI
	visible = true
	
	# Start timer
	timer.start()
	
	# Pause the game while selecting
	get_tree().paused = true

func create_card_button(card, index):
	var button = TextureButton.new()
	button.texture_normal = preload("res://Assets/UI/tarot_card_back.png") # Placeholder
	button.texture_hover = preload("res://Assets/UI/tarot_card_back_hover.png") # Placeholder
	button.expand = true
	button.stretch_mode = TextureButton.STRETCH_KEEP_ASPECT_CENTERED
	button.custom_minimum_size = Vector2(120, 200)
	
	# Add card name label
	var name_label = Label.new()
	
	# Get formatted card title
	var card_title = ""
	if card.has_method("GetFormattedTitle"):
		card_title = card.call("GetFormattedTitle")
	else:
		card_title = card.get("CardName")
	
	name_label.text = card_title
	name_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	name_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	name_label.custom_minimum_size = Vector2(100, 0)
	name_label.position = Vector2(10, 10)
	
	# Set color based on card type
	var card_type = card.get("Type")
	if card_type == 0: # Major Arcana
		name_label.add_theme_color_override("font_color", Color(0.9, 0.7, 0.2, 1.0))
	else: # Minor Arcana
		var suit = card.get("Suit")
		match suit:
			1: # Wands
				name_label.add_theme_color_override("font_color", Color(0.8, 0.3, 0.2, 1.0))
			2: # Cups
				name_label.add_theme_color_override("font_color", Color(0.2, 0.5, 0.8, 1.0))
			3: # Swords
				name_label.add_theme_color_override("font_color", Color(0.7, 0.7, 0.7, 1.0))
			4: # Pentacles
				name_label.add_theme_color_override("font_color", Color(0.5, 0.8, 0.3, 1.0))
	
	button.add_child(name_label)
	
	# Connect signals
	button.mouse_entered.connect(func(): _on_card_mouse_entered(index))
	button.mouse_exited.connect(func(): _on_card_mouse_exited())
	button.pressed.connect(func(): _on_card_pressed(index))
	
	# Store card resource as metadata
	button.set_meta("card_resource", card)
	
	return button

func _on_card_mouse_entered(index):
	var card = card_options[index]
	
	# Update description with card details
	var card_title = ""
	if card.has_method("GetFormattedTitle"):
		card_title = card.call("GetFormattedTitle")
	else:
		card_title = card.get("CardName")
	
	var card_description = card.get("Description")
	var primary_effect = card.get("PrimaryEffect")
	
	var description = card_title + "\n\n" + card_description + "\n\n" + primary_effect
	
	# If there's a secondary effect, add it
	var secondary_effect = card.get("SecondaryEffect")
	if secondary_effect and secondary_effect != "":
		description += "\n" + secondary_effect
	
	description_label.text = description
	
	# Apply card color
	var card_color = card.get("CardColor")
	if card_color:
		description_label.add_theme_color_override("font_color", card_color)
	else:
		description_label.add_theme_color_override("font_color", Color(1, 1, 1, 1))

func _on_card_mouse_exited():
	# Reset description
	description_label.text = "Select a card to add to your hand"
	description_label.add_theme_color_override("font_color", Color(1, 1, 1, 1))

func _on_card_pressed(index):
	timer.stop()
	selected_card = card_options[index]
	
	# Highlight selected card
	var card_buttons = card_container.get_children()
	for i in range(len(card_buttons)):
		if i == index:
			card_buttons[i].modulate = Color(1.5, 1.5, 1.5, 1.0)
		else:
			card_buttons[i].modulate = Color(0.5, 0.5, 0.5, 1.0)
	
	# Add a short delay before closing
	await get_tree().create_timer(1.0).timeout
	
	_finish_selection()

func _on_timer_timeout():
	# Auto-select first card if none was chosen
	if selected_card == null and len(card_options) > 0:
		selected_card = card_options[0]
	
	_finish_selection()

func _finish_selection():
	# Unpause the game
	get_tree().paused = false
	
	# Emit signal with selected card
	if selected_card:
		emit_signal("card_selected", selected_card)
	
	# Hide the selection UI
	visible = false
