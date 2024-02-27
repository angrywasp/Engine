# MacroProcessor

The MacroProcessor compiles and executes C# code. It is primarily used within the engine to generate game assets from code.

## Input File Types

.cs

## Output File Type

multiple

## Program Arguments

The program accepts a space separated list of .cs source files to compile. Additional settings are controlled via a configuration file 

### Example Usage

MacroProcessor a.cs b.cs c.cs ...

## Configuration Options

The MacroProcessor must be provided with the references and namespaces that will be used by the macros being processed. An example with useful defaults can be found in `Bin/Settings/MacroProcessor.config`. There are 4 sections in `Asset->Properties->ScriptEngine` used byu the MacroProcessor

- Includes: A list of namespaces that is automatically included in every macro file being processed.  
- RuntimeReferences: A list of .NET core references to be used. 
- ExternalReferences: A list of user defined libraries for referencing.
- ScriptFiles: Additional .cs files to reference. These are compiled to `ScriptEngine.Precompiled.MacroProcessor.dll` and added as a reference for your macros.

### File Structure

Macros executed by the MacroProcessor must be a valid C# class implementing the `Engine.Scripting.IMacro` interface. The IMacro interface exposes a single `Run` method which is the entry point for the macro.

Macros used by the engine are found in `Src/Macros` as an example, the macro to create the default skybox is `Src/Macros/BlueGreenSkybox.cs`, commented for readability

``` C#
using Engine.World.Components.Lights;
using Engine.World.MapObjects;
using Engine.Scripting;
using EngineScripting;
using Microsoft.Xna.Framework;

public class Macro_BlueGreenSkybox : IMacro
{
    public void Run()
    {
        //Create the skybox entity type
        SkyboxType t = Entity.Create<SkyboxType>("DemoContent/Maps/Skyboxes/BlueGreenSkybox.type");
        //Specify the texture to use
        t.Texture = "DemoContent/Maps/Skyboxes/BlueGreen/BlueGreen.texcube";

        //Add a directional light component to the skybox
        DirectionalLightComponentType d = Entity.AddDirectionalLightComponent("sun");
        //Set the light parameters. Color, shadows, and direction
        d.Color = new Color(Color.White, 1.0f).ToVector4();
        d.ShadowDistance = 150;
        d.LightDirection = new Vector3(1, -1, 1);

        //Save the entity. Will produce output at Bin/Content/DemoContent/Maps/Skyboxes/BlueGreenSkybox.type
        Entity.Save();
    }
}
```