using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    public class EffectProgramCollection : IEnumerable<EffectProgram>
    {
        private readonly Dictionary<string, EffectProgram> programsByName = new Dictionary<string, EffectProgram>();
        private readonly Dictionary<int, EffectProgram> programsByIndex = new Dictionary<int, EffectProgram>();

        private readonly int count;

        public EffectProgram this[int index] => programsByIndex[index];

        public EffectProgram this[string key] => programsByName[key];

        public int Count => count;

        public EffectProgramCollection(EffectProgram[] programs)
        {
            for (int i = 0; i < programs.Length; i++)
            {
                var p = programs[i];
                programsByIndex.Add(i, p);
                programsByName.Add(p.Name, p);
            }

            count = programs.Length;
        }

        public IEnumerator<EffectProgram> GetEnumerator() =>
            ((IEnumerable<EffectProgram>)programsByName.Values).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            programsByName.Values.GetEnumerator();
    }
}
