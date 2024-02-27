using System;

namespace Engine.World
{
    public struct TypeFactoryItem
    {
        private Type typeType;
        private Type classType;
        private GameObjectType typeInstance;

        public Type TypeType => typeType;

        public Type ClassType => classType;

        public GameObjectType TypeInstance => typeInstance;

        public TypeFactoryItem(GameObjectType instance, Type typeType, Type classType)
        {
            this.typeType = typeType;
            this.classType = classType;
            this.typeInstance = instance;
        }
    }
}
