# Copyright (c) 2025 Saihex Studios
# Licensed under the MIT License. See LICENSE file in the project root for full license information.

extends TextureRect

func _ready():	
	if RandomNumberGenerator.new().randi_range(1, 100) != 1:	
		self.set_visible(false)
		self.queue_free()
	else:	
		self.set_visible(true)
