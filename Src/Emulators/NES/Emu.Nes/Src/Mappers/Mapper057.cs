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
    class Mapper57 : Mapper
    {
        byte reg = 0;
        public Mapper57(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int address, byte data)
        {
            switch (address)
            {
                case 0x8000:
                case 0x8001:
                case 0x8002:
                case 0x8003:
                    if ((data & 0x40) == 0x40)
                    {
                        cpuMemory.Switch8kChrRom(((data & 0x03) + ((reg & 0x10) >> 1) + (reg & 0x07)) * 8);
                    }
                    break;
                case 0x8800:
                    reg = data;

                    if ((data & 0x80) == 0x80)
                    {
                        cpuMemory.Switch8kPrgRom((((data & 0x40) >> 6) * 4 + 8 + 0) * 2, 0);
                        cpuMemory.Switch8kPrgRom((((data & 0x40) >> 6) * 4 + 8 + 1) * 2, 1);
                        cpuMemory.Switch8kPrgRom((((data & 0x40) >> 6) * 4 + 8 + 2) * 2, 2);
                        cpuMemory.Switch8kPrgRom((((data & 0x40) >> 6) * 4 + 8 + 3) * 2, 3);
                    }
                    else
                    {
                        cpuMemory.Switch8kPrgRom((((data & 0x60) >> 5) * 2 + 0) * 2, 0);
                        cpuMemory.Switch8kPrgRom((((data & 0x60) >> 5) * 2 + 1) * 2, 1);
                        cpuMemory.Switch8kPrgRom((((data & 0x60) >> 5) * 2 + 0) * 2, 2);
                        cpuMemory.Switch8kPrgRom((((data & 0x60) >> 5) * 2 + 1) * 2, 3);
                    }

                    cpuMemory.Switch8kChrRom(((data & 0x07) + ((data & 0x10) >> 1)) * 8);

                    if ((data & 0x08) == 0x08)
                        cartridge.Mirroring = Mirroring.ModeHorz;
                    else
                        cartridge.Mirroring = Mirroring.ModeVert;
                    break;
            }
        }

        protected override void Initialize(bool initializing)
        {
            cpuMemory.Switch16kPrgRom(0, 0);
            cpuMemory.Switch16kPrgRom(0, 1);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
        }
    }
}
