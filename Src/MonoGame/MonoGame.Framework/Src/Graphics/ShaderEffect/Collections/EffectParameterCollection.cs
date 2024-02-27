using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics
{
    public class EffectParameterCollection : IEnumerable<EffectParameter>
    {
        public static readonly EffectParameterCollection Empty = new EffectParameterCollection(new EffectParameter[0]);
        private readonly Dictionary<string, EffectParameter> parametersByName = new Dictionary<string, EffectParameter>();
        private readonly Dictionary<int, EffectParameter> parametersByIndex = new Dictionary<int, EffectParameter>();

        public bool Contains(string name)
        {
            if (parametersByName.ContainsKey(name))
                return true;

            return false;
        }

        private readonly int count;

        public EffectParameter this[int index] => parametersByIndex.ContainsKey(index) ? parametersByIndex[index] : null;

        public EffectParameter this[string key] => parametersByName.ContainsKey(key) ? parametersByName[key] : null;

        public int Count => count;

        public EffectParameterCollection(EffectParameter[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                var p = parameters[i];
                parametersByIndex.Add(i, p);
                parametersByName.Add(p.Name, p);
            }

            count = parameters.Length;
        }

        public IEnumerator<EffectParameter> GetEnumerator() =>
            ((IEnumerable<EffectParameter>)parametersByName.Values).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            parametersByName.Values.GetEnumerator();
    }
}
