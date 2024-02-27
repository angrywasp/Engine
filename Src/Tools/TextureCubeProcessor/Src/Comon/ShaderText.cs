namespace TextureCubeProcessor.Common {
public static class ShaderText {
public const string VertexShaderCube =

@"/* Metadata
#attribute vs_Position Position 0
#attribute vs_TexCoord TextureCoordinate 0
*/

uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

layout(location = 0) in vec4 vs_Position;
layout(location = 1) in vec3 vs_TexCoord;

layout(location = 0) out vec4 ps_Position;
layout(location = 1) out vec3 ps_TexCoord;

void main()
{
	mat4 worldView = (View * World);
	mat4 worldViewProjection = (Projection * worldView);

	gl_Position = worldViewProjection * vs_Position;
	gl_Position.y = -gl_Position.y;

	vec4 VertexPosition = World * vs_Position;
	ps_TexCoord = VertexPosition.rgb;
}";

public const string VertexShaderQuad =

@"/* Metadata
#attribute vs_Position Position 0
#attribute vs_TexCoord TextureCoordinate 0
*/

layout(location = 0) in vec4 vs_Position;
layout(location = 1) in vec2 vs_TexCoord;

layout(location = 0) out vec4 ps_Position;
layout(location = 1) out vec2 ps_TexCoord;

void main() {
    gl_Position = vs_Position;
    gl_Position.y = -gl_Position.y;

    ps_Position = vs_Position;
    ps_TexCoord = vs_TexCoord;
}";

}}