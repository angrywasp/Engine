using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using AngryWasp.Helpers;
using AngryWasp.Random;
using Engine.Debug.Shapes;
using Engine.Editor.Scripts;
using Engine.World;
using Engine.World.Objects;
using Microsoft.Xna.Framework;

namespace Engine.Editor.InstanceEditor.Tools
{
    public class PaintTool : InstanceTool
    {
        public const int DEFAULT_RADIUS = 3;
        public const int DEFAULT_STRENGTH = 3;

        private Vector3 lastPaintedPosition = Vector3.Zero;

        public float Radius
        {
            get => toolShape.Radius;
            set => toolShape.Radius = value;
        }

        public float Density { get; set; } = DEFAULT_STRENGTH;
        
        public PaintTool(EngineCore engine, Map map, HeightmapTerrain terrain) : base(engine, map, terrain)
        {
            this.toolShape = new DebugCircle(Vector3.Zero, DEFAULT_RADIUS, 64, Color.Red);
        }

        protected override bool Active() => engine.Input.Mouse.ButtonDown(engine.Input.Mouse.RightButton);

        protected override void UpdateTool(Vector3 hitLocation, Vector3 normal)
        {
            if (MapScripts.IsPainting)
            {
                if (hitLocation == lastPaintedPosition)
                    return;

                for (int i = 0; i < Density; i++)
                {
                    var randomLocation = GetRandomPoint(hitLocation, toolShape.Radius);

                    Ray r = new Ray(new Vector3(randomLocation.X, 1000, randomLocation.Z), -Vector3.UnitY, 1000);
                    var rayHits = engine.Scene.Physics.RayCast(r);

                    if (rayHits.Count == 0)
                        continue;

                    Vector3 randomHitLocation;
                    r.GetPointOnRay(rayHits[0].T, out randomHitLocation);

                    var normalAtRandomLocation = rayHits[0].Normal;
                    randomLocation.Y = randomHitLocation.Y;

                    var w = AlignToTerrain ? (normalAtRandomLocation == Vector3.UnitY ? Matrix4x4.Identity : Matrix4x4.CreateWorld(hitLocation, Vector3.UnitZ, normalAtRandomLocation)) : Matrix4x4.Identity;

                    Vector3 scale = new Vector3(MapScripts.GetRandomScale());
                    Vector3 randomRot = MapScripts.GetRandomRotation();

                    Quaternion rotation =
                        Quaternion.CreateFromRotationMatrix(w) *
                        Quaternion.CreateFromYawPitchRoll(randomRot.Y, randomRot.X, randomRot.Z);

                    Matrix4x4 transform =
                        Matrix4x4.CreateScale(scale) *
                        Matrix4x4.CreateFromQuaternion(rotation) *
                        Matrix4x4.CreateTranslation(randomLocation);

                    if (MapScripts.IsRandomMesh)
                        AddInstancedMesh(MapScripts.GetRandomMesh(), transform);
                    else
                        AddInstancedMesh(MapScripts.PaintedMesh, transform);
                }

                lastPaintedPosition = hitLocation;
            }
        }

        private Vector3 GetRandomPoint(Vector3 location, float radius)
        {
            radius *= engine.Random.NextFloat();
            var d = engine.Random.NextFloat() * MathHelper.TwoPi;
            return new Vector3(radius * MathF.Cos(d) + location.X, location.Y, radius * MathF.Sin(d) + location.Z);
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
