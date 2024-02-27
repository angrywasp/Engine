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
    class Mapper246 : Mapper
    {
        public Mapper246(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int Address, byte data)
        {
            if (Address >= 0x6000 && Address < 0x8000)
            {
                switch (Address)
                {
                    case 0x6000:
                        cpuMemory.Switch8kPrgRom(data * 2, 0);
                        break;
                    case 0x6001:
                        cpuMemory.Switch8kPrgRom(data * 2, 1);
                        break;
                    case 0x6002:
                        cpuMemory.Switch8kPrgRom(data * 2, 2);
                        break;
                    case 0x6003:
                        cpuMemory.Switch8kPrgRom(data * 2, 3);
                        break;
                    case 0x6004:
                        cpuMemory.Switch2kChrRom(data * 2, 0);
                        break;
                    case 0x6005:
                        cpuMemory.Switch2kChrRom(data * 2, 1);
                        break;
                    case 0x6006:
                        cpuMemory.Switch2kChrRom(data * 2, 2);
                        break;
                    case 0x6007:
                        cpuMemory.Switch2kChrRom(data * 2, 3);
                        break;
                }
            }
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x6000, 0x7FFF, Poke);
            cpuMemory.Switch16kPrgRom(0, 0);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
        }
    }
}
