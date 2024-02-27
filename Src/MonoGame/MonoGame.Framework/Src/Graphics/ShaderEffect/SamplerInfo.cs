namespace Microsoft.Xna.Framework.Graphics
{
    public class SamplerInfo
    {
        public readonly string Name;
        public readonly SamplerType Type;
        public readonly int Slot;
        public readonly int Parameter;
        public readonly int Location;

        public SamplerState State;

        public SamplerInfo(string name, SamplerType type, int slot, int parameter, int location)
        {
            Name = name;
            Type = type;
            Slot = slot;
            Parameter = parameter;
            Location = location;
            State = new SamplerState();
        }
    }
}