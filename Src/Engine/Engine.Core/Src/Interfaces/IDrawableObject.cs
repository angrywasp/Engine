using System.Numerics;
using Engine.Content.Model.Instance;
using Engine.Cameras;
using Engine.Scene;
using Microsoft.Xna.Framework;

namespace Engine.Interfaces
{
    public interface IDrawableObjectInstanced : IDrawableObject
    {
        WorldTransform3 GlobalTransform { get; }
        MeshInstance Mesh { get; }
    }

    public interface IShadowCaster
    {
        public bool Intersects(BoundingFrustum frustum);
        public bool Intersects(BoundingSphere sphere);

        void RenderShadowMap(Matrix4x4 lightView, Matrix4x4 lightProjection);
    }

    public interface IDrawableObjectDeferred : IDrawableObject
    {
        void RenderToGBuffer(Camera camera);

        void ReconstructShading(Camera camera);
    }

    public interface IDrawableObjectForward : IDrawableObject
    {
        void RenderForwardPass(Camera camera);
    }

    public interface IDrawableObject
    {
        Render_Group RenderGroup { get; }

        bool ShouldDraw(Camera camera);

        void SetBuffers(LBuffer lBuffer, GBuffer gBuffer);

        void PreDrawReflection(Camera camera, Matrix4x4 matrix, Vector4 plane);

        void PostDrawReflection(Camera camera, Matrix4x4 matrix, Vector4 plane);
    }
}
