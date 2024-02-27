using System;
using System.Numerics;
using Microsoft.Xna.Framework;

namespace Engine.Editor
{
    public enum NodeType
    {
        Mesh,
        CollisionMesh,
        Effect,
        Texture,
        TextureCube,
        FontPackage,
        Font,
        Type,
        Map,
        Material,
        Sound,
        Video,
        Ui,
        Skin,
        Form,
        Script,
        Physics,
        Emitter,
        Terrain,
        Unknown
    }

    public static class Helpers
    {
        public static NodeType GetNodeTypeFromExtension(string extension)
        {
            switch (extension)
            {
                case "effect":
                    return NodeType.Effect;
                case "type":
                    return NodeType.Type;
                case "fontpkg":
                    return NodeType.FontPackage;
                case "font":
                    return NodeType.Font;
                case "map":
                    return NodeType.Map;
                case "material":
                    return NodeType.Material;
                case "mesh":
                    return NodeType.Mesh;
                case "collision":
                    return NodeType.CollisionMesh;
                case "sound":
                    return NodeType.Sound;
                case "video":
                    return NodeType.Video;
                case "texture":
                    return NodeType.Texture;
                case "texcube":
                    return NodeType.TextureCube;
                case "ui":
                    return NodeType.Ui;
                case "skin":
                    return NodeType.Skin;
                case "form":
                    return NodeType.Form;
                case "dll":
                case "script":
                    return NodeType.Script;
                case "physics":
                    return NodeType.Physics;
                case "emitter":
                    return NodeType.Emitter;
                case "terrain":
                    return NodeType.Terrain;
                default:
                    return NodeType.Unknown;
            }
        }

        public static void ResizeAndPositionTexture(Vector2i textureSize, Vector2 screenSize, out Vector2i p, out Vector2i s)
        {
            p = Vector2i.Zero;
            s = textureSize;

            float ratio = 1;
            if (s.X > screenSize.X)
                ratio = screenSize.X / s.X;

            if (s.Y > screenSize.Y)
                ratio = MathF.Min(ratio, screenSize.Y / s.Y);

            s = new Vector2i((int)(textureSize.X * ratio), (int)(textureSize.Y * ratio));

            float sx = (screenSize.X / 2.0f);
            float tx = s.X / 2.0f;

            float sy = (screenSize.Y / 2.0f);
            float ty = s.Y / 2.0f;

            p = new Vector2i((int)(sx - tx), (int)(sy - ty));
        }
    }
}
