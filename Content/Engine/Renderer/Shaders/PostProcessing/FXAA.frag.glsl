/* Metadata
#include fxaa Fxaa3_11.h
#sampler RenderTarget Linear Wrap
*/

#define FXAA_PC 1
#define FXAA_GLSL_130 1
#define FXAA_FAST_PIXEL_OFFSET 1
#define FXAA_GATHER4_ALPHA 1
#define FXAA_EARLY_EXIT 1
#define FXAA_DISCARD 0

#define FXAA_QUALITY__PRESET 39
#define FXAA_GREEN_AS_LUMA 0

uniform float FxaaQualitySubpix = 0.75;
uniform float FxaaQualityEdgeThreshold = 0.125;
uniform float FxaaQualityEdgeThresholdMin = 0.0625;

uniform float ScreenWidth;
uniform float ScreenHeight;

uniform sampler2D RenderTarget;

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec2 ps_TexCoord;

layout(location = 0) out vec4 result;

#include fxaa

void main() {
    vec2 rcpFrame = vec2(1.0 / ScreenWidth, 1.0 / ScreenHeight);
    vec2 pixelCenter = vec2(ps_TexCoord.x - rcpFrame.x, ps_TexCoord.y - rcpFrame.y);

    result = FxaaPixelShader(pixelCenter, RenderTarget, rcpFrame, FxaaQualitySubpix, FxaaQualityEdgeThreshold, FxaaQualityEdgeThresholdMin);
}
