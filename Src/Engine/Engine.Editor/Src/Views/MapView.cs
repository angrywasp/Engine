using Engine.Editor.Forms;
using Engine.World;
using Microsoft.Xna.Framework;

namespace Engine.Editor.Views
{
    public class MapView : View3D
    {
        private Map asset;

        public override void InitializeView(string path)
        {
            base.InitializeView(path);

            engine.Scene.MapLoaded += (_, map) => {
                CreateView(map, path);
            };
            
            engine.Scene.LoadMap(path);
        }

        private void CreateView(Map map, string assetPath)
        {
            asset = map;
            var form = new MapForm(engine, map, assetPath);
            ShowForm(form);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (asset != null)
            {
                engine.Interface.ScreenMessages.WriteStaticText(2, "Objects: " + asset.Objects.Count, Color.White);
                engine.Interface.ScreenMessages.WriteStaticText(3, "Meshes: " + asset.StaticMeshes.Count, Color.White);
            }
        }
    }
}

 /*           engine.Scene.MapLoaded += (_, map) =>
            {
                this.map = map;
                Scripting.ScriptEngine.LoadType(typeof(MapScripts), false);
                MapScripts.Load(map);

                cameraController.Position = map.EditorCameraPosition;
                cameraController.Yaw = map.EditorCameraYaw;
                cameraController.Pitch = map.EditorCameraPitch;

                foreach (var mo in map.Objects)
                {
                    if (mo.GameObject is HeightmapTerrain)
                    {
                        terrain = (HeightmapTerrain)mo.GameObject;
                        Scripting.ScriptEngine.LoadType(typeof(TerrainScripts), false);
                        TerrainScripts.Load(terrain);
                    }

                    if (mo.GameObject is WaterPlane)
                        waterPlane = (WaterPlane)mo.GameObject;
                }

                engine.Interface.CreateDefaultForm();
            };

            engine.Scene.LoadMap(path);
        }

        public override bool ShouldUpdateCameraController()
        {
            if (!base.ShouldUpdateCameraController())
                return false;

            if (gizmo.ActiveAxis != GizmoAxis.None)
                return false;

            return true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            engine.Interface.ScreenMessages.WriteStaticText(0, "FPS: " + FPSCounter.FramePerSecond.ToString(), Color.White);
            if (terrain != null)
                engine.Interface.ScreenMessages.WriteStaticText(1, $"Terrain: {QTNode.NodesVisible} nodes visible", Color.White);

            if (map != null)
            {
                engine.Interface.ScreenMessages.WriteStaticText(2, "Objects: " + map.Objects.Count, Color.White);
                engine.Interface.ScreenMessages.WriteStaticText(3, "Meshes: " + map.StaticMeshes.Count, Color.White);
            }

            if (engine.Interface.Terminal.Visible)
                return;

            gizmo.Update(engine.Camera, gameTime);

            if (engine.Input.Keyboard.KeyJustPressed(Keys.P))
                engine.Scene.Physics.SimulationUpdate = !engine.Scene.Physics.SimulationUpdate;

            if (engine.Input.Keyboard.KeyJustPressed(Keys.T))
                ToggleForm(new HeightmapTerrainForm(engine, terrain));

            if (engine.Input.Keyboard.KeyJustPressed(Keys.I))
                ToggleForm(new InstanceManagerForm(engine, map, terrain));

            if (engine.Input.Keyboard.KeyJustPressed(Keys.S) && engine.Input.Keyboard.KeyDown(Keys.LeftControl))
            {
                ContentLoader.SaveJson(map, EngineFolders.ContentPathVirtualToReal(assetPath));
                string path = EngineFolders.ContentPathVirtualToReal(Path.ChangeExtension(assetPath, ".instancemanager"));
                ContentLoader.SaveJson(map.InstanceManager, path);
                TerrainScripts.Save();

                Log.Instance.SetColor(ConsoleColor.DarkCyan);
                Log.Instance.Write($"Saved '{assetPath}'");
                Log.Instance.SetColor(ConsoleColor.White);
            }

            if (map != null)
            {
                map.EditorCameraPosition = cameraController.Position;
                map.EditorCameraYaw = cameraController.Yaw;
                map.EditorCameraPitch = cameraController.Pitch;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (!engine.Interface.Terminal.Visible)
            {
                gizmo.Draw();
                gizmo.Draw2D();
            }
        }
    }
}*/