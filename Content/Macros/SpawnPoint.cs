using System.Threading.Tasks;
using Engine.Scripting;
using Engine.World.Objects;
using EngineScripting;

public class Macro_SpawnPoint : IMacro
{
    public void Run()
    {
        var entity = Entity.Create<SpawnPointType>("DemoContent/MapObjects/SpawnPoint/SpawnPoint.type");

        var mesh = "DemoContent/MapObjects/SpawnPoint/Meshes/SpawnPoint.mesh".AsMeshTemplate();
        mesh.SubMeshes[0].UseMaterial("Engine/Materials/Colors/Yellow.material");
        mesh.Save();

        var meshComponent = entity.AddMeshComponent("DemoContent/MapObjects/SpawnPoint/Meshes/SpawnPoint.mesh", "mesh");
        meshComponent.OnlyInEditor = true;

        entity.Save();
    }
}
