@tool
@icon("icon.png")
extends Node2D
class_name Brush2D

@export var grid: Vector2 = Vector2(32,32)
@export var default_rect :Rect2 = Rect2(-16,-16,32,32)
@export var default_offset : Vector2 = Vector2.ZERO
@export var ignore_scene_rect :bool = false
@export var ignore_scene_offset :bool = false
@export var ignore_scene_transform :bool = false

const PAINT_BUTTON = MOUSE_BUTTON_LEFT
const ERASE_BUTTON = MOUSE_BUTTON_RIGHT

# should be set by plugin
var undo :EditorUndoRedoManager

## add & remove node

static func node_reset_owner(node :Node) -> void:
	if node.has_meta("_brush2d_owner"):
		node.owner = node.get_node(node.get_meta("_brush2d_owner"))
		node.remove_meta("_brush2d_owner")
	else:
		node.owner = EditorInterface.get_edited_scene_root()
	for c in node.get_children():
		node_reset_owner_rec(c)

static func node_reset_owner_rec(node :Node) -> void:
	if node.has_meta("_brush2d_owner"):
		node.owner = node.get_node(node.get_meta("_brush2d_owner"))
		node.remove_meta("_brush2d_owner")
	for c in node.get_children():
		node_reset_owner_rec(c)

static func node_record_owner(node :Node) -> void:
	node.set_meta("_brush2d_owner", node.get_path_to(node.owner))
	for c in node.get_children():
		node_record_owner(c)
	
func brush_add_node(node :Node) -> void:
	add_child(node, true)
	node_reset_owner(node)

func brush_remove_node(node :Node) -> void:
	node_record_owner(node)
	remove_child(node)

func brush_add_nodes(nodes :Array) -> void:
	for n in nodes:
		brush_add_node(n)

func brush_remove_nodes(nodes :Array) -> void:
	for n in nodes:
		brush_remove_node(n)

func brush_commit_add(nodes :Array) -> void:
	if undo == null: return
	if nodes.is_empty(): return
	undo.create_action("Brush2D Add")
	undo.add_do_method(self, &"brush_add_nodes", nodes)
	undo.add_undo_method(self, &"brush_remove_nodes", nodes)
	undo.commit_action()

func brush_commit_remove(nodes :Array) -> void:
	if undo == null: return
	if nodes.is_empty(): return
	undo.create_action("Brush2D Remove")
	undo.add_do_method(self, &"brush_remove_nodes", nodes)
	undo.add_undo_method(self, &"brush_add_nodes", nodes)
	undo.commit_action()

## rect & offset settings

func get_scene_rect(node :CanvasItem) -> Rect2:
	var rect := default_rect
	if not ignore_scene_rect and node.has_meta("_brush2d_rect"):
		rect = node.get_meta("_brush2d_rect")

	if ignore_scene_transform:
		return rect
	
	var pos :Vector2 = rect.position
	var end :Vector2 = rect.end
	pos = node.get_transform().basis_xform(pos)
	end = node.get_transform().basis_xform(end)
	return Rect2(pos, end - pos)

func get_scene_offset(node :CanvasItem) -> Vector2:
	var offset := default_offset
	if not ignore_scene_offset and node.has_meta("_brush2d_offset"):
		offset = node.get_meta("_brush2d_offset")

	if ignore_scene_transform:
		return offset

	return node.get_transform().basis_xform(offset)

## copy list

class CopyList:
	var nodes :Array
	var first_pos :Vector2
	var rect: Rect2

# duplicate_from_editor is not exposed
# we only have very limited choice here...

static func brush_duplicate_node(node :Node) -> Node:
	var result :Node = node.duplicate()
	node_remove_internals(result)
	node_duplicate_owner(node, result)
	return result

static func node_remove_internals(node :Node) -> void:
	for c in node.get_children(true):
		if c.owner == null:
			c.queue_free()
		node_remove_internals(c)

