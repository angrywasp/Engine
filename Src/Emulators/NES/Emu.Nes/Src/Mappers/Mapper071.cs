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
    class Mapper71 : Mapper
    {
        public Mapper71(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int address, byte data)
        {
            if ((address & 0xE000) == 0x6000)
                cpuMemory.Switch16kPrgRom(data * 4, 0);
            else
                switch (address & 0xF000)
                {
                    case 0xF000:
                    case 0xE000:
                    case 0xD000:
                    case 0xC000: cpuMemory.Switch16kPrgRom(data * 4, 0); break;
                    case 0x9000:
                        if ((data & 0x10) != 0)
                            cartridge.Mirroring = Mirroring.Mode1ScB;
                        else
                            cartridge.Mirroring = Mirroring.Mode1ScA;
                        break;
                }
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x4018, 0x7FFF, Poke);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
        }
    }
}