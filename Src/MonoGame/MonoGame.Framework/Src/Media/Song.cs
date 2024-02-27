// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Framework.Media
{
    public sealed class Song : IEquatable<Song>, IDisposable
    {
        private string name;
		private int playCount = 0;
        private TimeSpan duration = TimeSpan.Zero;
        private OggStream stream;
        private float volume = 1f;
        private bool disposed;
        
        public string Name => Path.GetFileNameWithoutExtension(name);

        public int PlayCount => playCount;

        public TimeSpan Duration => duration;	

        internal float Volume
        {
            get
            {
                if (stream == null)
                    return 0.0f;
                return volume; 
            }
            set
            {
                volume = value;
                if (stream != null)
                    stream.Volume = volume;
            }
        }

        public TimeSpan Position
        {
            get
            {
                if (stream == null)
                    return TimeSpan.FromSeconds(0.0);
                return stream.GetPosition();
            }
        }

        public bool IsDisposed => disposed;

        internal delegate void FinishedPlayingHandler(object sender, EventArgs args);

        internal Song(string fileName, int durationMS)
            : this(fileName)
        {
            duration = TimeSpan.FromMilliseconds(durationMS);
        }

		internal Song(string fileName)
		{			
			name = fileName;

            OpenALSoundController.EnsureInitialized();

            stream = new OggStream(fileName, OnFinishedPlaying);
            stream.Prepare();

            duration = stream.GetLength();
        }

        ~Song()
        {
            Dispose(false);
        }

        internal void Play(TimeSpan? startPosition)
        {
            if (stream == null)
                return;

            stream.Play();
            if (startPosition != null)
                stream.SeekToPosition((TimeSpan)startPosition);

            playCount++;
        }

        internal void Resume() => stream?.Resume();

        internal void Pause() => stream?.Pause();

        internal void Stop()
        {
            stream?.Stop();
            playCount = 0;
        }

        /// <summary>
        /// Returns a song that can be played via <see cref="MediaPlayer"/>.
        /// </summary>
        /// <param name="name">The name for the song. See <see cref="Song.Name"/>.</param>
        /// <param name="uri">The path to the song file.</param>
        /// <returns></returns>
        public static Song FromUri(string name, Uri uri)
        {
            var song = new Song(uri.OriginalString);
            song.name = name;
            return song;
        }
		
		public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (stream == null)
                        return;

                    stream.Dispose();
                    stream = null;
                }

                disposed = true;
            }
        }

        public bool Equals(Song song) => ((object)song != null) && (Name == song.Name);


        public override bool Equals(Object obj)
		{
			if(obj == null)
			{
				return false;
			}
			
			return Equals(obj as Song);  
		}
		
		public static bool operator ==(Song song1, Song song2)
		{
			if((object)song1 == null)
			{
				return (object)song2 == null;
			}

			return song1.Equals(song2);
		}

        public static bool operator !=(Song song1, Song song2) => !(song1 == song2);

        internal void OnFinishedPlaying() => MediaPlayer.OnSongFinishedPlaying(null, null);

        public override int GetHashCode() => base.GetHashCode();
    }
}
