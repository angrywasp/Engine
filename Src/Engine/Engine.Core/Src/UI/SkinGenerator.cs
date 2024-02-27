using Engine.UI.Controls;
using Microsoft.Xna.Framework;

namespace Engine.UI
{
    public class DefaultSkinGenerator
    {
        private const int DEFAULT_FONT_SIZE = 24;

        private static readonly Color BACKGROUND_NORMAL = new Color(Color.Black, 64);
        private static readonly Color BACKGROUND_OVER = new Color(Color.CornflowerBlue, 128);
        private static readonly Color BACKGROUND_ACTIVE = new Color(Color.LawnGreen, 192);
        private static readonly Color BACKGROUND_DISABLED = new Color(Color.DimGray, 255);

        private static readonly Color TEXT_COLOR = Color.LightGray;
        private static readonly Color TEXT_COLOR_OVER = Color.LightGray;
        private static readonly Color TEXT_COLOR_ACTIVE = Color.Black;
        private static readonly Color TEXT_COLOR_DISABLED = Color.DimGray;

        public static readonly string FONT_PATH = "Engine/Fonts/Default.fontpkg";

        public static SkinElement Blank<T>() where T : ControlState, new()
        {
            var s = new SkinElement();

            s.States.Add("Normal", new T());
            s.States.Add("Over", new T());
            s.States.Add("Active", new T());
            s.States.Add("Disabled", new T());

            return s;
        }

        public static SkinElement Default()
        {
            var s = new SkinElement();

            var n = new ControlState();
            var o = new ControlState();
            var a = new ControlState();
            var d = new ControlState();

            n.Background.Color = BACKGROUND_NORMAL;
            o.Background.Color = BACKGROUND_OVER;
            a.Background.Color = BACKGROUND_ACTIVE;
            d.Background.Color = BACKGROUND_DISABLED;

            s.States.Add("Normal", n);
            s.States.Add("Over", o);
            s.States.Add("Active", a);
            s.States.Add("Disabled", d);

            return s;
        }

        public static SkinElement CheckBoxButton()
        {
            var s = Default();

            var n = new ControlState();
            var o = new ControlState();
            var a = new ControlState();
            var d = new ControlState();

            n.Background.Color = new Color(Color.Red, 64);
            o.Background.Color = new Color(Color.Red, 128);
            a.Background.Color = new Color(Color.Red, 192);
            d.Background.Color = new Color(Color.Black, 255);

            s.States.Add("CheckedNormal", n);
            s.States.Add("CheckedOver", o);
            s.States.Add("CheckedActive", a);
            s.States.Add("CheckedDisabled", d);
            return s;
        }

        public static SkinElement ListBox()
        {
            var s = new SkinElement();

            var n = new ListBoxState();
            var o = new ListBoxState();
            var a = new ListBoxState();
            var d = new ListBoxState();

            GenerateListBoxState(ref n, BACKGROUND_NORMAL);
            GenerateListBoxState(ref o, BACKGROUND_OVER);
            GenerateListBoxState(ref a, BACKGROUND_ACTIVE);
            GenerateListBoxState(ref d, BACKGROUND_DISABLED);

            s.States.Add("Normal", n);
            s.States.Add("Over", o);
            s.States.Add("Active", a);
            s.States.Add("Disabled", d);

            return s;
        }

        public static SkinElement TextBox()
        {
            var s = new SkinElement();

            var n = new TextBoxState();
            var o = new TextBoxState();
            var a = new TextBoxState();
            var d = new TextBoxState();

            GenerateTextBoxState(ref n, BACKGROUND_NORMAL, TEXT_COLOR);
            GenerateTextBoxState(ref o, BACKGROUND_OVER, TEXT_COLOR_OVER);
            GenerateTextBoxState(ref a, BACKGROUND_ACTIVE, TEXT_COLOR_ACTIVE);
            GenerateTextBoxState(ref d, BACKGROUND_DISABLED, TEXT_COLOR_DISABLED);

            s.States.Add("Normal", n);
            s.States.Add("Over", o);
            s.States.Add("Active", a);
            s.States.Add("Disabled", d);

            return s;
        }

        public static SkinElement ProgressBar()
        {
            var s = new SkinElement();

            var n = new ProgressBarState();
            var o = new ProgressBarState();
            var a = new ProgressBarState();
            var d = new ProgressBarState();

            GenerateProgressBarState(ref n, BACKGROUND_NORMAL, new Color(Color.LawnGreen, 64));
            GenerateProgressBarState(ref o, BACKGROUND_NORMAL, new Color(Color.LawnGreen, 128));
            GenerateProgressBarState(ref a, BACKGROUND_NORMAL, new Color(Color.LawnGreen, 192));
            GenerateProgressBarState(ref d, BACKGROUND_NORMAL, new Color(Color.LawnGreen, 255));

            s.States.Add("Normal", n);
            s.States.Add("Over", o);
            s.States.Add("Active", a);
            s.States.Add("Disabled", d);

            return s;
        }

