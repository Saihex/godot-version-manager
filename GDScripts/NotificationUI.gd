extends PanelContainer

var notif_chime = preload("res://Sounds/notify_chime.ogg")
var notif_warn = preload("res://Sounds/notif_warning.ogg")

var player: AudioStreamPlayer

func hideNotify():	
	self.set_visible(false)
	self.modulate = Color8(255, 255, 255, 0)

func warning_notify(message: String):	
	player.stream = notif_warn
	player.play()
	self.set_visible(true)
	get_node("HBoxContainer/MarginContainer/warning").set_visible(true)
	get_node("HBoxContainer/MarginContainer/checkmark").set_visible(false)
	get_node("HBoxContainer/MarginContainer/cross").set_visible(false)
	get_node("HBoxContainer/Label").text = message
	var tween = get_tree().create_tween()
	tween.tween_property(self, "modulate", Color8(255, 255, 255, 255), 0.25)
	tween.play()

func info_notify(message: String):	
	player.stream = notif_chime
	player.play()
	self.set_visible(true)
	get_node("HBoxContainer/MarginContainer/warning").set_visible(false)
	get_node("HBoxContainer/MarginContainer/checkmark").set_visible(true)
	get_node("HBoxContainer/MarginContainer/cross").set_visible(false)
	get_node("HBoxContainer/Label").text = message
	var tween = get_tree().create_tween()
	tween.tween_property(self, "modulate", Color8(255, 255, 255, 255), 0.25)
	tween.play()
	
func cancel_notify(message: String):	
	player.stream = notif_warn
	player.play()
	self.set_visible(true)
	get_node("HBoxContainer/MarginContainer/warning").set_visible(false)
	get_node("HBoxContainer/MarginContainer/checkmark").set_visible(false)
	get_node("HBoxContainer/MarginContainer/cross").set_visible(true)
	get_node("HBoxContainer/Label").text = message
	var tween = get_tree().create_tween()
	tween.tween_property(self, "modulate", Color8(255, 255, 255, 255), 0.25)
	tween.play()

func _ready():	
	player = AudioStreamPlayer.new()
	player.bus = "UI"
	add_child(player)
	
	hideNotify()
	add_to_group("notify")
	get_node("Button").connect("pressed", Callable(self, "hideNotify"))
