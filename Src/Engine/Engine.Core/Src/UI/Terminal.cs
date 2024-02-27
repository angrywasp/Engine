using Engine.Helpers;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using Engine.Scripting;
using System.IO;
using Engine.Input;
using Engine.UI.Controls;
using System;
using Engine.Configuration;
using Microsoft.Xna.Framework.Input;
using Engine.Content;

namespace Engine.UI
{
    public class Terminal
    {
        internal class TerminalOutput
        {
            public string Message { get; set; }

            public Color Color { get; set; }

            public TerminalOutput(string message, Color color)
            {
                Message = message;
                Color = color;
            }
        }

        private Queue<TerminalOutput> output = new Queue<TerminalOutput>(10000);
        private Queue<string> userEnteredCommands = new Queue<string>();
        private int commandIndex;
        private string userInput = string.Empty;
        private FontPackage fontPackage;
        private Font font;
        private int lineSpace = 12;
        private int messageHistoryLength = 250;
        private int screenHistoryLength = 25;
        private Interface inter;
        private InputDeviceManager input;
        private int max;
        private Rectangle displayRect;
        private Color backgroundColor;
        private int outputStart = 0;
        private int count = 0;
        private bool visible;
        private int caretPosition;
        private int adjustedCaretPosition;

        public Color BackgroundColor => backgroundColor;

        public bool Visible
        {
            get { return visible; }
            set
            {
                if (value)
                {
                    caretPosition = 0;
                    adjustedCaretPosition = 0;
                }

                visible = value;
            }
        }

        private void AddMessage(string message, Color color)
        {
            output.Enqueue(new TerminalOutput(message, color));

            if (output.Count > messageHistoryLength)
                output.Dequeue();

            if (output.Count > screenHistoryLength)
                outputStart = output.Count - screenHistoryLength;

            CalculateOutputStart();
        }

        private void AddUserInputString(string s)
        {
            if (string.IsNullOrEmpty(s))
                return;

            if (userEnteredCommands.Count == 0)
                userEnteredCommands.Enqueue(s);
            else if (userEnteredCommands.ElementAt(userEnteredCommands.Count - 1) != s)
                userEnteredCommands.Enqueue(s);


            if (userEnteredCommands.Count > messageHistoryLength)
                userEnteredCommands.Dequeue();

            commandIndex = userEnteredCommands.Count;
        }

        public Terminal(Interface inter, InputDeviceManager input)
        {
            this.inter = inter;
            this.input = input;
            Visible = false;
            fontPackage = ContentLoader.LoadFontPackage(inter.GraphicsDevice, Settings.Engine.TerminalFont);
            SetFontSize(Settings.Engine.TerminalFontSize);

            Log.Instance.AddWriter("terminal", new TerminalLogWriter(this));
            ResetCommandHistory();

            backgroundColor = Color.Black;
            backgroundColor.A = 200;
        }

        public void SetFontSize(int size)
        {
            font = fontPackage.GetByFontSize(size);
            lineSpace = font.MeasureString("|").Y;
            
            screenHistoryLength = (Settings.Engine.TerminalHeight / lineSpace) - 2;
            max = lineSpace * screenHistoryLength + lineSpace;
        }

