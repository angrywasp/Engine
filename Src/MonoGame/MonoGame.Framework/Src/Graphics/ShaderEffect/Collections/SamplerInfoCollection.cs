using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    public class SamplerInfoCollection : IEnumerable<SamplerInfo>
    {
        public static readonly SamplerInfoCollection Empty = new SamplerInfoCollection(new SamplerInfo[0]);

		private readonly SamplerInfo[] _samplers;

        public SamplerInfoCollection(SamplerInfo[] samplers)
        {
            _samplers = samplers;
        }

        public SamplerInfo this[int index] => _samplers [index];

        public SamplerInfo this[string name]
        {
            get 
            {
                // TODO: Add a name to technique lookup table.
				foreach (var technique in _samplers) 
                {
					if (technique.Name == name)
						return technique;
			    }

			    return null;
		    }
        }

        public int Count => _samplers.Length;
        public IEnumerator<SamplerInfo> GetEnumerator() =>
            ((IEnumerable<SamplerInfo>)_samplers).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
            _samplers.GetEnumerator();
    }
}
