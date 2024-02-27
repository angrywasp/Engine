using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AngryWasp.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine.Input
{
	public class KeyboardExtensions
    {
        private KeyboardState state;
        private KeyboardState stateLastFrame;

        private GraphicsDevice graphicsDevice;

        public KeyboardState State => state;
            
        public KeyboardExtensions(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public void Update(GameTime gameTime)
        {
            stateLastFrame = state;
            state = Keyboard.GetState();
        }

        public void Clear()
        {
            state = stateLastFrame = Keyboard.GetState();
        }

        public bool KeyJustPressed(Keys key) => state.IsKeyDown(key) && stateLastFrame.IsKeyUp(key);

        public bool KeyJustReleased(Keys key) => state.IsKeyUp(key) && stateLastFrame.IsKeyDown(key);

        public bool KeyDown(Keys key) => state.IsKeyDown(key);

        public bool KeyUp(Keys key) => state.IsKeyUp(key);

        public bool ModifierKeyDown(Sdl.Keyboard.Keymod key)
		{
			var modifiers = Sdl.Keyboard.GetModState();

			return (modifiers & key) == key;
		}

        public bool IsSpecialKey(Keys key)
        {
            // All keys except A-Z, 0-9 and `-\[];',./= (and space) are special keys.
            // With shift pressed this also results in this keys:
            // ~_|{}:"<>? !@#$%^&*().
            int keyNum = (int)key;
            if ((keyNum >= (int)Keys.A && keyNum <= (int)Keys.Z) ||
                (keyNum >= (int)Keys.D0 && keyNum <= (int)Keys.D9) ||
                key == Keys.Space || // well, space ^^
                key == Keys.OemTilde || // `~
                key == Keys.OemMinus || // -_
                key == Keys.OemPipe || // \|
                key == Keys.OemOpenBrackets || // [{
                key == Keys.OemCloseBrackets || // ]}
                key == Keys.OemSemicolon || // ;:
                key == Keys.OemQuotes || // '"
                key == Keys.OemComma || // ,<
                key == Keys.OemPeriod || // .>
                key == Keys.OemQuestion || // /?
                key == Keys.OemPlus) // =+
                return false;
            
            //process numpad keys if numlock is active
            if (Keyboard.GetState().NumLock)
            {
                int zero = (int)Keys.NumPad0;
                int nine = (int)Keys.NumPad9;
                if (keyNum >= zero && keyNum <= nine)
                    return false;
            }

            // Else is is a special key
            return true;
        }

        /// <summary>
        /// Converts a Key enum value to its associated keyboard char
        /// </summary>
        /// <param name="key">The key to convert</param>
        /// <param name="shiftPressed">Is the shift key pressed?</param>
        /// <returns>a char representing the keyboard key pressed</returns>
        public char KeyToChar(Keys key, bool shiftPressed)
        {
            // If key will not be found, just return space
            char ret = ' ';
            int keyNum = (int)key;
            if (keyNum >= (int)Keys.A && keyNum <= (int)Keys.Z)
            {
                if (shiftPressed)
                    ret = key.ToString()[0];
                else
                    ret = key.ToString().ToLower()[0];
            }
            else if (keyNum >= (int)Keys.D0 && keyNum <= (int)Keys.D9 && shiftPressed == false)
                ret = (char)((int)'0' + (keyNum - (int)Keys.D0));
            else if (Keyboard.GetState().NumLock && keyNum >= (int)Keys.NumPad0 && keyNum <= (int)Keys.NumPad9)
                ret = (char)((int)'0' + (keyNum - (int)Keys.NumPad0));
            else if (key == Keys.D1 && shiftPressed)
                ret = '!';
            else if (key == Keys.D2 && shiftPressed)
                ret = '@';
            else if (key == Keys.D3 && shiftPressed)
                ret = '#';
            else if (key == Keys.D4 && shiftPressed)
                ret = '$';
            else if (key == Keys.D5 && shiftPressed)
                ret = '%';
            else if (key == Keys.D6 && shiftPressed)
                ret = '^';
            else if (key == Keys.D7 && shiftPressed)
                ret = '&';
            else if (key == Keys.D8 && shiftPressed)
                ret = '*';
            else if (key == Keys.D9 && shiftPressed)
                ret = '(';
            else if (key == Keys.D0 && shiftPressed)
                ret = ')';
            else if (key == Keys.OemTilde)
                ret = shiftPressed ? '~' : '`';
            else if (key == Keys.OemMinus)
                ret = shiftPressed ? '_' : '-';
            else if (key == Keys.OemPipe)
                ret = shiftPressed ? '|' : '\\';
            else if (key == Keys.OemOpenBrackets)
                ret = shiftPressed ? '{' : '[';
            else if (key == Keys.OemCloseBrackets)
                ret = shiftPressed ? '}' : ']';
            else if (key == Keys.OemSemicolon)
                ret = shiftPressed ? ':' : ';';
            else if (key == Keys.OemQuotes)
                ret = shiftPressed ? '"' : '\'';
            else if (key == Keys.OemComma)
                ret = shiftPressed ? '<' : ',';
            else if (key == Keys.OemPeriod)
                ret = shiftPressed ? '>' : '.';
            else if (key == Keys.OemQuestion)
                ret = shiftPressed ? '?' : '/';
            else if (key == Keys.OemPlus)
                ret = shiftPressed ? '+' : '=';

            // Return result
            return ret;
        }

        /// <summary>
        /// Handles keyboard input into text boxes etc
        /// </summary>
        public void HandleKeyboardTextInput(ref string inputText, int pos, out int adjustedPos, bool showCaret)
        {
            adjustedPos = pos;
            // Is a shift key pressed (we have to check both, left and right)
            bool isShiftPressed =
				ModifierKeyDown(Sdl.Keyboard.Keymod.LeftShift) ||
                ModifierKeyDown(Sdl.Keyboard.Keymod.RightShift);

            Keys[] pressedKeys = state.GetPressedKeys();

            // Go through all pressed keys
            foreach (Keys k in pressedKeys)
                if (KeyJustPressed(k))
                    inputText = ProcessKeyPressForTextInput(inputText, pos, out adjustedPos, showCaret, isShiftPressed, k);
        }

        /// <summary>
        /// Helper method for HandleKeyboardTextInput
        /// </summary>
        private string ProcessKeyPressForTextInput(string inputText, int pos, out int adjustedPos, bool showCaret, bool isShiftPressed, Keys pressedKey)
        {
            List<char> chars = inputText == null ? new List<char>() : inputText.ToList();
            MathHelper.Clamp(pos, 0, chars.Count);
            adjustedPos = pos;
            // No special key?

            if (!IsSpecialKey(pressedKey))
            {
                chars.Insert(pos, KeyToChar(pressedKey, isShiftPressed));
                ++adjustedPos;
            }
            else if (pressedKey == Keys.Back && pos > 0 && chars.Count > pos - 1)
            {
                chars.RemoveAt(pos - 1);
                --adjustedPos;
            }
            else if (pressedKey == Keys.Delete && pos < inputText.Length)
                chars.RemoveAt(pos);
            else if (pressedKey == Keys.Left && pos > 0)
                --adjustedPos;
            else if (pressedKey == Keys.Right && pos < inputText.Length)
                ++adjustedPos;

            inputText = string.Concat(chars);

            return inputText;
        }
    }
}
