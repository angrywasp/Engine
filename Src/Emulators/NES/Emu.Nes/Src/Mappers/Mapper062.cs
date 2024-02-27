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
    class Mapper62 : Mapper
    {
        public Mapper62(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int address, byte data)
        {
            if (address >= 0x8000 & address <= 0xBFFF)
            {
                cpuMemory.Switch8kChrRom(((address << 2) | (data & 0x3)) * 8);

                int data1 = ((address & 0x40) | (address >> 8 & 0x3F));
                int address1 = ~address >> 6 & 0x1;
                cpuMemory.Switch16kPrgRom((data1 & ~address1) * 4, 0);
                cpuMemory.Switch16kPrgRom((data1 | address1) * 4, 1);

                if ((address & 0x80) == 0x80)
                    cartridge.Mirroring = Mirroring.ModeHorz;
                else
                    cartridge.Mirroring = Mirroring.ModeVert;
            }
        }

        protected override void Initialize(bool initializing)
        {
            cpuMemory.Switch32kPrgRom(0);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
        }
    }
}
