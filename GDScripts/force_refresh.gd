# Copyright (c) 2025 Saihex Studios
# Licensed under the MIT License. See LICENSE file in the project root for full license information.

extends Button

func _pressed():	
	if has_meta("without_fetch") and get_meta("without_fetch"):
		get_tree().call_group("reactive_elements", "force_refresh_without_fetch")
		return
		
	get_tree().call_group("reactive_elements", "force_refresh_list")
