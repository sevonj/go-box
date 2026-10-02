shader_type spatial;

uniform sampler2D albedo : source_color, filter_nearest;
uniform vec3 modulate : source_color;

float randf(float value, float seed) {
	return fract(seed + sin(value) * 3.521231111113211111);
}


void vertex() {
	const float VERTEX_SCALE = 0.25;
	float rando = (VERTEX.x + VERTEX.y + VERTEX.z) * floor(TIME * 13.);
	if (fract(randf(rando, 1234.56789) * 41.0) < 0.001) {
		VERTEX.x += (randf(rando, 0.3330) - 0.5) * VERTEX_SCALE;
		VERTEX.y += (randf(rando, 0.2232) - 0.5) * VERTEX_SCALE;
		VERTEX.z += (randf(rando, 0.4182) - 0.5) * VERTEX_SCALE;
	}
	
	if (floor(randf(floor(TIME * 4.), 1234.56789) * 61.0) < 1.0) {
		VERTEX += sin(floor(TIME * 4.) - atan(VERTEX.y, VERTEX.x)) * 0.05 + sin(TIME) * 0.02;
	}
}

void fragment() {
	float scale = 100.0;
	float big_scale = 8.0;
	
	int row = int(UV.y * scale);
	int column = int(UV.x * scale);
	int big_row = int(UV.y * big_scale);
	int big_column = int(UV.x * big_scale);
	
	bool enable = (row + column) % 2 == 0;
	enable = enable && (row * column + int(TIME * 4.0)) % 6 > 0;
	enable = enable && (row / 4 * column / 4 + int(TIME * 1.0)) % 2 > 0;
	
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
	
	vec3 col = vec3(randf(rando, 42069.512), randf(rando, 6.9512), randf(rando, -.22643));
	vec2 uv = UV;
	if (layer3) {
		uv = vec2(randf(uv.x, 3.333) * 0.2, randf(uv.y * 0.4, 6.9512));
	}
	if (layer2) col = vec3(randf(rando * TIME, 3.333) * 0.2, randf(rando * (TIME) * 0.4, 6.9512), 0.) * sin(TIME * float(column * row));
	if (!enable) col = texture(albedo, uv).rgb * modulate;
	ALBEDO = col;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
