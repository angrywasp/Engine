using System;
using System.Numerics;
using Engine.Cameras;
using Engine.Configuration;
using Engine.World.Components.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Scene
{
    public class SpotShadowMapEntry
    {
        public RenderTarget2D Texture;
    }

    public class DirectionalShadowMapEntry : SpotShadowMapEntry
    {
        public DirectionalLightComponent LightComponent;
    }

    public class ShadowConstants
    {
        public const int NUM_CSM_SHADOWS = 1;
		public const int NUM_CSM_SPLITS = 3;
		public const float CASCADE_SPLIT_EXPONENT = 2.4f;

        private int shadowResolution;
        private int spotShadowMapCount;

        public int ShadowResolution => shadowResolution;

        public int SpotShadowMapCount => spotShadowMapCount;

        public Vector2 CsmShadowMapSize => new Vector2(shadowResolution * NUM_CSM_SPLITS, shadowResolution);

		public Vector2 CsmShadowMapPixelSize => new Vector2(0.5f / (shadowResolution * NUM_CSM_SPLITS), 0.5f / shadowResolution);

        public Vector2 SpotShadowMapSize => Vector2.One * shadowResolution;

		public Vector2 SpotShadowMapPixelSize => new Vector2(0.5f / shadowResolution, 0.5f / shadowResolution);

        private static ShadowConstants instance;
        
        public static ShadowConstants Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ShadowConstants();
                    instance.shadowResolution = Settings.Engine.ShadowResolution;
                    instance.spotShadowMapCount = Settings.Engine.SpotShadowMapCount;
                }

                return instance;
            }
        }

        public static float[] CalculateSplitDepths(Camera camera, float shadowDistance)
		{
			//todo: can optimize by creating in constructor
			float[] depths = new float[NUM_CSM_SPLITS + 1];

			float near = camera.NearClip;
			float far = MathF.Min(camera.FarClip, shadowDistance);

			depths[0] = near;
			depths[NUM_CSM_SPLITS] = far;

			for (int i = 1; i < depths.Length - 1; i++)
				depths[i] = near + (far - near) * (float)System.Math.Pow((i / (float)NUM_CSM_SPLITS), CASCADE_SPLIT_EXPONENT);

			return depths;
		}

        public static BoundingFrustum[] GenerateSplitCameraFrustums(Camera camera, float shadowDistance)
		{
			BoundingFrustum[] retVal = new BoundingFrustum[NUM_CSM_SPLITS];

            Matrix4x4 view;
            Matrix4x4.Invert(camera.Transform, out view);

            float[] depths = CalculateSplitDepths(camera, shadowDistance);
            float fov = camera.FovYDegrees.ToRadians();

            retVal[0] = new BoundingFrustum(camera.View * Matrix4x4.CreatePerspectiveFieldOfView(fov, camera.AspectRatio, depths[0], depths[1]));
            retVal[1] = new BoundingFrustum(camera.View * Matrix4x4.CreatePerspectiveFieldOfView(fov, camera.AspectRatio, depths[1], depths[2]));
            retVal[2] = new BoundingFrustum(camera.View * Matrix4x4.CreatePerspectiveFieldOfView(fov, camera.AspectRatio, depths[2], depths[3]));

            return retVal;
		}
    }
}