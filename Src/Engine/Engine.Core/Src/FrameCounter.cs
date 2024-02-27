using System.Diagnostics;
using System.Runtime.CompilerServices;
using Engine.UI;
using Microsoft.Xna.Framework;

namespace Engine
{
    public class FrameCounter
    {
        private readonly Stopwatch sw = new Stopwatch();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start() => sw.Restart();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stop() => sw.Stop();

        public void Report(Interface ui, int index, string label) => ui.ScreenMessages.WriteStaticText(index, $"{label}: {sw.ElapsedMilliseconds}ms", Color.White);
    }
}