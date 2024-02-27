using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Engine.Helpers;
using Engine.Scripting;
using Engine.World;
using EngineScripting;

public class Macro_PalmTrees : IMacro
{
    const float SCALE = 0.2f;
    static readonly Matrix4x4 TRANSFORM = Matrix4x4.CreateScale(Vector3.One * SCALE);

    public void Run()
    {
        return;
        /*for (int i = 1; i <= 6; i++)
        {
            string p = $"DemoContent/Entities/Palm/Meshes/Palm{i}.mesh";
            string p2 = $"DemoContent/Entities/Palm/Meshes/PalmTree{i}.mesh";

            if (File.Exists(EngineFolders.ContentPathVirtualToReal(p2)))
                continue;

            var mesh = await Mesh.Load(p).ConfigureAwait(false);
            mesh.Transform(TRANSFORM);
            mesh.SubMeshes[0].MaterialPath = "DemoContent/Entities/Palm/Materials/Bark.material";
            mesh.SubMeshes[1].MaterialPath = "DemoContent/Entities/Palm/Materials/Leaf.material";
            mesh.SaveCopy(p2);

            File.Delete(EngineFolders.ContentPathVirtualToReal(p));

            var e = Entity.Create<GameObjectType>($"DemoContent/Entities/Palm/Palm{i}.type");
            e.AddMeshComponent(p2, "mesh_0");
            e.Save();
        }
        */
    }
}
