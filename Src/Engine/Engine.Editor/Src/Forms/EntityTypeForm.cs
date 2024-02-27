using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using Engine.Content;
using Engine.Editor.Components;
using Engine.Editor.Components.Gizmo;
using Engine.Editor.Components.UI;
using Engine.Helpers;
using Engine.Physics;
using Engine.Physics.Collidables;
using Engine.UI;
using Engine.UI.Controls;
using Engine.World;
using Engine.World.Components;
using Engine.World.Components.Lights;
using Microsoft.Xna.Framework;

namespace Engine.Editor.Forms
{
    public class EntityTypeForm : EditorForm
    {
        public event EngineEventHandler<EntityTypeForm> Reload;

        private UiControl editorArea;
        private string path;
        private GameObject gameObject;
        private (UiControl Control, GizmoControl<ComponentGizmoSelection, Component> Gizmo) componentControl;
        private (UiControl Control, GizmoControl<PhysicsBodyGizmoSelection, IConvexShape> Gizmo) physicsControl;
        private static readonly Vector2i btnSize = new Vector2i(150, 25);
        private ComponentList lbx;

        public EntityTypeForm(EngineCore engine, GameObject gameObject, string path) : base(engine)
        {
            this.gameObject = gameObject;
            this.path = path;
            ConstructLayout();
            engine.Scene.Physics.SimulationUpdate = false;
            engine.Scene.Graphics.DebugRenderer.QueueShapeAdd(new BoundingSphere(Vector3.Zero, 0.05f), Color.Red);
        }

        private void ConstructLayout()
        {
            int w = ui.Size.X;
            int h = 105;
            int y = ui.Size.Y - h;

            editorArea = this.Control(new Vector2i(0, 0), new Vector2i(w - 200, y), "editorArea", "BlankControl");

            var bottomControlPanel = this.Control(new Vector2i(0, y), new Vector2i(w, h), "bottomControlPanel", "UiControl");
            var btnComponents = bottomControlPanel.RadioButton(new Vector2i(10, 10), btnSize, "btnComponents", "Components");
            var btnPhysics = bottomControlPanel.RadioButton(new Vector2i(10, 40), btnSize, "btnPhysics", "Physics");
            var btnSave = bottomControlPanel.Button(new Vector2i(10, 70), btnSize, "btnSave", "Save");
            
            var btnReload = bottomControlPanel.Button(new Vector2i(170, 70), btnSize, "btnReload", "Reload");

            componentControl = CreateComponentLayout(bottomControlPanel, w, h);
            physicsControl = CreatePhysicsLayout(bottomControlPanel, w, h);

            var sideControlPanel = this.Control(new Vector2i(w - 200, 0), new Vector2i(200, y), "sideControlPanel", "UiControl");
            lbx = new ComponentList(sideControlPanel, Vector2.Zero, Vector2.One, gameObject);

            lbx.SelectedItemsChanged += (sender, items) =>
            {
                if (componentControl.Gizmo.Visible)
                    componentControl.Gizmo.SelectItems(items.Select(x => (Component)x.Tag));
                else if (physicsControl.Gizmo.Visible)
                    physicsControl.Gizmo.SelectItems(items.Select(x => (IConvexShape)x.Tag));
            };

            btnComponents.CheckChanged += (s, isChecked) =>
            {
                if (!isChecked)
                    return;

                physicsControl.Control.Visible = false;
                physicsControl.Gizmo.Visible = false;
                componentControl.Control.Visible = true;
                componentControl.Gizmo.Visible = true;
                lbx.ListComponents();
            };

            btnPhysics.CheckChanged += (s, isChecked) =>
            {
                if (!isChecked)
                    return;

                componentControl.Control.Visible = false;
                componentControl.Gizmo.Visible = false;
                physicsControl.Control.Visible = true;
                physicsControl.Gizmo.Visible = true;
                lbx.ListPhysics();
            };

            btnSave.MouseClick += (s, e) => { Save(); };
            btnReload.MouseClick += (s, e) => { Reload?.Invoke(this); };

            btnComponents.Checked = true;
        }

