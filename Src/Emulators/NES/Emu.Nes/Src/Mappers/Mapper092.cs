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
    class Mapper92 : Mapper
    {
        public Mapper92(NesSystem nesSystem)
            : base(nesSystem) { }

        public override void Poke(int address, byte data)
        {
            data = (byte)(address & 0xFF);

            if (address >= 0x9000)
            {
                if ((data & 0xF0) == 0xD0)
                {
                    cpuMemory.Switch16kPrgRom((data & 0x0F) * 4, 1);

                }
                else if ((data & 0xF0) == 0xE0)
                {
                    cpuMemory.Switch8kChrRom((data & 0x0F) * 8);
                }
            }
            else
            {
                if ((data & 0xF0) == 0xB0)
                {
                    cpuMemory.Switch16kPrgRom((data & 0x0F) * 4, 1);
                }
                else if ((data & 0xF0) == 0x70)
                {
                    cpuMemory.Switch8kChrRom((data & 0x0F) * 8);
                }
            }
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Switch16kPrgRom(0, 0);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
        }
    }
}
