using Engine.Graphics.Vertices;
using Microsoft.Xna.Framework;
using System.Numerics;

namespace Engine.Editor.Components.Gizmo
{
    public class Vertices
    {
        private static readonly Color xColor = Constants.AxisColors[0];
        private static readonly Color yColor = Constants.AxisColors[1];
        private static readonly Color zColor = Constants.AxisColors[2];

        private VertexPositionColor[] translationLineVertices = new VertexPositionColor[]
        {
            // -- X Axis -- // index 0 - 5
            new VertexPositionColor(new Vector3(Constants.HALF_LINE_OFFSET, 0, 0), xColor),
            new VertexPositionColor(new Vector3(Constants.LINE_LENGTH, 0, 0), xColor),

            new VertexPositionColor(new Vector3(Constants.LINE_OFFSET, 0, 0), xColor),
            new VertexPositionColor(new Vector3(Constants.LINE_OFFSET, Constants.LINE_OFFSET, 0), xColor),

            new VertexPositionColor(new Vector3(Constants.LINE_OFFSET, 0, 0), xColor),
            new VertexPositionColor(new Vector3(Constants.LINE_OFFSET, 0, Constants.LINE_OFFSET), xColor),

            // -- Y Axis -- // index 6 - 11
            new VertexPositionColor(new Vector3(0, Constants.HALF_LINE_OFFSET, 0), yColor),
            new VertexPositionColor(new Vector3(0, Constants.LINE_LENGTH, 0), yColor),

            new VertexPositionColor(new Vector3(0, Constants.LINE_OFFSET, 0), yColor),
            new VertexPositionColor(new Vector3(Constants.LINE_OFFSET, Constants.LINE_OFFSET, 0), yColor),

            new VertexPositionColor(new Vector3(0, Constants.LINE_OFFSET, 0), yColor),
            new VertexPositionColor(new Vector3(0, Constants.LINE_OFFSET, Constants.LINE_OFFSET), yColor),

            // -- Z Axis -- // index 12 - 17
            new VertexPositionColor(new Vector3(0, 0, Constants.HALF_LINE_OFFSET), zColor),
            new VertexPositionColor(new Vector3(0, 0, Constants.LINE_LENGTH), zColor),

            new VertexPositionColor(new Vector3(0, 0, Constants.LINE_OFFSET), zColor),
            new VertexPositionColor(new Vector3(Constants.LINE_OFFSET, 0, Constants.LINE_OFFSET), zColor),

            new VertexPositionColor(new Vector3(0, 0, Constants.LINE_OFFSET), zColor),
            new VertexPositionColor(new Vector3(0, Constants.LINE_OFFSET, Constants.LINE_OFFSET), zColor),
        };

        public VertexPositionColor[] TranslationLineVertices => translationLineVertices;
    }
}