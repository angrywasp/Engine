using System.Collections;
using System.Collections.Generic;

namespace Engine.UI
{
    public class SkinDefinition : IEnumerable<SkinElement>
    {
        private Dictionary<string, SkinElement> elements = new Dictionary<string, SkinElement>();

        public Dictionary<string, SkinElement> Elements => elements;

        public void AddElement(SkinElement e, string name)
        {
            elements.Add(name, e);
        }

        #region IEnumerable implementation

        public IEnumerator<SkinElement> GetEnumerator()
        {
            foreach(SkinElement o in elements.Values)
                yield return o;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}