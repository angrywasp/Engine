// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed class GraphicsAdapter
    {
        private DisplayModeCollection _supportedDisplayModes;

        private static GraphicsAdapter defaultAdapter = new GraphicsAdapter();

        int _displayIndex;

        public DisplayMode CurrentDisplayMode
        {
            get
            {
                var displayIndex = Sdl.Display.GetWindowDisplayIndex(SdlGameWindow.Instance.Handle);

                Sdl.Display.Mode mode;
                Sdl.Display.GetCurrentDisplayMode(displayIndex, out mode);

                return new DisplayMode(mode.Width, mode.Height, SurfaceFormat.Rgba);
            }
        }

        public static GraphicsAdapter DefaultAdapter => defaultAdapter;

        public DisplayModeCollection SupportedDisplayModes
        {
            get
            {
                bool displayChanged = false;
                var displayIndex = Sdl.Display.GetWindowDisplayIndex (SdlGameWindow.Instance.Handle);
                displayChanged = displayIndex != _displayIndex;
                if (_supportedDisplayModes == null || displayChanged)
                {
                    var modes = new List<DisplayMode>(new[] { CurrentDisplayMode, });

                    _displayIndex = displayIndex;
                    modes.Clear();
                    
                    var modeCount = Sdl.Display.GetNumDisplayModes(displayIndex);

                    for (int i = 0;i < modeCount;i++)
                    {
                        Sdl.Display.Mode mode;
                        Sdl.Display.GetDisplayMode(displayIndex, i, out mode);
                        //var formatName = Sdl.Display.GetPixelFormatName(mode.Format);
                        //Odyssey G9 only has "SDL_PIXELFORMAT_RGB888" as the format, so just default to Rgba for everything

                        var displayMode = new DisplayMode(mode.Width, mode.Height, SurfaceFormat.Rgba);
                        if (!modes.Contains(displayMode))
                            modes.Add(displayMode);
                    }

                    modes.Sort(delegate(DisplayMode a, DisplayMode b)
                    {
                        if (a == b) return 0;
                        if (a.Format <= b.Format && a.Width <= b.Width && a.Height <= b.Height) return -1;
                        else return 1;
                    });
                    _supportedDisplayModes = new DisplayModeCollection(modes);
                }

                return _supportedDisplayModes;
            }
        }

        public bool IsWideScreen
        {
            get
            {
                const float limit = 4.0f / 3.0f;
                var aspect = CurrentDisplayMode.AspectRatio;
                return aspect > limit;
            }
        }
    }
}
