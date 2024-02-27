using AngryWasp.Logger;
using System.IO;
using AngryWasp.Cli.Args;

namespace Processor
{
    internal class Program
    {
        private static void Main(string[] rawArgs)
        {
            var args = Arguments.Parse(rawArgs);
            Log.CreateInstance(true);
            new SoundEffectProcessor().Process(args.Pop().Value, args.Pop().Value, args);
        }
    }

    public class SoundEffectProcessor
    {
        //todo: test more input formats
        private const string INPUT_FILE_FILTER = "*.wav";
        private const string OUTPUT_FILE_FILTER = ".sound";
        private const string EXE_NAME = "SoundEffectProcessor";

        //use to force a specific channel count
        //can be used to convert between stereo/mono
        public const string SWITCH_FORCE_CHANNELS = "c";

        private int outputNumChannels = int.MinValue;

        private string input;
        private string output;

        public void Process(string input, string output, Arguments args)
        {
            this.input = input;
            this.output = output;

            outputNumChannels = args.GetInt(SWITCH_FORCE_CHANNELS, int.MinValue).Value;

            Log.Instance.Write($"Compiling Sound Effect {Path.GetFileName(output)}");

            Wav.Probe(input);

            int sampleRate = Wav.Format.SampleRate;
            byte[] audioData = Wav.Data;

            //all sound effects will be converted to 16-bit audio
            if (Wav.Format.BitsPerSample != 16)
                audioData = AudioConverter.ConvertAudio(Wav.Format.BitsPerSample, audioData);

            if (outputNumChannels == int.MinValue)
                outputNumChannels = Wav.Format.ChannelCount;

            if (outputNumChannels != Wav.Format.ChannelCount)
            {
                if (Wav.Format.ChannelCount == 1 && outputNumChannels == 2)
                    audioData = AudioConverter.ConvertMonoToStereo(audioData);
                else if (Wav.Format.ChannelCount == 2 && outputNumChannels == 1)
                    audioData = AudioConverter.ConvertStereoToMono(audioData);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(output));
            using (var bw = new BinaryWriter(new FileStream(output, FileMode.Create, FileAccess.Write)))
            {
                bw.Write(sampleRate);
                bw.Write((byte)outputNumChannels);
                bw.Write(audioData);
            }
        }
    }
}