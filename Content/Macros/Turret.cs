using System.Numerics;
using Engine;
using Engine.Objects.GameObjects;
using Engine.Objects.GameObjects.Controllers;
using Engine.Scripting;
using Engine.World.Components;
using EngineScripting;

public class Macro_Turret : IMacro
{
    const string CUBE="Engine/Renderer/Meshes/Cube.mesh";
    const string PATH="DemoContent/Entities/Turret/Turret";

    public void Run()
    {
        Entity.Create<TurretControllerType>("DemoContent/Entities/Turret/Turret/TurretController.type");
        
        TurretType t = Entity.Create<TurretType>($"{PATH}.type");
        t.LifeMin = 0.0f;
        t.LifeMax = 500.0f;
        t.PhysicsModel = $"{PATH}.physics";
        t.Controller = "DemoContent/Entities/Turret/Turret/TurretController.type";

        MeshComponentType m;
        m = t.AddMeshComponent(CUBE, "Turret");
        m.PhysicsBodyIndex = 0;
        m.LocalTransform = WorldTransform3.Create(new Vector3(1.0f, 0.75f, 1.0f), Quaternion.Identity, Vector3.Zero);

        m = t.AddMeshComponent(CUBE, "Base");
        m.PhysicsBodyIndex = 1;
        m.LocalTransform = WorldTransform3.Create(new Vector3(1.5f, 1.0f, 1.5f), Quaternion.Identity, Vector3.Zero);

        t.Save();

        var physics = Physics.Create($"{PATH}.physics");

        /*Box box;

        box = Physics.AddBody<Box>("Turret");
        box.Height = 0.75f;
        box.Position = new Vector3(0.0f, 0.875f, 0.0f);
        box.Mass = 0;
        box.IsAffectedByGravity = false;

        box = Physics.AddBody<Box>("Base");
        box.Height = 1.0f;
        box.Width = box.Length = 1.5f;
        box.Position = new Vector3(0.0f, 0.0f, 0.0f);
        box.Mass = 0;
        box.IsAffectedByGravity = false;*/

        physics.Save();
    }
}
