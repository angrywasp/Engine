using Engine.Objects.GameObjects;
using Engine.Physics.Collidables;
using Engine.Scripting;
using Engine.World.Components;
using EngineScripting;

public class Macro_Shpere : IMacro
{
    public void Run()
    {
        {
            DynamicType d = Entity.Create<DynamicType>($"DemoContent/Entities/Sphere/Sphere.type");

            d.LifeMin = 0.0f;
            d.LifeMax = 100.0f;

            MeshComponentType mc = d.AddMeshComponent("Engine/Renderer/Meshes/Sphere.mesh", "Mesh_0");
            d.Save();
        }

        {
            DynamicType d = Entity.Create<DynamicType>($"DemoContent/Entities/Sphere/DynamicSphere.type");

            d.LifeMin = 0.0f;
            d.LifeMax = 100.0f;
            d.PhysicsModel = $"DemoContent/Entities/Sphere/DynamicSphere.physics";

            MeshComponentType mc = d.AddMeshComponent("Engine/Renderer/Meshes/Sphere.mesh", "Mesh_0");
            mc.PhysicsBodyIndex = 0;
            mc.Instance = false;
            d.Save();

            var physicsModel = Physics.Create($"DemoContent/Entities/Sphere/DynamicSphere.physics");

            physicsModel.AddShape(new Sphere(true));

            physicsModel.Save();
        }
    }
}
