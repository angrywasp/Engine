using System.Numerics;
using Engine;
using Engine.Scripting;
using Engine.World;
using EngineScripting;

public class Macro_TestMap: IMacro
{
    public void Run()
    {
        var map = EngineScripting.Map.Create("DemoContent/Maps/TestMap.map");
        map.AddMapObject("DemoContent/Maps/Terrains/Terrain_513.type", "Terrain_0");
        map.AddMapObject("DemoContent/Skyboxes/0.type", "Skybox_0");

        MapObject water = map.AddMapObject("DemoContent/Maps/Water/WaterPlane.type", "Water_0");
        water.InitialTransform = WorldTransform3.Create(new Vector3(1024, 1, 1024), Quaternion.Identity, new Vector3(256, 10, 256));

        MapObject sp = map.AddMapObject("DemoContent/MapObjects/SpawnPoint/SpawnPoint.type", "Spawnpoint_0");
        sp.InitialTransform = WorldTransform3.Create(new Vector3 (466, 25, 250));
        map.AddMapObject("DemoContent/MapObjects/PlayerManager.type", "PlayerManager_0");

        map.EditorCameraPitch = -14f;
        map.EditorCameraYaw = -239.6f;
        map.EditorCameraPosition = new Vector3(466.5f, 26f, 250f);

        map.Save();
    }
}