static func node_duplicate_owner(src :Node, dst :Node) -> void:
	var target = src.get_path_to(src.owner) if src.is_inside_tree() else src.get_meta("_brush2d_owner")
	dst.set_meta("_brush2d_owner", target)
	for i in dst.get_child_count():
		node_duplicate_owner(src.get_child(i), dst.get_child(i))

func duplicate_copy_list(list : CopyList) -> CopyList:
	var result := CopyList.new()
	var duplicates := []
	for n in list.nodes:
		duplicates.append(brush_duplicate_node(n))
	result.nodes = duplicates
	result.first_pos = list.first_pos
	result.rect = list.rect
	return result

func get_list_rect(list :Array, first_pos :Vector2) -> Rect2:
	var min_pos := Vector2(INF, INF)
	var max_pos := Vector2(-INF, -INF)
	for i in list:
		if i is not CanvasItem:
			continue

		var rect := get_scene_rect(i)
		min_pos.x = min(min_pos.x, i.position.x + rect.position.x)
		min_pos.y = min(min_pos.y, i.position.y + rect.position.y)
		max_pos.x = max(max_pos.x, i.position.x + rect.end.x)
		max_pos.y = max(max_pos.y, i.position.y + rect.end.y)
	return Rect2(min_pos - first_pos, max_pos - min_pos)

func create_copy_list(array :Array) -> CopyList:
	var has_canvas_item := false
	var first_pos :Vector2 = Vector2.ZERO
	for i in array:
		if i is CanvasItem:
			has_canvas_item = true
			first_pos = i.position
			break
	
	if not has_canvas_item:
		return null

	var nodes := []
	for i in array:
		if i is CanvasItem:
			nodes.append(brush_duplicate_node(i))

	var result := CopyList.new()
	result.nodes = nodes
	result.first_pos = first_pos
	if nodes.size() > 1:
		result.rect = get_list_rect(nodes, first_pos)
	else:
		result.rect = get_scene_rect(nodes[0])

	return result;

## grid coordinate

var grid_pos :Vector2
var last_grid_pos :Vector2
var first_grid := false

func update_grid_pos() -> void:
	last_grid_pos = grid_pos

	var mpos = get_local_mouse_position()
	grid_pos.x = floor(mpos.x/grid.x)*grid.x
	grid_pos.y = floor(mpos.y/grid.y)*grid.y

	if not first_grid:
		first_grid = true
		last_grid_pos = grid_pos

static func bresenham(p0 :Vector2i, p1 :Vector2i) ->Array[Vector2i]:
	var result :Array[Vector2i] = []
	var x0 := p0.x
	var y0 := p0.y
	var x1 := p1.x
	var y1 := p1.y
	var steep :bool = abs(y1 - y0) > abs(x1 - x0)
	if steep:
		var temp :int

		temp = x0
		x0 = y0
		y0 = temp

		temp = x1
		x1 = y1
		y1 = temp

	if x0 > x1:
		var temp :int

		temp = x0
		x0 = x1
		x1 = temp

		temp = y0
		y0 = y1
		y1 = temp

	var delta_x := x1 - x0
	var delta_y :int = abs(y1 - y0)
	var error :float = 0.0
	var delta_error :float = float(delta_y) / delta_x
	var yk := y0

	var y_step := 1 if y0 < y1 else -1

	for xk in range(x0, x1 + 1):
		if steep:
			result.append(Vector2i(yk, xk))
		else:
			result.append(Vector2i(xk, yk))

		error = error + delta_error
		if error >= 0.5:
			yk = yk + y_step
			error = error - 1.0
	return result

var collected_continus_set : Dictionary[Vector2i, bool] = {}

