using EMU7800.Core;
using System;
using System.Diagnostics;

namespace AtariEmulator.Renderers
{
    public abstract class FrameRendererBase
    {
        protected int firstScanline;
        protected int width;
        protected int height;
        protected int bufferElementsPerScanline;
        protected FrameBuffer frameBuffer;
        protected int[] palette;
        protected byte[] frameData;

        protected int startSourceIndex;
        protected int endSourceIndex;

        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }

        public FrameBuffer FrameBuffer
        {
            get { return frameBuffer; }
        }

        public byte[] FrameData
        {
            get { return frameData; }
        }

        public virtual void Create(MachineType machineType, MachineBase machine)
        {
            switch (machineType)
            {
                case MachineType.A2600NTSC:
                case MachineType.A2600PAL:
                    {
                        width = 160 << 1;
                        height = 230;
                        bufferElementsPerScanline = width >> 1 >> BufferElement.SHIFT;
                    }
                    break;
                case MachineType.A7800NTSC:
                case MachineType.A7800PAL:
                    {
                        width = 320;
                        height = 230;
                        bufferElementsPerScanline = width >> BufferElement.SHIFT;
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            firstScanline = machine.FirstScanline;
            frameBuffer = machine.CreateFrameBuffer();
            palette = machine.Palette;
            startSourceIndex = firstScanline * bufferElementsPerScanline;
            endSourceIndex = startSourceIndex + bufferElementsPerScanline * height;
            frameData = new byte[width * height * 4];
        }

        public abstract void UpdateRenderTarget();
    }
}
