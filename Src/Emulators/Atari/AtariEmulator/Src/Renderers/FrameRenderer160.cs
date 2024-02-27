
using EMU7800.Core;

namespace AtariEmulator.Renderers
{
    public class FrameRenderer160 : FrameRendererBase
    {
        public override void UpdateRenderTarget()
        {
            int di = 0;
            for (int si = startSourceIndex; si < endSourceIndex; si++)
            {
                var be = frameBuffer.VideoBuffer[si];
                for (var k = 0; k < BufferElement.SIZE; k++)
                {
                    var ci = be[k];
                    var nc = palette[ci];
                    var rn = (nc >> 16) & 0xff;
                    var gn = (nc >> 8) & 0xff;
                    var bn = (nc >> 0) & 0xff;
                    frameData[di++] = (byte)bn;
                    frameData[di++] = (byte)gn;
                    frameData[di++] = (byte)rn;
                    frameData[di++] = (byte)255;
                    frameData[di++] = (byte)bn;
                    frameData[di++] = (byte)gn;
                    frameData[di++] = (byte)rn;
                    frameData[di++] = (byte)255;
                }
            }
        }
    }
}
