using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System.Numerics;
using System.Collections;

namespace Engine.UI.Controls
{
    public abstract class UiControlBase : IEnumerable<UiControl>
    {
        protected internal List<UiControl> children = new List<UiControl>();

        protected GraphicsDevice graphicsDevice;
        protected bool isLoaded = false;
        protected UiForm form;
        protected UiControlBase parent;
        protected Interface ui;

        public UiControl this[string name]
        {
            get
            {
                foreach (var c in children)
                    if (c.Name == name)
                        return c;

                throw new ArgumentException(name + " does not exist in this control");
            }
        }

        public UiControl this[int index]
        {
            get
            {
                if (index < 0 || index >= children.Count)
                    throw new ArgumentOutOfRangeException();

                return children[index];
            }
        }

        public UiForm Form => form;
        public UiControlBase Parent => parent;
        public bool Visible { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public object Tag { get; set; }

        public abstract Vector2i PixelPosition { get; }
        public abstract Vector2i PixelSize { get; }
        public abstract Vector2 PercentPosition { get; }
        public abstract Vector2 PercentSize { get; }
        public abstract Rectangle Bounds { get; }

        public virtual void Load()
        {
            foreach (var item in children)
                item.Load();

            isLoaded = true;
        }

        public IEnumerator<UiControl> GetEnumerator() => children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => children.GetEnumerator();
    }
}
