using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

//todo: game pads
namespace Engine.Input
{
    public class InputDeviceManager
    {
        private KeyboardExtensions keyboard;
        private MouseExtensions mouse;

        public MouseExtensions Mouse => mouse;

        public KeyboardExtensions Keyboard => keyboard;
            
        public InputDeviceManager(GraphicsDevice graphicsDevice)
        {
            keyboard = new KeyboardExtensions(graphicsDevice);
            mouse = new MouseExtensions(graphicsDevice);
        }

        public void Update(GameTime gameTime)
        {
            keyboard.Update(gameTime);
            mouse.Update(gameTime);
        }
    }
}
