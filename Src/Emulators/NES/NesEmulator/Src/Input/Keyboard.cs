using Engine;
using Engine.Input;
using Microsoft.Xna.Framework.Input;
using MyNes.Nes.Input;

namespace NesEmulator.Input
{
    public class Keyboard : IJoypad
    {
    	private KeyboardExtensions kb;
    	private MouseExtensions m;
    	
    	public Keyboard(InputDeviceManager input)
    	{
    		kb = input.Keyboard;
    		m = input.Mouse;
    	}
    	
        public byte GetData()
        {
            byte num = 0;

            //start
            if (kb.KeyJustPressed(Keys.Enter))
                num |= 8;

            //select
            if (kb.KeyJustPressed(Keys.Space))
                num |= 4;

            //A
            if (m.ButtonDown(m.LeftButton))
                num |= 1;

            //B
            if (m.ButtonDown(m.RightButton))
                num |= 2;

            //up
            if (kb.KeyDown(Keys.W))
                num |= 0x10;

            //down
            if (kb.KeyDown(Keys.S))
                num |= 0x20;

            //left
            if (kb.KeyDown(Keys.A))
                num |= 0x40;

            //right
            if (kb.KeyDown(Keys.D))
                num |= 0x80;

            return num;
        }
    }
}
