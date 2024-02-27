using Engine;
using Engine.UI;
using Engine.UI.Controls;
using Microsoft.Xna.Framework;

namespace GameDemo.UI
{
    public static class MenuHelper
    {
        public static void CreateImageSkinElement(EngineCore engine, string texturePath, string name)
        {
            if (engine.Interface.Skin.Elements.ContainsKey(name))
                return;

            var skinElement = new SkinElement();

            var state = new ControlState
            {
                Background = new TextureRegion
                {
                    TexturePath = texturePath,
                    Color = Color.White
                }
            };

            skinElement.States.Add("Normal", state);
            skinElement.States.Add("Over", state);
            skinElement.States.Add("Active", state);
            skinElement.States.Add("Disabled", state);

            skinElement.Load(engine.Interface);

            engine.Interface.Skin.AddElement(skinElement, name);
        }

        public static void CreateHeadingTextSkinElement(EngineCore engine, string name, string fontPath, int fontSize, Color textColor)
        {
             if (engine.Interface.Skin.Elements.ContainsKey(name))
                return;

            var skinElement  = new SkinElement();

            var state = new TextBoxState
            {
                FontSize = fontSize,
                FontPath = fontPath,
                TextColor = textColor
            };

            skinElement.States.Add("Normal", state);
            skinElement.States.Add("Over", state);
            skinElement.States.Add("Active", state);
            skinElement.States.Add("Disabled", state);

            skinElement.Load(engine.Interface);

            engine.Interface.Skin.AddElement(skinElement, name);
        }
    }
}