func collect_continous_grid_pos(size :Vector2, origin: Vector2, p2 :Vector2, p1 :Vector2) ->void:
	size.x = ceil(size.x/grid.x)*grid.x
	size.y = ceil(size.y/grid.y)*grid.y
	var o1 = p1 - origin
	var i1 = Vector2i(floor(o1.x/size.x), floor(o1.y/size.y))
	var o2 = p2 - origin
	var i2 = Vector2i(floor(o2.x/size.x), floor(o2.y/size.y))
	var points := bresenham(i1, i2)
	for p in points:
		collected_continus_set[p] = true

func build_continous_grid_pos(size :Vector2, origin: Vector2) ->Array[Vector2]:
	size.x = ceil(size.x/grid.x)*grid.x
	size.y = ceil(size.y/grid.y)*grid.y
	var result :Array[Vector2] = []
	for v in collected_continus_set.keys():
		result.append(Vector2(v.x, v.y) * size + origin)
	return result

var restrict_key := KEY_SHIFT

func get_line_pos(size :Vector2, p1 :Vector2, p2 :Vector2) ->Array[Vector2]:
	size.x = ceil(size.x/grid.x)*grid.x
	size.y = ceil(size.y/grid.y)*grid.y

	var d := p2 - p1
	var r := Vector2i(floor(d.x/size.x), floor(d.y/size.y))
	if Input.is_key_pressed(restrict_key):
		var s :Vector2 = r
		var a = round(s.angle() / (PI / 4.0)) * (PI / 4.0)
		var f := Vector2.RIGHT.rotated(a)
		var l = s.dot(f)
		f *= l
		r = f
	var points := bresenham(Vector2i.ZERO, r)
	var result :Array[Vector2] = []
	for p in points:
		result.append(p1 + Vector2(size.x * p.x, size.y * p.y))
	return result

func get_rect_pos(size :Vector2, p1 :Vector2, p2 :Vector2) ->Array[Vector2]:
	if Input.is_key_pressed(restrict_key):
		var dx = p2.x - p1.x
		var dy = p2.y - p1.y
		var m = min(abs(dx), abs(dy))
		p2.x = p1.x + m * sign(dx)
		p2.y = p1.y + m * sign(dy)

	size.x = ceil(size.x/grid.x)*grid.x * sign(p2.x - p1.x)
	size.y = ceil(size.y/grid.y)*grid.y * sign(p2.y - p1.y)

	var result :Array[Vector2] = []
	var x := p1.x
	while (p1.x <= p2.x && x <= p2.x) || (p1.x > p2.x && x >= p2.x):
		var y := p1.y
		while (p1.y <= p2.y && y <= p2.y) || (p1.y > p2.y && y >= p2.y):
			var r := Vector2(x, y)
			result.append(r)
			if size.y == 0: break
			y += size.y
		if size.x == 0: break
		x += size.x
	if result.is_empty():
		result.append(p1)
	return result

## copy & cut

var copy_data :CopyList = null

func clear_copy_data():
	if copy_data == null:
		return
	
	for n in copy_data.nodes:
		n.queue_free()
	copy_data = null

var copy_key := KEY_C
var cut_key := KEY_X

var copy_pressed := false
var cut_pressed := false

func do_copy_or_cut(cut):
	var sel = EditorInterface.get_selection().get_selected_nodes()
	var sel_children := []
	for c in get_children():
		if sel.has(c):
			sel_children.append(c)
	
	clear_copy_data()
	if sel_children.size() > 0:
		copy_data = create_copy_list(sel_children)
		if cut:
			brush_commit_remove(sel_children)

func process_copy() -> void:
	if Input.is_key_pressed(copy_key):
		if !copy_pressed:
			copy_pressed = true
			do_copy_or_cut(false)
	else:
		copy_pressed = false
	
	if Input.is_key_pressed(cut_key):
		if !cut_pressed:
			cut_pressed = true
			do_copy_or_cut(true)
	else:
		cut_pressed = false

## sel scene

var last_sel_scene : CanvasItem = null
var last_sel_packed : PackedScene = null
var last_sel_path : String = ""

func clear_sel_scene() -> void:
	if last_sel_scene != null:
		last_sel_scene.queue_free()
	last_sel_packed = null
	last_sel_scene = null
	last_sel_path = ""

