shader_type spatial;
render_mode unshaded, cull_front;

uniform vec4 fg_color : source_color = vec4(1.0);
uniform vec4 bg_color : source_color = vec4(1.0);

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec2 uv = UV * vec2(4., 2.);
	uv.y -= 1.;
	float arrow_offset = abs(uv.y);
	uv.x += arrow_offset;
	uv.x = mod(uv.x, 4.);
	
	float bg = floor(fract(uv.x + 0.1) * 2.);
	float bga = (1.0 - smoothstep(0.5, 1., arrow_offset)) * bg;
	vec3 bgcol = vec3(bg);
	
	float fg_width = 0.15;
	
	vec2 fguv = uv * vec2(1., .55);
	bool fg_visible = floor(uv.x) == floor(mod(TIME * 3.25, 4.));
	fguv.x = fract(fguv.x);
	fguv.x -= .5;
    vec2 corner = vec2(0.5 - fg_width);
	vec2 offset = max(abs(fguv) - corner, 0.0);
    
	float fga = 1.0 - abs(1.0 - smoothstep(0.0, fg_width, length(offset)) * 2.0);
	if (!fg_visible) fga = 0.0;
	
	ALBEDO = mix(bg_color, fg_color, fga).rgb;
	bga *= bg_color.a;
	fga *= fg_color.a;
	ALPHA = clamp(fga + bga, 0.0, 1.0);
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
