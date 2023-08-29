shader_type canvas_item;

uniform float rainbow_alpha = 0;
uniform float rainbow_max_power = 0.8;
uniform float rainbow_flash_speed = 5;
uniform float rainbow_saturation_scale = 1;
uniform float rainbow_brightness_buff = 0.2;

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
	vec3 hsv = rgb2hsv(orig_color.xyz);
	hsv.x = mod(hsv.x + TIME * rainbow_flash_speed, 1.0);
	hsv.y *= rainbow_saturation_scale;
	hsv.z = mix(hsv.z, 1, rainbow_alpha * rainbow_brightness_buff);
	COLOR.xyz = mix(orig_color.xyz, hsv2rgb(hsv), rainbow_alpha * rainbow_max_power);
	COLOR.a = orig_color.a;
}
