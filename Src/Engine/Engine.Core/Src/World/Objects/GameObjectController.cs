using Engine.Cameras;
using Microsoft.Xna.Framework;

namespace Engine.World.Objects
{
    public class GameObjectControllerType : GameObjectType
    {
    }

    public class GameObjectController : GameObject
    {
        private GameObjectControllerType _type = null;

        public new GameObjectControllerType Type => _type;

        private GameObject controlledObject;

        public GameObject ControlledObject => controlledObject;

        public virtual void AssignControlledObject(GameObject gameObject)
        {
            controlledObject = gameObject;
            this.transform = controlledObject.Transform;
        }

        public virtual void DoInputControllerCommand(float[] keys) { }
    }
}
