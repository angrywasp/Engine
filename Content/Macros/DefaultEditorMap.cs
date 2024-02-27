using System;
using System.Numerics;
using AngryWasp.Math;
using Engine.Content.Model;
using Engine.Physics;
using Engine.Physics.Collidables;
using Engine.Scripting;
using EngineScripting;

public class Macro_DefaultEditorMap: IMacro
{
    public void Run()
    {
        var floor = "Engine/Renderer/Meshes/Cube.mesh".AsMeshTemplate();
        floor = floor.Transform(Matrix4x4.CreateScale(new Vector3(50, 0.1f, 50)) * Matrix4x4.CreateTranslation(new Vector3(0, -3.05f, 0)));
        floor.SaveAs("DemoContent/Maps/DefaultEditorMap_floor.mesh");

        var map = Map.Create("DemoContent/Maps/DefaultEditorMap.map");

        map.AddStaticMesh("DemoContent/Maps/DefaultEditorMap_floor.mesh");
        map.AddMapObject("DemoContent/Skyboxes/Forest/Plants.type", "Skybox_0");

        Vector3 cp = new Vector3(0, 2f, 5f);
        map.EditorCameraPitch = -(MathF.Atan2(cp.Y, cp.Z) * Radian.ToDegCoefficient);
        map.EditorCameraYaw = 0;
        map.EditorCameraPosition = new Vector3(466.5f, 26f, 250f);

        PhysicsModel model = Physics.Create("DemoContent/Maps/DefaultEditorMap.physics");
        model.AddShape(new Box(false)
        {
            Width = 50,
            Length = 50,
            Height = 0.1f,
            Pose = new Pose(new Vector3(0, -3.05f, 0), Quaternion.Identity)
        });

        model.Save();

        map.PhysicsModelPath = "DemoContent/Maps/DefaultEditorMap.physics";

        map.Save();
    }
}
