using System.Numerics;
using Microsoft.Xna.Framework;

namespace Engine.Interfaces
{
    public interface ICameraController
    {
        Matrix4x4 Transform { get; set; }
        EngineCore Engine { get; set; }
        void Update(GameTime gameTime);
    }
}
