/*********************************************************************\
*This file is part of My Nes                                          *
*A Nintendo Entertainment System Emulator.                            *
*                                                                     *
*Copyright � Ala I.Hadid 2009 - 2012                                  *
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

    class Mapper09 : Mapper
    {
        public byte latch_a = 0xFE;
        public byte latch_b = 0xFE;
        public byte[] reg = new byte[4];
        public Mapper09(NesSystem nesSystem)
            : base(nesSystem) { }
        public override string Name
        {
            get
            {
                return "Nintendo MMC2";
            }
        }
        public override void Poke(int address, byte data)
        {
            address &= 0xF000;
            if (address == 0xA000)
            {
                cpuMemory.Switch8kPrgRom(data * 2, 0);
            }
            else if (address == 0xB000)
            {
                reg[0] = data;
                if (latch_a == 0xFD)
                {
                    cpuMemory.Switch4kChrRom(reg[0] * 4, 0);
                }
            }
            else if (address == 0xC000)
            {
                reg[1] = data;
                if (latch_a == 0xFE)
                {
                    cpuMemory.Switch4kChrRom(reg[1] * 4, 0);
                }
            }
            else if (address == 0xD000)
            {
                reg[2] = data;
                if (latch_b == 0xFD)
                {
                    cpuMemory.Switch4kChrRom(reg[2] * 4, 1);
                }
            }
            else if (address == 0xE000)
            {
                reg[3] = data;
                if (latch_b == 0xFE)
                {
                    cpuMemory.Switch4kChrRom(reg[3] * 4, 1);
                }
            }
            else if (address == 0xF000)
            {
                if ((data & 1) == 1)
                {
                    cartridge.Mirroring = Mirroring.ModeHorz;
                }
                else
                {
                    cartridge.Mirroring = Mirroring.ModeVert;
                }
            }
        }
        protected override void Initialize(bool initializing)
        {
            reg[0] = 0; reg[1] = 4;
            reg[2] = 0; reg[3] = 0;
            ppuMemory.Map(0x0000, 0x1FFF, ChrPeek, ChrPoke);
            cpuMemory.Switch32kPrgRom((cartridge.PrgPages - 1) * 4 - 4);
            cpuMemory.Switch8kPrgRom(0, 0);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
        }
        private byte ChrPeek(int addr)
        {
            CHRlatch(addr);
            return cartridge.Chr[cpuMemory.ChrBank[addr >> 10 & 0x07]][addr & 0x03FF];
        }
        private void ChrPoke(int addr, byte data)
        {
            cartridge.Chr[cpuMemory.ChrBank[addr >> 10 & 0x07]][addr & 0x03FF] = data;
        }
        public void CHRlatch(int Address)
        {
            if ((Address & 0x1FF0) == 0x0FD0 && latch_a != 0xFD)
            {
                latch_a = 0xFD;
                cpuMemory.Switch4kChrRom(reg[0] * 4, 0);
            }
            else if ((Address & 0x1FF0) == 0x0FE0 && latch_a != 0xFE)
            {
                latch_a = 0xFE;
                cpuMemory.Switch4kChrRom(reg[1] * 4, 0);
            }
            else if ((Address & 0x1FF0) == 0x1FD0 && latch_b != 0xFD)
            {
                latch_b = 0xFD;
                cpuMemory.Switch4kChrRom(reg[2] * 4, 1);
            }
            else if ((Address & 0x1FF0) == 0x1FE0 && latch_b != 0xFE)
            {
                latch_b = 0xFE;
                cpuMemory.Switch4kChrRom(reg[3] * 4, 1);
            }
        }
        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(latch_a);
            stateStream.Write(latch_b);
            stateStream.Write(reg);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            latch_a = stateStream.ReadByte();
            latch_b = stateStream.ReadByte();
            stateStream.Read(reg);
            base.LoadState(stateStream);
        }
    }
}
