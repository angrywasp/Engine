using System.Numerics;
using Engine;
using Engine.Objects.GameObjects;
using Engine.Objects.GameObjects.Controllers;
using Engine.Objects.GameObjects.Pickups;
using Engine.Scripting;
using EngineScripting;

public class Macro_Weapons : IMacro
{
    public void Run()
    {
        var primaryBullet = Entity.Create<BulletType>("DemoContent/Entities/Talon/PrimaryBullet.type");
        primaryBullet.FireSound = "DemoContent/Entities/Talon/Sounds/PrimaryFire.sound";
        primaryBullet.Save();

        var secondaryBullet = Entity.Create<BulletType>("DemoContent/Entities/Talon/SecondaryBullet.type");
        secondaryBullet.FireSound = "DemoContent/Entities/Talon/Sounds/SecondaryFire.sound";
        secondaryBullet.Save();

        var gun = Entity.Create<WeaponType>("DemoContent/Entities/Talon/Talon.type");
        gun.LifeMin = 0.0f;
        gun.LifeMax = 100.0f;
        gun.BoneName = "RightHand";
        gun.PrimaryBulletType = "DemoContent/Entities/Talon/PrimaryBullet.type";
        gun.SecondaryBulletType = "DemoContent/Entities/Talon/SecondaryBullet.type";

        var gunMesh = gun.AddMeshComponent("DemoContent/Entities/Talon/Meshes/Talon.mesh", "Mesh_0");
        gunMesh.LocalTransform = WorldTransform3.Create(Vector3.One * 0.1f, Quaternion.Identity, Vector3.Zero);
        gun.Save();

        var gunPickup = Entity.Create<WeaponPickupType>("DemoContent/Entities/Talon/TalonPickup.type");
        var pickupMesh = gunPickup.AddMeshComponent("DemoContent/Entities/Talon/Meshes/Talon.mesh", "Mesh_0");
        pickupMesh.LocalTransform = WorldTransform3.Create(Vector3.One * 0.1f, Quaternion.Identity, Vector3.Zero);
        gunPickup.WeaponType = "DemoContent/Entities/Talon/Talon.type";
        gunPickup.PickupSound = "DemoContent/Entities/Talon/Sounds/Reload.sound";
        gunPickup.Controller = "DemoContent/MapObjects/PickupController.type";
        gunPickup.Save();

        var gunPickupController = Entity.Create<PickupControllerType>("DemoContent/MapObjects/PickupController.type");
        gunPickupController.SpinSpeed = 45;
        gunPickupController.Save();
    }
}