        public void Write(string message, Color color)
        {
            //todo: split messages that are too long to display on the screen

            string[] messages = message.Split(new char[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var m in messages)
                AddMessage(StringHelper.TabsToSpaces(m, 4).TrimEnd(), color);
        }

        public void Draw()
        {
            inter.DrawRectangle(backgroundColor, Vector2i.Zero, new Vector2i(inter.Size.X, max + (lineSpace * 2)));
            int y = lineSpace;
            var l = output.ToArray();

            count = outputStart + screenHistoryLength;
            if (count > output.Count)
                count = output.Count;
                
            for (int i = outputStart; i < count; i++)
            {
                TerminalOutput to = l[i];
                inter.DrawString(to.Message, new Vector2i(lineSpace, y), font, to.Color);
                 y += lineSpace;
            }

            Vector2i caretDrawPos;
            Vector2i startPos = new Vector2i(lineSpace, max);
            inter.DrawString(userInput, startPos, font, Color.White, caretPosition, out caretDrawPos);
            inter.DrawString("_", new Vector2i(caretDrawPos.X, max), font, Color.Red);
        }

        public void Update(GameTime gameTime)
        {
            displayRect = new Rectangle(0, 0, inter.Size.X, max + (lineSpace * 2));

            if (input.Mouse.InsideRect(displayRect))
            {
                if (input.Mouse.ScrollDelta != 0)
                    OnMouseScroll(new UiControlMouseEventArgs(input.Mouse, input.Mouse.Position - inter.Position));
            }

            if (Visible)
            {
                if (input.Keyboard.KeyJustPressed(Keys.Enter))
                {
                    DoInputEnter();
                    caretPosition = adjustedCaretPosition = 0;
                }
                else if (input.Keyboard.KeyJustPressed(Keys.Down))
                {
                    ShowNextInputString();
                    caretPosition = adjustedCaretPosition = userInput.Length;
                }
                else if (input.Keyboard.KeyJustPressed(Keys.Up))
                {
                    ShowPreviousInputString();
                    caretPosition = adjustedCaretPosition = userInput.Length;
                }
                else if (input.Keyboard.State.GetPressedKeys().Length > 0)
                {
                    //todo: can we update this with an event
                    input.Keyboard.HandleKeyboardTextInput(ref userInput, caretPosition, out adjustedCaretPosition, true);
                    caretPosition = adjustedCaretPosition;
                }
            }   
        }

        public void Clear()
        {
            output.Clear();
            userInput = string.Empty;
            AddMessage("Engine Terminal", Color.DarkGray);
            CalculateOutputStart();
        }

        public void ShowPreviousInputString()
        {
            --commandIndex;

            if (commandIndex < 0)
                commandIndex = 0;

            if (commandIndex < userEnteredCommands.Count)
                userInput = userEnteredCommands.ElementAt(commandIndex);
        }

        public void ShowNextInputString()
        {
            ++commandIndex;

            if (commandIndex >= userEnteredCommands.Count)
                commandIndex = userEnteredCommands.Count;

            if (commandIndex < userEnteredCommands.Count)
                userInput = userEnteredCommands.ElementAt(commandIndex);
        }

        public void DoInputEnter()
        {
            userInput = userInput.Trim();

            Write(userInput, Color.Turquoise);

            if (!ScriptEngine.ExecuteCode(userInput))
                output.Last().Color = Color.Red;
            else
            {
                AddUserInputString(userInput);
                File.WriteAllLines(EngineFolders.SettingsPathVirtualToReal("TerminalCommands"), userEnteredCommands);
                ResetCommandHistory();
            }
        }

        public void RunMacro(string macro)
        {
            string fullMacroFile = EngineFolders.ContentPathVirtualToReal(macro);

            if (!File.Exists(fullMacroFile))
            {
                Log.Instance.WriteWarning("Macro file does not exist");
                return;
            }

            string cmd = File.ReadAllText(fullMacroFile);

            string trimmed = cmd.Trim();
            Write(trimmed, Color.Turquoise);

            if (!ScriptEngine.ExecuteCode(trimmed))
            {
                output.Last().Color = Color.Red;
                Log.Instance.Write("Macro execution failed");
            }
            else
            {
                AddUserInputString(userInput);
                File.WriteAllLines(EngineFolders.SettingsPathVirtualToReal("TerminalCommands"), userEnteredCommands);
                ResetCommandHistory();
            }
        }

        public void RunMacroLines(string macro)
        {
            string fullMacroFile = EngineFolders.ContentPathVirtualToReal(macro);

            if (!File.Exists(fullMacroFile))
            {
                Log.Instance.WriteWarning("Macro file does not exist");
                return;
            }

            string[] cmd = File.ReadAllLines(fullMacroFile);

            bool exited = false;

            foreach (string c in cmd)
            {
                string trimmed = c.Trim();
                Write(trimmed, Color.Turquoise);

                if (!ScriptEngine.ExecuteCode(trimmed))
                {
                    output.Last().Color = Color.Red;
                    exited = true;
                    Log.Instance.Write("Macro execution failed");
                    break;
                }
            }

            if (!exited)
            {
                AddUserInputString(userInput);
                File.WriteAllLines(EngineFolders.SettingsPathVirtualToReal("TerminalCommands"), userEnteredCommands);
                ResetCommandHistory();
            }
        }

        public void ResetCommandHistory()
        {
            userInput = string.Empty;
            userEnteredCommands.Clear();

            string fullPath = EngineFolders.SettingsPathVirtualToReal("TerminalCommands");

            if (!File.Exists(fullPath))
            {
                commandIndex = 0;
                caretPosition = adjustedCaretPosition = 0;
                return;
            }

            string[] lines = File.ReadAllLines(fullPath);

            foreach (var item in lines)
                userEnteredCommands.Enqueue(item);

            commandIndex = userEnteredCommands.Count;
            caretPosition = adjustedCaretPosition = 0;
        }

        private void OnMouseScroll(UiControlMouseEventArgs e)
        {
            if (e.Mouse.ScrollDelta > 0)
                outputStart--;
            else if (e.Mouse.ScrollDelta < 0)
                outputStart++;

            CalculateOutputStart();
        }

        private void CalculateOutputStart()
        {
            if (outputStart >= output.Count - screenHistoryLength)
                outputStart = output.Count - screenHistoryLength;

            if (outputStart <= 0)
                outputStart = 0;
        }
    }
}