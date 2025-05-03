# Copyright (c) 2025 Saihex Studios
# Licensed under the MIT License. See LICENSE file in the project root for full license information.

extends Panel

var launcher

func _launch_installation(path: String):	
	launcher.launchProcess(path, "")

func updateList():	
	# clear the list
	var early_installations = get_node("List/uwu").get_children()
	for installation in early_installations:	
		if installation.name == "ButtonTemplate":	
			continue
		installation.set_visible(false)
		installation.queue_free()
	
	# setup new list
	var button_template = get_node("List/uwu/ButtonTemplate")
	var installations = launcher.getInstallationList()
	
	if (installations == null):	
		return
	
	for data in installations:	
		var button = button_template.duplicate()
		button.get_node("Panel/Name/Text").text = data.Name
		button.get_node("Button").tooltip_text = data.Executable
		button.set_visible(true)
		get_node("List/uwu").add_child(button)
		button.get_node("Button").connect("pressed", Callable(self, "_launch_installation").bind(data.Executable))
		button.get_node("Panel/Uninstall/Button").set_meta("VersionPath", data.Executable.split("/")[data.Executable.split("/").size() - 2])

func refresh_list():	
	updateList()

func force_refresh_list():	
	get_node("LoadingThrobber").set_visible(true)
	updateList()
	get_node("LoadingThrobber").set_visible(false)

func force_refresh_without_fetch():	
	force_refresh_list()

func _ready():	
	await get_tree().create_timer(1.0).timeout
	var launcherClass = preload("res://CSharpScripts/GodotLauncher.cs")
	launcher = launcherClass.new() 
	add_child(launcher)
	updateList()
	get_node("LoadingThrobber").set_visible(false)
	add_to_group("reactive_elements")
