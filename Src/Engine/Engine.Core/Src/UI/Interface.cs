using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Newtonsoft.Json;
using AngryWasp.Helpers;
using Engine.Content;
using Engine.Helpers;
using Engine.Input;
using Engine.UI.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Engine.UI.UIRenderer;
using AngryWasp.Logger;
using System;
using Engine.Graphics.Vertices;
using System.Runtime.CompilerServices;

namespace Engine.UI
{
    public class Interface
    {
        public event EngineEventHandler<Interface> SizeChanged;

        private GraphicsDevice graphicsDevice;
        private UiForm mainForm;
        private UiForm defaultLoadingScreen = null;
        private Vector2i size = Vector2i.Zero;
        private Vector2i position = Vector2i.Zero;
        private Terminal terminal;
        private ScreenMessages screenMessages;
        private FocusManager focusManager;
        private UIRenderer uiRenderer;
        private InputDeviceManager input;
        private SkinDefinition skin;
        private FontPackage defaultFont;

        public GraphicsDevice GraphicsDevice => graphicsDevice;
        public UiForm MainForm => mainForm;
        public Vector2i Size => size;
        public Vector2i Position => position;
        public Terminal Terminal => terminal;
        public ScreenMessages ScreenMessages => screenMessages;
        public InputDeviceManager Input => input;
        public SkinDefinition Skin => skin;
        public FontPackage DefaultFont => defaultFont;

        public Interface(GraphicsDevice graphicsDevice, InputDeviceManager input)
        {
            this.graphicsDevice = graphicsDevice;
            this.input = input;

            defaultFont = ContentLoader.LoadFontPackage(graphicsDevice, "Engine/Fonts/Default.fontpkg");

            uiRenderer = new UIRenderer(graphicsDevice);
            focusManager = new FocusManager(this);
            terminal = new Terminal(this, input);
            screenMessages = new ScreenMessages(this);

            focusManager.FocusGained += (s, e) => {
                ScreenMessages.Write($"Focus Gained: {e.ControlPath}", Color.GreenYellow);
            };

            focusManager.FocusLost += (s, e) => {
                ScreenMessages.Write($"Focus Lost: {e.ControlPath}", Color.Plum);
            };

            LoadDefaultSkin();
            CreateDefaultForm();
        }

        public void Resize(int x, int y, int w, int h)
        {
            size = new Vector2i(w, h);
            position = new Vector2i(x, y);
            uiRenderer.SetBufferSize();

            if (mainForm != null)
                mainForm.Resize();

            SizeChanged?.Invoke(this);
        }

        public void Update(GameTime gameTime)
        {
            mainForm.Update(gameTime);
            terminal.Update(gameTime);
            screenMessages.Update(gameTime);
            focusManager.Update(mainForm);
        }

        public void Draw()
        {
            //graphicsDevice.BlendState = BlendState.Additive;
            if (mainForm.Visible)
                mainForm.Draw();

            if (terminal.Visible)
                terminal.Draw();
            else
                screenMessages.Draw(graphicsDevice);

            uiRenderer.DrawDeferredText();
        }

        public void TraverseChildren(UiControl start, Action<UiControl> callback)
        {
            callback(start);

            for (int i = 0; i < start.children.Count; i++)
                TraverseChildren(start.children[i], callback);
        }

        public void CreateDefaultForm()
        {
            var form = new UiForm(this);
            form.ApplySkinElement("BlankControl");
            form.Control(Vector2.Zero, Vector2.One, skinElement: "BlankControl");
            SetMainForm(form);
        }

        public Action<int> CreateDefaultLoadingScreen(int count)
        {
            if (defaultLoadingScreen == null)
            {
                var cb = new ControlState
                {
                    Background = new TextureRegion
                    {
                        TexturePath = "DemoContent/Textures/Background.texture",
                        Color = Color.White
                    }
                };

                var tb = new TextBoxState
                {
                    FontSize = 72,
                    TextColor = Color.White
                };

                cb.Load(this);
                tb.Load(this);

                var ls = new SkinElement();
                ls.States.Add("Normal", cb);
                ls.States.Add("Over", cb);
                ls.States.Add("Active", cb);
                ls.States.Add("Disabled", cb);

                var ts = new SkinElement();
                ts.States.Add("Normal", tb);
                ts.States.Add("Over", tb);
                ts.States.Add("Active", tb);
                ts.States.Add("Disabled", tb);

                skin.AddElement(ls, "LoadingScreen");
                skin.AddElement(ts, "LoadingText");
            }

            defaultLoadingScreen = new UiForm(this);
            defaultLoadingScreen.Add<UiControl>(Vector2.Zero, Vector2.One, skinElement: "LoadingScreen");

            var text = defaultLoadingScreen.Add<TextBox>(new Vector2(0.1f, 0.1f), new Vector2(0.5f, 0.5f), "LoadingText", skinElement: "LoadingText");
            text.TextCropMode = TextBox.Text_Crop_Mode.None;
            text.TextVerticalAlignment = TextBox.Align_Mode.Near;
            text.TextHorizontalAlignment = TextBox.Align_Mode.Near;
            text.AutoSize = true;
            text.Text = "Loading...";

            var pb = defaultLoadingScreen.Add<ProgressBar>(new Vector2(0.1f, 0.75f), new Vector2(0.8f, 0.05f), "ProgressBar", "ProgressBar");
            pb.Orientation = ProgressBar.Orientation_Mode.LeftToRight;
            pb.Maximum = count;
            pb.Value = 0;

            defaultLoadingScreen.Load();

            Log.Instance.Write("Applying loading screen");
            SetMainForm(defaultLoadingScreen);

            return (val) => {
                pb.Value = val;
            };
        }

