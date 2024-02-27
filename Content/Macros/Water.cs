using System.Numerics;
using Engine.Scripting;
using Engine.World;
using Engine.World.Objects;
using EngineScripting;
using Microsoft.Xna.Framework;

public class Macro_Water : IMacro
{
    public void Run()
    {
        WaterPlaneType type = Entity.Create<WaterPlaneType>("DemoContent/Maps/Water/WaterPlane.type");
        type.WaveMap0 = "DemoContent/Maps/Water/Textures/WaveMap0.texture";
        type.WaveMap1 = "DemoContent/Maps/Water/Textures/WaveMap1.texture";
        type.WaveMap0Velocity = new Vector2(0, 0.01f);
        type.WaveMap1Velocity = new Vector2(0.01f, 0);
        type.WaveMapScale = 64;
        type.WaterColor = new Vector4(0.5f, 0.79f, 0.75f, 0.25f);
        type.RenderTargetSize = new Vector2i(1024, 1024);
        type.MaximumLightExtinction = 0.25f;
        type.SynchronizationMethod = Synchronization_Method.ClientOnly;

        type.Save();
    }
}
