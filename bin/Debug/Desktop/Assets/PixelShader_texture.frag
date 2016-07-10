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
	//NormalizedHeight liest Pixel aus einzeiliger Textur.
	//vec3 resultingAlbedo = (1.0-texmix) * albedo + texmix * vec3(texture2D(texture, uv));
	vec3 resultingAlbedo = vec3(texture2D(texture, uv));

    gl_FragColor = vec4(resultingAlbedo, 1);
    //gl_FragColor = vec4(vec3(0,1,1), 1);
}
