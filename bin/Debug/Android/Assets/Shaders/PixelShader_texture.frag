#ifdef GL_ES
    precision highp float;
#endif
varying vec3 viewpos;
varying vec3 normal;
varying vec2 uv;
uniform vec3 albedo;
uniform sampler2D texture;
uniform float texmix;
varying vec3 modelpos;


void main()
{
	vec3 resultingAlbedo = vec3(texture2D(texture, uv));

    gl_FragColor = vec4(resultingAlbedo, 1);
}
