using System.Collections.Generic;
using System.Numerics;
using Engine.Content.Model;
using Engine.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Scene
{
    public class SpotShadowRenderer
    {
		Plane[] _directionalClippingPlanes = new Plane[6];

        public List<SpotShadowMapEntry> spotShadowMaps = new List<SpotShadowMapEntry>();
        private int currentFreeSpotShadowMap;

        private List<(SharedMeshData SharedMeshData, List<IShadowCaster> MeshList)> shadowCasters =
            new List<(SharedMeshData SharedMeshData, List<IShadowCaster> MeshList)>();

		Vector3[] frustumCornersWS = new Vector3[8];
		Vector3[] frustumCornersVS = new Vector3[8];
        BoundingFrustum _tempFrustum = new BoundingFrustum(Matrix4x4.Identity);

        public SpotShadowRenderer(EngineCore engine)
        {
            for (int i = 0; i < ShadowConstants.Instance.SpotShadowMapCount; i++)
            {
                var entry = new SpotShadowMapEntry();
                entry.Texture = new RenderTarget2D(engine.GraphicsDevice, ShadowConstants.Instance.ShadowResolution,
                                                   ShadowConstants.Instance.ShadowResolution, false, SurfaceFormat.Single,
                                                   DepthFormat.Depth32F, 0, RenderTargetUsage.DiscardContents);
                entry.Texture.Name = "SpotShadowMap" + i.ToString();

				spotShadowMaps.Add(entry);
            }
        }

        internal void Clear()
        {
            currentFreeSpotShadowMap = 0;
        }

        internal SpotShadowMapEntry GetFreeSpotShadowMap()
        {
            if (currentFreeSpotShadowMap < spotShadowMaps.Count)
                return spotShadowMaps[currentFreeSpotShadowMap++];
            
            return null;
        }

        internal void GenerateShadowTexture(SceneGraphics sceneGraphics, GraphicsDevice graphicsDevice, SpotLightEntry entry)
        {
            graphicsDevice.SetRenderTarget(entry.ShadowMap.Texture);
            graphicsDevice.Clear(GraphicsDevice.DiscardWhite);
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            shadowCasters.Clear();
            sceneGraphics.GetShadowCasters(entry.Light.BoundingFrustum, shadowCasters);

            foreach (var list in shadowCasters)
            {
                graphicsDevice.SetVertexBuffers(list.SharedMeshData.ShadowBinding);
                graphicsDevice.SetIndexBuffer(list.SharedMeshData.IndexBuffer);

                foreach (var sc in list.MeshList)
                    sc.RenderShadowMap(entry.Light.LightView, entry.Light.LightProjection);
            }

            foreach (var ig in EngineCore.Instance.Scene.Graphics.DynamicInstancingGroupManager.Groups.Values)
                ig.ShadowMap(ig.GetShadowCasters(entry.Light.BoundingFrustum), entry.Light.LightView, entry.Light.LightProjection);
				
            entry.Light.ShadowMap = entry.ShadowMap.Texture;
        }
    }
}
