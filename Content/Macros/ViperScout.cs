using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Engine.Helpers;
using Engine.Scripting;
using EngineScripting;

public class Macro_ViperScout : IMacro
{
    const string MESH_DIR="DemoContent/Entities/ViperScout/Meshes";
    const string MAT_DIR="DemoContent/Entities/ViperScout/Materials";

    static readonly Matrix4x4 TRANSFORM = Matrix4x4.CreateScale(Vector3.One * 0.015f);

    public void Run()
    {
        ProcessMesh("AmmoBox", "Guns2");
        ProcessMesh("Bullbars", "Doors", "Suspension", "Chassis");
        ProcessMesh("Chassis", "Interior", "Chassis");
        ProcessMesh("DriversDoor", "Doors");
        ProcessMesh("PassengerDoor", "Doors");
        ProcessMesh("Gun1", "Guns1");
        ProcessMesh("Gun2", "Guns2");
        ProcessMesh("Rim", "Wheel");
        ProcessMesh("Tire", "Wheel");
        ProcessMesh("SteeringWheel", "Interior");
        ProcessMesh("Suspension", "Suspension");
    }

    private void ProcessMesh(string meshName, params string[] materials)
    {
        /*string p = $@"{MESH_DIR}/{meshName}.mesh";
        string p2 = $@"{MESH_DIR}/VS_{meshName}.mesh";

        if (!File.Exists(EngineFolders.ContentPathVirtualToReal(p)))
            return;

        var mesh = Mesh.Load(p);
        mesh.Transform(TRANSFORM);
        for (int i = 0; i < materials.Length; i++)
            mesh.SubMeshes[i].MaterialPath = $@"{MAT_DIR}/{materials[i]}.material";
        
        mesh.SaveCopy(p2);*/
    }
}