        public static SkinElement TextInput()
        {
            var s = new SkinElement();

            var n = new TextInputState();
            var o = new TextInputState();
            var a = new TextInputState();
            var d = new TextInputState();

            GenerateTextInputState(ref n, BACKGROUND_NORMAL);
            GenerateTextInputState(ref o, BACKGROUND_NORMAL);
            GenerateTextInputState(ref a, BACKGROUND_NORMAL);
            GenerateTextInputState(ref d, BACKGROUND_NORMAL);

            s.States.Add("Normal", n);
            s.States.Add("Over", o);
            s.States.Add("Active", a);
            s.States.Add("Disabled", d);

            return s;
        }

        public static SkinElement Slider()
        {
            var s = new SkinElement();

            var n = new SliderState();
            var o = new SliderState();
            var a = new SliderState();
            var d = new SliderState();

            GenerateSliderState(ref n);
            GenerateSliderState(ref o);
            GenerateSliderState(ref a);
            GenerateSliderState(ref d);

            s.States.Add("Normal", n);
            s.States.Add("Over", o);
            s.States.Add("Active", a);
            s.States.Add("Disabled", d);

            return s;
        }

        public static void GenerateListBoxState(ref ListBoxState state, Color backgroundColor)
        {
            state.Background.Color = backgroundColor;
            state.FontPath = FONT_PATH;
            state.FontSize = DEFAULT_FONT_SIZE;
            state.TextColor = TEXT_COLOR;

            state.Item.Background.Color = Color.Transparent;
            state.ItemHover.Background.Color = BACKGROUND_OVER;
            state.ItemSelected.Background.Color = BACKGROUND_ACTIVE;

            state.Item.TextColor = TEXT_COLOR;
            state.ItemHover.TextColor = TEXT_COLOR_OVER;
            state.ItemSelected.TextColor = TEXT_COLOR_ACTIVE;
        }

        public static void GenerateTextBoxState(ref TextBoxState state, Color backgroundColor, Color textColor)
        {
            state.Background.Color = backgroundColor;
            state.FontPath = FONT_PATH;
            state.FontSize = DEFAULT_FONT_SIZE;
            state.TextColor = textColor;
        }

        public static void GenerateProgressBarState(ref ProgressBarState state, Color backgroundColor, Color foregroundColor)
        {
            state.Background.Color = backgroundColor;
            state.Foreground.Color = foregroundColor;
        }

        private static void GenerateTextInputState(ref TextInputState state, Color backgroundColor)
        {
            state.Background.Color = backgroundColor;
            state.FontPath = FONT_PATH;
            state.FontSize = DEFAULT_FONT_SIZE;
            state.TextColor = TEXT_COLOR;
            state.CaretColor = Color.Red;
        }

        private static void GenerateSliderState(ref SliderState state)
        {
            state.Background.Color = BACKGROUND_NORMAL;
            state.NearEnd.Color = BACKGROUND_NORMAL;
            state.FarEnd.Color = BACKGROUND_NORMAL;
        }

        public SkinDefinition Generate()
        {
            var skin = new SkinDefinition();

            skin.AddElement(Blank<ControlState>(), "BlankControl");
            skin.AddElement(Blank<TextBoxState>(), "BlankTextBox");

            skin.AddElement(Default(), "Button");
            skin.AddElement(CheckBoxButton(), "CheckBox");
            skin.AddElement(Default(), "ComboBox");
            skin.AddElement(ListBox(), "ListBox");
            skin.AddElement(ProgressBar(), "ProgressBar");
            skin.AddElement(CheckBoxButton(), "RadioButton");
            skin.AddElement(Default(), "ScrollableContainer");
            skin.AddElement(Slider(), "Slider");
            skin.AddElement(TextBox(), "TextBox");
            skin.AddElement(TextInput(), "TextInput");
            skin.AddElement(Default(), "UiControl");

            var s = new SkinElement();

            var normalState = new TextBoxState();
            var overState = new TextBoxState();
            var activeState = new TextBoxState();
            var disabledState = new TextBoxState();

            GenerateTextBoxState(ref normalState, Color.Transparent, TEXT_COLOR);
            GenerateTextBoxState(ref overState, Color.Transparent, TEXT_COLOR_OVER);
            GenerateTextBoxState(ref activeState, Color.Transparent, TEXT_COLOR_ACTIVE);
            GenerateTextBoxState(ref disabledState, Color.Transparent, TEXT_COLOR_DISABLED);

            s.States.Add("Normal", normalState);
            s.States.Add("Over", overState);
            s.States.Add("Active", activeState);
            s.States.Add("Disabled", disabledState);

            skin.AddElement(s, "ComboBoxTextBox");
            skin.AddElement(s, "CheckBoxTextBox");
            skin.AddElement(s, "ButtonTextBox");
            skin.AddElement(s, "RadioButtonTextBox");
            skin.AddElement(Default(), "ComboBoxButton");
            skin.AddElement(ListBox(), "ComboBoxListBox");

            skin.AddElement(Default(), "SliderHandle");

            return skin;
        }
    }
}