        private (UiControl Control, GizmoControl<ComponentGizmoSelection, Component> Gizmo) CreateComponentLayout(UiControl parent, int w, int h)
        {
            var ctrl = parent.Control(new Vector2i(320, 0), new Vector2i(w - 320, h), "ComponentControl", "BlankControl");
            ctrl.Visible = false;

            var btnMeshComponent = ctrl.Button(new Vector2i(10, 10), btnSize, "btnMeshComponent", "Mesh");
            var btnSoundComponent = ctrl.Button(new Vector2i(170, 10), btnSize, "btnSoundComponent", "Sound");
            var btnEmitterComponent = ctrl.Button(new Vector2i(10, 40), btnSize, "btnEmitterComponent", "Emitter");
            var btnObjectComponent = ctrl.Button(new Vector2i(170, 40), btnSize, "btnObjectComponent", "Object");

            var btnDelete = ctrl.Button(new Vector2i(10, 70), btnSize, "btnDelete", "Delete");

            var btnDirectionalLightComponent = ctrl.Button(new Vector2i(330, 10), btnSize, "btnDirectionalLightComponent", "Dir");
            var btnSpotLightComponent = ctrl.Button(new Vector2i(330, 40), btnSize, "btnSpotLightComponent", "Spot");
            var btnPointLightComponent = ctrl.Button(new Vector2i(330, 70), btnSize, "btnPointLightComponent", "Point");

            btnDelete.MouseClick += (s, e) =>
            {
                if (lbx.SelectedItems == null)
                    return;

                foreach (var i in lbx.SelectedItems)
                {
                    if (i.Tag is not Component)
                        continue;

                    gameObject.UnloadComponent(((Component)i.Tag).Type.Name);
                }

                lbx.UpdateDataStore();
            };

            btnMeshComponent.MouseClick += async (s, e) =>
            {
                await BrowseAndAddType<MeshComponentType>(NodeType.Mesh, (componentType, browserPath) => {
                    componentType.Mesh = browserPath;
                    componentType.PhysicsBodyIndex = -1;
                }).ConfigureAwait(false);
            };

            btnSoundComponent.MouseClick += async (s, e) =>
            {
                await BrowseAndAddType<SoundComponentType>(NodeType.Sound, (componentType, browserPath) => {
                    componentType.Sound = browserPath;
                }).ConfigureAwait(false);
            };

            btnEmitterComponent.MouseClick += async (s, e) =>
            {
                await BrowseAndAddType<EmitterComponentType>(NodeType.Emitter, (componentType, browserPath) => {
                    componentType.ParticleSettings.Add(browserPath);
                }).ConfigureAwait(false);
            };

            btnObjectComponent.MouseClick += async (s, e) =>
            {
                await BrowseAndAddType<GameObjectComponentType>(NodeType.Type, (componentType, browserPath) => {
                    componentType.GameObjectPath = browserPath;
                }).ConfigureAwait(false);
            };

            btnDirectionalLightComponent.MouseClick += (s, e) =>
            {
                LoadComponent(CreateComponent<DirectionalLightComponentType>(GetFreeComponentName("DirectionalLight")));
            };

            btnSpotLightComponent.MouseClick += (s, e) =>
            {
                LoadComponent(CreateComponent<SpotLightComponentType>(GetFreeComponentName("SpotLight")));
            };

            btnPointLightComponent.MouseClick += (s, e) =>
            {
                LoadComponent(CreateComponent<PointLightComponentType>(GetFreeComponentName("PointLight")));
            };

            var gizmoControl = new GizmoControl<ComponentGizmoSelection, Component>(engine, parent, new Vector2i(parent.PixelSize.X - 500, 0), GizmoMode.UniformScale, "Component");

            gizmoControl.Translate += ComponentGizmoSelection.Translate;
            gizmoControl.Rotate += ComponentGizmoSelection.Rotate;
            gizmoControl.Scale += ComponentGizmoSelection.Scale;

            return (ctrl, gizmoControl);
        }

        private async Task BrowseAndAddType<T>(NodeType nodeType, Action<T, string> browseComplete) where T : ComponentType, new()
        {
            var browserResult = await Browse(nodeType).ConfigureAwait(false);
            if (!browserResult.ValidResult)
                return;

            if (gameObject.Type == null)
            {
                Log.Instance.WriteError("Cannot add components to the specified type");
                return;
            }

            var name = GetFreeComponentName(Path.GetFileNameWithoutExtension(browserResult.Path));

            T ct = new T();
            ct.Name = name;

            browseComplete(ct, browserResult.Path);
            LoadComponent(ct);
        }

        private T CreateComponent<T>(string name) where T : ComponentType, new()
        {
            if (gameObject.Type == null)
            {
                Log.Instance.WriteError("Cannot add components to the specified type");
                return null;
            }

            T ct = new T();
            ct.Name = name;

            return ct;
        }

