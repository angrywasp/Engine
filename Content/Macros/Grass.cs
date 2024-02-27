using System.Threading.Tasks;
using Engine.Scripting;
using EngineScripting;

public class Macro_Grass : IMacro
{
    public void Run()
    {
        for (int i = 1; i <= 6; i++)
        {
            var mesh = $"DemoContent/Grass/Meshes/HighPoly/Var{i}.mesh".AsMeshTemplate();
            mesh.SubMeshes[0].UseMaterial("DemoContent/Grass/Grass.material");
            mesh.Save();
        }

        for (int i = 1; i <= 6; i++)
        {
            var mesh = $"DemoContent/Grass/Meshes/LowPoly/Var{i}.mesh".AsMeshTemplate();
            mesh.SubMeshes[0].UseMaterial("DemoContent/Grass/Grass.material");
            mesh.Save();
        }
    }
}