using System.Collections.Generic;
using System.Linq;

namespace MeshProcessor.Materials
{
    public enum MaterialBlendMode
    {
        Opaque = 0,
        Mask = 1,
        Blend = 2
    }
    
    public class MaterialChannelTemplate
    {
        private string target;

        public string Target => target;

        public int TextureIndex { get; set; } = -1;

        internal MaterialChannelTemplate(string name)
        {
            this.target = name;
        }
    }

    public class MaterialTemplate
    {
        private string name;

        public string Name => name;

        public MaterialTemplate(string name)
        {
            this.name = name;
        }

        private List<MaterialChannelTemplate> channels = new List<MaterialChannelTemplate>();

        public MaterialBlendMode Mode { get; set; }

        public bool DoubleSided { get; set; }

        public float AlphaCutoff { get; set; }

        public MaterialChannelTemplate UseChannel(string name)
        {
            var channel = FindChannel(name);

            if (channel == null)
            {
                channel = new MaterialChannelTemplate(name);
                channels.Add(channel);
            }

            return channel;
        }

        public MaterialChannelTemplate FindChannel(string name) => channels.FirstOrDefault(item => item.Target == name);
    }
}
