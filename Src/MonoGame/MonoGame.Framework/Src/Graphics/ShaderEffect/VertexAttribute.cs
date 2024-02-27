namespace Microsoft.Xna.Framework.Graphics
{
    public struct VertexAttribute
    {
        private VertexElementUsage usage;
        private int index;
        private int location;

        public VertexElementUsage Usage => usage;
        public int Index => index;
        public int Location => location;

        public VertexAttribute(VertexElementUsage usage, int index, int location)
        {
            this.usage = usage;
            this.index = index;
            this.location = location;
        }
    }
}