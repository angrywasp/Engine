using System;
using Engine.Content;
using Engine.Helpers;
using Engine.Objects.GameObjects;
using Engine.Scripting;
using Engine.World.Components;
using EngineScripting;

public class Macro_FireEmitter : IMacro
{
    public void Run()
    {
        ParticleSettings smoke = new ParticleSettings();
        smoke.TextureName = "DemoContent/Emitters/Smoke.texture";
        smoke.MinStartSize = 0.25f;
        smoke.MaxStartSize = 1.0f;
        smoke.MinEndSize = 1.0f;
        smoke.MaxEndSize = 5.0f;
        smoke.MinVerticalVelocity = 0.5f;
        smoke.MaxVerticalVelocity = 1.0f;
        smoke.MinHorizontalVelocity = 0.0f;
        smoke.MaxHorizontalVelocity = 0.0f;
        smoke.Duration = TimeSpan.FromSeconds(10);

        ParticleSettings fire = new ParticleSettings();
        fire.TextureName = "DemoContent/Emitters/Fire.texture";
        fire.MinStartSize = 0.25f;
        fire.MaxStartSize = 1.0f;
        fire.MinEndSize = 1.0f;
        fire.MaxEndSize = 3.0f;
        fire.MinVerticalVelocity = 0.25f;
        fire.MaxVerticalVelocity = 0.5f;
        fire.MinHorizontalVelocity = -0.25f;
        fire.MaxHorizontalVelocity = 0.25f;

        ContentLoader.SaveJson(smoke, EngineFolders.ContentPathVirtualToReal("DemoContent/Emitters/Smoke.emitter"));
        ContentLoader.SaveJson(fire, EngineFolders.ContentPathVirtualToReal("DemoContent/Emitters/Fire.emitter"));

        var d = Entity.Create<DynamicType>("DemoContent/Emitters/FireEmitter.type");
        d.LifeMin = 0.0f;
        d.LifeMax = 100.0f;

        EmitterComponentType ec = d.AddEmitterComponent("Emitter_0");
        ec.ParticleSettings.Add("DemoContent/Emitters/Fire.emitter");
        ec.ParticleSettings.Add("DemoContent/Emitters/Smoke.emitter");
        d.Save();
    }
}