        private (UiControl Control, GizmoControl<PhysicsBodyGizmoSelection, IConvexShape> Gizmo) CreatePhysicsLayout(UiControl parent, int w, int h)
        {
            var ctrl = parent.Control(new Vector2i(320, 0), new Vector2i(w - 320, h), "PhysicsContainer", "BlankControl");
            ctrl.Visible = false;

            var btnStatic = ctrl.RadioButton(new Vector2i(10, 10), btnSize, "btnStatic", "Static");
            var btnDynamic = ctrl.RadioButton(new Vector2i(10, 40), btnSize, "btnDynamic", "Dynamic");
            var btnDelete = ctrl.Button(new Vector2i(10, 70), btnSize, "btnDelete", "Delete");

            var btnBox = ctrl.Button(new Vector2i(170, 10), btnSize, "btnBox", "Box");
            var btnSphere = ctrl.Button(new Vector2i(170, 40), btnSize, "btnSphere", "Sphere");
            var btnCylinder = ctrl.Button(new Vector2i(170, 70), btnSize, "btnCylinder", "Cylinder");
            var btnCapsule = ctrl.Button(new Vector2i(330, 10), btnSize, "btnCapsule", "Capsule");
            var btnMesh = ctrl.Button(new Vector2i(330, 40), btnSize, "btnMesh", "Mesh");

            btnStatic.Checked = true;

            btnDelete.MouseClick += (s, e) =>
            {
                if (lbx.SelectedItems == null)
                    return;

                foreach (var i in lbx.SelectedItems)
                {
                    if (i.Tag is not IConvexShape)
                        continue;

                    gameObject.PhysicsModel.RemoveShape((IConvexShape)i.Tag);
                }

                lbx.UpdateDataStore();
            };

            btnBox.MouseClick += (s, e) => { AddShape(new Box(btnDynamic.Checked)); };
            btnSphere.MouseClick += (s, e) => { AddShape(new Sphere(btnDynamic.Checked)); };
            btnCylinder.MouseClick += (s, e) => { AddShape(new Cylinder(btnDynamic.Checked)); };
            btnCapsule.MouseClick += (s, e) => { AddShape(new Capsule(btnDynamic.Checked)); };
            btnMesh.MouseClick += async (s, e) =>
            {
                var fileBrowserResult = await new FileBrowser(engine.Interface, "*.collision").Show().ConfigureAwait(false);

                if (fileBrowserResult.Type != FileBrowser.File_Type.File)
                    return;

                string extension = Path.GetExtension(fileBrowserResult.Path).Trim('.').ToLower();
                var t = Helpers.GetNodeTypeFromExtension(extension);

                var m = new Mesh();
                m.Path = fileBrowserResult.Path;
                AddShape(m);
            };

            var gizmoControl = new GizmoControl<PhysicsBodyGizmoSelection, IConvexShape>(engine, parent, new Vector2i(parent.PixelSize.X - 500, 0), GizmoMode.NonUniformScale, "Physics");

            gizmoControl.Translate += PhysicsBodyGizmoSelection.Translate;
            gizmoControl.Rotate += PhysicsBodyGizmoSelection.Rotate;
            gizmoControl.Scale += PhysicsBodyGizmoSelection.Scale;

            return (ctrl, gizmoControl);
        }

        private void AddShape(IConvexShape shape)
        {
            CreatePhysicsModelIfRequired();
            shape.AddToScene(engine);
            gameObject.PhysicsModel.AddShape(shape);
            lbx.UpdateDataStore();
        }

        private void CreatePhysicsModelIfRequired()
        {
            if (!string.IsNullOrEmpty(gameObject.Type.PhysicsModel))
                return;

            var model = new PhysicsModel();
            model.Load(engine);

            var physicsModelPath = Path.ChangeExtension(path, "physics");
            gameObject.Type.PhysicsModel = physicsModelPath;

            gameObject.PhysicsModel = model;

            Save();
        } 

        private void Save()
        {
            if (gameObject.PhysicsModel != null)
                ContentLoader.SaveJson(gameObject.PhysicsModel, EngineFolders.ContentPathVirtualToReal(gameObject.Type.PhysicsModel));
                
            ContentLoader.SaveJson(gameObject.Type, EngineFolders.ContentPathVirtualToReal(path));
        }

        private string GetFreeComponentName(string prefix)
        {
            int start = 0;
            while (true)
            {
                string name = $"{prefix}_{start++}";
                if (!gameObject.Components.ContainsKey(name))
                    return name;
            }
        }

