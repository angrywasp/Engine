namespace Engine.World.Objects
{
    public class LightType : GameObjectType
    {
    }

    public class Light : GameObject
    {
        private LightType _type = null;

        public new LightType Type => _type;
    }
}