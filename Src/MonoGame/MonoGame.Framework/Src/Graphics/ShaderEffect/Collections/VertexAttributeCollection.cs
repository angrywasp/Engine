using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Xna.Framework.Graphics
{
    public class VertexAttributeCollection : IEnumerable<VertexAttribute>
    {
        public static readonly VertexAttributeCollection Empty = new VertexAttributeCollection(new VertexAttribute[0]);

		private readonly VertexAttribute[] _attributes;

        public VertexAttributeCollection(IEnumerable<VertexAttribute> attributes)
        {
            _attributes = attributes.OrderBy(x => x.Location).ToArray();
        }

        public VertexAttribute this[int index] => _attributes [index];

        public VertexAttribute this[VertexElementUsage usage, int index]
        {
            get
            {
                foreach (var a in _attributes) 
                    if (a.Usage == usage && a.Index == index)
                        return a;

                throw new KeyNotFoundException($"Vertex attribute with usage {usage}:{index} not found");
            }
        }

        public int Count => _attributes.Length;
        public IEnumerator<VertexAttribute> GetEnumerator() =>
            ((IEnumerable<VertexAttribute>)_attributes).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
            _attributes.GetEnumerator();
    }
}
