// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Framework
{
    public static class FrameworkDispatcher
    {
        private static bool _initialized = false;

        public static void Update()
        {
            if (!_initialized)
                Initialize();

            DoUpdate();
        }

        private static void DoUpdate()
        {
            DynamicSoundEffectInstanceManager.UpdatePlayingInstances();
            SoundEffectInstancePool.Update();
            Microphone.UpdateMicrophones();
        }

        private static void Initialize()
        {
            _initialized = true;
        }
    }
}