        public override bool ShouldUpdateCameraController() =>
            engine.Input.Mouse.InsideRect(editorArea.Bounds) &&
                componentControl.Gizmo.ActiveAxis == GizmoAxis.None &&
                physicsControl.Gizmo.ActiveAxis == GizmoAxis.None;

        private void LoadComponent(ComponentType ct)
        {
            gameObject.Type.Components.Add(ct);
#pragma warning disable CS4014
            gameObject.LoadComponentAsync(ct);
#pragma warning restore CS4014
        }

        public void ListProperties()
        {
            Type t = gameObject.Type.GetType();
            var allProperties = ReflectionHelper.Instance.GetProperties(t, Property_Access_Mode.Read | Property_Access_Mode.Write);

            Log.Instance.SetColor(ConsoleColor.Green);
            Log.Instance.Write("Type Properties");
            Log.Instance.SetColor(ConsoleColor.White);

            foreach (var p in allProperties)
                Log.Instance.Write($"\t{p.Value.PropertyType.Name} {p.Key}");
        }

        public void SetProperty<T>(string propertyName, T value)
        {
            SetPropertyInternal<T>(propertyName, gameObject.Type, value);
            ReloadFromType();
        }

        public void SetComponentProperty<T>(string componentName, string propertyName, T value)
        {
            GameObjectType dgo = gameObject.Type as GameObjectType;

            if (dgo == null)
            {
                Log.Instance.WriteError("Cannot add components to the specified type");
                return;
            }

            ComponentType ct = null;
            foreach (var c in dgo.Components)
            {
                if (c.Name == componentName)
                {
                    ct = c;
                    break;
                }
            }

            if (ct == null)
            {
                Log.Instance.SetColor(ConsoleColor.Red);
                Log.Instance.Write($"Component '{propertyName}' does not exist");
                Log.Instance.SetColor(ConsoleColor.White);
                return;
            }

            SetPropertyInternal<T>(propertyName, ct, value);
            ReloadFromType();
        }

        private void SetPropertyInternal<T>(string propertyName, object instance, T value)
        {
            Type t = instance.GetType();
            var allProperties = ReflectionHelper.Instance.GetProperties(t, Property_Access_Mode.Read | Property_Access_Mode.Write);

            if (!allProperties.ContainsKey(propertyName))
            {
                Log.Instance.SetColor(ConsoleColor.Red);
                Log.Instance.Write($"Property '{propertyName}' does not exist");
                Log.Instance.SetColor(ConsoleColor.White);
                return;
            }

            var property = allProperties[propertyName];
            object oldValue = property.GetValue(instance);
            string oldValueString = oldValue == null ? "Null" : oldValue.ToString();
            property.SetValue(instance, value);
            object newValue = property.GetValue(instance);
            string newValueString = newValue == null ? "Null" : newValue.ToString();

            Log.Instance.SetColor(ConsoleColor.Green);
            Log.Instance.Write($"Property '{propertyName}' changed: {oldValueString} -> {newValueString}");
            Log.Instance.SetColor(ConsoleColor.White);
        }

        public void ReloadFromType()
        {
            Debugger.Break();
            //gameObject.LoadFromTypeAsync();
        }

        public override void ViewUpdate(GameTime gameTime)
        {
            if (componentControl.Gizmo.Visible)
                componentControl.Gizmo.Update(gameTime);
            else if (physicsControl.Gizmo.Visible)
            {
                physicsControl.Gizmo.Update(gameTime);

                /*if (gameObject.PhysicsModel == null)
                    return;

                if (engine.Input.Mouse.LeftJustPressed)
                {
                    Ray r = engine.Input.Mouse.ToRay(engine.Camera.View, engine.Camera.Projection);
                    var rayHits = engine.Scene.Physics.RayCast(r);

                    if (rayHits.Count == 0)
                        return;

                    var body = gameObject.PhysicsModel.GetBodyForReference(rayHits[0].Collidable);

                    if (body == null)
                        return;

                    if (!engine.Input.Keyboard.KeyDown(Keys.LeftShift))
                        physicsControl.Gizmo.Clear();

                    physicsControl.Gizmo.SelectItem(body);
                }*/
            }
        }

        public override void ViewDraw()
        {
            if (componentControl.Gizmo.Visible)
                componentControl.Gizmo.Draw();
            else if (physicsControl.Gizmo.Visible)
                physicsControl.Gizmo.Draw();
        }
    }
}