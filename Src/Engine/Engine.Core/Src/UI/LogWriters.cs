using AngryWasp.Logger;
using Microsoft.Xna.Framework;
using Engine.Helpers;
using System;
using AngryWasp.Helpers;

namespace Engine.UI
{
    public abstract class EngineLogWriter : ILogWriter
    {
        private static readonly Color[] colors = new Color[]
        {
            Color.Black,
            Color.DarkBlue,
            Color.DarkGreen,
            Color.DarkCyan,
            Color.DarkRed,
            Color.DarkMagenta,
            Color.DarkGoldenrod,
            Color.Gray,
            Color.DarkGray,
            Color.Blue,
            Color.LimeGreen,
            Color.Cyan,
            Color.Red,
            Color.Magenta,
            Color.Yellow,
            Color.White
        };

        protected Color color = Color.White;

        public void SetColor(ConsoleColor color)
        {
            this.color = colors[(int)color];
        }

        public EngineLogWriter() { }

        public void Close() { }

		public void Flush() { }

        protected abstract void Write(string value, Color color);

        public void WriteInfo(string value)
        {
            Write(StringHelper.TabsToSpaces(value, 4).TrimEnd(), color);
        }

        public void WriteWarning(string value)
        {
            Write(StringHelper.TabsToSpaces(value, 4).TrimEnd(), Color.Yellow);
        }

        public void WriteError(string value)
        {
            Write(StringHelper.TabsToSpaces(value, 4).TrimEnd(), Color.Red);
        }
    }

    public class TerminalLogWriter : EngineLogWriter
	{
        private Terminal term;

        public TerminalLogWriter(Terminal term) : base()
        {
            this.term = term;
        }

		protected override void Write(string value, Color color)
		{
            term.Write(value, color);
		}
	}

    public class ScreenMessageLogWriter : EngineLogWriter
    {
        ScreenMessages messages;

        public ScreenMessageLogWriter(ScreenMessages messages) : base()
        {
            this.messages = messages;
        }

		protected override void Write(string value, Color color)
		{
            messages.Write(value, color);
		}
    }
}