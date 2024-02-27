using System.Numerics;
using System.Threading.Tasks;
using Engine.Scripting;
using Engine.World.Components.Lights;
using Engine.World.Objects;
using EngineScripting;
using Microsoft.Xna.Framework;

public class Macro_BlueGreenSkybox : IMacro
{
    private void Create(string set, string name)
    {

        SkyboxType t = Entity.Create<SkyboxType>($"DemoContent/Skyboxes/{set}/{name}.type");
        t.EnvironmentMap = $"DemoContent/Skyboxes/{set}/Textures/{name}.texcube";
        t.IrradianceMap = $"DemoContent/Skyboxes/{set}/Textures/{name}.irradiance.texcube";
        t.ReflectanceMap = $"DemoContent/Skyboxes/{set}/Textures/{name}.reflectance.texcube";

        DirectionalLightComponentType d = t.AddDirectionalLightComponent("sun");
        d.Color = new Vector4(1, 1, 1, 1.5f);
        d.ShadowDistance = 100;
        d.LightDirection = new Vector3(1, -1, 1);

        t.Save();
    }

    private void Default(string name)
    {
        SkyboxType t = Entity.Create<SkyboxType>($"Engine/Skyboxes/{name}.type");
        t.EnvironmentMap = $"Engine/Textures/Cubes/{name}.texcube";
        t.IrradianceMap = $"Engine/Textures/Cubes/{name}texcube";
        t.ReflectanceMap = $"Engine/Textures/Cubes/{name}.texcube";

        DirectionalLightComponentType d = t.AddDirectionalLightComponent("sun");
        d.Color = new Vector4(1, 1, 1, 1.5f);
        d.ShadowDistance = 100;
        d.LightDirection = new Vector3(1, -1, 1);

        t.Save();
    }

    public void Run()
    {
        for (int i = 0; i < 10; i++)
            Create("Space", i.ToString());

        Create("Forest", "Brudslojan");
        Create("Forest", "Langholmen2");
        Create("Forest", "Langholmen3");
        Create("Forest", "MountainPath");
        Create("Forest", "Plants");

        Default("Default");
        Default("Black");
        Default("White");
    }
}
