using System.Numerics;
using System.Threading.Tasks;
using AngryWasp.Helpers;
using Engine.Content.Model;
using Engine.Scripting;
using Engine.World;
using EngineScripting;

public class Macro_LightMapObject : IMacro
{
    static readonly Matrix4x4 TRANSFORM = Matrix4x4.CreateRotationX(MathHelper.PiOver2);

    public void Run()
    {
        var mesh = "Engine/Renderer/Meshes/LightMeshes/LightMeshRaw.mesh".AsMeshTemplate();
        mesh = mesh.Transform(TRANSFORM);
        mesh.SaveAs("Engine/Renderer/Meshes/LightMeshes/LightMesh.mesh");
        
        {
            var e = Entity.Create<GameObjectType>("DemoContent/Lights/DirectionalLight.type");
            var l = e.AddDirectionalLightComponent("light");
            l.LightDirection = -Vector3.UnitY;
            l.ShadowDistance = 150;
            e.Save();
        }

        {
            var e = Entity.Create<GameObjectType>("DemoContent/Lights/SpotLight.type");
            var l = e.AddSpotLightComponent("light");
            l.SpotAngle = 35;
            l.SpotExponent = 5;
            l.Radius = 10;
            e.Save();
        }

        {
            var e = Entity.Create<GameObjectType>("DemoContent/Lights/PointLight.type");
            var l = e.AddPointLightComponent("light");
            l.Radius = 10;
            e.Save();
        }
    }
}
