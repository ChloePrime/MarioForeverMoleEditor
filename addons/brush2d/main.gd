@tool
extends EditorPlugin

var button :HBoxContainer = null

var brush :Brush2D:
	get:
		return _brush
	set(value):
		if value == null && _brush != null:
			_brush.brush_clear()
		_brush = value

var _brush :Brush2D = null

# settings

const setting_key = "brush_2d";
const editor_copy_key = setting_key + "/control/copy_key"
const editor_cut_key = setting_key + "/control/cut_key"
const editor_restrict_key = setting_key + "/control/restrict_key"

const editor_preview_enable = setting_key + "/preview/enable"
const editor_preview_alpha = setting_key + "/preview/alpha"
const editor_preview_border = setting_key + "/preview/draw_border"
const editor_preview_border_width = setting_key + "/preview/border_width"
const editor_preview_border_color = setting_key + "/preview/border_color"
const editor_preview_paint_color = setting_key + "/preview/paint_color"
const editor_preview_erase_color = setting_key + "/preview/erase_color"

static func add_editor_setting(key :String, hint :int, default) ->void:
	var setting := EditorInterface.get_editor_settings()
	setting.set_initial_value(key, default, false)
	setting.add_property_info({
		"name" : key,
		"type" : typeof(default),
		"hint" : hint
	})
	if setting.has_setting(key):
		return
	setting.set_setting(key, default)

static func remove_editor_setting(key :String) ->void:
	var setting := EditorInterface.get_editor_settings()
	if setting.has_setting(key):
		setting.erase(key)

static func create_key(key):
	var ev = InputEventKey.new()
	ev.keycode = key
	return ev

static func get_key(ev, def):
	if ev is InputEventKey:
		var key = ev.keycode
		if key == KEY_NONE:
			return def
		return key
	return def

func apply_settings(p_brush :Brush2D) ->void:
	var setting := EditorInterface.get_editor_settings()
	p_brush.undo = get_undo_redo()
	p_brush.place_mode = button.get_mode()
	p_brush.copy_key = get_key(setting.get_setting(editor_copy_key), KEY_C)
	p_brush.cut_key = get_key(setting.get_setting(editor_cut_key), KEY_X)
	p_brush.restrict_key = get_key(setting.get_setting(editor_restrict_key), KEY_SHIFT)
	p_brush.preview = setting.get_setting(editor_preview_enable)
	p_brush.preview_alpha = setting.get_setting(editor_preview_alpha)
	p_brush.preview_border = setting.get_setting(editor_preview_border)
	p_brush.border_color = setting.get_setting(editor_preview_border_color)
	p_brush.border_width = setting.get_setting(editor_preview_border_width)
	p_brush.paint_color = setting.get_setting(editor_preview_paint_color)
	p_brush.erase_color = setting.get_setting(editor_preview_erase_color)

var dock
var dock_content

func _enter_tree() ->void:
	try_create_button()
	add_editor_setting(editor_copy_key, PROPERTY_HINT_NONE, create_key(KEY_C))
	add_editor_setting(editor_cut_key, PROPERTY_HINT_NONE, create_key(KEY_X))
	add_editor_setting(editor_restrict_key, PROPERTY_HINT_NONE, create_key(KEY_SHIFT))
	add_editor_setting(editor_preview_enable, PROPERTY_HINT_NONE, true)
	add_editor_setting(editor_preview_alpha, PROPERTY_HINT_NONE, 0.5)
	add_editor_setting(editor_preview_border, PROPERTY_HINT_NONE, true)
	add_editor_setting(editor_preview_border_width, PROPERTY_HINT_NONE, 2)
	add_editor_setting(editor_preview_border_color, PROPERTY_HINT_NONE, Color(0.9,0.4,0.3,0.7))
	add_editor_setting(editor_preview_paint_color, PROPERTY_HINT_NONE, Color(1.0,1.0,1.0,0.2))
	add_editor_setting(editor_preview_erase_color, PROPERTY_HINT_NONE, Color(0.0,0.0,0.0,0.4))

	dock = EditorDock.new()
	dock.title = "Brush2D"
	dock.default_slot = EditorDock.DOCK_SLOT_BOTTOM
	dock_content = preload("param_dock.tscn").instantiate()
	dock.add_child(dock_content)
	add_dock(dock)

func _exit_tree() ->void:
	if is_instance_valid(button):
		if button.visible:
			remove_control_from_container(CONTAINER_CANVAS_EDITOR_MENU, button)
		button.queue_free()

	remove_dock(dock)
	dock.queue_free()
	dock = null

func _disable_plugin() ->void:
	remove_editor_setting(editor_copy_key)
	remove_editor_setting(editor_cut_key)
	remove_editor_setting(editor_restrict_key)
	remove_editor_setting(editor_preview_alpha)
	remove_editor_setting(editor_preview_border)
	remove_editor_setting(editor_preview_border_width)
	remove_editor_setting(editor_preview_border_color)
	remove_editor_setting(editor_preview_paint_color)
	remove_editor_setting(editor_preview_erase_color)

# handle brush

func try_create_button() ->void:
	if not is_instance_valid(button):
		button = preload("tool_button.tscn").instantiate()
		var button_node :Button = button.get_node("ToolButton")
		button_node.toggled.connect(select_update)

func is_brush_enabled() ->bool:
	return button != null && button.visible && button.get_node("ToolButton").button_pressed

func select_update(_pressed :bool = false) ->void:
	var select := EditorInterface.get_selection()
	if brush != null && is_brush_enabled():
		select.clear()
		select.add_node(brush)

func _handles(object :Object) ->bool:
	brush = find_brush(object)
	return brush != null

func _forward_canvas_gui_input(event :InputEvent) ->bool:
	canvas_gui_process()

	if !is_brush_enabled():
		return false
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT || event.button_index == MOUSE_BUTTON_RIGHT:
			return true
	return false

func canvas_gui_process() -> void:
	try_create_button()

	if brush != null:
		if !button.visible:
			add_control_to_container(CONTAINER_CANVAS_EDITOR_MENU, button)
			button.visible = true
			select_update()

		apply_settings(brush)
		if is_brush_enabled():
			brush.brush_process()
		else:
			brush.brush_clear()
	elif button.visible:
		remove_control_from_container(CONTAINER_CANVAS_EDITOR_MENU, button)
		button.visible = false

func process_dock():
	var root = EditorInterface.get_edited_scene_root()
	if root == null || root is not CanvasItem:
		dock.close()
		return
	dock.open()
	dock_content.process_with_root(root)

func process_brush():
	var sel = EditorInterface.get_selection().get_selected_nodes()
	if sel.is_empty():
		return
	
	brush = find_brush(sel[0])
	if brush != null:
		if !button.visible:
			add_control_to_container(CONTAINER_CANVAS_EDITOR_MENU, button)
			button.visible = true
			select_update()

		apply_settings(brush)
		brush.process_copy()
	elif button.visible:
		remove_control_from_container(CONTAINER_CANVAS_EDITOR_MENU, button)
		button.visible = false

func _process(_delta: float) -> void:
	process_dock()
	process_brush()

static func find_brush(node) ->Brush2D:
	if node is not Node:
		return null
	if node is Brush2D:
		return node
	var i :Node = node.get_parent()
	var root :Node = node.get_tree().get_edited_scene_root().get_parent()
	while i != root:
		if i is Brush2D:
			return i
		i = i.get_parent()
	return null
