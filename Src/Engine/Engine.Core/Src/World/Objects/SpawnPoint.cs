using System.Threading.Tasks;
using Engine.World;

namespace Engine.World.Objects
{
    public class SpawnPointType : GameObjectType
    {
    }

    public class SpawnPoint : GameObject
    {
        private SpawnPointType _type = null;

        public new SpawnPointType Type => _type;

        public override async Task LoadAsync(EngineCore engine)
        {
            Map.SpawnPoints.Add(this);
            await base.LoadAsync(engine); 
        }
    }
}
