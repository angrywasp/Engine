using Microsoft.Xna.Framework.Graphics;

namespace C64Emulator
{
    public class VideoOutput : C64Interfaces.IVideoOutput
    {
        private RenderTarget2D rt;
        private uint[] buffer;

        public RenderTarget2D RenderTarget => rt;

        public VideoOutput(RenderTarget2D rt)
        {
            this.rt = rt;
            buffer = new uint[rt.Width * rt.Height];

            for (int i = 0; i < Video.Pallete.CvtColors.Length; i++)
                Video.Pallete.CvtColors[i] = Video.Pallete.Colors[i];
        }

        public void Flush() => rt.SetData<uint>(0, buffer, 0, rt.Width * rt.Height);

        public void OutputPixel(uint pos, uint color) => buffer[pos] = color;
    }
}