func get_sel_path() -> String:
	var path := EditorInterface.get_selected_paths();
	for p in path:
		if p.ends_with(".tscn") or p.ends_with(".scn") or p.ends_with(".res"):
			return p
	return ""

func get_sel_scene() -> CanvasItem:
	var pstr := get_sel_path()
	if pstr == "":
		clear_sel_scene()
		return null

	if pstr == last_sel_path:
		return last_sel_scene

	if pstr == EditorInterface.get_edited_scene_root().scene_file_path:
		clear_sel_scene()
		return null

	var res = load(pstr)

	if not (res is PackedScene):
		clear_sel_scene()
		return null

	var node = res.instantiate()
	if node is CanvasItem:
		last_sel_scene = node
		last_sel_packed = res
		last_sel_path = pstr
		return node
	else:
		node.queue_free()
		clear_sel_scene()
		return null

func get_sel_packed() -> PackedScene:
	get_sel_scene()
	return last_sel_packed

## placement

func get_placement_size() -> Vector2:
	if is_erase:
		return grid
	if copy_data != null:
		return copy_data.rect.size
	else:
		var sel = get_sel_scene()
		if sel != null:
			return get_scene_rect(sel).size
		else:
			return default_rect.size

func set_scene_place_pos(node :CanvasItem, pos :Vector2) -> void:
	node.position = pos - get_scene_rect(node).position + get_scene_offset(node)

func set_list_node_place_pos(node :CanvasItem, first_pos : Vector2, topleft : Vector2, pos :Vector2) -> void:
	node.position += pos - first_pos - topleft + get_scene_offset(node)

func set_copy_list_place_pos(list :CopyList, pos :Vector2) -> void:
	var first_pos := list.first_pos
	var topleft := list.rect.position
	for n in list.nodes:
		set_list_node_place_pos(n, first_pos, topleft, pos)

# 0: paint, 1: line, 2: rectangle, -1: error
var place_mode :int
var place_array : Array[Vector2] = []
var place_origin = null

func process_placement(size : Vector2) -> void:
	if place_origin == null:
		place_origin = grid_pos
		place_array.clear()
		collected_continus_set.clear()

	match place_mode:
		0:
			collect_continous_grid_pos(size, place_origin, grid_pos, last_grid_pos)
			place_array = build_continous_grid_pos(size, place_origin)
		1:
			place_array = get_line_pos(size, place_origin, grid_pos)
		2:
			place_array = get_rect_pos(size, place_origin, grid_pos)

var is_erase := false
var is_placing := false

func submit_placement(size : Vector2) -> void:
	if !is_placing: return

	is_placing = false
	place_origin = null

	var result = place_array
	if is_erase:
		var del := []
		for c in get_children():
			if c is not CanvasItem: 
				continue
			var rect = get_scene_rect(c)
			rect.position += c.position
			for r in result:
				if Rect2(r, size).intersects(rect):
					del.append(c)
					break
		brush_commit_remove(del)
	else:
		var add := []
		for r in result:
			if copy_data != null:
				var new = duplicate_copy_list(copy_data)
				set_copy_list_place_pos(new, r)
				add.append_array(new.nodes)
			else:
				var res := get_sel_packed()
				if res != null:
					var node = res.instantiate(PackedScene.GEN_EDIT_STATE_INSTANCE)
					set_scene_place_pos(node, r)
					add.append(node)
		brush_commit_add(add)

	is_erase = false
	place_array.clear()

func do_placement() -> void:
	if !is_placing:
		var place = Input.is_mouse_button_pressed(PAINT_BUTTON)
		var erase = Input.is_mouse_button_pressed(ERASE_BUTTON)
		if place || erase:
			is_erase = erase
			is_placing = true

	if is_placing:
		var size := get_placement_size()
		process_placement(size)
		
		if (is_erase && !Input.is_mouse_button_pressed(ERASE_BUTTON)) || \
			(!is_erase && !Input.is_mouse_button_pressed(PAINT_BUTTON)):
			submit_placement(size)

