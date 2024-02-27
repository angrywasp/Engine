// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
    public class GraphicsDeviceInformation
    {	
        public GraphicsAdapter Adapter { get; set; }

        public PresentationParameters PresentationParameters { get; set; }
    }
}

