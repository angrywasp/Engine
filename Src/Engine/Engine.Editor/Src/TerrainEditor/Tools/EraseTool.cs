using Microsoft.Xna.Framework;
using AngryWasp.Helpers;
using Engine.World.Objects;

namespace Engine.Editor.TerrainEditor.Tools
{
    public class EraseTool : TerrainTool
    {
        public EraseTool(EngineCore engine, HeightmapTerrain terrain) : base(engine, terrain) { }

        protected override void UpdatePixel(Color mask, ref Color blendTex)
        {
            float c1 = (mask.A / 255.0f) * ((float)Strength / 255.0f);

            { //clear R
                float c2 = blendTex.R / 255.0f;
                byte value = (byte)(MathHelper.Clamp(c2 - c1, 0.0f, 1.0f) * 255.0f);
                blendTex.R = value;
            }

            { //clear G
                float c2 = blendTex.G / 255.0f;
                byte value = (byte)(MathHelper.Clamp(c2 - c1, 0.0f, 1.0f) * 255.0f);
                blendTex.G = value;
            }

            { //clear B
                float c2 = blendTex.B / 255.0f;
                byte value = (byte)(MathHelper.Clamp(c2 - c1, 0.0f, 1.0f) * 255.0f);
                blendTex.B = value;
            }

            { //clear A
                float c2 = blendTex.A / 255.0f;
                byte value = (byte)(MathHelper.Clamp(c2 - c1, 0.0f, 1.0f) * 255.0f);
                blendTex.A = value;
            }
            
        }
    }
}