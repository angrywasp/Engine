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
    class Mapper79 : Mapper
    {
        public Mapper79(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int address, byte data)
        {
            if ((address & 0x0100) != 0)
            {
                cpuMemory.Switch32kPrgRom(((data >> 3) & 0x01) * 8);
                cpuMemory.Switch8kChrRom((data & 0x07) * 8);
            }
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x4018, 0x7FFF, Poke);
            cpuMemory.Switch32kPrgRom(0);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(8);
            cpuMemory.Switch8kChrRom(0);
        }
    }
}
