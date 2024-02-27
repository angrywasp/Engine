using System;
using System.Collections.Generic;
using System.Diagnostics;
using Engine.Input;
using Engine.Objects.Controllers;
using Microsoft.Xna.Framework.Input;

namespace Engine.Objects
{
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
        private int controlKey = ControlKeys.None;
        private Keys key = Keys.None;
        private MouseButtons mouseButton = MouseButtons.None;
        private MouseDirection direction = MouseDirection.None;
        private MouseScrollDirection scrollDirection = MouseScrollDirection.None;

        public InputType InputType => inputType;
        public int ControlKey => controlKey;
        public Keys Key => key;
        public MouseButtons MouseButton => mouseButton;
        public MouseDirection Direction => direction;
        public MouseScrollDirection ScrollDirection => scrollDirection;

        public static LayoutItem Create(int controlKey, MouseDirection mouseDirection)
        {
            var i = new LayoutItem();
            i.inputType = InputType.MouseDirection;
            i.controlKey = controlKey;
            i.direction = mouseDirection;
            return i;
        }

        public static LayoutItem Create(int controlKey, Keys key)
        {
            var i = new LayoutItem();
            i.inputType = InputType.Keyboard;
            i.controlKey = controlKey;
            i.key = key;
            return i;
        }

        public static LayoutItem Create(int controlKey, MouseScrollDirection scrollDirection)
        {
            var i = new LayoutItem();
            i.inputType = InputType.MouseWheel;
            i.controlKey = controlKey;
            i.scrollDirection = scrollDirection;
            return i;
        }

        public static LayoutItem Create(int controlKey, MouseButtons mouseButton)
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
        private List<LayoutItem> bindings = new List<LayoutItem>();

        public List<LayoutItem> Bindings => bindings;

        public bool HideCursor { get; set; }

        public static InputLayout Default()
        {
            var layout = new InputLayout();
            var b = layout.Bindings;
            
            b.Add(LayoutItem.Create(ControlKeys.Forward, Keys.W));
            b.Add(LayoutItem.Create(ControlKeys.Backward, Keys.S));
            b.Add(LayoutItem.Create(ControlKeys.Left, Keys.A));
            b.Add(LayoutItem.Create(ControlKeys.Right, Keys.D));

            b.Add(LayoutItem.Create(ControlKeys.LookLeft, MouseDirection.Left));
            b.Add(LayoutItem.Create(ControlKeys.LookRight, MouseDirection.Right));
            b.Add(LayoutItem.Create(ControlKeys.LookUp, MouseDirection.Up));
            b.Add(LayoutItem.Create(ControlKeys.LookDown, MouseDirection.Down));

            b.Add(LayoutItem.Create(ControlKeys.Jump, Keys.Space));
            b.Add(LayoutItem.Create(ControlKeys.Run, Keys.LeftShift));
            b.Add(LayoutItem.Create(ControlKeys.Use, Keys.E));
            b.Add(LayoutItem.Create(ControlKeys.Crouch, Keys.Z));
            b.Add(LayoutItem.Create(ControlKeys.Reload, Keys.R));
            b.Add(LayoutItem.Create(ControlKeys.ChangeCamera, Keys.C));

            b.Add(LayoutItem.Create(ControlKeys.Weapon1, Keys.D1));
            b.Add(LayoutItem.Create(ControlKeys.Weapon2, Keys.D2));
            b.Add(LayoutItem.Create(ControlKeys.Weapon3, Keys.D3));
            b.Add(LayoutItem.Create(ControlKeys.Weapon4, Keys.D4));

            b.Add(LayoutItem.Create(ControlKeys.NextWeapon, MouseScrollDirection.Up));
            b.Add(LayoutItem.Create(ControlKeys.PreviousWeapon, MouseScrollDirection.Down));
            
            b.Add(LayoutItem.Create(ControlKeys.Fire, MouseButtons.Left));
            b.Add(LayoutItem.Create(ControlKeys.AltFire, MouseButtons.Right));

            return layout;
        }
    }
}
