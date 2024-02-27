using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using AngryWasp.Logger;
using AngryWasp.Random;
using Engine.Content;
using Engine.Graphics.Vertices;
using Engine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.UI.Controls
{
    public class UiControl : UiControlBase
    {
        public delegate void MouseInteractionEventHandler(UiControl sender, UiControlMouseEventArgs e);

        #region Mouse Events

        public event MouseInteractionEventHandler MouseEnter;

        public event MouseInteractionEventHandler MouseLeave;

        public event MouseInteractionEventHandler MouseDown;

        public event MouseInteractionEventHandler MouseUp;

        public event MouseInteractionEventHandler MouseScroll;

        public event MouseInteractionEventHandler MouseClick;

        #endregion

        private bool entered;
        private bool clicked;
        private bool mouseDownOnEnter;

        protected ControlState currentState;
        protected SkinElement skinElement;
        protected string skinElementKey;

        protected internal virtual bool CanFocus => false;

        protected Vector2 perPosition;
        protected Vector2 perSize;
        protected Vector2i pixPosition;
        protected Vector2i pixSize;

        protected string name;

        public virtual List<string> SupportedStates => new string[] { "Normal", "Over", "Active", "Disabled" }.ToList();

        public override Rectangle Bounds
        {
            get
            {
                Vector2i p = PixelPosition;
                Vector2i s = PixelSize;
                return new Rectangle(p.X, p.Y, s.X, s.Y);
            }
        }

        public string ControlPath
        {
            get
            {
                string path = name;
                UiControl p = this;
                while ((p = (UiControl)p.Parent) != null)
                    path = $"{p.name}->{path}";

                return path;
            }
        }

        public Rectangle ScissorRectangle { get; set; } = Rectangle.Empty;

        public override Vector2i PixelPosition => pixPosition;

        public override Vector2i PixelSize => pixSize;

        public override Vector2 PercentPosition => perPosition;

        public override Vector2 PercentSize => perSize;

        public string Name => name;

        public T Create<T>(Vector2 position = default(Vector2), Vector2 size = default(Vector2), string name = null, string skinElement = "UiControl") where T : UiControl, new()
        {
            T t = new T();
            t.parent = this;
            t.form = form;
            t.ui = ui;
            t.graphicsDevice = ui.GraphicsDevice;
            t.name = name != null ? name : RandomString.AlphaNumeric(8);

            if (position != Vector2.Zero)
                t.SetPosition(position);

            if (size != Vector2.Zero)
                t.SetSize(size);

            t.ApplySkinElement(skinElement);

            return t;
        }

        public T Create<T>(Vector2i position = default(Vector2i), Vector2 size = default(Vector2), string name = null, string skinElement = "UiControl") where T : UiControl, new()
        {
            T t = new T();
            t.parent = this;
            t.form = form;
            t.ui = ui;
            t.graphicsDevice = ui.GraphicsDevice;
            t.name = name != null ? name : RandomString.AlphaNumeric(8);

            if (position != Vector2i.Zero)
                t.SetPosition(position);

            if (size != Vector2.Zero)
                t.SetSize(size);

            t.ApplySkinElement(skinElement);

            return t;
        }

        public T Create<T>(Vector2i position = default(Vector2i), Vector2i size = default(Vector2i), string name = null, string skinElement = "UiControl") where T : UiControl, new()
        {
            T t = new T();
            t.parent = this;
            t.form = form;
            t.ui = ui;
            t.graphicsDevice = ui.GraphicsDevice;
            t.name = name != null ? name : RandomString.AlphaNumeric(8);

            if (position != Vector2i.Zero)
                t.SetPosition(position);

            if (size != Vector2i.Zero)
                t.SetSize(size);

            t.ApplySkinElement(skinElement);

            return t;
        }

        public T Create<T>(Vector2 position = default(Vector2), Vector2i size = default(Vector2i), string name = null, string skinElement = "UiControl") where T : UiControl, new()
        {
            T t = new T();
            t.parent = this;
            t.form = form;
            t.ui = ui;
            t.graphicsDevice = ui.GraphicsDevice;
            t.name = name != null ? name : RandomString.AlphaNumeric(8);

            if (position != Vector2.Zero)
                t.SetPosition(position);

            if (size != Vector2i.Zero)
                t.SetSize(size);

            t.ApplySkinElement(skinElement);

            return t;
        }

        public T Add<T>(Vector2 position = default(Vector2), Vector2 size = default(Vector2), string name = null, string skinElement = "UiControl") where T : UiControl, new()
        {
            var t = Create<T>(position, size, name, skinElement);
            Add(t);
            return t;
        }

        public T Add<T>(Vector2i position = default(Vector2i), Vector2i size = default(Vector2i), string name = null, string skinElement = "UiControl") where T : UiControl, new()
        {
            var t = Create<T>(position, size, name, skinElement);
            Add(t);
            return t;
        }

        public T Add<T>(Vector2i position = default(Vector2i), Vector2 size = default(Vector2), string name = null, string skinElement = "UiControl") where T : UiControl, new()
        {
            var t = Create<T>(position, size, name, skinElement);
            Add(t);
            return t;
        }

        public T Add<T>(Vector2 position = default(Vector2), Vector2i size = default(Vector2i), string name = null, string skinElement = "UiControl") where T : UiControl, new()
        {
            var t = Create<T>(position, size, name, skinElement);
            Add(t);
            return t;
        }

        private List<UiControl> queuedAdd = new List<UiControl>();
        private List<UiControl> queuedRemove = new List<UiControl>();

        public void Add(UiControl t) => queuedAdd.Add(t);

        public void Remove(UiControl t) => queuedRemove.Add(t);

        protected virtual Vector2 PositionToPercent(Vector2i position)
        {
            if (Parent == null)
                return new Vector2(position.X / (float)ui.Size.X, position.Y / (float)ui.Size.Y);

            return new Vector2(position.X / (float)Parent.PixelSize.X, position.Y / (float)Parent.PixelSize.Y);
        }

        protected virtual Vector2i PositionToPixel(Vector2 position)
        {
            if (Parent == null)
                return new Vector2i((int)(position.X * (float)ui.Size.X), (int)(position.Y * (float)ui.Size.Y));

            int x = (int)(Parent.PixelPosition.X + (position.X * Parent.PixelSize.X));
            int y = (int)(Parent.PixelPosition.Y + (position.Y * Parent.PixelSize.Y));
            return new Vector2i(x, y);
        }

        protected Vector2 SizeToPercent(Vector2i size)
        {
            if (Parent == null)
                return new Vector2(size.X / (float)ui.Size.X, size.Y / (float)ui.Size.Y);

            if (size == Vector2i.Zero)
                return Vector2.Zero;

            return new Vector2(size.X / (float)Parent.PixelSize.X, size.Y / (float)Parent.PixelSize.Y);
        }

        protected Vector2i SizeToPixel(Vector2 size)
        {
            if (Parent == null)
                return new Vector2i((int)MathF.Ceiling(size.X * (float)ui.Size.X), (int)MathF.Ceiling(size.Y * (float)ui.Size.Y));

            if (size == Vector2.Zero)
                return Vector2i.Zero;

            return new Vector2i((int)MathF.Ceiling(size.X * Parent.PixelSize.X), (int)MathF.Ceiling(size.Y * Parent.PixelSize.Y));
        }

        public virtual void SetPosition(Vector2 position)
        {
            perPosition = position;
            OnResize();
        }

        public virtual void SetPosition(Vector2i position)
        {
            perPosition = PositionToPercent(position);
            OnResize();
        }

        public virtual void SetSize(Vector2 size)
        {
            perSize = size;
            OnResize();
        }

        public virtual void SetSize(Vector2i size)
        {
            perSize = SizeToPercent(size);
            OnResize();
        }

        public bool HasFocus { get; set; }

        public ControlState CurrentState => currentState;

        public string SkinElementKey
        {
            get { return skinElementKey; }
            set
            {
                skinElementKey = value;

                if (isLoaded)
                    ApplySkinElement(value);
            }
        }

        public virtual void ApplySkinElement(string elementKey)
        {
            if (string.IsNullOrEmpty(elementKey))
                throw new ArgumentNullException(nameof(elementKey));

            skinElement = ui.Skin.Elements[elementKey];
            skinElementKey = elementKey;
            ApplyDefaultElementState();
        }

        /// <summary>
        /// Applies a specific state within the skin element to the control
        /// </summary>
        /// <param name="key">The key of the state within the skin element to use</param>
        public virtual void ApplySkinElementState(string key) => currentState = skinElement.States[key];

        /// <summary>
        /// Applies the default state of the skin element to the control
        /// </summary>
        /// <remarks>
        /// The default state is the 'Normal' state. If this does not exist the first state in the skin element is used
        /// </remarks>
        private void ApplyDefaultElementState()
        {
            //no skin element set on the control
            if (skinElement == null)
                return;

            if (skinElement.States.Count == 0)
                return;

            //the element does not contain a state with the specified key
            if (skinElement.States.ContainsKey("Normal"))
            {
                ApplySkinElementState("Normal");
                return;
            }

            ApplySkinElementState(skinElement.States.First().Key);
        }

        public virtual void Update(GameTime gameTime)
        {
            if (queuedAdd.Count > 0)
            {
                var al = queuedAdd.ToArray();
                queuedAdd.Clear();
                foreach (var a in al)
                    children.Add(a);
            }

            if (queuedRemove.Count > 0)
            {
                var al = queuedRemove.ToArray();
                queuedRemove.Clear();
                foreach (var a in al)
                    children.Remove(a);
            }

            if (!Visible || PixelSize.X == 0 || PixelSize.Y == 0)
                return;

            MouseExtensions m = ui.Input.Mouse;

            Vector2i p = PixelPosition;
            Vector2i s = PixelSize;

            //Bounds is relative to the Interface the control is contained in
            //Mouse works on absolute screen coordinates so we need to offset the 
            //bounds of the control to an absolute screen position before checking
            //if the mouse is inside that rectangle
            Rectangle b = ScissorRectangle;
            if (b == Rectangle.Empty)
                b = new Rectangle(p.X + ui.Position.X, p.Y + ui.Position.Y, s.X, s.Y);

            //todo: need to fix this. control requires mouse to work. this should be replaced so controls will work with a GamePad or mouse
            if (m.InsideRect(b))
            {
                if (m.ScrollDelta != 0)
                    OnMouseScroll(new UiControlMouseEventArgs(m, m.Position - ui.Position));

                //entered will be false if this is the first time the mouse has been detected in the box. in which case we fire OnMouseEntered
                if (!entered)
                {
                    OnMouseEnter(new UiControlMouseEventArgs(m, m.Position - ui.Position));
                    entered = true;

                    mouseDownOnEnter = m.LeftJustPressed;
                }

                if (m.LeftJustPressed)
                {
                    if (!mouseDownOnEnter)
                    {
                        OnMouseDown(new UiControlMouseEventArgs(m, m.Position - ui.Position));
                        clicked = true;
                    }
                }

                if (m.LeftJustReleased)
                {
                    OnMouseUp(new UiControlMouseEventArgs(m, m.Position - ui.Position));
                    mouseDownOnEnter = false;
                    if (clicked)
                    {
                        OnMouseClick(new UiControlMouseEventArgs(m, m.Position - ui.Position));
                        clicked = false;
                    }
                }
            }
            else
            {
                //entered will be true if this is the first update where the mouse has not been inside the box
                if (entered)
                {
                    OnMouseLeave(new UiControlMouseEventArgs(m, m.Position - ui.Position));
                    entered = false;
                }

                clicked = false;
            }

            foreach (UiControl control in children)
                control.Update(gameTime);
        }

        protected bool drawThisFrame = true;

        public virtual void CheckNeedDraw()
        {
            if (!Visible)
                drawThisFrame = false;

            if (skinElement == null && currentState == null)
                drawThisFrame = false;

            // We need to check if the width or height of the bounds are 0 as well, because Size.Value may not be 0
            // when bounds is. This can happen when using Transform_Mode.Pixel and supplying a size < 1. When calculating bounds
            //this would be truncated to 0 and the UiControl does not need to be drawn.
            //Most commonly, this state would occur as a result of user error as values < 1 are applied to Transform_Mode.Percent
            //We could just enforce a miniumum size of 1x1 for a user control, but this places restrictions on the user. Best to
            //handle the 0 size of the control and allow the user to do it if they choose to.

            if (PixelSize.X == 0 || PixelSize.Y == 0)
                drawThisFrame = false;

            //todo: check if object is positioned off screen
        }

        public virtual void EndDraw()
        {
            drawThisFrame = true;

            foreach (UiControl control in children)
                control.EndDraw();
        }

        public virtual void Draw()
        {
            CheckNeedDraw();
            if (!drawThisFrame)
                return;

            currentState.Background.Draw(ui, Bounds, PixelPosition, PixelSize);

            foreach (UiControl control in children)
                control.Draw();
        }

        public Rectangle GetSpriteSheetRectangle(Rectangle pos, Texture2D tex)
        {
            int x, y, w, h = 0;

            x = (int)(pos.X * tex.Width);
            y = (int)(pos.Y * tex.Height);
            w = (int)((pos.Width - pos.X) * tex.Width);
            h = (int)((pos.Height - pos.Y) * tex.Height);

            return new Rectangle(x, y, w, h);
        }

        internal virtual void OnResize()
        {
            //when the size and position are set with SetSize and SetPosition
            //the size and position relative to the parent are calculated
            //when we resize, we use this information to calculate new pixel dimensions
            //which are used throughout the code
            pixPosition = PositionToPixel(perPosition);
            pixSize = SizeToPixel(perSize);

            foreach (UiControl control in children)
                control.OnResize();
        }

        #region Mouse

        protected virtual void OnMouseDown(UiControlMouseEventArgs e) => MouseDown?.Invoke(this, e);

        protected virtual void OnMouseUp(UiControlMouseEventArgs e) => MouseUp?.Invoke(this, e);

        protected virtual void OnMouseClick(UiControlMouseEventArgs e) => MouseClick?.Invoke(this, e);

        protected virtual void OnMouseScroll(UiControlMouseEventArgs e) => MouseScroll?.Invoke(this, e);

        protected virtual void OnMouseEnter(UiControlMouseEventArgs e) => MouseEnter?.Invoke(this, e);

        protected virtual void OnMouseLeave(UiControlMouseEventArgs e) => MouseLeave?.Invoke(this, e);

        #endregion

        public override string ToString() => $"{Name}: {this.GetType().Name}";
    }

    public class ControlState
    {
        protected Interface ui;

        public TextureRegion Background { get; set; } = new TextureRegion();

        public virtual void Load(Interface ui)
        {
            this.ui = ui;
            Background.Load(ui);
        }

        public override string ToString() => this.GetType().Name;
    }

    public class UiControlMouseEventArgs
    {
        public MouseExtensions Mouse { get; set; }

        public Vector2i MousePosition { get; set; }

        public bool Handled { get; set; }

        public UiControlMouseEventArgs(MouseExtensions m, Vector2i mp)
        {
            Mouse = m;
            MousePosition = mp;
            Handled = false;
        }
    }

    public class UiControlKeyboardEventArgs
    {
        public KeyboardExtensions Keyboard { get; set; }

        public bool Handled { get; set; }

        public UiControlKeyboardEventArgs(KeyboardExtensions kb)
        {
            Keyboard = kb;
            Handled = false;
        }
    }

    public class TextureRegion
    {
        public string TexturePath { get; set; }

        public Rectangle Region { get; set; } = Rectangle.Empty;

        public Texture2D Texture { get; set; }

        public Color Color { get; set; } = new Color(Color.White, 0);

        public bool HasTexture => Texture != null && Region != Rectangle.Empty;

        public bool Wrap { get; set; }

        public void Load(Interface ui)
        {
            if (!string.IsNullOrEmpty(TexturePath))
            {
#pragma warning disable CS4014
                new AsyncUiTask().Run(() =>
                {
                    Texture = ContentLoader.LoadTexture(ui.GraphicsDevice, TexturePath);
                    Region = new Rectangle(0, 0, Texture.Width, Texture.Height);
                });
#pragma warning restore CS4014
            }
        }

        public void Draw(Interface ui, Rectangle bounds, Vector2i positionInPixels, Vector2i sizeInPixels)
        {
            Rectangle b = bounds;
            Rectangle destinationRectangle = new Rectangle((int)b.X, (int)b.Y, b.Width, b.Height);

            if (HasTexture)
            {
                if (!Wrap)
                    ui.DrawRectangle(Texture, Color, positionInPixels, sizeInPixels, Region);
                else
                {
                    Vector2i pip = positionInPixels;

                    Vector2i s = new Vector2i(Region.Width, Region.Height);
                    Vector2i d = sizeInPixels - pip;

                    List<VertexPositionColorTexture> v = new List<VertexPositionColorTexture>();
                    List<int> i = new List<int>();

                    //todo: this needs to be tested
                    for (int x = pip.X; x < d.X; x += s.X)
                        for (int y = pip.Y; y < d.Y; y += s.Y)
                            ui.DeferRectangle(Texture, Color, new Vector2i(x, y), s, Region, ref v, ref i);

                    ui.DrawVertices(v.ToArray(), i.ToArray(), Texture);
                }
            }
            else
                ui.DrawRectangle(Color,
                new Vector2i(destinationRectangle.X, destinationRectangle.Y),
                new Vector2i(destinationRectangle.Width, destinationRectangle.Height));
        }
    }
}