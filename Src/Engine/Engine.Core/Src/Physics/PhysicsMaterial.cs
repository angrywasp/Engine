namespace Engine.Physics
{
    public struct PhysicsMaterial
    {
        public BepuPhysics.Constraints.SpringSettings SpringSettings;
        public float FrictionCoefficient;
        public float MaximumRecoveryVelocity;
    }
}