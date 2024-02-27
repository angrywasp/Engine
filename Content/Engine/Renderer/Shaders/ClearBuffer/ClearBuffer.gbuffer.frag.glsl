/* Metadata
#include gbuffer GBuffer.glsl
*/

#include gbuffer

void main()
{
    rt_GAlbedo = vec4(0);
    rt_GNormal = vec4(0.5, 0.5, 0, 0);
    rt_GPbr = vec4(0);
    rt_GDepth = vec4(1, vec3(0));
}
