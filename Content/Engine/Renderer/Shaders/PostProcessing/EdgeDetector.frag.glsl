/* Metadata
#sampler RenderTarget Linear Wrap
*/

uniform vec2 halfPixel;

uniform sampler2D RenderTarget;

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec2 ps_TexCoord;

layout(location = 0) out vec4 result;

float threshold(float thr1, float thr2, float val) {
    if(val < thr1)
        return 0.0;

    if(val > thr2)
        return 1.0;

    return val;
}

// averaged pixel intensity from 3 color channels
float avgIntensity(vec4 pix) {
    return (pix.r + pix.g + pix.b) / 3.0;
}

vec4 getPixel(float dx, float dy) {
    return texture(RenderTarget, ps_TexCoord + vec2(dx, dy));
}

// returns pixel color
float isEdge() {
    float dxtex = halfPixel.x;
    float dytex = halfPixel.y;
    float pix[9];
    int k = -1;
    float delta;

	// read neighboring pixel intensities
    for(int i = -1; i < 2; i++) {
        for(int j = -1; j < 2; j++) {
            k++;
            pix[k] = avgIntensity(getPixel(float(i) * dxtex, float(j) * dytex));
        }
    }

	// average color differences around neighboring pixels
    delta = (abs(pix[1] - pix[7]) +
        abs(pix[5] - pix[3]) +
        abs(pix[0] - pix[8]) +
        abs(pix[2] - pix[6])) / 4.0;

    return threshold(0.25, 0.4, clamp(1.8 * delta, 0.0, 1.0));
}

void main() {
    result = vec4(0.0, 0.0, 0.0, 1.0);
    result.g = isEdge();
}
