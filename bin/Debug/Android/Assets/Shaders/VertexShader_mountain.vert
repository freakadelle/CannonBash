attribute vec3 fuVertex;
attribute vec3 fuNormal;
attribute vec2 fuUV;
uniform mat4 FUSEE_MVP;
uniform mat4 FUSEE_MV;
uniform mat4 FUSEE_ITMV;
varying vec3 normal;
varying vec3 viewpos;
varying vec2 uv;
varying vec3 modelpos;
uniform vec2 minMaxHeight;
uniform float alpha;
uniform float waveHeight;
uniform float waveRoughnessX;
uniform float waveRoughnessY;

void main()
{
	normal = normalize(mat3(FUSEE_ITMV) * fuNormal);
	normal = normalize(mat3(FUSEE_MV) * fuNormal);
	viewpos = (FUSEE_MV * vec4(fuVertex, 1.0)).xyz;
	uv = fuUV;

	float normalizedHeight = (fuVertex.y - minMaxHeight.x)/(minMaxHeight.y - minMaxHeight.x);
	float waterLevel = 0.22;

	//Water animation
	if(normalizedHeight < waterLevel) {
		float waveAmplitudeSin = (sin(((fuVertex.x + fuVertex.y) + alpha) * waveRoughnessX) - 1.0) * waveHeight;
		float waveAmplitudeCos = (cos(((fuVertex.x + fuVertex.y) - alpha) * waveRoughnessY) - 1.0) * waveHeight;
		float waveAmplifier = ((waveAmplitudeSin + waveAmplitudeCos) * (normalizedHeight - waterLevel));
		modelpos = vec3(fuVertex.x + waveAmplifier * -2.0, (fuVertex.y) + waveAmplifier * 0.75, fuVertex.z - waveAmplifier * -2.0);
	} else {
		modelpos = vec3(fuVertex.x, fuVertex.y, fuVertex.z);
	}

	gl_Position = FUSEE_MVP * vec4(modelpos, 1.0);
}


