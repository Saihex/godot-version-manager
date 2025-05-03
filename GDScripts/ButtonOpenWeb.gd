# Copyright (c) 2025 Saihex Studios
# Licensed under the MIT License. See LICENSE file in the project root for full license information.

extends Button

var url = "https://www.saihex.com/"

func _ready():	
	url = get_meta("url")
	self.pressed.connect(_button_pressed)
	
	
func _button_pressed():	
	OS.shell_open(url)
