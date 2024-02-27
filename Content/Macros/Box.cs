using Engine.Objects.GameObjects;
using Engine.Physics.Collidables;
using Engine.Scripting;
using Engine.World.Components;
using EngineScripting;

public class Macro_Box : IMacro
{
    public void Run()
    {
        {
            DynamicType d = Entity.Create<DynamicType>($"DemoContent/Entities/Box/Box.type");

            d.LifeMin = 0.0f;
            d.LifeMax = 100.0f;

            MeshComponentType mc = d.AddMeshComponent("Engine/Renderer/Meshes/Cube.mesh", "Mesh_0");
            d.Save();
        }

        {
            DynamicType d = Entity.Create<DynamicType>("DemoContent/Entities/Box/DynamicBox.type");

            d.LifeMin = 0.0f;
            d.LifeMax = 100.0f;
            d.PhysicsModel = "DemoContent/Entities/Box/DynamicBox.physics";

            MeshComponentType mc = d.AddMeshComponent("Engine/Renderer/Meshes/Cube.mesh", "Mesh_0");
            mc.PhysicsBodyIndex = 0;
            mc.Instance = false;
            d.Save();

            var physicsModel = Physics.Create($"DemoContent/Entities/Box/DynamicBox.physics");
            physicsModel.AddShape(new Box(true));

            physicsModel.Save();
        }
    }
}
