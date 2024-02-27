using System.Numerics;
using Engine.UI.Controls;
using Microsoft.Xna.Framework;

namespace Engine.UI
{
    //todo: pass skin element names in as method arguments
    public static class FormBuilder
    {
        public static UiControl Control(this UiControl parent, Vector2 position, Vector2 size, string name = null, string skinElement = null)
        {
            var c = parent.Add<UiControl>(position, size, name, skinElement);
            c.Load();

            return c;
        }

        public static UiControl Control(this UiControl parent, Vector2i position, Vector2i size, string name = null, string skinElement = null)
        {
            var c = parent.Add<UiControl>(position, size, name, skinElement);
            c.Load();

            return c;
        }

        #region AddScrollableContainer

        private static ScrollableContainer SkinScrollableContainerChildren(this ScrollableContainer c)
        {
            c.HorizontalSlider.ApplySkinElement("Slider");
            c.VerticalSlider.ApplySkinElement("Slider");

            c.HorizontalSlider.Handle.ApplySkinElement("SliderHandle");
            c.VerticalSlider.Handle.ApplySkinElement("SliderHandle");
            return c;
        }

        public static ScrollableContainer ScrollableContainer(this UiControl parent, Vector2 position, Vector2 size, string name = null)
        {
            var c = parent.Add<ScrollableContainer>(position, size, name, "ScrollableContainer");
            c.Load();
            return SkinScrollableContainerChildren(c);
        }

        public static ScrollableContainer ScrollableContainer(this UiControl parent, Vector2i position, Vector2i size, string name = null)
        {
            var c = parent.Add<ScrollableContainer>(position, size, name, "ScrollableContainer");
            c.Load();
            return c.SkinScrollableContainerChildren();
        }

        #endregion

        #region AddButton

        public static Button Button(this UiControl parent, Vector2 position, Vector2 size, string name = null, string text = null, string skinElement = "Button", string textBoxSkinElement = "ButtonTextBox")
        {
            var c = parent.Add<Button>(position, size, name, skinElement);
            c.TextBoxSkinElementKey = textBoxSkinElement;
            c.Text = text;
            c.Load();

            return c;
        }

        public static Button Button(this UiControl parent, Vector2i position, Vector2i size, string name = null, string text = null, string skinElement = "Button", string textBoxSkinElement = "ButtonTextBox")
        {
            var c = parent.Add<Button>(position, size, name, skinElement);
            c.TextBoxSkinElementKey = textBoxSkinElement;
            c.Text = text;
            c.Load();

            return c;
        }

        #endregion

        #region AddComboBox

        public static ComboBox ComboBox(this UiControl parent, Vector2 position, Vector2 size, string name = null)
        {
            var c = parent.Add<ComboBox>(position, size, name, "ComboBox");
            c.TextBoxSkinElementKey = "ComboBoxTextBox";
            c.ButtonSkinElementKey = "ComboBoxButton";
            c.ListBoxSkinElementKey = "ComboBoxListBox";
            c.Load();

            return c;
        }

        public static ComboBox ComboBox(this UiControl parent, Vector2i position, Vector2i size, string name = null)
        {
            var c = parent.Add<ComboBox>(position, size, name, "ComboBox");
            c.TextBoxSkinElementKey = "ComboBoxTextBox";
            c.ButtonSkinElementKey = "ComboBoxButton";
            c.ListBoxSkinElementKey = "ComboBoxListBox";
            c.Load();

            return c;
        }

        #endregion

        #region AddCheckBox

        public static CheckBox CheckBox(this UiControl parent, Vector2 position, Vector2 size, string name = null, string text = null)
        {
            var c = parent.Add<CheckBox>(position, size, name, "CheckBox");
            c.TextBoxSkinElementKey = "CheckBoxTextBox";
            c.Text = text;
            c.Load();

            return c;
        }

        public static CheckBox CheckBox(this UiControl parent, Vector2i position, Vector2i size, string name = null, string text = null)
        {
            var c = parent.Add<CheckBox>(position, size, name, "CheckBox");
            c.TextBoxSkinElementKey = "CheckBoxTextBox";
            c.Text = text;
            c.Load();

            return c;
        }

        #endregion

        #region AddListBox

        public static ListBox ListBox(this UiControl parent, Vector2 position, Vector2 size, string name = null, string skinElement = "ListBox")
        {
            var c = parent.Add<ListBox>(position, size, name, skinElement);
            c.Load();

            return c;
        }

