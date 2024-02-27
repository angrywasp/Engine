using System.Numerics;
using Microsoft.Xna.Framework;

namespace Engine.Editor.Components.Gizmo
{
    public static class Constants
    {
        public const float MULTI_AXIS_THICKNESS = 0.05f;
        public const float SINGLE_AXIS_THICKNESS = 0.2f;
        public const float LINE_LENGTH = 3f;
        public const float LINE_OFFSET = 1f;
        public const float PRECISION_MODE_SCALE = 0.1f;
        public const float HALF_LINE_OFFSET = LINE_OFFSET / 2;
        public const float SPHERE_RADIUS = 1f;

        public static readonly Matrix4x4[] ModelLocalSpace = new Matrix4x4[]
        {
            Matrix4x4.CreateWorld(new Vector3(Constants.LINE_LENGTH, 0, 0), Vector3Orientation.Left, Vector3Orientation.Up),
            Matrix4x4.CreateWorld(new Vector3(0, Constants.LINE_LENGTH, 0), Vector3Orientation.Down, Vector3Orientation.Left),
            Matrix4x4.CreateWorld(new Vector3(0, 0, Constants.LINE_LENGTH), Vector3Orientation.Forward, Vector3Orientation.Up)
        };

        public static readonly Color[] AxisColors = new Color[]
        {
            Color.Red,
            Color.Green,
            Color.Blue
        };

        public static readonly Vector3 AxisTextOffset = new Vector3(0, 0.5f, 0);

        public static readonly Quad[] Quads = new Quad[]
        {
            new Quad(new Vector3(Constants.HALF_LINE_OFFSET, Constants.HALF_LINE_OFFSET, 0), Vector3Orientation.Backward, Vector3Orientation.Up, Constants.LINE_OFFSET, Constants.LINE_OFFSET), //XY
            new Quad(new Vector3(Constants.HALF_LINE_OFFSET, 0, Constants.HALF_LINE_OFFSET), Vector3Orientation.Up, Vector3Orientation.Right, Constants.LINE_OFFSET, Constants.LINE_OFFSET), //XZ
            new Quad(new Vector3(0, Constants.HALF_LINE_OFFSET, Constants.HALF_LINE_OFFSET), Vector3Orientation.Right, Vector3Orientation.Up, Constants.LINE_OFFSET, Constants.LINE_OFFSET) //ZY
        };

        public static readonly string[] AxisText = new string[]
        {
            "X",
            "Y",
            "Z"
        };
    }
}