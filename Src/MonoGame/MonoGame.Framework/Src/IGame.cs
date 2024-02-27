using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
    public interface IGame
    {
        string WindowTitle { get; }
        SdlGamePlatform Platform { get; }
        GraphicsDeviceManager GraphicsDeviceManager { get; }
        GraphicsDevice GraphicsDevice { get; }
        void Tick();
    }
}