        public static ListBox ListBox(this UiControl parent, Vector2i position, Vector2i size, string name = null, string skinElement = "ListBox")
        {
            var c = parent.Add<ListBox>(position, size, name, skinElement);
            c.Load();

            return c;
        }

        public static MultiSelectListBox MultiSelectListBox(this UiControl parent, Vector2 position, Vector2 size, string name = null, string skinElement = "ListBox")
        {
            var c = parent.Add<MultiSelectListBox>(position, size, name, skinElement);
            c.Load();

            return c;
        }

        public static MultiSelectListBox MultiSelectListBox(this UiControl parent, Vector2i position, Vector2i size, string name = null, string skinElement = "ListBox")
        {
            var c = parent.Add<MultiSelectListBox>(position, size, name, skinElement);
            c.Load();

            return c;
        }

        #endregion

        #region AddTextBox

        public static TextBox TextBox(this UiControl parent, Vector2 position, Vector2 size, string name = null, string text = null, string skinElement = "TextBox")
        {
            var c = parent.Add<TextBox>(position, size, name, skinElement);
            c.Text = text;
            c.Load();

            return c;
        }

        public static TextBox TextBox(this UiControl parent, Vector2i position, Vector2i size, string name = null, string text = null, string skinElement = "TextBox")
        {
            var c = parent.Add<TextBox>(position, size, name, skinElement);
            c.Text = text;
            c.Load();

            return c;
        }

        #endregion

        #region AddProgressBar

        public static ProgressBar ProgressBar(this UiControl parent, Vector2 position, Vector2 size, string name = null)
        {
            var c = parent.Add<ProgressBar>(position, size, name, "ProgressBar");
            c.Load();

            return c;
        }

        public static ProgressBar ProgressBar(this UiControl parent, Vector2i position, Vector2i size, string name = null)
        {
            var c = parent.Add<ProgressBar>(position, size, name, "ProgressBar");
            c.Load();

            return c;
        }

        #endregion

        #region AddSlider

        private static Slider SetSliderDimensions(this Slider c)
        {
            c.Handle.SetSize(new Vector2(0.1f, 1f));
            c.SetEndSize(new Vector2(0.05f, 1f));

            return c;
        }

        private static Slider SkinSliderChildren(this Slider c)
        {
            c.Handle.ApplySkinElement("SliderHandle");
            c.Value = 0.5f;

            return c;
        }

        public static Slider Slider(this UiControl parent, Vector2 position, Vector2 size, string name = null, float min = 0, float max = 100, float value = 0)
        {
            var c = parent.Add<Slider>(position, size, name, "Slider");
            c.Load();

            c.SkinSliderChildren();
            c.SetSliderDimensions();

            c.Minimum = min;
            c.Maximum = max;
            c.Value = value;

            return c;
        }

        public static Slider Slider(this UiControl parent, Vector2i position, Vector2i size, string name = null, float min = 0, float max = 100, float value = 0)
        {
            var c = parent.Add<Slider>(position, size, name, "Slider");
            c.Load();

            c.SkinSliderChildren();
            c.SetSliderDimensions();

            c.Minimum = min;
            c.Maximum = max;
            c.Value = value;

            return c;
        }

        #endregion

        #region AddTextInput

        public static TextInput TextInput(this UiControl parent, Vector2 position, Vector2 size, string name = null)
        {
            var c = parent.Add<TextInput>(position, size, name, "TextInput");
            c.Load();

            return c;
        }

        public static TextInput TextInput(this UiControl parent, Vector2i position, Vector2i size, string name = null)
        {
            var c = parent.Add<TextInput>(position, size, name, "TextInput");
            c.Load();

            return c;
        }

        #endregion

        #region AddRadioButton

        public static RadioButton RadioButton(this UiControl parent, Vector2 position, Vector2 size, string name = null, string text = null)
        {
            var c = parent.Add<RadioButton>(position, size, name, "RadioButton");
            c.TextBoxSkinElementKey = "RadioButtonTextBox";
            c.Text = text;
            c.Load();

            return c;
        }

        public static RadioButton RadioButton(this UiControl parent, Vector2i position, Vector2i size, string name = null, string text = null)
        {
            var c = parent.Add<RadioButton>(position, size, name, "RadioButton");
            c.TextBoxSkinElementKey = "RadioButtonTextBox";
            c.Text = text;
            c.Load();

            return c;
        }

        #endregion
    }
}