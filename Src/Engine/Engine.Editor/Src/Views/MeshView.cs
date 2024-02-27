using Engine.Editor.Forms;
using Engine.World.Objects;
using Engine.World.Components;
using Engine.Content.Model.Instance;

namespace Engine.Editor.Views
{
    public class MeshView : View3D
    {
        private MeshInstance mesh;

        public override void InitializeView(string path)
        {
            base.InitializeView(path);

            engine.Scene.MapLoaded += (_, _) => {
                var mo = engine.Scene.CreateMapObject<RuntimeMapObject>("Asset");
                mo.Loaded += async (mapObject, gameObject) =>
                {
                    var meshComponent = await mo.AddMeshComponent(new MeshComponentType
                    {
                        Mesh = assetPath,
                        LocalTransform = new WorldTransform3()
                    }, "Container").ConfigureAwait(false);

                    this.mesh = meshComponent.Mesh;
                    ShowForm(new MeshForm(engine, path, meshComponent.Mesh, mo));
                };
            };

            CreateDefaultScene();
        }
    }
}
