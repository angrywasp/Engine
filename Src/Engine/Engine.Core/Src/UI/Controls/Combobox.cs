using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Numerics;

namespace Engine.UI.Controls
{
    public enum ComboBox_DropDirection
    {
        Down,
        Up
    }

    public class ComboBox : UiControl
    {
        public event EngineEventHandler<ComboBox, (int Index, ListBoxItem Item)> SelectedItemChanged;

        private Button button;
        private TextBox textBox;
        private ListBox listBox;

        private int dropDownitemCount;

        private string textBoxSkinElementKey;
        private string buttonSkinElementKey;
        private string listBoxSkinElementKey;

        public string TextBoxSkinElementKey
        {
            get { return textBoxSkinElementKey; }
            set
            {
                textBoxSkinElementKey = value;

                if (textBox != null)
                    textBox.ApplySkinElement(value);
            }
        }

        public string ButtonSkinElementKey
        {
            get { return buttonSkinElementKey; }
            set
            {
                buttonSkinElementKey = value;

                if (button != null)
                    button.ApplySkinElement(value);
            }
        }

        public string ListBoxSkinElementKey
        {
            get { return listBoxSkinElementKey; }
            set
            {
                listBoxSkinElementKey = value;

                if (listBox != null)
                    listBox.ApplySkinElement(value);
            }
        }

        public int DropDownItemCount
        {
            get { return dropDownitemCount; }
            set
            {
                dropDownitemCount = value;

                if (isLoaded)
                    needUpdateChildSizes = true;
            }
        }

        public ComboBox_DropDirection DropDirection { get; set; } = ComboBox_DropDirection.Down;

        public override void SetSize(Vector2 size)
        {
            base.SetSize(size);
            needUpdateChildSizes = true;
        }

        public override void SetSize(Vector2i size)
        {
            base.SetSize(size);
            needUpdateChildSizes = true;
        }

        public int SelectedIndex
        {
            get
            {
                if (listBox != null)
                    return listBox.SelectedIndex;

                return -1;
            }
            set
            {
                if (listBox == null)
                    return;

                listBox.SelectedIndex = value;
            }
        }

        public ListBoxItem SelectedItem => listBox.SelectedItem;

        public override void Load()
        {
            if (children.Count > 0)
            {
                textBox = (TextBox)this["textBox"];
                textBox.SetPosition(textBox.PercentPosition);
                textBox.SetSize(textBox.PercentSize);

                button = (Button)this["button"];
                button.SetPosition(button.PercentPosition);
                button.SetSize(button.PercentSize);

                listBox = (ListBox)this["listBox"];
                listBox.SetPosition(listBox.PercentPosition);
                listBox.SetSize(listBox.PercentSize);
            }
            else
            {
                textBox = Add<TextBox>(Vector2.Zero, Vector2.Zero, "textBox",textBoxSkinElementKey);
                button = Add<Button>(Vector2.Zero, Vector2.Zero, "button", buttonSkinElementKey);
                listBox = Add<ListBox>(Vector2.Zero, Vector2.Zero, "listBox",listBoxSkinElementKey);
                listBox.Visible = false;

                button.MouseClick += button_MouseClick;
                listBox.SelectedItemChanged += listBox_SelectedItemChanged;

                //todo: maybe we need a skin element here
                button.TextBoxSkinElementKey = "BlankTextBox";
            }

            base.Load();
        }

        void button_MouseClick(UiControl sender, UiControlMouseEventArgs e)
        {
            listBox.HoverIndex = -1;
            listBox.Visible = !listBox.Visible;
        }

        void listBox_SelectedItemChanged(UiControl sender, (int Index, ListBoxItem Item) selectedItem)
        {
            textBox.Text = selectedItem.Item == null ? string.Empty : selectedItem.Item.ToString();
            SelectedIndex = selectedItem.Index;
            listBox.Visible = false;
            listBox.HoverIndex = -1;
            SelectedItemChanged?.Invoke(this, selectedItem);
        }

        bool needUpdateChildSizes = true;
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (needUpdateChildSizes)
            {
                button.SetSize(Vector2i.One * PixelSize.Y);
                button.SetPosition(new Vector2i(PixelSize.X - button.PixelSize.X, 0));

                textBox.SetSize(new Vector2i(PixelSize.X - button.PixelSize.X, PixelSize.Y));
                textBox.SetPosition(Vector2i.Zero);

                listBox.SetSize(new Vector2i(PixelSize.X - button.PixelSize.X, listBox.ItemHeight * listBox.Items.Count));
                listBox.SetPosition(new Vector2i(0, DropDirection == ComboBox_DropDirection.Down ? PixelSize.Y : -listBox.PixelSize.Y));
                
                needUpdateChildSizes = false;
            }

            button.Update(gameTime);
            listBox.Update(gameTime);
            textBox.Update(gameTime);
        }

        public override void Draw()
        {
            base.Draw();
            if (!drawThisFrame)
                return;

            button.Draw();
            listBox.Draw();
            textBox.Draw();
        }

        public void DataBind<T>(Dictionary<string, T> dataSource) => listBox.DataBind(dataSource);
    }

    public class ComboBoxState : ControlState
    {

    }
}
