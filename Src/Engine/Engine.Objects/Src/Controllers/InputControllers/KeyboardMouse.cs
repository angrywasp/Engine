using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Engine.Input;
using System.Numerics;
using Engine.World;
using AngryWasp.Logger;

namespace Engine.Objects.Controllers.InputControllers
{
    public class KeyboardMouse : Base
    {
        private bool mouseLook = true;

        InputLayout layout = InputLayout.Default();

        Dictionary<Keys, int> continuousKeyLookup = new Dictionary<Keys, int>();
        Dictionary<Keys, int> pressedKeyLookup = new Dictionary<Keys, int>();
        Dictionary<MouseButtons, int> mouseBtnLookup = new Dictionary<MouseButtons, int>();
        Dictionary<MouseDirection, int> mouseDirLookup = new Dictionary<MouseDirection, int>();
        Dictionary<MouseScrollDirection, int> mouseScrollLookup = new Dictionary<MouseScrollDirection, int>();

        public KeyboardMouse(EngineCore engine, GameObject gameObject)
            : base(engine, gameObject)
        {
            var keyLookup = new Dictionary<int, Keys>();

            foreach (var b in layout.Bindings)
            {
                switch (b.InputType)
                {
                    case InputType.Keyboard:
                        keyLookup.Add(b.ControlKey, b.Key);
                        break;
                    case InputType.MouseButton:
                        mouseBtnLookup.Add(b.MouseButton, b.ControlKey);
                        break;
                    case InputType.MouseDirection:
                        mouseDirLookup.Add(b.Direction, b.ControlKey);
                        break;
                    case InputType.MouseWheel:
                        mouseScrollLookup.Add(b.ScrollDirection, b.ControlKey);
                        break;
                }
            }

            foreach (var k in keyLookup)
            {
                if (k.Key <= 10)
                    //for these Control Keys we fire a command each frame the key is down
                    continuousKeyLookup.Add(k.Value, k.Key);
                else
                    //for these keys we only fire the command once when the key is first pressed
                    pressedKeyLookup.Add(k.Value, k.Key);
            }
        }

        public override void Update(GameTime gameTime)
        {
            float totalSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 delta;
            Vector2i ss = new Vector2i(engine.GraphicsDevice.Viewport.Width, engine.GraphicsDevice.Viewport.Height) / 2;
            var mpv = engine.Input.Mouse.Position.ToVector2();
            var ssv = ss.ToVector2();

            if (mouseLook)
            {
                // Calculate delta relative to the screen center
                if (engine.Input.Mouse.State.Position == engine.Input.Mouse.StateLastFrame.Position)
                    delta = Vector2.Zero;
                else
                    delta = (mpv - ssv);

                engine.Input.Mouse.Position = ss;
            }
            else
                delta = engine.Input.Mouse.MovementDelta;

            delta /= ssv;

            MouseDirection md;
            MouseScrollDirection msd;
            int scrollDelta;
            int ck;

            engine.Input.Mouse.CalculateMouseDirection(delta, out md);
            MouseButtons mb = engine.Input.Mouse.GetPressedButtons();
            engine.Input.Mouse.CalculateMouseScrollDirectionAndDelta(out msd, out scrollDelta);
            Keys[] allPressedKeys = engine.Input.Keyboard.State.GetPressedKeys();

            var keys = new float[ControlKeys.KeyCount];

            foreach (var key in allPressedKeys)
            {
                if (engine.Input.Keyboard.KeyJustPressed(key))
                {
                    if (pressedKeyLookup.TryGetValue(key, out ck))
                        keys[ck] = 1f;
                }
                else
                {
                    if (continuousKeyLookup.TryGetValue(key, out ck))
                        keys[ck] = 1f;
                }
            }

            engine.Input.Keyboard.Clear();

            //Mouse buttons. don't worry about checking X1 and X2

            if (mb.HasFlag(MouseButtons.Left) && mouseBtnLookup.TryGetValue(MouseButtons.Left, out ck))
                keys[ck] = 1f;

            if (mb.HasFlag(MouseButtons.Middle) && mouseBtnLookup.TryGetValue(MouseButtons.Middle, out ck))
                keys[ck] = 1f;

            if (mb.HasFlag(MouseButtons.Right) && mouseBtnLookup.TryGetValue(MouseButtons.Right, out ck))
                keys[ck] = 1f;

            //Mouse direction    

            if (md.HasFlag(MouseDirection.Left) && mouseDirLookup.TryGetValue(MouseDirection.Left, out ck))
                keys[ck] = delta.X;
            else if (md.HasFlag(MouseDirection.Right) && mouseDirLookup.TryGetValue(MouseDirection.Right, out ck))
                keys[ck] = delta.X;

            if (md.HasFlag(MouseDirection.Up) && mouseDirLookup.TryGetValue(MouseDirection.Up, out ck))
                keys[ck] = delta.Y;
            else if (md.HasFlag(MouseDirection.Down) && mouseDirLookup.TryGetValue(MouseDirection.Down, out ck))
                keys[ck] = delta.Y;

            //Mouse scroll
            if (msd == MouseScrollDirection.Up && mouseScrollLookup.TryGetValue(MouseScrollDirection.Up, out ck))
                keys[ck] = scrollDelta;
            else if (msd == MouseScrollDirection.Down && mouseScrollLookup.TryGetValue(MouseScrollDirection.Down, out ck))
                keys[ck] = -scrollDelta;

            controlledObject.Intellect.DoInputControllerCommand(keys);
        }
    }
}
