namespace Processor
{
    public class AudioFormat
    {
        private int bitsPerSample;
        private int blockAlign;
        private int channelCount;
        private int format;
        private int sampleRate;

        public int BitsPerSample => bitsPerSample;
        public int BlockAlign => blockAlign;
        public int ChannelCount => channelCount;
        public int Format => format;
        public int SampleRate => sampleRate;

        public AudioFormat(int bitsPerSample, int blockAlign, int channelCount, int format, int sampleRate)
        {
            this.bitsPerSample = bitsPerSample;
            this.blockAlign = blockAlign;
            this.channelCount = channelCount;
            this.format = format;
            this.sampleRate = sampleRate;
        }
    }
}
