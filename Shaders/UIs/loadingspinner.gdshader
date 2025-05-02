shader_type canvas_item;

uniform vec2 radial_center = vec2(0.5, 0.5);

void fragment() {
    vec2 uv = UV;
	vec2 delta = uv - radial_center;
	
	float angle = atan(delta.y, delta.x);
	float animated_angle = mod(angle + TIME * -10.0, 2.0 * PI);
	float normalized_angle = animated_angle / (2.0 * PI);
	
	normalized_angle *= length(delta) < 0.5 && length(delta) > 0.42 ? 1.0 : 0.0;
	
	COLOR = vec4(vec3(1.0), normalized_angle);
}
