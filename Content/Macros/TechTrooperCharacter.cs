using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using AngryWasp.Helpers;
using Engine.Helpers;
using Engine.Objects.GameObjects;
using Engine.Scripting;
using Engine.World.Components;
using EngineScripting;

public class Macro_TechTrooperCharacter : IMacro
{
    public void Run()
    {
        return;
        /*string p = $"DemoContent/Entities/TechTrooper/Meshes/TechTrooper.mesh";
        string p2 = $"DemoContent/Entities/TechTrooper/Meshes/TechTrooperCharacter.mesh";

        if (!File.Exists(EngineFolders.ContentPathVirtualToReal(p2)))
        {
            var mesh = await Mesh.Load(p).ConfigureAwait(false);
            mesh.SubMeshes[0].MaterialPath = "DemoContent/Entities/TechTrooper/Materials/TechTrooper_Torso.material";
            mesh.SubMeshes[1].MaterialPath = "DemoContent/Entities/TechTrooper/Materials/TechTrooper_LowerBody.material";
            mesh.SaveCopy(p2);

            File.Delete(EngineFolders.ContentPathVirtualToReal(p));
        }

        CharacterType t = Entity.Create<CharacterType>("DemoContent/MapObjects/TechTrooper.type");

        t.Height = 2.1f;
        t.Radius = 0.3f;
        t.Mass = 1.0f;
        t.WalkForce = 10.0f;
        t.WalkSpeed = 1.0f;
        t.SprintCoefficient = 6.0f;
        t.AllowPlayerControl = true;
        t.FpsCameraOffset = new Vector3(0f, 1.8f, 0f);
        t.TpsCameraOffset = new Vector3(0f, 2.5f, 5f);
        t.ViewRadius = 10.0f;
        t.TakeItemsRadius = 2.0f;
        t.LifeMin = 0.0f;
        t.LifeMax = 100.0f;

        MeshComponentType m = t.AddMeshComponent("DemoContent/Entities/TechTrooper/Meshes/TechTrooperCharacter.mesh", "mesh");
        m.Position = new Vector3(0, -1.05f, 0);
        m.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.Pi);
        m.AlwaysDraw = true;

        t.Save();*/
    }
}
