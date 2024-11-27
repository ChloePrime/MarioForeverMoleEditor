// SuperSAI 2x by Kreed
// This is a gdshader version rewritten and modified by dasasdhba

shader_type canvas_item;

// convert RGB to YUV
const mat3 yuv_matrix = mat3(
    vec3(0.299, -0.169, 0.5),
    vec3(0.587, -0.331, -0.419),
    vec3(0.114, 0.5,    -0.081));
const vec3 yuv_threshold = vec3(48.0/255.0, 7.0/255.0, 6.0/255.0);
const vec3 yuv_offset = vec3(0, 0.5, 0.5);
const float alpha_threshold = 0.2;

bool like(vec4 c1, vec4 c2) {
    vec3 a = yuv_matrix * c1.rgb;
    vec3 b = yuv_matrix * c2.rgb;

    bvec3 res = greaterThan(abs((a + yuv_offset) - (b + yuv_offset)), yuv_threshold);
    bool alpha = abs(c1.a - c2.a) > alpha_threshold;
	return !(res.x || res.y || res.z || alpha);
}

float cond(vec4 a, vec4 b, vec4 c, vec4 d) {
    bool ac = like(a, c);
    bool ad = like(a, d);
    bool bc = like(b, c);
    bool bd = like(b, d);

    float x = float(ac) + float(ad);
    float y = float(bc && !ac) + float(bd && !ad);
    return float(x <= 1.0) - float(y <= 1.0);
}

void vertex() {
	VERTEX *= 2.0;
}

void fragment () {
	vec2 size = vec2(textureSize(TEXTURE, 0));
    vec2 unit = 1.0/size;

    // c0  c1  c2  d3
    // c3  c4  c5  d4
    // c6  c7  c8  d5
    // d0  d1  d2  d6
    // c4 is center point
    vec4 c0 = texture(TEXTURE, UV + vec2(-1.0, -1.0)*unit);
    vec4 c1 = texture(TEXTURE, UV + vec2( 0.0, -1.0)*unit);
    vec4 c2 = texture(TEXTURE, UV + vec2( 1.0, -1.0)*unit);
    vec4 d3 = texture(TEXTURE, UV + vec2( 2.0, -1.0)*unit);
    vec4 c3 = texture(TEXTURE, UV + vec2(-1.0,  0.0)*unit);
    vec4 c4 = texture(TEXTURE, UV + vec2( 0.0,  0.0)*unit);
    vec4 c5 = texture(TEXTURE, UV + vec2( 1.0,  0.0)*unit);
    vec4 d4 = texture(TEXTURE, UV + vec2( 2.0,  0.0)*unit);
    vec4 c6 = texture(TEXTURE, UV + vec2(-1.0,  1.0)*unit);
    vec4 c7 = texture(TEXTURE, UV + vec2( 0.0,  1.0)*unit);
    vec4 c8 = texture(TEXTURE, UV + vec2( 1.0,  1.0)*unit);
    vec4 d5 = texture(TEXTURE, UV + vec2( 2.0,  1.0)*unit);
    vec4 d0 = texture(TEXTURE, UV + vec2(-1.0,  2.0)*unit);
    vec4 d1 = texture(TEXTURE, UV + vec2( 0.0,  2.0)*unit);
    vec4 d2 = texture(TEXTURE, UV + vec2( 1.0,  2.0)*unit);
    vec4 d6 = texture(TEXTURE, UV + vec2( 2.0,  2.0)*unit);

    // e00 e01
    // e10 e11
    // the output four pixels
    vec4 e00 = c4;
    vec4 e01 = c4;
    vec4 e10;
    vec4 e11 = c4;

    // e01 e11
    if (like(c7, c5) && !like(c4, c8)) {
        vec4 c57 = mix(c7, c5, 0.5);
        e11 = c57;
        e01 = c57;
    } else if (like(c4, c8) && !like(c7, c5)) {
        // pass
    } else if (like(c4, c8) && like(c7, c5)) {
        vec4 c57 = mix(c7, c5, 0.5);
        vec4 c48 = mix(c4, c8, 0.5);

        float conc = 0.0;
        conc += cond(c57, c48, c6, d1);
        conc += cond(c57, c48, c3, c1);
        conc += cond(c57, c48, d2, d5);
        conc += cond(c57, c48, c2, d4);

        if (conc > 0.0) {
            e11 = c57;
            e01 = c57;
        } else if (conc == 0.0) {
            e11 = mix(c48, c57, 0.5);
            e01 = e11;
        }
    } else {
        if (like(c8, c5) && like(c8, d1) && !like(c7, d2) && !like(c8, d0)) {
            e11 = mix((c8+c5+d1)/3.0, c7, 0.75);
        } else if (like(c7, c4) && like(c7, d2) && !like(c7, d6) && !like(c8, d1)) {
            e11 = mix((c7+c4+d2)/3.0, c8, 0.75);
        } else {
            e11 = mix(c7, c8, 0.5);
        }

        if (like(c5, c8) && like(c5, c1) && !like(c5, c0) && !like(c4, c2)) {
            e01 = mix((c5+c8+c1)/3.0, c4, 0.75);
        } else if (like(c4, c7) && like(c4, c2) && !like(c5, c1) && !like(c4, d3)) {
            e01 = mix((c4+c7+c2)/3.0, c5, 0.75);
        } else {
            e01 = mix(c4, c5, 0.5);
        }
    }

    // e10
    if (like(c4, c8) && like(c4, c3) && !like(c7, c5) && !like(c4, d2)) {
        e10 = mix(c7, (c4+c8+c3)/3.0, 0.5);
    } else if (like(c4, c6) && like(c4, c5) && !like(c7, c3) && !like(c4 ,d0)) {
        e10 = mix(c7, (c4+c6+c5)/3.0, 0.5);
    } else {
        e10 = c7;
    }

    // e00
    if (like(c7, c5) && like(c7, c6) && !like(c4, c8) && !like(c7, c2)) {
        e00 = mix((c7+c5+c6)/3.0, c4, 0.5);
    } else if (like(c7, c3) && like(c7, c8) && !like(c4, c6) && !like(c7, c0)) {
        e00 = mix((c7+c3+c8)/3.0, c4, 0.5);
    }

    // mix result
    vec2 pos = floor(fract(UV * size) * 2.0);
    COLOR = mix(
		mix(e00, e01, pos.x),
		mix(e10, e11, pos.x),
		pos.y
	);
}
