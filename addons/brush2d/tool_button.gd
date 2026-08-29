@tool
extends HBoxContainer

@onready var main :Button = $ToolButton
@onready var paint :Button = $Paint
@onready var rectangle :Button = $Rectangle
@onready var line :Button = $Line

func _ready() ->void:
	paint.pressed.connect(func():
		paint.button_pressed = true
		rectangle.button_pressed = false
		line.button_pressed = false
	)
	rectangle.pressed.connect(func():
		paint.button_pressed = false
		rectangle.button_pressed = true
		line.button_pressed = false
	)
	line.pressed.connect(func():
		paint.button_pressed = false
		rectangle.button_pressed = false
		line.button_pressed = true
	)

func get_mode() ->int:
	if paint.button_pressed:
		return 0
	if rectangle.button_pressed:
		return 2
	if line.button_pressed:
		return 1
	return -1

func _process(delta: float) -> void:
	paint.visible = main.button_pressed
	rectangle.visible = main.button_pressed
	line.visible = main.button_pressed
