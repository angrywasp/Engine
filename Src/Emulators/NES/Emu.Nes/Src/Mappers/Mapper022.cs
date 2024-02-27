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

    class Mapper22 : Mapper
    {
        public Mapper22(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int address, byte data)
        {
            if (address == 0x8000)
            {
                cpuMemory.Switch8kPrgRom(data * 2, 0);
            }
            else if (address == 0x9000)
            {
                switch (data & 0x3)
                {
                case 0: cartridge.Mirroring = Mirroring.ModeVert; break;
                case 1: cartridge.Mirroring = Mirroring.ModeHorz; break;
                case 2: cartridge.Mirroring = Mirroring.Mode1ScB; break;
                case 3: cartridge.Mirroring = Mirroring.Mode1ScA; break;
                }
            }
            else if (address == 0xA000)
            {
                cpuMemory.Switch8kPrgRom(data * 2, 1);
            }
            else if (address == 0xB000)
            {
                cpuMemory.Switch1kChrRom((data >> 1), 0);
            }
            else if (address == 0xB001)
            {
                cpuMemory.Switch1kChrRom((data >> 1), 1);
            }
            else if (address == 0xC000)
            {
                cpuMemory.Switch1kChrRom((data >> 1), 2);
            }
            else if (address == 0xC001)
            {
                cpuMemory.Switch1kChrRom((data >> 1), 3);
            }
            else if (address == 0xD000)
            {
                cpuMemory.Switch1kChrRom((data >> 1), 4);
            }
            else if (address == 0xD001)
            {
                cpuMemory.Switch1kChrRom((data >> 1), 5);
            }
            else if (address == 0xE000)
            {
                cpuMemory.Switch1kChrRom((data >> 1), 6);
            }
            else if (address == 0xE001)
            {
                cpuMemory.Switch1kChrRom((data >> 1), 7);
            }
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
        }
    }
}