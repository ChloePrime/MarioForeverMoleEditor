shader_type canvas_item;

uniform vec4 tint_color: source_color = vec4(1, 0, 0, 1);
uniform float tint_alpha = 0.0;

void fragment() {
	vec4 orig_color = texture(TEXTURE, UV);
	COLOR = orig_color.a < 0.05 ? orig_color : mix(orig_color, tint_color, tint_alpha);
}
