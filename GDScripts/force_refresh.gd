extends Button

func _pressed():	
	get_tree().call_group("reactive_elements", "force_refresh_list")
