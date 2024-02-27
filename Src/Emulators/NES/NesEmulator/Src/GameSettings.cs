using System;
using System.Diagnostics;
using Engine.Input;
using Microsoft.Xna.Framework.Input;

namespace EmulatorCore
{
    public enum Control_Keys
    {
        None,

        Left,
        Right,
        Up,
        Down,

        Fire1,
        Fire2,
        
        Start,
        Reset
    }
    
    public enum InputType
    {
        None,
        Keyboard,
        MouseButton,
        MouseDirection,
        MouseWheel
    }

    public class LayoutItem
    {
        private InputType inputType = InputType.None;
        private Control_Keys controlKey = Control_Keys.None;
        private Keys key = Keys.None;
        private MouseButtons mouseButton = MouseButtons.None;
        private MouseDirection direction = MouseDirection.None;
        private MouseScrollDirection scrollDirection = MouseScrollDirection.None;

        public InputType InputType => inputType;
        public Control_Keys ControlKey => controlKey;
        public Keys Key => key;
        public MouseButtons MouseButton => mouseButton;
        public MouseDirection Direction => direction;
        public MouseScrollDirection ScrollDirection => scrollDirection;

        public static LayoutItem Create(Control_Keys controlKey, MouseDirection mouseDirection)
        {
            var i = new LayoutItem();
            i.inputType = InputType.MouseDirection;
            i.controlKey = controlKey;
            i.direction = mouseDirection;
            return i;
        }

        public static LayoutItem Create(Control_Keys controlKey, Keys key)
        {
            var i = new LayoutItem();
            i.inputType = InputType.Keyboard;
            i.controlKey = controlKey;
            i.key = key;
            return i;
        }

        public static LayoutItem Create(Control_Keys controlKey, MouseScrollDirection scrollDirection)
        {
            var i = new LayoutItem();
            i.inputType = InputType.MouseWheel;
            i.controlKey = controlKey;
            i.scrollDirection = scrollDirection;
            return i;
        }

        public static LayoutItem Create(Control_Keys controlKey, MouseButtons mouseButton)
        {
            var i = new LayoutItem();
            i.inputType = InputType.MouseButton;
            i.controlKey = controlKey;
            i.mouseButton = mouseButton;
            return i;
        }

        public override string ToString()
        {
            switch (InputType)
            {
                case InputType.Keyboard:
                    return "KB: " + Key.ToString();
                case InputType.MouseButton:
                    return "MB: " + MouseButton.ToString();
                case InputType.MouseDirection:
                    return "MD: " + Direction.ToString();
                case InputType.MouseWheel:
                    return "MW: " + ScrollDirection.ToString();
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class InputLayout
    {
        private InputType inputType = InputType.None;
        private Control_Keys controlKey = Control_Keys.None;
        private Keys key = Keys.None;
        private MouseButtons mouseButton = MouseButtons.None;
        private MouseDirection direction = MouseDirection.None;
        private MouseScrollDirection scrollDirection = MouseScrollDirection.None;

        public InputType InputType => inputType;
        public Control_Keys ControlKey => controlKey;
        public Keys Key => key;
        public MouseButtons MouseButton => mouseButton;
        public MouseDirection Direction => direction;
        public MouseScrollDirection ScrollDirection => scrollDirection;

        public override string ToString()
        {
            switch (InputType)
            {
                case InputType.Keyboard:
                    return "KB: " + Key.ToString();
                case InputType.MouseButton:
                    return "MB: " + MouseButton.ToString();
                case InputType.MouseDirection:
                    return "MD: " + Direction.ToString();
                case InputType.MouseWheel:
                    return "MW: " + ScrollDirection.ToString();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
