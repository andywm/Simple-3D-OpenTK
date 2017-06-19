#version 330

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

in vec2 vPosition;

void main()
{
	gl_Position = vec4(vPosition, 0, 1) * uModel * uView * uProjection;
}