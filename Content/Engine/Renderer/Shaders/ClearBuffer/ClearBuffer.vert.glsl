/* Metadata
#attribute vs_Position Position 0
*/

layout(location = 0) in vec3 vs_Position;

void main() {
    gl_Position = vec4(vs_Position, 1);
}
