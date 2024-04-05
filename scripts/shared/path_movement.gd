extends PathFollow2D


@export var speed = 50


func _process(delta: float) -> void:
	progress += speed * delta
