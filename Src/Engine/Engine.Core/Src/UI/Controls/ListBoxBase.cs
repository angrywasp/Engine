using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Numerics;
using Engine.Graphics.Vertices;
using static Engine.UI.UIRenderer;

namespace Engine.UI.Controls
{
    public abstract class ListBoxBase : UiControl
    {
        public enum Vertical_Size_Mode
        {
            Fixed,
            FitToItems
        }

        protected List<ListBoxItem> items = new List<ListBoxItem>();
        protected int itemHeight = 0;
        protected int textIndent = 10;
        protected bool needUpdate = true;
        protected int hoverIndex = -1;
        protected Vertical_Size_Mode verticalSizeMode = Vertical_Size_Mode.FitToItems;

        public int HoverIndex
        {
            get { return hoverIndex; }
            set { hoverIndex = value; }
        }

        public IReadOnlyList<ListBoxItem> Items => items;

        public new ListBoxState CurrentState => (ListBoxState)currentState;

        public int ItemHeight => itemHeight;

        public Vertical_Size_Mode VerticalSizeMode
        {
            get { return verticalSizeMode; }
            set { verticalSizeMode = value; }
        }

        public override void SetPosition(Vector2 position)
        {
            base.SetPosition(position);
            needUpdate = true;
        }

        public override void SetPosition(Vector2i position)
        {
            base.SetPosition(position);
            needUpdate = true;
        }

        public override void SetSize(Vector2 size)
        {
            base.SetSize(size);
            needUpdate = true;
        }

        public override void SetSize(Vector2i size)
        {
            base.SetSize(size);
            needUpdate = true;
        }

        public void DataBind<T>(IEnumerable<KeyValuePair<string, T>> dataSource)
        {
            Clear();
            items.Clear();
            int idx = 0;
            foreach (var d in dataSource)
                items.Add(new ListBoxItem
                {
                    Text = d.Key,
                    Tag = d.Value,
                    Index = idx++
                });

            if (verticalSizeMode == Vertical_Size_Mode.FitToItems)
                base.SetSize(new Vector2i(PixelSize.X, itemHeight * items.Count));

            needUpdate = true;

            if (this.Parent.Parent is ScrollableContainer)
                ((ScrollableContainer)this.Parent.Parent).SetContent(this);
        }

        public abstract void Clear();

        private void CalculateFontDependentSizes()
        {
            if (CurrentState == null || CurrentState.Font == null)
            {
                itemHeight = 0;
                return;
            }

            itemHeight = CurrentState.Font.MeasureChar('|').Y;
        }

        public override void CheckNeedDraw()
        {
            base.CheckNeedDraw();

            if (!drawThisFrame)
                return;

            if (CurrentState == null || CurrentState.Font == null)
                drawThisFrame = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (needUpdate)
            {
                float currentYPosition = 0;
                foreach (ListBoxItem i in items)
                {
                    i.textSize = CurrentState.Font.MeasureString(i.Text).ToVector2();
                    i.itemBounds = new Rectangle(Bounds.X, Bounds.Y + (int)currentYPosition, Bounds.Width, (int)(i.textSize.Y));
                    currentYPosition += itemHeight;
                }

                needUpdate = false;
            }

            if (Visible)
            {
                if (ui.Input.Mouse.InsideRect(Bounds))
                {
                    hoverIndex = -1;
                    foreach (var i in items)
                    {
                        ++hoverIndex;
                        Rectangle r = i.itemBounds;

                        if (ui.Input.Mouse.InsideRect(r))
                            return;
                    }

                    hoverIndex = -1;
                }
            }
        }

        public override void Draw()
        {
            CheckNeedDraw();
            if (!drawThisFrame)
                return;

            //todo: override bounds and update scissor rect and preserve
            if (ScissorRectangle == Rectangle.Empty)
            {
                Rectangle b = Bounds;
                ScissorRectangle = new Rectangle(b.X, b.Y, b.Width, b.Height);
                ui.GraphicsDevice.ScissorRectangle = ScissorRectangle;
            }
            else
                ui.GraphicsDevice.ScissorRectangle = ScissorRectangle;

            if (currentState != null)
                currentState.Background.Draw(ui, Bounds, PixelPosition, PixelSize);

            Dictionary<int, DeferredText> deferredText = new Dictionary<int, DeferredText>();

            for (int x = 0; x < items.Count; x++)
            {
                ListBoxItem i = items[x];

                if (i.itemBounds.Y + itemHeight < Bounds.Top)
                    continue;

                if (i.itemBounds.Y > Bounds.Bottom)
                    break;

                var s = ResolveItemState(x);

                Rectangle r = i.itemBounds;
                s.Background.Draw(ui, r, new Vector2i(r.X, r.Y), new Vector2i(r.Width, r.Height));

                if (!string.IsNullOrEmpty(i.Text))
                {
                    ui.DrawString(i.Text, new Vector2i(r.X + textIndent, r.Y), CurrentState.Font, s.TextColor, out VertexPositionColorTexture[] vertices, out int[] indices);
                    ui.DeferText(deferredText, vertices, indices, CurrentState.Font.Texture);
                }
            }

            ui.DrawDeferredText(deferredText);
        }

        protected abstract ListBoxItemState ResolveItemState(int x);

        public override void EndDraw()
        {
            base.EndDraw();
            //ScissorRectangle = Rectangle.Empty;
            ui.GraphicsDevice.ScissorRectangle = ui.GraphicsDevice.Viewport.Bounds;
        }

        public override void ApplySkinElement(string elementKey)
        {
            base.ApplySkinElement(elementKey);
            CalculateFontDependentSizes();
        }

        public override void ApplySkinElementState(string key)
        {
            base.ApplySkinElementState(key);
            CalculateFontDependentSizes();
        }

        protected override void OnMouseLeave(UiControlMouseEventArgs e)
        {
            hoverIndex = -1;
            base.OnMouseLeave(e);
        }

        internal override void OnResize()
        {
            base.OnResize();
            needUpdate = true;
        }
    }

    public class ListBoxItem
    {
        internal Vector2 textSize;
        internal Rectangle itemBounds;

        public int Index { get; set; }
        public string Text { get; set; }
        public object Tag { get; set; }

        public override string ToString() => Text;
    }

    public class ListBoxItemState : ControlState
    {
        public Color TextColor { get; set; } = new Color(Color.White, 0);
    }

    public class ListBoxState : TextBoxState
    {
        public ListBoxItemState Item { get; set; } = new ListBoxItemState();

        public ListBoxItemState ItemHover { get; set; } = new ListBoxItemState();

        public ListBoxItemState ItemSelected { get; set; } = new ListBoxItemState();

        public override void Load(Interface ui)
        {
            base.Load(ui);
        }
    }
}
