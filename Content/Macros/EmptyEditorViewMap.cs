using System;
using System.Numerics;
using AngryWasp.Math;
using Engine;
using Engine.Content.Model;
using Engine.Physics;
using Engine.Physics.Collidables;
using Engine.Scripting;
using EngineScripting;

public class Macro_EmptyEditorViewMap: IMacro
{
    public void Run()
    {
        var floor = "Engine/Renderer/Meshes/Cube.mesh".AsMeshTemplate();
        floor = floor.Transform(Matrix4x4.CreateScale(new Vector3(500, 0.1f, 500)) * Matrix4x4.CreateTranslation(new Vector3(0, -3.05f, 0)));
        floor.SaveAs("DemoContent/Maps/EmptyEditorViewMap_floor.mesh");

        var map = Map.Create("DemoContent/Maps/EmptyEditorViewMap.map");

        map.AddStaticMesh("DemoContent/Maps/EmptyEditorViewMap_floor.mesh");
        map.AddMapObject("DemoContent/Skyboxes/Forest/Brudslojan.type", "Skybox_0");

        for (int i = 0; i < 12; i++)
        {
            var obj = map.AddMapObject("DemoContent/Entities/Sphere/Sphere.type", $"DielectricObject_{i}");
            var obj2 = map.AddMapObject("DemoContent/Entities/Sphere/Sphere.type", $"MetallicObject_{i}");
            obj.InitialTransform = WorldTransform3.Create(new Vector3((i - 5) + (0.1f * i), -1, -2));
            obj2.InitialTransform = WorldTransform3.Create(new Vector3((i - 5) + (0.1f * i), -1, 2));
        }

        for (int x = -10; x < 10; x += 5)
        {
            for (int z = -10; z < 10; z += 5)
            {
                var spot = map.AddMapObject("DemoContent/Lights/SpotLight.type", $"Spot_{x}_{z}");
                spot.InitialTransform = WorldTransform3.Create(Quaternion.CreateFromAxisAngle(Vector3.UnitX, new Degree(-90).ToRadians()), new Vector3(x, 2, z));
            }
        }

        Vector3 cp = new Vector3(0, 2f, 5f);
        map.EditorCameraPitch = -(MathF.Atan2(cp.Y, cp.Z) * Radian.ToDegCoefficient);
        map.EditorCameraYaw = 0;
        map.EditorCameraPosition = Vector3.Zero;
        

        PhysicsModel model = Physics.Create("DemoContent/Maps/EmptyEditorViewMap.physics");
        model.AddShape(new Box(false)
        {
            Width = 500,
            Length = 500,
            Height = 0.1f,
            Pose = new Pose(new Vector3(0, -3.05f, 0), Quaternion.Identity)
        });

        model.Save();

        map.PhysicsModelPath = "DemoContent/Maps/EmptyEditorViewMap.physics";

        map.Save();
    }
}
