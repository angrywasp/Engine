using System;
using System.Numerics;
using AngryWasp.Logger;
using Engine.Bitmap.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;

namespace TextureCubeProcessor.Common
{
    public static class CubeFace
    {
        public static Matrix4x4 CreateViewMatrix(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
        {
            var vector = Vector3.Normalize(cameraPosition - cameraTarget);
            var vector2 = -Vector3.Normalize(Vector3.Cross(cameraUpVector, vector));
            var vector3 = Vector3.Cross(-vector, vector2);
            Matrix4x4 result = Matrix4x4.Identity;
            result.M11 = vector2.X;
            result.M12 = vector3.X;
            result.M13 = vector.X;
            result.M14 = 0f;
            result.M21 = vector2.Y;
            result.M22 = vector3.Y;
            result.M23 = vector.Y;
            result.M24 = 0f;
            result.M31 = vector2.Z;
            result.M32 = vector3.Z;
            result.M33 = vector.Z;
            result.M34 = 0f;
            result.M41 = -Vector3.Dot(vector2, cameraPosition);
            result.M42 = -Vector3.Dot(vector3, cameraPosition);
            result.M43 = -Vector3.Dot(vector, cameraPosition);
            result.M44 = 1f;
            return result;
        }
        
        public static Matrix4x4 CreateProjectionMatrix(float near, float far) => Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI * .5f, 1, near, far);

        public static void WriteMapFace(GraphicsDevice g, RenderTargetCube rt, Effect e, CubeMapFace face)
        {
            switch (face)
            {
                case CubeMapFace.PositiveX:
                    e.Parameters["View"].SetValue(CreateViewMatrix(Vector3.Zero, Vector3.UnitX, Vector3.UnitY));
                    g.SetRenderTarget(rt, CubeMapFace.PositiveX);
                    break;
                case CubeMapFace.PositiveY:
                    e.Parameters["View"].SetValue(CreateViewMatrix(Vector3.Zero, Vector3.UnitY, -Vector3.UnitZ));
                    g.SetRenderTarget(rt, CubeMapFace.PositiveY);
                    break;
                case CubeMapFace.PositiveZ:
                    e.Parameters["View"].SetValue(CreateViewMatrix(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY));
                    g.SetRenderTarget(rt, CubeMapFace.PositiveZ);
                    break;
                case CubeMapFace.NegativeX:
                    e.Parameters["View"].SetValue(CreateViewMatrix(Vector3.Zero, -Vector3.UnitX, Vector3.UnitY));
                    g.SetRenderTarget(rt, CubeMapFace.NegativeX);
                    break;
                case CubeMapFace.NegativeY:
                    e.Parameters["View"].SetValue(CreateViewMatrix(Vector3.Zero, -Vector3.UnitY, Vector3.UnitZ));
                    g.SetRenderTarget(rt, CubeMapFace.NegativeY);
                    break;
                case CubeMapFace.NegativeZ:
                    e.Parameters["View"].SetValue(CreateViewMatrix(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY));
                    g.SetRenderTarget(rt, CubeMapFace.NegativeZ);
                    break;
            }

            g.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.White.ToVector4(), 1.0f, 0);
            e.DefaultProgram.Apply();
			g.DrawIndexedPrimitives(GLPrimitiveType.Triangles, 0, 0, 36);
            //g.SetRenderTarget(null);
        }
    }
}