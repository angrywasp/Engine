
using System.Numerics;
using Engine;
using Engine.Objects.GameObjects;
using Engine.Objects.GameObjects.Controllers;
using Engine.Scripting;
using Engine.World.Components;
using EngineScripting;

public class Macro_BoxGroup : IMacro
{
    const string CUBE = "Engine/Renderer/Meshes/Cube.mesh";

    public void Run()
    {
        Entity.Create<RotationControllerType>("DemoContent/MapObjects/RotationController.type");
        
        DynamicType dt = Entity.Create<DynamicType>("DemoContent/Entities/BoxGroup/BoxGroup.type");
        dt.Controller = "DemoContent/MapObjects/RotationController.type";

        MeshComponentType mc = dt.AddMeshComponent(CUBE, "Mesh_0");
        mc.LocalTransform = WorldTransform3.Create(new Vector3(0, -1.1f, 0));

        mc = dt.AddMeshComponent(CUBE, "Mesh_1");
        mc.LocalTransform = WorldTransform3.Create(new Vector3(0, 1.1f, 0));

        mc = dt.AddMeshComponent(CUBE, "Mesh_2");
        mc.LocalTransform = WorldTransform3.Create(new Vector3(-1.1f, 0, 0));

        mc = dt.AddMeshComponent(CUBE, "Mesh_3");
        mc.LocalTransform = WorldTransform3.Create(new Vector3(1.1f, 0, 0));

        mc = dt.AddMeshComponent(CUBE, "Mesh_4");
        mc.LocalTransform = WorldTransform3.Create(new Vector3(0, 0, -1.1f));

        mc = dt.AddMeshComponent(CUBE, "Mesh_5");
        mc.LocalTransform = WorldTransform3.Create(new Vector3(0, 0, 1.1f));

        dt.Save();
    }
}
