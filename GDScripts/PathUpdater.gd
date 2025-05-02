extends Label

func _process(_delta):	
	var path = InstallationLocationSingleton.InstallationLocation
	if path == "":	
		self.text = "INSTALLATION LOCATION NOT SET"
	else:	
		self.text = path
