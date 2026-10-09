shader_type spatial;
render_mode unshaded, cull_disabled;

float randf(float value, float seed) {
	return fract(seed + sin(value) * 3.521231111113211111);
}

void vertex() {
	UV.x = length(VERTEX.xz) * .1;
	if (VERTEX.x < 0.) UV.x *= -1.;
	if (VERTEX.z < 0.) UV.x *= -1.;
	UV2.x = UV.x / abs(VERTEX.y) * 4.0 + TIME * 2.;
	UV2.y = UV.y;
}

void fragment() {
	vec2 uv = UV;
	
	uv.y += sin(TIME + uv.x * 10.) * 0.1;
	uv.y += sin(TIME + uv.x * 3.) * 0.1;
	uv.y += sin(TIME * 2.5 + uv.x * 5.) * 0.05;
	uv.y += sin(TIME * 2.7 + uv.x * 7.) * 0.05;
	uv.y += sin(TIME * 2.1 + uv.x * 11.) * 0.05;
	uv.y += sin(TIME * 2.3 + uv.x * 13.) * 0.05;
	uv.y += sin(TIME * 0.9 + uv.x * 19.) * 0.05;
	uv.y += sin(TIME * 1.7 + uv.x * 37.) * 0.05;
	uv.y += sin(TIME * -27. + uv.x * 73.) * 0.05;
	uv.y += sin(TIME * 50. + uv.x * 23.) * 0.03;
	
	uv.y -= 0.5;
	uv.y *= -20.0;
	uv.y = 1.0 - abs(uv.y);
	float hue = TIME * 0.01 - uv.x * .5;
	vec3 col = abs(mod(hue * 6.0 + vec3(0.0, 4.0, 2.0), 6.0) - 3.0) - 1.0;
	
	float scale = 100.0;
	float big_scale = 16.0;
	
	int row = int(UV2.y * scale);
	int column = int(UV2.x * scale);
	int big_row = int(UV2.y * big_scale);
	int big_column = int(UV2.x * big_scale);
	
	bool enable = (row + column) % 2 == 0;
	enable = enable && (row * column + int(TIME * 4.0)) % 6 > 0;
	enable = enable && (row / 4 * column / 4 + int(TIME * 1.0 )) % 2 > 0;
	
	bool layer1 = (big_row + (big_column * 7) % 13 * int(TIME * 2.0)) % 9 < 4;
	layer1 = layer1 && ((big_row * 7) % 13 * int(TIME * 2.3)) % 9 < 4;
	enable = enable && layer1;
	
	bool layer2 = ((((big_row * 7) % 27 * int(TIME * 2.1)) % 9 < 4) && row % 3 + column % 4 < 2);
	layer2 = layer2 && (big_column + row) * int(sin(TIME) * 4.1) % 5 < 3;
	layer2 = layer2 && (big_column) * int(sin(TIME) * 7.) % 5 < 3;
	enable = enable || layer2;
	
	bool layer3 = (big_row + (big_column * 7) % 13 * int(TIME * 4.0)) % 9 < 4;
	layer3 = layer3 && ((big_row * 7) % 13 * int(TIME * 2.3)) % 9 < 4;
	
	float rando = float(row * column) + floor(TIME);
	
	vec3 art_col = vec3(randf(rando, 42069.512), randf(rando, 6.9512), randf(rando, -.22643));
	if (layer3) {
		uv = vec2(randf(uv.x, 3.333) * 0.2, randf(uv.y * 0.4, 6.9512));
		col = art_col;
	}
	if (layer2) {
		col = vec3(randf(rando * TIME, 3.333) * 0.2, randf(rando * (TIME) * 0.4, 6.9512), 0.) * sin(TIME * float(column * row));
		col = -art_col;
		}
	
	float a = clamp(uv.y, 0., 1.);
	a += clamp(uv.x, 0., 1.);
	ALBEDO = col * a;
	ALPHA = a;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
