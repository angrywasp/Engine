using Engine.Interfaces;
using Engine.Cameras;
using Microsoft.Xna.Framework;
using System.Numerics;
using Engine.World;

namespace Engine.Objects.Controllers.CameraControllers
{
    public abstract class Base : ICameraController
    {
        protected Camera camera;
        protected GameObject gameObject;

        public Base(Camera c, GameObject g)
        {
            camera = c;
            gameObject = g;
        }

        public Matrix4x4 Transform { get; set; }
        public EngineCore Engine { get; set; }
        public float Pitch { get; set; }

        public abstract void Update(GameTime gameTime);
    }

}
