/* Metadata
#attribute vs_Corner Position 0
#attribute vs_Position Position 1
#attribute vs_Velocity Normal 0
#attribute vs_Random Color 0
#attribute vs_Time TextureCoordinate 0
*/

uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

uniform vec2 ViewportScale;
uniform float CurrentTime;
uniform float Duration;
uniform float DurationRandomness;
uniform vec3 Gravity;
uniform float EndVelocity;
uniform vec4 MinColor;
uniform vec4 MaxColor;
uniform vec2 RotateSpeed;
uniform vec2 StartSize;
uniform vec2 EndSize;

layout(location = 0) in vec2 vs_Corner;
layout(location = 1) in vec3 vs_Position;
layout(location = 2) in vec3 vs_Velocity;
layout(location = 3) in vec4 vs_Random;
layout(location = 4) in float vs_Time;

layout(location = 0) out vec4 ps_Position;
layout(location = 1) out vec2 ps_TextureCoordinate;
layout(location = 2) out vec4 ps_Color;

vec4 calcPosition(vec3 position, vec3 velocity, float age, float normalizedAge) {
    float startVelocity = length(velocity);
    float endVelocity = startVelocity * EndVelocity;

    float velocityIntegral = startVelocity * normalizedAge + (endVelocity - startVelocity) * normalizedAge * normalizedAge / 2.0;

    position += normalize(velocity) * velocityIntegral * Duration;

    // Apply the gravitational force.
    position += Gravity * age * normalizedAge;

    mat4 worldView = (View * World);
    mat4 worldViewProjection = (Projection * worldView);

    vec4 retVal = worldViewProjection * vec4(position, 1.0);
    retVal.y = -retVal.y;
    return retVal;
}

float calcSize(float randomValue, float normalizedAge) {
    // Apply a random factor to make each particle a slightly different size.
    float startSize = mix(StartSize.x, StartSize.y, randomValue);
    float endSize = mix(EndSize.x, EndSize.y, randomValue);
    float size = mix(startSize, endSize, normalizedAge);

    return size * Projection[1][1];
}

mat2 calcRotation(float randomValue, float age) {    
    // Apply a random factor to make each particle rotate at a different speed.
    float rotateSpeed = mix(RotateSpeed.x, RotateSpeed.y, randomValue);

    float rotation = rotateSpeed * age;

    // Compute a 2x2 rotation matrix.
    float c = cos(rotation);
    float s = sin(rotation);

    return mat2(c, -s, s, c);
}

vec4 calcColor(vec4 projectedPosition, float randomValue, float normalizedAge) {
    vec4 color = mix(MinColor, MaxColor, randomValue);
    color.a *= normalizedAge * (1.0 - normalizedAge) * (1.0 - normalizedAge) * 6.7;
    return color;
}

void main() {
    // Compute the age of the particle.
    float age = CurrentTime - vs_Time;

    // Apply a random factor to make different particles age at different rates.
    age *= 1.0 + vs_Random.x * DurationRandomness;

    // Normalize the age into the range zero to one.
    float normalizedAge = clamp(age / Duration, 0.0, 1.0);// saturate(age / Duration);

    // Compute the particle position, size, color, and rotation.
    ps_Position = calcPosition(vs_Position, vs_Velocity, age, normalizedAge);

    float size = calcSize(vs_Random.y, normalizedAge);
    mat2 rotation = calcRotation(vs_Random.w, age);

    ps_Position.xy += (vs_Corner * rotation) * size * ViewportScale;// mul(Corner, rotation) * size * ViewportScale;

    ps_Color = calcColor(ps_Position, vs_Random.z, normalizedAge);
    ps_TextureCoordinate = (vs_Corner + 1.0) / 2.0;

    gl_Position = ps_Position;
}
