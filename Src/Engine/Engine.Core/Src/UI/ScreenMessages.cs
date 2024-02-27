using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using AngryWasp.Logger;
using AngryWasp.Helpers;

namespace Engine.UI
{
    public class ScreenMessages
    {
        private const float MESSAGE_LIFE = 10000f;
        private Font font;
        private int lineSpace = 12;
        public List<ScreenMessageOutput> output = new List<ScreenMessageOutput>();
        public List<ScreenMessageOutput> staticText = new List<ScreenMessageOutput>();
        private Interface inter;

        private int maxStaticMessages = 25;

        public Font Font => font;

        public class ScreenMessageOutput
        {
            public string Message { get; set; }

            public Color Color { get; set; }

            public float LifeRemaining { get; set; }

            public ScreenMessageOutput(string message, Color color)
            {
                Message = message;
                Color = color;
                LifeRemaining = MESSAGE_LIFE;
            }
        }

        public ScreenMessages(Interface i)
        {
            this.inter = i;

			font = inter.DefaultFont.Smallest();
            lineSpace = font.MeasureString("|").Y;

            //Log.Instance.AddWriter("screen", new ScreenMessageLogWriter(this));
        }

        public void Update(GameTime gameTime)
        {
            try
            {
                List<ScreenMessageOutput> dead = new List<ScreenMessageOutput>();

                foreach (ScreenMessageOutput message in output)
                {
                    message.LifeRemaining -= gameTime.ElapsedGameTime.Milliseconds;
                    if (message.LifeRemaining <= 0)
                        dead.Add(message);
                }

                while (dead.Count > 0)
                {
                    output.Remove(dead[0]);
                    dead.RemoveAt(0);
                }

                while (output.Count > 10)
                    output.RemoveAt(0);
            }
            catch (Exception)
            {
                output.Clear();
            }
        }

        public void Draw(GraphicsDevice graphicsDevice)
        {
            //int y = graphicsDevice.Viewport.Width - (lineSpace * 10);
            int x = 10;
            int y = 10;

            for (int i = 0; i < staticText.Count; i++)
            {
                inter.DrawString(staticText[i].Message, new Vector2i(x, y), font, staticText[i].Color);
                y += lineSpace;
            }

            y = inter.Size.Y - lineSpace - 110;

            for (int i = output.Count - 1; i >= 0; i--)
            {
                if (output[i] == null)
                    continue;
                    
                inter.DrawString(output[i].Message, new Vector2i(x, y), font, output[i].Color);
                y -= lineSpace;
            }

            y = lineSpace;
        }

        public void Write(string message, Color color)
        {
            if (string.IsNullOrEmpty(message))
                return;

            string[] messages = message.Split(new char[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var m in messages)
                output.Add(new ScreenMessageOutput(StringHelper.TabsToSpaces(m, 4).TrimEnd(), color));
        }

        public void WriteStaticText(string message, Color color)
        {
            if (staticText.Count >= maxStaticMessages)
                staticText.RemoveAt(0);

            staticText.Add(new ScreenMessageOutput(message, color));
        }

        public void WriteStaticText(int index, string message, Color color)
        {
            if (index < 0)
                throw new Exception("Static text index cannot be < 0");
            
            //fill with empty lines if index is above range
            if (index >= staticText.Count)
                for (int i = staticText.Count; i <= index; i++)
                    staticText.Add(new ScreenMessageOutput(string.Empty, color));

            staticText[index] = new ScreenMessageOutput(message, color);
        }

        public void ClearStaticText() => staticText.Clear();
    }
}