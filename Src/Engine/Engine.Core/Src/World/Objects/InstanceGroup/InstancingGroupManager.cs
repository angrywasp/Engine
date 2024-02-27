using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

//todo: rewrite this garbage
namespace Engine.World.Objects
{
    //todo: need to frustum cull hidden instances
    //todo: need to implement this for all meshes in MeshComponents etc
    [JsonObject(MemberSerialization.OptIn)]
    public class InstancingGroupManager
    {
        public event EngineEventHandler<InstancingGroupManager> Loaded;

        private EngineCore engine;

        [JsonProperty] public Dictionary<string, InstancingGroup> Groups { get; set; } = new Dictionary<string, InstancingGroup>();

        public void Load(EngineCore engine)
        {
            Loaded?.Invoke(this);
            /*this.engine = engine;
            Task.Run(async () => {
                foreach (var g in Groups)
                {
                    g.Value.MeshPath = g.Key;
                    var mesh = await ContentLoader.LoadMeshAsync(engine, g.Key).ConfigureAwait(false);
                    await g.Value.Load(engine, mesh).ConfigureAwait(false);
                }

                Loaded?.Invoke(this);
            });*/
        }

        public async Task Add(string meshPath)
        {
            /*if (Groups.ContainsKey(meshPath))
                return;

            var ig = new InstancingGroup();
            await ig.LoadAsync(engine, meshPath).ConfigureAwait(false);
            
            Groups.Add(meshPath, ig);*/
        }
    }
}