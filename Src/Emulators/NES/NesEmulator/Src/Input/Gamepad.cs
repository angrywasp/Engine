using MyNes.Nes.Input;

namespace NesEmulator.Input
{
    public class Gamepad : IJoypad
    {
        // Methods
        /*public byte GetData()
        {
            byte num = 0;

            if (Engine.Input.GamePadAPressed)
                num |= 1;

            if (Engine.Input.GamePadXPressed)
                num |= 2;

            if (Engine.Input.GamePad.Buttons.BigButton == ButtonState.Pressed)
                num |= 4;

            if (Engine.Input.GamePadStartPressed)
                num |= 8;

            if (Engine.Input.GamePadUpPressed)
                num |= 0x10;

            if (Engine.Input.GamePadDownPressed)
                num |= 0x20;

            if (Engine.Input.GamePadLeftPressed)
                num |= 0x40;

            if (Engine.Input.GamePadRightPressed)
                num |= 0x80;

            return num;
        }*/
        public byte GetData()
        {
            return 0;
        }
    }
}
