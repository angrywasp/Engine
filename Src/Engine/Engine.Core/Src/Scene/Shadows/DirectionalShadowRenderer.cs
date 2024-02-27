using System;
using System.Collections.Generic;
using System.Numerics;
using Engine.Cameras;
using Engine.Content.Model;
using Engine.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;

namespace Engine.Scene
{
    public class DirectionalShadowRenderer
    {
        private int currentFreeDirectionalShadowMap;
        private List<(SharedMeshData SharedMeshData, List<IShadowCaster> MeshList)> shadowCasters =
            new List<(SharedMeshData SharedMeshData, List<IShadowCaster> MeshList)>();

        BoundingSphere[] boundingSphere = new BoundingSphere[ShadowConstants.NUM_CSM_SPLITS];
        Vector3[] frustumCornersWS = new Vector3[8];
        //Vector3[] frustumCornersVS = new Vector3[8];
        //Vector3[] splitFrustumCornersVS = new Vector3[8];
        //Plane[] _directionalClippingPlanes = new Plane[6];

        EngineCore engine;

        public DirectionalShadowRenderer(EngineCore engine)
        {
            this.engine = engine;
        }

        public DirectionalShadowMapEntry GetDirectionalShadowMapEntry()
        {
            DirectionalShadowMapEntry entry = new DirectionalShadowMapEntry();
            entry.Texture = new RenderTarget2D(engine.GraphicsDevice, ShadowConstants.Instance.ShadowResolution * ShadowConstants.NUM_CSM_SPLITS,
                                                   ShadowConstants.Instance.ShadowResolution, false, SurfaceFormat.Single,
                                                   DepthFormat.Depth32F, 0, RenderTargetUsage.DiscardContents);

            entry.Texture.Name = "CSMShadowMap" + (currentFreeDirectionalShadowMap++).ToString();
            return entry;
        }

        internal void GenerateShadowTexture(SceneGraphics sceneGraphics, GraphicsDevice graphicsDevice, DirectionalLightEntry entry, Camera camera)
        {
            graphicsDevice.SetRenderTarget(entry.ShadowMap.Texture);
            graphicsDevice.Clear(GraphicsDevice.DiscardWhite);

            // Get the corners of the frustum
            camera.Frustum.GetCorners(frustumCornersWS);
            //Matrix4x4 eyeTransform = camera.View;

            //frustumCornersWS.Transform(ref eyeTransform, frustumCornersVS);

            float[] depths = ShadowConstants.CalculateSplitDepths(camera, entry.Light.ShadowDistance);

            Vector3 lightDir = Vector3.Normalize(entry.Light.GlobalTransform.Matrix.Backward());

            for (int i = 0; i < ShadowConstants.NUM_CSM_SPLITS; i++)
                CreateLightViewProjectionMatrix(lightDir, camera, depths[i], depths[i + 1], out entry.Light.LightView[i], out entry.Light.LightProjection[i], out boundingSphere[i]);

            for (int i = 0; i < ShadowConstants.NUM_CSM_SPLITS; i++)
            {
                entry.Light.ClipPlanes[i].X = -depths[i];
                entry.Light.ClipPlanes[i].Y = -depths[i + 1];

                Matrix4x4 v = entry.Light.LightView[i];
                Matrix4x4 p = entry.Light.LightProjection[i];

                graphicsDevice.Viewport = new Viewport
                (
                    i * ShadowConstants.Instance.ShadowResolution, 0,
                    ShadowConstants.Instance.ShadowResolution,
                    ShadowConstants.Instance.ShadowResolution
                );

                shadowCasters.Clear();
                sceneGraphics.GetShadowCasters(boundingSphere[i], shadowCasters);

                foreach (var list in shadowCasters)
                {
                    graphicsDevice.SetVertexBuffers(list.SharedMeshData.ShadowBinding);
                    graphicsDevice.SetIndexBuffer(list.SharedMeshData.IndexBuffer);

                    foreach (var sc in list.MeshList)
                        sc.RenderShadowMap(v, p);
                }

                foreach (var ig in EngineCore.Instance.Scene.Graphics.DynamicInstancingGroupManager.Groups.Values)
                    ig.ShadowMap(ig.GetShadowCasters(boundingSphere[i]), v, p);
            }

            entry.Light.ShadowMap = entry.ShadowMap.Texture;
        }

