using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Engine.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Graphics.Effects
{
    public static class EffectHelper
    {
        public static void LoadTexture(GraphicsDevice graphicsDevice, string path, EffectParameter param)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (param == null)
                        return;

                    if (string.IsNullOrEmpty(path))
                    {
                        param.SetValue((Texture2D)null);
                        return;
                    }

                    var tex = await ContentLoader.LoadTextureAsync(graphicsDevice, path).ConfigureAwait(false);
                    param.SetValue(tex);
                }
                catch (Exception ex)
                {
                    Debugger.Break();
                }
            }).ConfigureAwait(false);
        }

        public static Texture2D LoadTexture(Texture2D tex, EffectParameter param)
        {
            try
            {
                if (param == null)
                    return tex;

                param.SetValue(tex);
                return tex;
            }
            catch (Exception ex)
            {
                Debugger.Break();
                return null;
            }
        }

        public static void LoadTextureCube(GraphicsDevice graphicsDevice, string path, EffectParameter param)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (param == null)
                        return;

                    if (string.IsNullOrEmpty(path))
                    {
                        param.SetValue((TextureCube)null);
                        return;
                    }

                    var tex = await ContentLoader.LoadTextureCubeAsync(graphicsDevice, path).ConfigureAwait(false);
                    param.SetValue(tex);
                }
                catch (Exception ex)
                {
                    Debugger.Break();
                }
            }).ConfigureAwait(false);
        }

        public static TextureCube LoadTexture(TextureCube tex, EffectParameter param)
        {
            if (param == null)
                return tex;

            param.SetValue(tex);
            return tex;
        }
    }
}