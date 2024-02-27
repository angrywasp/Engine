using System.Collections.Generic;
using Engine.Cameras;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.Debug.Shapes;
using Engine.Graphics.Effects;
using Engine.Physics.Collidables;
using Engine.Debug.Shapes.Physics;
using BepuUtilities.Memory;
using Engine.Content.Model.Instance;

namespace Engine.Debug
{
    public class DebugRenderer
    {
        private List<IDebugShape> shapes = new List<IDebugShape>();

        private ColorEffect effect;
        private GraphicsDevice graphicsDevice;

        private ChildLoaderList<IDebugShape> loader = new ChildLoaderList<IDebugShape>();

        public ColorEffect Effect => effect;

        public DebugRenderer(GraphicsDevice graphicsDevice)
        {
            effect = new ColorEffect();
            this.graphicsDevice = graphicsDevice;
            effect.LoadAsync(graphicsDevice);

            loader.Run(default, default);
        }

        #region Add

        public T QueueShapeAdd<T>(T shape) where T : IDebugShape
        {
            loader.QueueObjectAdd(shape);
            return shape;
        }

        public DebugBoundingBox QueueShapeAdd(BoundingBox boundingBox, Color color)
        {
            var s = new DebugBoundingBox(boundingBox, color);
            loader.QueueObjectAdd(s);
            return s;
        }

        public DebugBoundingSphere QueueShapeAdd(BoundingSphere boundingSphere, Color color)
        {
            var s = new DebugBoundingSphere(boundingSphere, color);
            loader.QueueObjectAdd(s);
            return s;
        }

        public DebugBoundingFrustum QueueShapeAdd(BoundingFrustum boundingFrustum, Color color)
        {
            var s = new DebugBoundingFrustum(boundingFrustum, color);
            loader.QueueObjectAdd(s);
            return s;
        }

        public DebugPhysicsBox QueueShapeAdd(Box shape)
        {
            var s = new DebugPhysicsBox(shape);

            loader.QueueObjectAdd(s);
            return s;
        }

        public DebugPhysicsCapsule QueueShapeAdd(Capsule shape)
        {
            var s = new DebugPhysicsCapsule(shape);

            loader.QueueObjectAdd(s);
            return s;
        }

        public DebugPhysicsCylinder QueueShapeAdd(Cylinder shape)
        {
            var s = new DebugPhysicsCylinder(shape);

            loader.QueueObjectAdd(s);
            return s;
        }

        public DebugPhysicsSphere QueueShapeAdd(Sphere shape)
        {
            var s = new DebugPhysicsSphere(shape);

            loader.QueueObjectAdd(s);
            return s;
        }

        public DebugMesh QueueShapeAdd(MeshInstance shape, int subMeshIndex, Color color)
        {
            var s = new DebugMesh(shape, subMeshIndex, color);

            loader.QueueObjectAdd(s);
            return s;
        }

        public DebugPhysicsMesh QueueShapeAdd(Mesh shape, Buffer<BepuPhysics.Collidables.Triangle> buffer)
        {
            var s = new DebugPhysicsMesh(shape, buffer);

            loader.QueueObjectAdd(s);
            return s;
        }

        public void QueueShapeRemove<T>(T shape) where T : IDebugShape => shapes.Remove(shape);

        #endregion

        public void Update()
        {
            loader.Update(
                (i) => { shapes.Add(i); },
                (i) => { shapes.Remove(i); }
            );
        }

        public void Draw(Camera camera)
        {
            graphicsDevice.RasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
                FillMode = FillMode.WireFrame
            };

            effect.View = camera.View;
            effect.Projection = camera.Projection;

            foreach (var i in shapes)
            {
                effect.World = i.WorldMatrix;
                effect.Apply();
                i.Draw(graphicsDevice);
            }
        }
    }
}