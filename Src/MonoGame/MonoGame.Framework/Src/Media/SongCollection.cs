// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Media
{
	public class SongCollection : ICollection<Song>, IEnumerable<Song>, IEnumerable
	{
		private bool isReadOnly = false;
		private List<Song> innerlist = new List<Song>();

        internal SongCollection()
        {

        }

        internal SongCollection(List<Song> songs)
        {
            innerlist = songs;
        }
        
        public IEnumerator<Song> GetEnumerator() => innerlist.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => innerlist.GetEnumerator();

        public int Count => innerlist.Count;
		
		public bool IsReadOnly => isReadOnly;

        public Song this[int index] => innerlist[index];
		
		public void Add(Song item)
        {

            if (item == null)
                throw new ArgumentNullException();

            if (innerlist.Count == 0)
            {
                innerlist.Add(item);
                return;
            }

            innerlist.Add(item);
        }

        public void Clear() => innerlist.Clear();

        public SongCollection Clone()
        {
            SongCollection sc = new SongCollection();
            foreach (Song song in innerlist)
                sc.Add(song);
            return sc;
        }

        public bool Contains(Song item) => innerlist.Contains(item);

        public void CopyTo(Song[] array, int arrayIndex) => innerlist.CopyTo(array, arrayIndex);

        public int IndexOf(Song item) => innerlist.IndexOf(item);

        public bool Remove(Song item) => innerlist.Remove(item);
    }
}

