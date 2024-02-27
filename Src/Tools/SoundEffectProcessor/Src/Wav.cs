// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Globalization;
using System.IO;
using Engine.Tools;

namespace Processor
{
    public static class Wav
    {
        private static byte[] _data;
        private static AudioFormat _format;

        public static byte[] Data  => _data;

        public static AudioFormat Format => _format;


        public static void Probe(string audioFileName)
        {
            try
            {
                // Get the full path to the file.
                audioFileName = Path.GetFullPath(audioFileName);

                // Use probe to get the details of the file.
                //ProbeFormat(audioFileName, out _format);

                byte[] rawData;

                using (var fs = new FileStream(audioFileName, FileMode.Open, FileAccess.Read))
                {
                    rawData = new byte[fs.Length];
                    fs.Read(rawData, 0, rawData.Length);
                }

                _data = StripRiffWaveHeader(rawData, out _format);

                if (_format != null)
                {
                    if ((_format.Format != 2 && _format.Format != 17) && _format.BlockAlign != _format.BlockAlign)
                        throw new InvalidOperationException("Calculated block align does not match RIFF " + _format.BlockAlign + " : " + _format.BlockAlign);
                    if (_format.ChannelCount != _format.ChannelCount)
                        throw new InvalidOperationException("Probed channel count does not match RIFF: " + _format.ChannelCount + ", " + _format.ChannelCount);
                    if (_format.Format != _format.Format)
                        throw new InvalidOperationException("Probed audio format does not match RIFF: " + _format.Format + ", " + _format.Format);
                    if (_format.SampleRate != _format.SampleRate)
                        throw new InvalidOperationException("Probed sample rate does not match RIFF: " + _format.SampleRate + ", " + _format.SampleRate);
                }
            }
            catch (Exception ex)
            {
                var message = string.Format("Failed to open file {0}. Ensure the file is a valid audio file and is not DRM protected.", Path.GetFileNameWithoutExtension(audioFileName));
                throw new Exception(message, ex);
            }
        }

        private static byte[] StripRiffWaveHeader(byte[] data, out AudioFormat audioFormat)
        {
            audioFormat = null;

            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                var signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    return data;

                reader.ReadInt32(); // riff_chunck_size

                var wformat = new string(reader.ReadChars(4));
                if (wformat != "WAVE")
                    return data;

                // Look for the data chunk.
                while (true)
                {
                    var chunkSignature = new string(reader.ReadChars(4));
                    if (chunkSignature.ToLowerInvariant() == "data")
                        break;
                    if (chunkSignature.ToLowerInvariant() == "fmt ")
                    {
                        int fmtLength = reader.ReadInt32();
                        short formatTag = reader.ReadInt16();
                        short channels = reader.ReadInt16();
                        int sampleRate = reader.ReadInt32();
                        int avgBytesPerSec = reader.ReadInt32();
                        short blockAlign = reader.ReadInt16();
                        short bitsPerSample = reader.ReadInt16();
                        audioFormat = new AudioFormat(bitsPerSample, blockAlign, channels, formatTag, sampleRate);

                        fmtLength -= 2 + 2 + 4 + 4 + 2 + 2;
                        if (fmtLength < 0)
                            throw new InvalidOperationException("riff wave header has unexpected format");
                        reader.BaseStream.Seek(fmtLength, SeekOrigin.Current);
                    }
                    else
                    {
                        reader.BaseStream.Seek(reader.ReadInt32(), SeekOrigin.Current);
                    }
                }

                var dataSize = reader.ReadInt32();
                data = reader.ReadBytes(dataSize);
            }

            return data;
        }
    }
}
