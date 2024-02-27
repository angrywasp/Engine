using AngryWasp.Helpers;
using Engine.World.Objects;
using Microsoft.Xna.Framework;
using System;

namespace Engine.Editor.TerrainEditor.Tools
{
    public class PaintTool : TerrainTool
    {
        private int index;

        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        /// <summary>
        /// Paints a terrain layer
        /// </summary>
        /// <param name="index">The layer index to paint</param>
        public PaintTool(EngineCore engine, HeightmapTerrain terrain, int index) : base(engine, terrain)
        {
            this.index = index;
        }

        protected override void UpdatePixel(Color mask, ref Color blendTex)
        {
            float c1 = (mask.A / 255.0f) * ((float)Strength / 255.0f);
            float c2 = 0;

            switch (index)
            {
                case 0:
                    c2 = blendTex.R / 255.0f;
                    break;
                case 1:
                    c2 = blendTex.G / 255.0f;
                    break;
                case 2:
                    c2 = blendTex.B / 255.0f;
                    break;
                case 3:
                    c2 = blendTex.A / 255.0f;
                    break;
                default:
                    throw new NotImplementedException();
            }

            byte value = (byte)(MathHelper.Clamp(c1 + c2, 0.0f, 1.0f) * 255.0f);

            switch (index)
            {
                case 0:
                    blendTex.R = value;
                    break;
                case 1:
                    blendTex.G = value;
                    break;
                case 2:
                    blendTex.B = value;
                    break;
                case 3:
                    blendTex.A = value;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}