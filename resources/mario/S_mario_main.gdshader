shader_type canvas_item;

uniform float outline_width = 2;
uniform vec4 outline_color: source_color = vec4(0, 0, 0, 1);

uniform float rainbow_alpha = 0;
uniform float rainbow_max_power = 0.8;
uniform float rainbow_flash_speed = 5;
uniform float rainbow_saturation_scale = 1;
uniform float rainbow_brightness_buff = 0.2;

const float SQRT_0p5 = 0.70710678118;

vec3 rgb2hsv(vec3 c)
{
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

vec3 hsv2rgb(vec3 c)
{
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}


void fragment() {
	vec4 orig_color = texture(TEXTURE, UV);
	bool is_outline = false;
	if (outline_width > 0.0)
	{
		vec2 delta = outline_width * TEXTURE_PIXEL_SIZE;
		float px = texture(TEXTURE, UV + vec2(delta.x, 0.0)).a;
		float nx = texture(TEXTURE, UV - vec2(delta.x, 0.0)).a;
		float py = texture(TEXTURE, UV + vec2(0.0, delta.y)).a;
		float ny = texture(TEXTURE, UV - vec2(0.0, delta.y)).a;
		
		float pp = texture(TEXTURE, UV + delta * SQRT_0p5).a;
		float nn = texture(TEXTURE, UV - delta * SQRT_0p5).a;
		float pn = texture(TEXTURE, UV + SQRT_0p5 * vec2(delta.x, -delta.y)).a;
		float np = texture(TEXTURE, UV + SQRT_0p5 * vec2(-delta.x, delta.y)).a;
		float conv = (px + nx + py + ny + pp + nn + pn + np) / 8.0 - orig_color.a;
		if (conv > 0.0)
		{
			COLOR = outline_color;
			is_outline = true;
		}
	}
	if (!is_outline)
	{
		vec3 hsv = rgb2hsv(orig_color.xyz);
		hsv.x = mod(hsv.x + TIME * rainbow_flash_speed, 1.0);
		hsv.y *= rainbow_saturation_scale;
		hsv.z = mix(hsv.z, 1, rainbow_alpha * rainbow_brightness_buff);
		COLOR.xyz = mix(orig_color.xyz, hsv2rgb(hsv), rainbow_alpha * rainbow_max_power);
		COLOR.a = orig_color.a;	
	}
}
