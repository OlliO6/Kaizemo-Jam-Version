shader_type canvas_item;

uniform sampler2D mask;
uniform vec4 color : hint_color;

void fragment()
{
	COLOR = mix(texture(TEXTURE, UV), color, texture(mask, UV).r);
}