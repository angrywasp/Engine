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
   
    class Mapper41 : Mapper
    {
        byte[] reg = new byte[2];
        public Mapper41(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int address, byte data)
        {
            if (address >= 0x6000 & address < 0x6800)
            {
                cpuMemory.Switch32kPrgRom((address & 0x07) * 8);
                reg[0] = (byte)(address & 0x04);
                reg[1] = (byte)(((address >> 1) & 0x0C) | reg[1] & 0x03);
                cpuMemory.Switch8kChrRom(reg[1] * 8);
                if ((address & 0x20) == 0x20)
                    cartridge.Mirroring = Mirroring.ModeHorz;
                else
                    cartridge.Mirroring = Mirroring.ModeVert;
            }
            else if (address >= 0x8000 & address < 0xFFFF)
            {
                if (reg[0] != 0)
                {
                    reg[1] = (byte)(reg[1] & 0x0C | address & 0x03);
                    cpuMemory.Switch8kChrRom(reg[1] * 8);
                }
            }
        }
        protected override void Initialize(bool initializing)
        {
            reg[0] = 0x04;
            cpuMemory.Map(0x6000, 0xFFFF, Poke);
            cpuMemory.Switch32kPrgRom(0);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
        }
        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(reg);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            stateStream.Read(reg);
            base.LoadState(stateStream);
        }
    }
}
