using System.IO;
using Engine.Content;

namespace Engine.AssetTransport
{
    public static class SoundReader
    {
        public static Sound Read(byte[] data)
        {
            using (var br = new BinaryReader(new MemoryStream(data)))
            {
                int sampleRate = br.ReadInt32();
                byte numChannels = br.ReadByte();

                return Sound.Create(numChannels, sampleRate, 16, false, br.ReadBytes((int)br.BaseStream.Length - 4));
            }
        }
    }
}