        private Vector3 CreateLightViewProjectionMatrix(Vector3 lightDir, Camera camera, float minZ, float maxZ, out Matrix4x4 view, out Matrix4x4 projection, out BoundingSphere bs)
        {
            Vector3 cameraUpVector = Vector3Orientation.Up;
            if (System.Math.Abs(Vector3.Dot(cameraUpVector, lightDir)) > 0.9f)
            	cameraUpVector = Vector3Orientation.Forward;

            Matrix4x4 lightRotation = Matrix4x4.CreateLookAt(Vector3.Zero, -lightDir, cameraUpVector);

            // Transform the positions of the corners into the direction of the light
            for (int i = 0; i < frustumCornersWS.Length; i++)
                frustumCornersWS[i] = Vector3.Transform(frustumCornersWS[i], lightRotation);

            // Find the smallest box around the points
            Vector3 mins = frustumCornersWS[0], maxes = frustumCornersWS[0];
            for (int i = 1; i < frustumCornersWS.Length; i++)
            {
                Vector3 p = frustumCornersWS[i];
                if (p.X < mins.X) mins.X = p.X;
                if (p.Y < mins.Y) mins.Y = p.Y;
                if (p.Z < mins.Z) mins.Z = p.Z;
                if (p.X > maxes.X) maxes.X = p.X;
                if (p.Y > maxes.Y) maxes.Y = p.Y;
                if (p.Z > maxes.Z) maxes.Z = p.Z;
            }

            BoundingBox _lightBox = new BoundingBox(mins, maxes);
            float diagonalLength = (frustumCornersWS[0] - frustumCornersWS[6]).Length();

            diagonalLength *= 1.2f; //Without this, the shadow map isn't big enough in the world.
            float worldsUnitsPerTexel = diagonalLength / (float)ShadowConstants.Instance.ShadowResolution;

            Vector3 vBorderOffset = (new Vector3(diagonalLength, diagonalLength, diagonalLength) -
                (_lightBox.Max - _lightBox.Min)) * 0.5f;
            _lightBox.Max += vBorderOffset;
            _lightBox.Min -= vBorderOffset;

            _lightBox.Min /= worldsUnitsPerTexel;
            _lightBox.Min.X = MathF.Floor(_lightBox.Min.X);
            _lightBox.Min.Y = MathF.Floor(_lightBox.Min.Y);
            _lightBox.Min.Z = MathF.Floor(_lightBox.Min.Z);
            _lightBox.Min *= worldsUnitsPerTexel;

            _lightBox.Max /= worldsUnitsPerTexel;
            _lightBox.Max.X = MathF.Floor(_lightBox.Max.X);
            _lightBox.Max.Y = MathF.Floor(_lightBox.Max.Y);
            _lightBox.Max.Z = MathF.Floor(_lightBox.Max.Z);
            _lightBox.Max *= worldsUnitsPerTexel;

            Vector3 boxSize = _lightBox.Max - _lightBox.Min;
            Vector3 halfBoxSize = boxSize * 0.5f;

            // The position of the light should be in the center of the back
            // pannel of the box. 
            Vector3 lightPosition = _lightBox.Min + halfBoxSize;
            lightPosition.Z = _lightBox.Min.Z;

            // We need the position back in world coordinates so we transform 
            // the light position by the inverse of the lights rotation
            Matrix4x4 inverseLightRotation;
            Matrix4x4.Invert(lightRotation, out inverseLightRotation);
            lightPosition = Vector3.Transform(lightPosition, inverseLightRotation);

            view = Matrix4x4.CreateLookAt(lightPosition, lightPosition - lightDir, cameraUpVector);
            projection = Matrix4x4.CreateOrthographic(boxSize.X, boxSize.Y, -boxSize.Z, 0);
            bs = new BoundingSphere(camera.Position, diagonalLength / 2);

            return lightPosition;
        }
    }
}
