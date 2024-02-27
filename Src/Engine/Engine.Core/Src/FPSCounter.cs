using Microsoft.Xna.Framework;
using System;

namespace Engine
{
    public static class FPSCounter
    {
        private static TimeSpan lastFrameReportTime = TimeSpan.Zero;
        private static int frameCount = 0;

        private static float framesPerSecond = 0;

        public static float FramePerSecond => framesPerSecond;

        public static void Update(GameTime gameTime)
        {
            lastFrameReportTime += gameTime.ElapsedGameTime;
            frameCount++;

            if (lastFrameReportTime.TotalSeconds >= 1.0f)
            {
                framesPerSecond = frameCount;
                frameCount = 0;
                lastFrameReportTime = TimeSpan.Zero;
            }
        }
    }
}
