using System.Numerics;
using Engine;
using Engine.Objects.GameObjects;
using Engine.Physics;
using Engine.Physics.Collidables;
using Engine.Physics.Constraints;
using Engine.Scripting;
using Engine.World.Components;
using EngineScripting;

public class Macro_Cart : IMacro
{
    const string CUBE = "Engine/Renderer/Meshes/Cube.mesh";
    const string SPHERE = "Engine/Renderer/Meshes/Sphere.mesh";

    public void Run()
    {
        DynamicType t = Entity.Create<DynamicType>("DemoContent/Entities/Cart/Cart.type");
        t.LifeMin = 0.0f;
        t.LifeMax = 100.0f;
        t.PhysicsModel = "DemoContent/Entities/Cart/Cart.physics";

        EmitterComponentType ec = t.AddEmitterComponent("Emitter_0");
        ec.ParticleSettings.Add("DemoContent/Emitters/Fire.emitter");
        ec.ParticleSettings.Add("DemoContent/Emitters/Smoke.emitter");

        MeshComponentType m;
        m = t.AddMeshComponent(CUBE, "Mesh_0");
        m.PhysicsBodyIndex = 0;
        m.LocalTransform = WorldTransform3.Create(new Vector3(1.0f, 0.4f, 1.0f), Quaternion.Identity, Vector3.Zero);

        m = t.AddMeshComponent(CUBE, "Mesh_1");
        m.PhysicsBodyIndex = 1;
        m.LocalTransform = WorldTransform3.Create(new Vector3(1.0f, 0.4f, 1.0f), Quaternion.Identity, Vector3.Zero);

        m = t.AddMeshComponent(SPHERE, "Mesh_2");
        m.PhysicsBodyIndex = 2;

        m = t.AddMeshComponent(SPHERE, "Mesh_3");
        m.PhysicsBodyIndex = 3;

        m = t.AddMeshComponent(SPHERE, "Mesh_4");
        m.PhysicsBodyIndex = 4;

        m = t.AddMeshComponent(SPHERE, "Mesh_5");
        m.PhysicsBodyIndex = 5;

        t.Save();

        PhysicsModel model = Physics.Create("DemoContent/Entities/Cart/Cart.physics");

        model.AddShape(new Box(true)
        {
            Height = 0.4f,
            Pose = new Pose(new Vector3(-0.6f, 0.0f, 0.0f))
        });

        var mat = new Material(1000, 0.01f, 1, 1);

        model.AddShape(new Box(true)
        {
            Height = 0.4f,
            Pose = new Pose(new Vector3(0.6f, 0.0f, 0.0f)),
            Material = mat
        });

        model.AddShape(new Sphere(true) { Pose = new Pose(new Vector3(-0.75f, 0.0f, 1.1f)), Material = mat });
        model.AddShape(new Sphere(true) { Pose = new Pose(new Vector3(0.75f, 0.0f, 1.1f)), Material = mat });
        model.AddShape(new Sphere(true) { Pose = new Pose(new Vector3(-0.75f, 0.0f, -1.1f)), Material = mat });
        model.AddShape(new Sphere(true) { Pose = new Pose(new Vector3(0.75f, 0.0f, -1.1f)), Material = mat });

        model.AddConstraint(new BallSocketLimit
        {
            PhysicsModelIndexA = 0,
            PhysicsModelIndexB = 1,
            LocalOffsetA = Vector3.UnitX * 0.7f,
            LocalOffsetB = -(Vector3.UnitX * 0.7f),
            SpringSettings = new SpringSettings(30, 1)
        });
        model.AddConstraint(new AngularHinge
        {
            PhysicsModelIndexA = 0,
            PhysicsModelIndexB = 1,
            LocalHingeAxisA = Vector3.UnitY,
            LocalHingeAxisB = Vector3.UnitY,
            SpringSettings = new SpringSettings(30, 1)
        });
        model.AddConstraint(new AngularServo
        {
            PhysicsModelIndexA = 0,
            PhysicsModelIndexB = 1,
            TargetRelativeRotationLocalA = Quaternion.Identity,
            ServoSettings = new ServoSettings(10, 1, 10),
            SpringSettings = new SpringSettings(30, 1)
        });

        model.AddConstraint(new BallSocketLimit
        {
            PhysicsModelIndexA = 0,
            PhysicsModelIndexB = 2,
            LocalOffsetA = new Vector3(0, 0, 0.5f),
            LocalOffsetB = new Vector3(0, 0, -0.5f),
            SpringSettings = new SpringSettings(30, 1)
        });
        model.AddConstraint(new AngularHinge
        {
            PhysicsModelIndexA = 0,
            PhysicsModelIndexB = 2,
            LocalHingeAxisA = Vector3.UnitZ,
            LocalHingeAxisB = Vector3.UnitZ,
            SpringSettings = new SpringSettings(30, 1)
        });

        model.AddConstraint(new BallSocketLimit
        {
            PhysicsModelIndexA = 1,
            PhysicsModelIndexB = 3,
            LocalOffsetA = new Vector3(0, 0, 0.5f),
            LocalOffsetB = new Vector3(0, 0, -0.5f),
            SpringSettings = new SpringSettings(30, 1)
        });
        model.AddConstraint(new AngularHinge
        {
            PhysicsModelIndexA = 1,
            PhysicsModelIndexB = 3,
            LocalHingeAxisA = Vector3.UnitZ,
            LocalHingeAxisB = Vector3.UnitZ,
            SpringSettings = new SpringSettings(30, 1)
        });

        model.AddConstraint(new BallSocketLimit
        {
            PhysicsModelIndexA = 0,
            PhysicsModelIndexB = 4,
            LocalOffsetA = new Vector3(0, 0, -0.5f),
            LocalOffsetB = new Vector3(0, 0, 0.5f),
            SpringSettings = new SpringSettings(30, 1)
        });
        model.AddConstraint(new AngularHinge
        {
            PhysicsModelIndexA = 0,
            PhysicsModelIndexB = 4,
            LocalHingeAxisA = Vector3.UnitZ,
            LocalHingeAxisB = Vector3.UnitZ,
            SpringSettings = new SpringSettings(30, 1)
        });

        model.AddConstraint(new BallSocketLimit
        {
            PhysicsModelIndexA = 1,
            PhysicsModelIndexB = 5,
            LocalOffsetA = new Vector3(0, 0, -0.5f),
            LocalOffsetB = new Vector3(0, 0, 0.5f),
            SpringSettings = new SpringSettings(30, 1)
        });
        model.AddConstraint(new AngularHinge
        {
            PhysicsModelIndexA = 1,
            PhysicsModelIndexB = 5,
            LocalHingeAxisA = Vector3.UnitZ,
            LocalHingeAxisB = Vector3.UnitZ,
            SpringSettings = new SpringSettings(30, 1)
        });

        model.Save();
    }
}