## preview

var preview :bool = true
var preview_alpha :float = 0.5
var preview_border :bool = true
var border_width :float = 2
var border_color :Color = Color(0.9,0.4,0.3,0.7)
var paint_color :Color = Color(1.0,1.0,1.0,0.2)
var erase_color :Color = Color(0.0,0.0,0.0,0.4)

class Brush2DPreviewNode:
	extends Node2D

	@onready var brush = get_parent()

	var last_sel_data = null
	var last_copy_list = []

	func _init() -> void:
		z_index = RenderingServer.CANVAS_ITEM_Z_MAX

	func clear_children() -> void:
		for c in get_children():
			c.queue_free()

	func update_sel_data() -> void:
		if brush.copy_data != null:
			if typeof(last_sel_data) == TYPE_STRING || last_sel_data != brush.copy_data:
				last_sel_data = brush.copy_data
				clear_children()
				var dup = brush.duplicate_copy_list(brush.copy_data)
				last_copy_list = dup.nodes
				for n in dup.nodes:
					add_child(n)
		elif typeof(last_sel_data) != TYPE_STRING || last_sel_data != brush.last_sel_path:
			last_sel_data = brush.last_sel_path
			clear_children()
			if brush.last_sel_path != "":
				var dup = brush.get_sel_packed().instantiate()
				add_child(dup)

	func _process(_delta) -> void:
		if !brush.is_active || brush.is_erase || !brush.preview:
			hide()
			if last_sel_data != null:
				last_sel_data = null
				clear_children()
			return

		modulate = Color(1, 1, 1, brush.preview_alpha)
		update_sel_data()
		show()
		var pos = brush.grid_pos
		if last_sel_data is CopyList:
			for i in last_copy_list.size():
				last_copy_list[i].position = last_sel_data.nodes[i].position \
					+ pos - last_sel_data.first_pos - last_sel_data.rect.position \
					+ brush.get_scene_offset(last_sel_data.nodes[i])
		else:
			for c in get_children():
				if c is not CanvasItem:
					continue
				brush.set_scene_place_pos(c, pos)

class Brush2DBlockDraw:
	extends Node2D

	@onready var brush = get_parent()

	func _init() -> void:
		z_index = RenderingServer.CANVAS_ITEM_Z_MAX

	func _draw() ->void:
		if !brush.is_active:
			return

		var pos = brush.grid_pos
		var size = brush.get_placement_size()
		if !brush.is_placing:
			if brush.preview_border:
				var rect = Rect2(pos, size)
				rect.position -= brush.border_width * Vector2.ONE
				rect.size += brush.border_width * Vector2.ONE
				draw_rect(rect, brush.border_color, false, brush.border_width)
			return

		var color = brush.erase_color if brush.is_erase else brush.paint_color
		for p in brush.place_array:
			draw_rect(Rect2(p, size), color, true)

	func _process(_delta: float) -> void:
		queue_redraw()

const PREVIEW_NODE = "_Brush2DPreviewNode"
const PREVIEW_BLOCK = "_Brush2DBlockDraw"

func _enter_tree() -> void:
	if !has_node(PREVIEW_BLOCK):
		var n = Brush2DBlockDraw.new()
		n.name = PREVIEW_BLOCK
		add_child(n, false, INTERNAL_MODE_FRONT)
	if !has_node(PREVIEW_NODE):
		var n = Brush2DPreviewNode.new()
		n.name = PREVIEW_NODE
		add_child(n, false, INTERNAL_MODE_FRONT)

## plugin access

var is_active := false

func brush_clear() -> void:
	if is_active:
		submit_placement(get_placement_size())
	is_active = false
	first_grid = false
	clear_sel_scene()

func brush_process() -> void:
	is_active = true
	update_grid_pos()
	do_placement()
