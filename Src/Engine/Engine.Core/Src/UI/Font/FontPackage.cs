using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Engine.UI
{
    public class FontPackage : IEnumerable<Font>
	{
		private Dictionary<int, Font> fonts = new Dictionary<int, Font>();

		public Font GetByFontSize(int index) => fonts[index];

		public Font GetByIndex(int index) => fonts.ElementAt(index).Value;

		public int Count => fonts.Count;

		public bool AddFont(Font font)
		{
			if (!fonts.ContainsKey(font.FontSize))
			{
				fonts.Add(font.FontSize, font);
				return true;
			}

			return false;
		}

		public Font Smallest() => fonts.FirstOrDefault().Value;

		public Font Largest() => fonts.LastOrDefault().Value;

        public bool Smallest(out int size, out Font font)
        {
            size = -1;
            font = null;

            if(fonts.Count == 0)
                return false;

            var first = fonts.First();

            size = first.Key;
            font = first.Value;

            return true;
        }

        #region IEnumerable implementation

        public IEnumerator<Font> GetEnumerator()
        {
            foreach(Font o in fonts.Values)
                yield return o;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}