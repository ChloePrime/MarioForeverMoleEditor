@tool
extends VBoxContainer

@onready var rx = get_node("%Rx")
@onready var ry = get_node("%Ry")
@onready var rw = get_node("%Rw")
@onready var rh = get_node("%Rh")
@onready var ox = get_node("%Ox")
@onready var oy = get_node("%Oy")

func get_preview_color():
	var setting := EditorInterface.get_editor_settings()
	var result = setting.get_setting("brush2d/preview/border_color")
	if result == null:
		return Color(0.9,0.4,0.3,0.7)
	return result

func get_param_rect():
	return Rect2(rx.value, ry.value, rw.value, rh.value)

func get_param_offset():
	return Vector2(ox.value, oy.value)

func get_final_rect():
	var rect = get_param_rect()
	rect.position -= get_param_offset()
	return rect

class BrushParamPreview:
	extends Node2D

	var dock = null

	func _init():
		z_index = RenderingServer.CANVAS_ITEM_Z_MAX

	func _draw():
		if !is_instance_valid(dock) || !dock.is_visible_in_tree():
			return

		var r :Rect2 = dock.get_final_rect()
		var color = dock.get_preview_color()
		r.position -= 2 * Vector2.ONE
		r.size += 4 * Vector2.ONE
		draw_rect(r, color, false, 2)

	func _process(_delta):
		queue_redraw()

const PREVIEW_NODE = "_Brush2D_Param_Preview"
var last_root = null

func _update_rect():
	if !is_visible_in_tree() || !is_instance_valid(last_root):
		return
	last_root.set_meta("_brush2d_rect", get_param_rect())

func _update_offset():
	if !is_visible_in_tree() || !is_instance_valid(last_root):
		return
	last_root.set_meta("_brush2d_offset", get_param_offset())

func _ready():
	rx.value_changed.connect(func (v): _update_rect())
	ry.value_changed.connect(func (v): _update_rect())
	rw.value_changed.connect(func (v): _update_rect())
	rh.value_changed.connect(func (v): _update_rect())
	ox.value_changed.connect(func (v): _update_offset())
	oy.value_changed.connect(func (v): _update_offset())

func process_with_root(root : CanvasItem):
	if !root.has_node(PREVIEW_NODE):
		var n = BrushParamPreview.new()
		n.dock = self
		n.name = PREVIEW_NODE
		root.add_child(n, false, INTERNAL_MODE_FRONT)

	if !is_visible_in_tree():
		return

	if last_root != root:
		last_root = root
		if root.has_meta("_brush2d_rect"):
			var rect = root.get_meta("_brush2d_rect")
			rx.set_value_no_signal(rect.position.x)
			ry.set_value_no_signal(rect.position.y)
			rw.set_value_no_signal(rect.size.x)
			rh.set_value_no_signal(rect.size.y)
		else:
			rx.set_value_no_signal(-16.0)
			ry.set_value_no_signal(-16.0)
			rw.set_value_no_signal(32.0)
			rh.set_value_no_signal(32.0)
		if root.has_meta("_brush2d_offset"):
			var offset = root.get_meta("_brush2d_offset")
			ox.set_value_no_signal(offset.x)
			oy.set_value_no_signal(offset.y)
		else:
			ox.set_value_no_signal(0.0)
			oy.set_value_no_signal(0.0)