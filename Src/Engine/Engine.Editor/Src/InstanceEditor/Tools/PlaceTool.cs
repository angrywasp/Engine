using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Engine.Debug.Shapes;
using Engine.Editor.Scripts;
using Engine.World;
using Engine.World.Objects;
using Microsoft.Xna.Framework;

namespace Engine.Editor.InstanceEditor.Tools
{
    public class PlaceTool : InstanceTool
    {
        private Vector3 lastPaintedPosition = Vector3.Zero;

        public PlaceTool(EngineCore engine, Map map, HeightmapTerrain terrain) : base(engine, map, terrain)
        {
            this.toolShape = new DebugCircle(Vector3.Zero, 0.1f, 6, Color.Red);
        }

        protected override bool Active() => engine.Input.Mouse.RightJustPressed;

        protected override void UpdateTool(Vector3 hitLocation, Vector3 normal)
        {
            if (MapScripts.IsPainting)
            {
                if (hitLocation == lastPaintedPosition)
                    return;

                var w = AlignToTerrain ? (normal == Vector3.UnitY ? Matrix4x4.Identity : Matrix4x4.CreateWorld(hitLocation, Vector3.UnitZ, normal)) : Matrix4x4.Identity;

                Vector3 scale = new Vector3(MapScripts.GetRandomScale());
                Vector3 randomRot = MapScripts.GetRandomRotation();

                Quaternion rotation =
                    Quaternion.CreateFromRotationMatrix(w) *
                    Quaternion.CreateFromYawPitchRoll(randomRot.Y, randomRot.X, randomRot.Z);

                Matrix4x4 transform =
                    Matrix4x4.CreateScale(scale) *
                    Matrix4x4.CreateFromQuaternion(rotation) *
                    Matrix4x4.CreateTranslation(hitLocation);

                if (MapScripts.IsRandomMesh)
                    AddInstancedMesh(MapScripts.GetRandomMesh(), transform);
                else
                    AddInstancedMesh(MapScripts.PaintedMesh, transform);

                lastPaintedPosition = hitLocation;
            }
        }

        public void AddInstancedMesh(string meshPath, Matrix4x4 transform)
        {
            Debugger.Break();
            /*Task.Run(async () => {
                await map.InstanceManager.Add(meshPath).ConfigureAwait(false);
                map.InstanceManager.Groups[meshPath].AddMesh(transform);
            });*/
        }
    }
}