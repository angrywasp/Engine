using System.Numerics;
using Engine.Editor.Forms;
using System.Threading.Tasks;
using AngryWasp.Logger;
using Engine.World;
using Engine.World.Objects;

namespace Engine.Editor.Views
{
    public class EntityTypeView : View3D
    {
        private GameObject asset;

        public override void InitializeView(string assetPath)
        {
            base.InitializeView(assetPath);
            engine.Scene.MapLoaded += (_, _) => { CreateView(assetPath); };
            CreateDefaultScene();
        }

        private void CreateView(string assetPath)
        {
            var mo = engine.Scene.CreateMapObject<RuntimeMapObject>("PreviewMapObject", assetPath);
            mo.Loaded += (mapObject, gameObject) =>
            {
                Log.Instance.Write("Entity loaded");
                /*if (mo.GameObject is HeightmapTerrain)
                {
                    var terrain = (HeightmapTerrain)mo.GameObject;

                    Scripting.ScriptEngine.LoadType(typeof(TerrainScripts), false);
                    TerrainScripts.Load(terrain);

                    ToggleForm(new HeightmapTerrainForm(engine, terrain));
                }*/

                asset = gameObject;

                var form = new EntityTypeForm(engine, asset, assetPath);
                form.Reload += (s) => { engine.Scene.Map.QueueObjectRemove(asset.UID); };

                ShowForm(form);
            };

            mo.RemovedFromMap += (s, e) => {
                CreateView(assetPath);
            };
        }
    }
}