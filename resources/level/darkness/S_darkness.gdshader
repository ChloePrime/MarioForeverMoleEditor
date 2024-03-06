shader_type canvas_item;

uniform	float base_darkness = 1.0;

void fragment() {
	float alpha = base_darkness * (1.0 - texture(TEXTURE, UV).r);
	COLOR = mix(COLOR, vec4(0.0, 0.0, 0.0, alpha), COLOR.a);
}
