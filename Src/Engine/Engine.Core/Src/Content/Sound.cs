using System;
using Microsoft.Xna.Framework.Audio;

namespace Engine.Content
{
    /// <summary>
    /// The Sound file represents a .sound binary file. This is consumed by a SoundInstance provided by the AudioPool
    /// </summary>
    public class Sound : IDisposable
    {
        private AudioChannels numChannels;
        private int sampleRate;
        private int bitDepth;
		private byte[] audioData;

		public AudioChannels NumChannels => numChannels;
        public int SampleRate => sampleRate;
        public int BitDepth => bitDepth;
        public byte[] AudioData => audioData;
			
        public Sound() { }

        public static Sound Create(int numChannels, int sampleRate, int bitDepth, bool floatingPoint, byte[] audioData)
        {
            Sound snd = new Sound();
			snd.numChannels = (AudioChannels)numChannels;
            snd.sampleRate = sampleRate;
            snd.bitDepth = bitDepth;
            snd.audioData = audioData;

            return snd;
        }

        public void Unload()
        {
            //todo: return to the audio pool and dispose
        }

        #region IDisposable implementation

        private bool isDisposed = false;

        public bool IsDisposed => isDisposed;

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;
            }

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}