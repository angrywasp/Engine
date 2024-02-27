using System;
using System.Threading.Tasks;
using AngryWasp.Random;
using Engine.World.Components;
using Engine.World.Components.Lights;

namespace Engine.World.Objects
{
    public class RuntimeMapObject : MapObject
    {
        public object Tag { get; set; }
        
        public async Task<MeshComponent> AddMeshComponent(MeshComponentType type, string name = null)
        {
            type.Name = name ?? GetRandomName();
            return (MeshComponent)(await gameObject.LoadComponentAsync(type).ConfigureAwait(false));
        }

		public async Task<PointLightComponent> AddPointLightComponent(PointLightComponentType type, string name = null)
		{
            type.Name = name ?? GetRandomName();
			return (PointLightComponent)(await gameObject.LoadComponentAsync(type).ConfigureAwait(false));
		}

        public async Task<SpotLightComponent> AddSpotLightComponent(SpotLightComponentType type, string name = null)
        {
            type.Name = name ?? GetRandomName();
            return (SpotLightComponent)(await gameObject.LoadComponentAsync(type).ConfigureAwait(false));
        }

        public async Task<DirectionalLightComponent> AddDirectionalLightComponent(DirectionalLightComponentType type, string name = null)
        {
            type.Name = name ?? GetRandomName();
            return (DirectionalLightComponent)(await gameObject.LoadComponentAsync(type).ConfigureAwait(false));
        }

        public async Task<Tcomponent> AddGenericComponent<Tcomponent, Ttype>(Ttype type, string name = null)
            where Tcomponent : Component
            where Ttype : ComponentType
        {
            type.Name = name ?? GetRandomName();
            return (Tcomponent)(await gameObject.LoadComponentAsync(type).ConfigureAwait(false));
        }

        public async Task SetGameObject<T>(GameObjectType newType) where T : GameObject
        {
            GameObject newObject = (GameObject)Activator.CreateInstance(typeof(T));
            engine.TypeFactory.SetTypeFields(typeof(T), newObject, newType);
            //gameObject.Unload();
            await newObject.LoadAsync(engine);
            gameObject = newObject;
        }

        private string GetRandomName()
        {
            string s = RandomString.AlphaNumeric(4);

            while (gameObject.Components.ContainsKey((s = RandomString.AlphaNumeric(4))))
                continue;

            return s;
        }
    }
}
