using Engine.World;
using Microsoft.Xna.Framework;

namespace Engine.Objects.Controllers.InputControllers
{
    public abstract class Base
    {
        protected GameObject controlledObject;
        protected EngineCore engine;

        public Base(EngineCore engine, GameObject controlledObject)
        {
            this.engine = engine;
            this.controlledObject = controlledObject;
        }

        public abstract void Update(GameTime gameTime);
    }
}
