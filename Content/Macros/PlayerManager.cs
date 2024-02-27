using Engine.Scripting;
using Engine.World.Objects;
using EngineScripting;

public class Macro_PlayerManager : IMacro
{
    public void Run()
    {
        PlayerManagerType t = Entity.Create<PlayerManagerType>("DemoContent/MapObjects/PlayerManager.type");
        t.PlayerUnit = "DemoContent/MapObjects/Character.type";
        t.PlayerController = "DemoContent/MapObjects/CharacterController.type";
        t.Save();
    }
}
