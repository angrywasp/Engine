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
    class Mapper89 : Mapper
    {
        public Mapper89(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int Address, byte data)
        {
            if ((Address & 0xFF00) == 0xC000)
            {
                cpuMemory.Switch16kPrgRom(((data & 0x70) >> 4) * 4, 0);

                cpuMemory.Switch8kChrRom((((data & 0x80) >> 4) | (data & 0x07)) * 8);

                if ((data & 0x08) != 0)
                    cartridge.Mirroring = Mirroring.Mode1ScB;
                else
                    cartridge.Mirroring = Mirroring.Mode1ScA;
            }
        }
        protected override void Initialize(bool initializing)
        {
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);

            cpuMemory.Switch8kChrRom(0);
            cpuMemory.Switch16kPrgRom(0, 0);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
        }
    }
}