using System.Numerics;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Debug
{
    public interface IDebugShape
    {
        Matrix4x4 WorldMatrix { get; }

        void Draw(GraphicsDevice graphicsDevice);
    }
}