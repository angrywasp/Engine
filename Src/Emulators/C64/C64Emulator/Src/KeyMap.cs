using System.Collections.Generic;
using System.Diagnostics;
using Input;

namespace C64Emulator
{
    public class KeyMap
    {
        public class KeyValue
        {
            private Keyboard.Keys key;
            private bool shifted;

            public Keyboard.Keys Key => key;
            public bool Shifted => shifted;

            public KeyValue(Keyboard.Keys k, bool s)
            {
                key = k;
                shifted = s;
            }
        }

        private static Dictionary<char, KeyValue> keymap = new Dictionary<char, KeyValue>();

        private static Keyboard keyboard;

        public static void Initialize(Keyboard kb)
        {
            keyboard = kb;
            //0-9
            for (int i = 0; i <= 9; i++)
            {
                char c = (char)(i + 48);
                keymap.Add(c, new KeyValue((Keyboard.Keys)i, false));
                //shifted

                if (i == 0)
                    continue;

                c = (char)(i + 32);
                keymap.Add(c, new KeyValue((Keyboard.Keys)i, true));
            }

            //A-Z
            for (int i = 0; i <= 26; i++)
            {
                char c = (char)(i + 65);
                keymap.Add(c, new KeyValue((Keyboard.Keys)(i + 10), false));
            }

            keymap.Add('+', new KeyValue(Keyboard.Keys.KEY_PL, false));
            keymap.Add('-', new KeyValue(Keyboard.Keys.KEY_PL, false));
            keymap.Add('=', new KeyValue(Keyboard.Keys.KEY_EQ, false));
            keymap.Add('@', new KeyValue(Keyboard.Keys.KEY_AT, false));
            keymap.Add('*', new KeyValue(Keyboard.Keys.KEY_STAR, false));
            keymap.Add(':', new KeyValue(Keyboard.Keys.KEY_COL, false));
            keymap.Add('[', new KeyValue(Keyboard.Keys.KEY_COL, true));
            keymap.Add(';', new KeyValue(Keyboard.Keys.KEY_SCOL, false));
            keymap.Add(']', new KeyValue(Keyboard.Keys.KEY_SCOL, true));
            keymap.Add(' ', new KeyValue(Keyboard.Keys.KEY_SP, false));
            keymap.Add(',', new KeyValue(Keyboard.Keys.KEY_COM, false));
            keymap.Add('<', new KeyValue(Keyboard.Keys.KEY_COM, true));
            keymap.Add('.', new KeyValue(Keyboard.Keys.KEY_DOT, false));
            keymap.Add('>', new KeyValue(Keyboard.Keys.KEY_DOT, true));
            keymap.Add('/', new KeyValue(Keyboard.Keys.KEY_RSH, true));
        }

        public static KeyValue GetValue(char c)
        {
            if (!keymap.ContainsKey(c))
                throw new KeyNotFoundException();

            return keymap[c];
        }
    }
}