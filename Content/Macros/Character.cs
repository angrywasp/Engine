using System.Numerics;
using AngryWasp.Helpers;
using Engine;
using Engine.Objects.GameObjects;
using Engine.Objects.GameObjects.Controllers;
using Engine.Scripting;
using Engine.World.Components;
using EngineScripting;

public class Macro_TestCharacter: IMacro
{
    public void Run()
    {
        var controller = Entity.Create<CharacterControllerType>("DemoContent/MapObjects/CharacterController.type");
        controller.AllowTakeItems = true;
        controller.TakeItemsRadius = 1;
        controller.Save();

        CharacterType t = Entity.Create<CharacterType>("DemoContent/MapObjects/Character.type");

        t.Height = 1.9f;
        t.Radius = 0.5f;
        t.Mass = 1.0f;
        t.WalkForce = 10.0f;
        t.WalkSpeed = 3.0f;
        t.AllowPlayerControl = true;
        t.FpsCameraOffset = new Vector3(0f, 0.95f, 0f);
        t.TpsCameraOffset = new Vector3(0f, 1f, 1.5f);
        t.ViewRadius = 10.0f;
        t.LifeMin = 0.0f;
        t.LifeMax = 100.0f;

        // The robot mesh origin is at the bottom of the mesh. So we need to translate this
        // component down half it's height. However the macro does not load the mesh, so we just
        //need to know how tall the mesh is

        MeshComponentType m = t.AddMeshComponent("DemoContent/Entities/Robot/Meshes/Robot.mesh", "mesh");
        m.LocalTransform = WorldTransform3.Create(Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.Pi), -(Vector3.UnitY * (t.Height / 2)));

        t.Save();
    }
}
