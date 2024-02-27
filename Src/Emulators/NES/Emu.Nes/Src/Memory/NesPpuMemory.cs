/*********************************************************************\
*This file is part of My Nes                                          *
*A Nintendo Entertainment System Emulator.                            *
*                                                                     *
*Copyright © Ala I.Hadid 2009 - 2012                                  *
*E-mail: mailto:ahdsoftwares@hotmail.com                              *
*                                                                     *
*My Nes is free software: you can redistribute it and/or modify       *
*it under the terms of the GNU General Public License as published by *
*the Free Software Foundation, either version 3 of the License, or    *
*(at your option) any later version.                                  *
*                                                                     *
*My Nes is distributed in the hope that it will be useful,            *
*but WITHOUT ANY WARRANTY; without even the implied warranty of       *
*MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the        *
*GNU General Public License for more details.                         *
*                                                                     *
*You should have received a copy of the GNU General Public License    *
*along with this program.  If not, see <http://www.gnu.org/licenses/>.*
\*********************************************************************/
namespace MyNes.Nes
{
    public class NesPpuMemory : NesMemory
    {
        public const int AddrBusLines = 15;

        public byte[] nmtBank;
        private byte[] pal = new byte[]//Misc, real nes load these values at power up
        {
           0x09,0x01,0x00,0x01,0x00,0x02,0x02,0x0D,0x08,0x10,0x08,0x24,0x00,0x00,0x04,0x2C,
           0x09,0x01,0x34,0x03,0x00,0x04,0x00,0x14,0x08,0x3A,0x00,0x02,0x00,0x20,0x2C,0x08
        };

        public byte[][] nmt;
        public byte[] ObjRam = new byte[256];


        public NesPpuMemory(NesSystem nesSystem)
            : base(nesSystem, 1 << AddrBusLines)
        {
            nmtBank = new byte[0x04];

            nmt = new byte[0x04][];

            nmt[0] = new byte[0x0400];
            nmt[1] = new byte[0x0400];
            nmt[2] = new byte[0x0400];
            nmt[3] = new byte[0x0400];
        }

        public byte ChrPeek(int addr)
        {
            return cartridge.Chr[cpuMemory.ChrBank[addr >> 10 & 0x07]][addr & 0x03FF];
        }
        private void ChrPoke(int addr, byte data)
        {
            cartridge.Chr[cpuMemory.ChrBank[addr >> 10 & 0x07]][addr & 0x03FF] = data;
        }
        public byte NmtPeek(int addr)
        {
            return nmt[nmtBank[addr >> 10 & 0x03]][addr & 0x03FF];
        }
        private void NmtPoke(int addr, byte data)
        {
            nmt[nmtBank[addr >> 10 & 0x03]][addr & 0x03FF] = data;
        }
        public byte PalPeek(int addr)
        {
            return pal[addr & ((addr & 0x03) == 0 ? 0x0C : 0x1F)];
        }
        private void PalPoke(int addr, byte data)
        {
            pal[addr & ((addr & 0x03) == 0 ? 0x0C : 0x1F)] = data;
        }

        protected override void Initialize(bool initializing)
        {
            if (initializing)
            {
                CONSOLE.WriteLine(this, "Initializing Video Memory....", DebugStatus.None);
                base.Map(0x0000, 0x1FFF, ChrPeek, ChrPoke);
                base.Map(0x2000, 0x3EFF, NmtPeek, NmtPoke);
                base.Map(0x3F00, 0x3FFF, PalPeek, PalPoke);
                CONSOLE.WriteLine(this, "Video Memory Initialized OK.", DebugStatus.Cool);
            }

            base.Initialize(initializing);
        }

        public void SwitchNmt(Mirroring mirroring)
        {
            var value = (byte)(mirroring);

            nmtBank[0] = (byte)(value >> 6 & 0x03);
            nmtBank[1] = (byte)(value >> 4 & 0x03);
            nmtBank[2] = (byte)(value >> 2 & 0x03);
            nmtBank[3] = (byte)(value >> 0 & 0x03);
        }
        public void SwitchNmt(byte value)
        {
            nmtBank[3] = (byte)(value >> 6 & 0x03);
            nmtBank[2] = (byte)(value >> 4 & 0x03);
            nmtBank[1] = (byte)(value >> 2 & 0x03);
            nmtBank[0] = (byte)(value >> 0 & 0x03);
        }

        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(nmtBank);
            stateStream.Write(pal);
            for (int i = 0; i < 4; i++)
                stateStream.Write(nmt[i]);
            stateStream.Write(ObjRam);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            stateStream.Read(nmtBank);
            stateStream.Read(pal);
            for (int i = 0; i < 4; i++)
                stateStream.Read(nmt[i]);
            stateStream.Read(ObjRam);
            base.LoadState(stateStream);
        }
    }
}