        #region Load

        public SkinDefinition LoadSkin(string path)
        {
            path = EngineFolders.ContentPathVirtualToReal(path);

            string jsonString = File.ReadAllText(path);
            var s = JsonConvert.DeserializeObject<SkinDefinition>(jsonString, ContentLoader.DefaultJsonSerializerOptions());

            foreach (var item in s.Elements)
                item.Value.Load(this);

            skin = s;
            return s;
        }

        public SkinDefinition LoadDefaultSkin()
        {
            var s = new DefaultSkinGenerator().Generate();
            foreach (var item in s.Elements)
                item.Value.Load(this);

            skin = s;
            return s;
        }

        #endregion

        //Adds a control tot he current main form
        public void AddControl(UiControl control)
        {
            Log.Instance.Write($"Applying UI element {control.Name}");

            foreach (var item in control.children)
            {
                TraverseChildren(item, x => x.ApplySkinElement(x.SkinElementKey));
                item.ApplySkinElement(item.SkinElementKey);
            }

            control.Load();
            mainForm.Add(control);
            mainForm.Resize();
        }

        //set the main form
        public void SetMainForm(UiForm form)
        {
            Log.Instance.Write($"Setting main form {form.Name}");

            foreach (var item in form.children)
            {
                TraverseChildren(item, x => x.ApplySkinElement(x.SkinElementKey));
                item.ApplySkinElement(item.SkinElementKey);
            }

            form.Load();
            form.Resize();
            mainForm = form;
        }

        #region UIRenderer redirect methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawString(string message, Vector2i p, Font f, Color c) =>
            uiRenderer.DrawString(StringHelper.TabsToSpaces(message, 4), p + position, f, c);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawString(string message, Vector2i p, Font f, Color c, out VertexPositionColorTexture[] vertices, out int[] indices) =>
            uiRenderer.DrawString(StringHelper.TabsToSpaces(message, 4), p + position, f, c, out vertices, out indices);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawString(string message, Vector2i p, Font f, Color c, int caretCharPosition, out Vector2i caretDrawPosition) =>
            uiRenderer.DrawString(StringHelper.TabsToSpaces(message, 4), p + position, f, c, caretCharPosition, out caretDrawPosition);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeferText(VertexPositionColorTexture[] vertices, int[] indices, Texture2D texture) =>
            uiRenderer.DeferText(vertices, indices, texture);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeferText(Dictionary<int, DeferredText> dt, VertexPositionColorTexture[] vertices, int[] indices, Texture2D texture) =>
            uiRenderer.DeferText(dt, vertices, indices, texture);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawDeferredText() =>
            uiRenderer.DrawDeferredText();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawDeferredText(Dictionary<int, DeferredText> dt) =>
            uiRenderer.DrawDeferredText(dt);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRectangle(Texture2D tex, Color c, Vector2i p, Vector2i s, Rectangle r) =>
            uiRenderer.DrawRectangle(tex, c, p + position, s, r);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRectangle(Texture2D tex, Color c, Vector2i p, Vector2i s) =>
            uiRenderer.DrawRectangle(tex, c, p + position, s);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRectangle(Texture2D tex, Color c, Vector2i p, Vector2i s, BlendState b) =>
            uiRenderer.DrawRectangle(tex, c, p + position, s, b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRectangle(Color c, Vector2i p, Vector2i s) =>
            uiRenderer.DrawRectangle(c, p + position, s);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeferRectangle(Texture2D tex, Color c, Vector2i p, Vector2i s, Rectangle r,
            ref List<VertexPositionColorTexture> vpt, ref List<int> ind) =>
            uiRenderer.DeferRectangle(tex, c, p + position, s, r, ref vpt, ref ind);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawVertices(VertexPositionColorTexture[] vpt, int[] ind, Texture2D tex) =>
            uiRenderer.DrawVertices(vpt, ind, tex);

        #endregion
    }
}
