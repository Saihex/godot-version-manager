extends Panel

var fetcher
var ListContainer
var InstallatingList = {}

func _on_version_downloaded(installerNode: Node, button: PanelContainer, checker: String):	
	installerNode.queue_free()
	button.get_node("HBoxContainer/NOTINSTALLED").set_visible(false)
	button.get_node("HBoxContainer/Installed").set_visible(true)
	button.get_node("HBoxContainer/Installing").set_visible(false)
	get_tree().call_group("reactive_elements", "refresh_list")
	get_tree().call_group("notify", "info_notify", "Successfully downloaded " + checker);
	InstallatingList.erase(checker)
	
func _on_version_download_failed(msg: String, installerNode: Node, button: PanelContainer, checker: String):	
	installerNode.queue_free()
	button.get_node("HBoxContainer/NOTINSTALLED").set_visible(true)
	button.get_node("HBoxContainer/Installed").set_visible(false)
	button.get_node("HBoxContainer/Installing").set_visible(false)
	get_tree().call_group("reactive_elements", "refresh_list")
	var version_name = button.get_node("HBoxContainer/Name/Text").text
	get_tree().call_group("notify", "warning_notify", "Failed to download " + version_name + "\n" + msg)
	InstallatingList.erase(checker)

func checkIfVersionInstalled(_name: String, mono: bool):	
	var dirName = InstallationLocationSingleton.InstallationLocation + "/" + _name
	
	if mono:	
		dirName += "_mono"
	
	var dir = DirAccess.open(dirName)
	return dir != null

func _on_version_pressed(button: PanelContainer, version_data: Object, mono: bool):	
	if InstallationLocationSingleton.InstallationLocation == "":	
		get_tree().call_group("notify", "warning_notify", "Please set a installation location.")
		return
	
	var checker = version_data.Name
	if mono:	
		checker += " (Mono)"
	
	if InstallatingList.get(checker, false):	
		if not InstallatingList[checker].canCancel:	
			get_tree().call_group("notify", "warning_notify", "Cannot cancel this installation!")
			return
			
		InstallatingList[checker].Cancel()
		button.get_node("HBoxContainer/NOTINSTALLED").set_visible(true)
		button.get_node("HBoxContainer/Installed").set_visible(false)
		button.get_node("HBoxContainer/Installing").set_visible(false)
		get_tree().call_group("notify", "cancel_notify", checker + " installation has been cancelled.")
		InstallatingList.erase(checker)
		return
	
	var InstallationHandler = preload("res://CSharpScripts/InstallationHandler.cs")
	InstallationHandler = InstallationHandler.new()
	InstallationHandler.name = checker
	add_child(InstallationHandler)
	
	InstallatingList[checker] = InstallationHandler
	
	button.get_node("HBoxContainer/NOTINSTALLED").set_visible(false)
	button.get_node("HBoxContainer/Installed").set_visible(false)
	button.get_node("HBoxContainer/Installing").set_visible(true)
	
	InstallationHandler.connect("InstallationCompleted", Callable(self, "_on_version_downloaded").bind(InstallationHandler, button, checker))
	InstallationHandler.connect("InstallationFailed", Callable(self, "_on_version_download_failed").bind(InstallationHandler, button, checker))
	
	InstallationHandler.beginDownload(version_data, mono)

func _on_versions_fetched(versions: Array):	
	# signal to update the list
	var button_template = ListContainer.get_node("ButtonTemplate")
	
	for version_data in versions:
		if version_data.Draft or version_data.Prerelease:
			continue
			
		# normal version
		if version_data.normalAsset != "":	
			var button = button_template.duplicate()
			
			var filename = version_data.normalAsset.split("/")
			filename = filename[filename.size() - 1]
			
			button.set_visible(true)
			button.get_node("HBoxContainer/Name/Text").text = version_data.Name
			if (!checkIfVersionInstalled(version_data.Name, false)): 
				button.get_node("Button").connect("pressed", Callable(self, "_on_version_pressed").bind(button, version_data, false))
			
			button.get_node("Button").tooltip_text = filename + "\n" + version_data.PublishedAt
			button.name = version_data.Name
			button.visible = true
			get_node("ScrollContainer/VBoxContainer").add_child(button)
			button.get_node("HBoxContainer/NOTINSTALLED").set_visible(!checkIfVersionInstalled(version_data.Name, false))
			button.get_node("HBoxContainer/Installed").set_visible(checkIfVersionInstalled(version_data.Name, false))
			button.get_node("HBoxContainer/Installing").set_visible(false)
		
		# mono
		if version_data.monoAsset != "":	
			var button = button_template.duplicate()
			
			var filename = version_data.monoAsset.split("/")
			filename = filename[filename.size() - 1]
			
			button.set_visible(true)
			button.get_node("HBoxContainer/Name/Text").text = version_data.Name + " (mono)"
			
			if (!checkIfVersionInstalled(version_data.Name, true)): 
				button.get_node("Button").connect("pressed", Callable(self, "_on_version_pressed").bind(button, version_data, true))
			
			button.get_node("Button").tooltip_text = filename + "\n" + version_data.PublishedAt
			button.name = version_data.Name + " (mono)"
			button.visible = true
			get_node("ScrollContainer/VBoxContainer").add_child(button)
			button.get_node("HBoxContainer/NOTINSTALLED").set_visible(!checkIfVersionInstalled(version_data.Name, true))
			button.get_node("HBoxContainer/Installed").set_visible(checkIfVersionInstalled(version_data.Name, true))
			button.get_node("HBoxContainer/Installing").set_visible(false)
	
	get_node("LoadingThrobber").set_visible(false)

func force_refresh_list():
	var early_installations = get_node("ScrollContainer/VBoxContainer").get_children()
	for installation in early_installations:	
		if installation.name == "ButtonTemplate":	
			continue
		installation.set_visible(false)
		installation.queue_free()
	
	get_node("LoadingThrobber").set_visible(true)
	get_node("Warning").set_visible(false)
	
	fetcher.Fetch()

func _on_failed_fetch(msg: String):	
	get_node("LoadingThrobber").set_visible(false)
	get_node("Warning").set_visible(true)
	get_tree().call_group("notify", "warning_notify", "Failed to fetch releases!\n" + msg)

func _ready():	
	ListContainer = get_node("ScrollContainer/VBoxContainer")
	var FetcherClass = preload("res://CSharpScripts/GodotVersionFetcher.cs")
	fetcher = FetcherClass.new()
	add_child(fetcher)
	
	get_node("LoadingThrobber").set_visible(true)
	get_node("Warning").set_visible(false)
	fetcher.connect("VersionsFetched", Callable(self, "_on_versions_fetched"))
	fetcher.connect("FetchFailed", Callable(self, "_on_failed_fetch"))
	add_to_group("reactive_elements")
	
	fetcher.Fetch()
