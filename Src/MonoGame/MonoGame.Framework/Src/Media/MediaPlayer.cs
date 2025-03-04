// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Media
{
    public static partial class MediaPlayer
    {
        // Need to hold onto this to keep track of how many songs
        // have played when in shuffle mode
        private static int _numSongsInQueuePlayed = 0;
        private static MediaState _state = MediaState.Stopped;
        private static float _volume = 1.0f;
        private static bool _isMuted;
        private static bool _isRepeating;
        private static bool _isShuffled;
        private static readonly MediaQueue _queue = new MediaQueue();

        public static event EventHandler ActiveSongChanged;
        public static event EventHandler MediaStateChanged;

        #region Properties

        public static MediaQueue Queue { get { return _queue; } }

        public static bool IsMuted
        {
            get { return _isMuted; }
            set
            {
                _isMuted = value;

                if (_queue.Count == 0)
                    return;

                var newVolume = _isMuted ? 0.0f : _volume;
                _queue.SetVolume(newVolume);
            }
        }

        public static bool IsRepeating
        {
            get { return _isRepeating; }
            set { _isRepeating = value; }
        }

        public static bool IsShuffled
        {
            get { return _isShuffled; }
            set { _isShuffled = value; }
        }

        public static bool IsVisualizationEnabled { get { return false; } }

        public static TimeSpan PlayPosition
        {
            get
            {
                if (_queue.ActiveSong == null)
                    return TimeSpan.Zero;

                return _queue.ActiveSong.Position;
            }
        }

        public static MediaState State
        {
            get { return _state;; }
            private set
            {
                if (_state != value)
                {
                    _state = value;
                    MediaStateChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        public static float Volume
        {
            get { return _volume; }
            set
            {
                var volume = MathHelper.Clamp(value, 0, 1);

                _volume = volume;

                if (_queue.ActiveSong == null)
                    return;

                _queue.SetVolume(_isMuted ? 0.0f : _volume);
            }
        }

        #endregion

        public static void Pause()
        {
            if (State != MediaState.Playing || _queue.ActiveSong == null)
                return;

            if (_queue.ActiveSong == null)
                return;

            _queue.ActiveSong.Pause();

            State = MediaState.Paused;
        }

        /// <summary>
        /// Play clears the current playback queue, and then queues up the specified song for playback. 
        /// Playback starts immediately at the beginning of the song.
        /// </summary>
        public static void Play(Song song)
        {
            Play(song, null);
        }

        /// <summary>
        /// Play clears the current playback queue, and then queues up the specified song for playback. 
        /// Playback starts immediately at the given position of the song.
        /// </summary>
        public static void Play(Song song, TimeSpan? startPosition)
        {
            var previousSong = _queue.Count > 0 ? _queue[0] : null;
            _queue.Clear();
            _numSongsInQueuePlayed = 0;
            _queue.Add(song);
            _queue.ActiveSongIndex = 0;

            PlaySong(song, startPosition);

            if (previousSong != song)
                ActiveSongChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void Play(SongCollection collection, int index = 0)
        {
            _queue.Clear();
            _numSongsInQueuePlayed = 0;

            foreach (var song in collection)
                _queue.Add(song);

            _queue.ActiveSongIndex = index;

            PlaySong(_queue.ActiveSong, null);
        }

        private static void PlaySong(Song song, TimeSpan? startPosition)
        {
            if (song != null && song.IsDisposed)
                throw new ObjectDisposedException("song");

            if (_queue.ActiveSong == null)
                return;

            song.Volume = _isMuted ? 0.0f : _volume;
            song.Play(startPosition);

            State = MediaState.Playing;
        }

        internal static void OnSongFinishedPlaying(object sender, EventArgs args)
        {
            // TODO: Check args to see if song sucessfully played
            _numSongsInQueuePlayed++;

            if (_numSongsInQueuePlayed >= _queue.Count)
            {
                _numSongsInQueuePlayed = 0;
                if (!IsRepeating)
                {
                    Stop();
                    ActiveSongChanged?.Invoke(null, EventArgs.Empty);
                    return;
                }
            }

            MoveNext();
        }

        public static void Resume()
        {
            if (State != MediaState.Paused)
                return;

            if (_queue.ActiveSong == null)
                return;

            _queue.ActiveSong.Resume();

            State = MediaState.Playing;
        }

        public static void Stop()
        {
            if (State == MediaState.Stopped)
                return;

            foreach (var song in Queue.Songs)
                _queue.ActiveSong.Stop();

            State = MediaState.Stopped;
        }

        public static void MoveNext()
        {
            NextSong(1);
        }

        public static void MovePrevious()
        {
            NextSong(-1);
        }

        private static void NextSong(int direction)
        {
            Stop();

            if (IsRepeating && _queue.ActiveSongIndex >= _queue.Count - 1)
            {
                _queue.ActiveSongIndex = 0;

                // Setting direction to 0 will force the first song
                // in the queue to be played.
                // if we're on "shuffle", then it'll pick a random one
                // anyway, regardless of the "direction".
                direction = 0;
            }

            var nextSong = _queue.GetNextSong(direction, IsShuffled);

            if (nextSong != null)
                PlaySong(nextSong, null);

            ActiveSongChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}

