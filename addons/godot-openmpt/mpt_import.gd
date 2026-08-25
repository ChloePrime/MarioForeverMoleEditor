@tool
extends EditorPlugin

var import_plugin: EditorImportPlugin = null
const ICON_PATH = "res://addons/godot-openmpt/AudioStreamMPT.png"

func _enter_tree() -> void:
	import_plugin = preload("mpt_importer.gd").new()
	add_import_plugin(import_plugin)

func _ready() -> void:
	var base_control = EditorInterface.get_base_control()
	if !base_control.theme:
		base_control.theme = Theme.new()
	var custom_icon_texture = load(ICON_PATH)
	
	if custom_icon_texture:
		base_control.theme.set_icon("AudioStreamMPT", "EditorIcons", custom_icon_texture)


func _exit_tree() -> void:
	remove_import_plugin(import_plugin)
	import_plugin = null
