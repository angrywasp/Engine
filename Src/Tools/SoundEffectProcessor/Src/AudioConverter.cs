using AngryWasp.Math;

namespace Processor
{
    public static class AudioConverter
    {
        public static byte[] ConvertAudio(int bitsPerSample, byte[] o)
        {
            byte[] soundData = null;

            switch (bitsPerSample)
            {
                case 8:
                    {
                        soundData = new byte[o.Length * 2];

                            int x = 0;
                            for (int i = 0; i < o.Length; i++)
                                WriteSample(ref soundData, ref x, (short)((o[i] - 128) << 8));
                    }
                    break;
                case 16:
                    return o; //no conversion required;
                case 24:
                    {
                        soundData = new byte[(o.Length / 3) * 2];

                        int x = 0;
                        for (int i = 0; i < o.Length; i += 3)
                        {
                            int j = (int)o[i] | (int)o[i + 1] << 8 | (int)o[i + 2] << 16;
                            WriteSample(ref soundData, ref x, (short)(j >> 8));
                        }
                    }
                    break;
                case 32:
                    {
                        soundData = new byte[o.Length / 2];

                        int x = 0;
                        for (int i = 0; i < o.Length; i += 4)
                        {
                            int start = i;
                            int j = o.ToInt(ref start);
                            WriteSample(ref soundData, ref x, (short)(j >> 16));
                        }
                    }
                    break;
            }

            return soundData;
        }

        //converting between mono & stereo is only supported for 16-bit sound files
        //use ConvertAudio to convert to 16-bit first

        public static byte[] ConvertStereoToMono(byte[] audioData)
        {
            byte[] temp = new byte[audioData.Length / 2];

            int x = 0;
            for (int i = 0; i < audioData.Length; i += 4)
            {
                byte l1 = audioData[i];
                byte l2 = audioData[i + 1];

                byte r1 = audioData[i + 2];
                byte r2 = audioData[i + 3];

                short l = (short)(l1 | l2 << 8);
                short r = (short)(r1 | r2 << 8);

                short m = (short)((l + r) / 2);

                temp[x++] = (byte)m;
                temp[x++] = (byte)(m >> 8);
            }

            return temp;
        }

        public static byte[] ConvertMonoToStereo(byte[] audioData)
        {
            byte[] temp = new byte[audioData.Length * 2];

            int x = 0;
            for (int i = 0; i < audioData.Length; i += 2)
            {
                byte m1 = audioData[i];
                byte m2 = audioData[i + 1];
                temp[x++] = m1;
                temp[x++] = m2;
                temp[x++] = m1;
                temp[x++] = m2;
            }

            return temp;
        }

        private static void WriteSample(ref byte[] soundData, ref int x, short s)
        {
            byte[] b = s.ToByte();
            soundData[x++] = b[0];
            soundData[x++] = b[1];
        }
    }
}