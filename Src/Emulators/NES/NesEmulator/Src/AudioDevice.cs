using MyNes.Nes.Output.Audio;
using System;
using Microsoft.Xna.Framework.Audio;
using MyNes.Nes;

namespace NesEmulator
{
    public class AudioDevice : IAudioDevice
    {
        DynamicSoundEffectInstance snd;
        NesApu apu;
        bool paused;
        DateTime prev;
        byte[] buff = new byte[0];

        public System.IntPtr Handle
        {
            get { return IntPtr.Zero; }
        }

        public NesApu APU
        {
            get { return apu; }
            set { apu = value; }
        }

        public IWaveRecorder Recorder
        {
            get { throw new System.NotImplementedException(); }
        }

        public int Volume
        {
            get { return (int)(snd.Volume * 100.0f); }
        }

        public AudioDevice(NesApu apu)
        {
            this.apu = apu;
        }

        public void Initialize()
        {
            snd = new DynamicSoundEffectInstance(44100, AudioChannels.Mono);
            snd.BufferNeeded += new EventHandler<EventArgs>(snd_BufferNeeded);
            prev = DateTime.Now;
            snd.Play();
        }

        public void Play()
        {
            if (paused)
            {
                snd.Resume();
                prev = DateTime.Now;
            }
            else
                snd.Play();

            paused = false;
        }

        public void SetPan(int pan)
        {
            throw new System.NotImplementedException();
        }

        public void SetVolume(int volume)
        {
            snd.Volume = (float)volume / 100.0f;
        }

        public void Shutdown()
        {
            paused = true;
            snd.Stop();
            snd.Dispose();
        }

        public void Stop()
        {
            if (!paused)
                snd.Stop(true);

            paused = true;
        }
        
        int NearestEvenInt(int to)
		{
		  return (to % 2 == 0) ? to : (to - 1);
		}

        public void UpdateBuffer()
        {
            if (paused)
                return;
            int count = snd.GetSampleSizeInBytes(DateTime.Now - prev);
            prev = DateTime.Now;

            buff = new byte[NearestEvenInt(count)];
            for (int i = 0; i < buff.Length; i++)
            {
                int sample = apu.PullSample();
                buff[i] = (byte)((sample & 0xFF00) >> 8);
				i++;
                buff[i] = (byte)((sample & 0xFF));
            }

            if (buff.Length != 0)
                snd.SubmitBuffer(buff);
        }

        void snd_BufferNeeded(object sender, EventArgs e)
        {
            if (paused)
                return;
            if (buff.Length != 0)
                snd.SubmitBuffer(buff);
        }
    }
}
