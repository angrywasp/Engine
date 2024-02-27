using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    public class ShaderMetadata
    {
        public string ProgramPath { get; set; } = string.Empty;

        public Dictionary<string, string> Includes { get; set; } = new Dictionary<string, string>();

        public List<VertexElementMetadata> Layout { get; set; } = new List<VertexElementMetadata>();

        public List<SamplerStateMetadata> SamplerStates { get; set; } = new List<SamplerStateMetadata>();
    }

    public class VertexElementMetadata
    {
        public VertexElementUsage Usage { get; set; } = VertexElementUsage.Position;

        public string Name { get; set; }

        public int Location { get; set; } = 0;

        public int UsageIndex { get; set; } = 0;

        public VertexElementMetadata(string name, VertexElementUsage usage, int location, int usageIndex)
        {
            Name = name;
            Usage = usage;
            Location  = location;
            UsageIndex = usageIndex;
        }
    }

    public class SamplerStateMetadata
    {
        public string Name { get; set; } = string.Empty;

        public TextureFilter Filter { get; set; } = TextureFilter.Linear;

        public TextureAddressMode AddressU { get; set; } = TextureAddressMode.Wrap;
        public TextureAddressMode AddressV { get; set; } = TextureAddressMode.Wrap;
        public TextureAddressMode AddressW { get; set; } = TextureAddressMode.Wrap;

        public SamplerStateMetadata(string name, TextureFilter filter, TextureAddressMode u, TextureAddressMode v, TextureAddressMode w)
        {
            Name = name;
            AddressU = u;
            AddressV = v;
            AddressW = w;
            Filter = filter;
        }
    }
}