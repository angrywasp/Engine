using System.Numerics;
using Engine.Editor.Forms;
using Engine.Objects.GameObjects.Controllers;
using System.Threading.Tasks;
using Engine.World.Objects;
using Engine.World.Components;
using AngryWasp.Random;
using Microsoft.Xna.Framework;
using Engine.World;

namespace Engine.Editor.Views
{
    public class MaterialView : View3D
    {
        private string addedComponentName;
        private MaterialForm form;

        public override void InitializeView(string path)
        {
            base.InitializeView(path);

            cameraController.RotateAroundObject = true;
            cameraController.UpdatePosition = false;

            cameraController.Position = new Vector3(0, 0, 2.5f);
            cameraController.Yaw = 90;

            form = new MaterialForm(engine, path);

            engine.Scene.MapLoaded += async (_, _) =>
            {
                var controller = await engine.TypeFactory.CreateGameObjectAsync<RotationController>("DemoContent/MapObjects/RotationController.type").ConfigureAwait(false);
                var mo = engine.Scene.CreateMapObject<RuntimeMapObject>("Asset");
                mo.Loaded += async (m, g) =>
                {
                    await CreateMeshComponent(mo, "Engine/Renderer/Meshes/Sphere.mesh", controller, path, RandomString.AlphaNumeric(8)).ConfigureAwait(false);
                    form.ShapeChanged += async (shape) =>
                    {
                        mo.GameObject.UnloadComponent(addedComponentName);
                        await CreateMeshComponent(mo, shape, controller, path, RandomString.AlphaNumeric(8)).ConfigureAwait(false);
                    };
                    ShowForm(form);
                };
            };

            CreateDefaultScene();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            cameraController.Position = new Vector3(0, 0, 2.5f);
        }

        private async Task CreateMeshComponent(RuntimeMapObject parent, string shape, RotationController controller, string materialPath, string name)
        {
            addedComponentName = name;
            var mc = await parent.AddMeshComponent(new MeshComponentType
            {
                Mesh = shape,
                LocalTransform = new WorldTransform3()
            }, addedComponentName).ConfigureAwait(false);

            parent.GameObject.AssignController(controller);
            await mc.Mesh.SetMaterialAsync(engine, materialPath, 0).ConfigureAwait(false);
            form.UpdateMeshMaterial(mc.Mesh.SubMeshes[0].Material);
        }
    }
}