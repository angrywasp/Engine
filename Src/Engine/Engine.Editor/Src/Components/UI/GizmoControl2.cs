using System.Collections.Generic;
using System.Numerics;
using Engine.Configuration;
using Engine.Editor.Components.Gizmo;
using Engine.UI;
using Engine.UI.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Editor.Components.UI
{
    public class GizmoControl2<T, U> where T : IGizmoSelection<U>, new()
    {
        public event EngineEventHandler<T, Quaternion> Rotate;
        public event EngineEventHandler<T,Vector3> Translate;

        private UiControl parent;
        private EngineCore engine;

        private GizmoComponent<T, U> gizmo;

        private UiControl ctrl;
        private RadioButton btnLocal;
        private RadioButton btnWorld;
        private CheckBox chkPrecise;
        private RadioButton btnRotate;
        private RadioButton btnTranslate;
        private CheckBox chkRotateSnap;
        private CheckBox chkTranslateSnap;
        private TextInput txtRotateSnap;
        private TextInput txtTranslateSnap;

        public GizmoAxis ActiveAxis => gizmo.ActiveAxis;

        public bool Visible
        {
            get => ctrl.Visible;
            set => ctrl.Visible = value;
        }

        public GizmoControl2(EngineCore engine, UiControl parent, Vector2i position, string name)
        {
            this.engine = engine;
            this.parent = parent;
            this.gizmo = new GizmoComponent<T, U>(engine);
            ConstructLayout(name, parent, position);

            gizmo.Rotate += (s, v) => { Rotate?.Invoke(s, v); };
            gizmo.Translate += (s, v) => { Translate?.Invoke(s, v); };
        }

        public void ConstructLayout(string name, UiControl parent, Vector2i position)
        {
            var x = 10;
            var sz = new Vector2i(500, 105);
            var btnSz = new Vector2i(150, 25);
            var chkSz = new Vector2i(75, 25);
            var txtSz = new Vector2i(75, 25);

            ctrl = parent.Control(position, sz, $"{name}_GizmoControl", "BlankControl");
            var radioGroupSpace = ctrl.Control(new Vector2i(x, 0), new Vector2i(150, 105), "SpaceGroup", "BlankControl");

            btnLocal = radioGroupSpace.RadioButton(new Vector2i(0, 10), btnSz, "btnLocal", "Local");
            btnWorld = radioGroupSpace.RadioButton(new Vector2i(0, 40), btnSz, "btnWorld", "World");
            chkPrecise = radioGroupSpace.CheckBox(new Vector2i(0, 70), btnSz, "chkPrecise", "Precise");

            x += btnSz.X + 10;
            var radioGroupMode = ctrl.Control(new Vector2i(x, 0), new Vector2i(150, 105), "ModeGroup", "BlankControl");

            btnRotate = radioGroupMode.RadioButton(new Vector2i(0, 10), btnSz, "btnRotate", "Rotate");
            btnTranslate = radioGroupMode.RadioButton(new Vector2i(0, 40), btnSz, "btnTranslate", "Translate");

            x += btnSz.X + 10;

            chkRotateSnap = ctrl.CheckBox(new Vector2i(x, 10), chkSz, "chkRotateSnap", "Snap");
            chkTranslateSnap = ctrl.CheckBox(new Vector2i(x, 40), chkSz, "chkTranslateSnap", "Snap");

            x += chkSz.X + 10;

            txtRotateSnap = ctrl.TextInput(new Vector2i(x, 10), txtSz, "txtRotateSnap");
            txtTranslateSnap = ctrl.TextInput(new Vector2i(x, 40), txtSz, "txtTranslateSnap");

            Refresh();
        }

        public void Refresh()
        {
            btnRotate.CheckChanged -= btnRotate_CheckChanged;
            btnTranslate.CheckChanged -= btnTranslate_CheckChanged;

            btnLocal.CheckChanged -= btnLocal_CheckChanged;
            btnWorld.CheckChanged -= btnWorld_CheckChanged;
            chkPrecise.CheckChanged -= chkPrecise_CheckChanged;

            chkRotateSnap.CheckChanged -= chkRotateSnap_CheckChanged;
            chkTranslateSnap.CheckChanged -= chkTranslateSnap_CheckChanged;

            txtRotateSnap.TextChanged -= txtRotateSnap_TextChanged;
            txtTranslateSnap.TextChanged -= txtTranslateSnap_TextChanged;

            switch (gizmo.ActiveMode)
            {
                case GizmoMode.Translate:
                    btnTranslate.Checked = true;
                    break;
                case GizmoMode.Rotate:
                    btnRotate.Checked = true;
                    break;
            }

            switch (gizmo.ActiveSpace)
            {
                case Gizmo.TransformSpace.Local:
                    btnLocal.Checked = true;
                    break;
                case Gizmo.TransformSpace.World:
                    btnWorld.Checked = true;
                    break;
            }

            chkPrecise.Checked = Settings.Engine.EditorGizmo.PrecisionModeEnabled;

            chkRotateSnap.Checked = Settings.Engine.EditorGizmo.RotationSnapEnabled;
            chkTranslateSnap.Checked = Settings.Engine.EditorGizmo.TranslationSnapEnabled;

            txtRotateSnap.Text = Settings.Engine.EditorGizmo.RotationSnapValue.ToString();
            txtTranslateSnap.Text = Settings.Engine.EditorGizmo.TranslationSnapValue.ToString();

            btnRotate.CheckChanged += btnRotate_CheckChanged;
            btnTranslate.CheckChanged += btnTranslate_CheckChanged;

            btnLocal.CheckChanged += btnLocal_CheckChanged;
            btnWorld.CheckChanged += btnWorld_CheckChanged;
            chkPrecise.CheckChanged += chkPrecise_CheckChanged;

            chkRotateSnap.CheckChanged += chkRotateSnap_CheckChanged;
            chkTranslateSnap.CheckChanged += chkTranslateSnap_CheckChanged;

            txtRotateSnap.TextChanged += txtRotateSnap_TextChanged;
            txtTranslateSnap.TextChanged += txtTranslateSnap_TextChanged;
        }

        private void btnRotate_CheckChanged(CheckBox sender, bool isChecked)
        {
            if (!isChecked)
                return;

            gizmo.ActiveMode = GizmoMode.Rotate;
            Refresh();
        }

        private void btnTranslate_CheckChanged(CheckBox sender, bool isChecked)
        {
            if (!isChecked)
                return;

            gizmo.ActiveMode = GizmoMode.Translate;
            Refresh();
        }

        private void btnLocal_CheckChanged(CheckBox sender, bool isChecked)
        {
            if (!isChecked)
                return;

            gizmo.ActiveSpace = Gizmo.TransformSpace.Local;
            Settings.Engine.EditorGizmo.Space = Configuration.TransformSpace.Local;
            Settings.Save();
            Refresh();
        }

        private void btnWorld_CheckChanged(CheckBox sender, bool isChecked)
        {
            if (!isChecked)
                return;

            gizmo.ActiveSpace = Gizmo.TransformSpace.World;
            Settings.Engine.EditorGizmo.Space = Configuration.TransformSpace.World;
            Settings.Save();
            Refresh();
        }

        private void chkPrecise_CheckChanged(CheckBox sender, bool isChecked)
        {
            Settings.Engine.EditorGizmo.PrecisionModeEnabled = isChecked;
            Settings.Save();
            Refresh();
        }

        private void chkRotateSnap_CheckChanged(CheckBox sender, bool isChecked)
        {
            Settings.Engine.EditorGizmo.RotationSnapEnabled = isChecked;
            Settings.Save();
            Refresh();
        }

        private void chkTranslateSnap_CheckChanged(CheckBox sender, bool isChecked)
        {
            Settings.Engine.EditorGizmo.TranslationSnapEnabled = isChecked;
            Settings.Save();
            Refresh();
        }

        private void txtRotateSnap_TextChanged(TextInput sender, string oldValue, string newValue)
        {
            if (float.TryParse(newValue, out float parsed))
            {
                Settings.Engine.EditorGizmo.RotationSnapValue = parsed;
                Settings.Save();
            }
        }

        private void txtTranslateSnap_TextChanged(TextInput sender, string oldValue, string newValue)
        {
            if (float.TryParse(newValue, out float parsed))
            {
                Settings.Engine.EditorGizmo.TranslationSnapValue = parsed;
                Settings.Save();
            }
        }

        public void SelectItems(IEnumerable<U> items)
        {
            gizmo.SelectedItems.Clear();
            foreach (var i in items)
            {
                T t = new T();
                t.Tag = i;
                gizmo.SelectedItems.Add(t);
            }
            
            gizmo.ActivePivot = PivotType.SelectionCenter;
        }

        public void Update(GameTime gameTime)
        {
            if (engine.Interface.Terminal.Visible)
                return;
            
            gizmo.Update(engine.Camera, gameTime);
        }

        public void Draw()
        {
            if(engine.Interface.Terminal.Visible)
                return;
            
            engine.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            gizmo.Draw();
            gizmo.Draw2D();
        }

        public void Clear() => gizmo.Clear();
    }
}