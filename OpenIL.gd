# Copyright (c) 2025 Saihex Studios
# Licensed under the MIT License. See LICENSE file in the project root for full license information.

extends Button

func _pressed():	
	var path = InstallationLocationSingleton.InstallationLocation
	
	if path != "" and DirAccess.dir_exists_absolute(path):	
		OS.shell_open(path)
	else:	
		get_tree().call_group("notify", "warning_notify", "Please set a installation location.")
