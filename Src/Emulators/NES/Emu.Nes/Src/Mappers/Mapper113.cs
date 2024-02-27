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
    class Mapper113 : Mapper
    {
        public Mapper113(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int address, byte data)
        {
            switch (address)
            {
                case 0x4100:
                case 0x4111:
                case 0x4120:
                case 0x4194:
                case 0x4195:
                case 0x4900:
                case 0x8008:
                case 0x8009:
                    cpuMemory.Switch32kPrgRom((data >> 3) * 8);
                    cpuMemory.Switch8kChrRom((((data >> 3) & 0x08) + (data & 0x07)) * 8);
                    break;
                case 0x8E66:
                case 0x8E67:
                    cpuMemory.Switch8kChrRom(((data & 0x07) == 0x07) ? 0 : 8);
                    break;
                case 0xE00A: cartridge.Mirroring = Mirroring.Mode1ScA; break;
            }
        }
        protected override void Initialize(bool initializing)
        {
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(8);
            cpuMemory.Map(0x4018, 0x7FFF, Poke);
            cpuMemory.Switch8kChrRom(0);
            cpuMemory.Switch32kPrgRom(0);
        }
    }
}
