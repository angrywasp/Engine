using System.Numerics;
using Engine;
using Engine.UI;
using Engine.UI.Controls;
using Microsoft.Xna.Framework;

namespace GameDemo.UI.Intro
{
    public class DevLogoForm : IntroForm
    {
        public DevLogoForm(EngineCore engine, IntroLoader il) : base(engine, il) { }

        protected override void ConstructLayout()
        {
            MenuHelper.CreateImageSkinElement(engine, "DemoContent/Textures/Background.texture", "menuBackground");
            MenuHelper.CreateImageSkinElement(engine, "DemoContent/Textures/DevLogoFull_White.texture", "logoForeground");

            this.Control(Vector2.Zero, Vector2.One, skinElement: "menuBackground");
            this.Control(Vector2.Zero, Vector2.One, skinElement: "logoForeground");

            ctrlFader = this.Control(Vector2.Zero, Vector2.One, skinElement: "UiControl");
        }
    }
}
