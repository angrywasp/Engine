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
   
    class Mapper15 : Mapper
    {
        public Mapper15(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int address, byte data)
        {
            switch (address)
            {
                case 0x8000:
                    if ((data & 0x80) == 0x80)
                    {
                        cpuMemory.Switch8kPrgRom(((data & 0x3F) * 2 + 1) * 2, 0);
                        cpuMemory.Switch8kPrgRom(((data & 0x3F) * 2 + 0) * 2, 1);
                        cpuMemory.Switch8kPrgRom(((data & 0x3F) * 2 + 3) * 2, 2);
                        cpuMemory.Switch8kPrgRom(((data & 0x3F) * 2 + 2) * 2, 3);
                    }
                    else
                    {
                        cpuMemory.Switch8kPrgRom(((data & 0x3F) * 2 + 0) * 2, 0);
                        cpuMemory.Switch8kPrgRom(((data & 0x3F) * 2 + 1) * 2, 1);
                        cpuMemory.Switch8kPrgRom(((data & 0x3F) * 2 + 2) * 2, 2);
                        cpuMemory.Switch8kPrgRom(((data & 0x3F) * 2 + 3) * 2, 3);
                    }
                    if ((data & 0x40) == 0x40)
                        cartridge.Mirroring = Mirroring.ModeHorz;
                    else
                        cartridge.Mirroring = Mirroring.ModeVert;
                    break;
                case 0x8001:
                    if ((data & 0x80) == 0x80)
                    {
                        cpuMemory.Switch8kPrgRom(((data & 0x3F) * 2 + 1) * 2, 2);
                        cpuMemory.Switch8kPrgRom(((data & 0x3F) * 2 + 0) * 2, 3);
                    }
                    else
                    {
                        cpuMemory.Switch8kPrgRom(((data & 0x3F) * 2 + 0) * 2, 2);
                        cpuMemory.Switch8kPrgRom(((data & 0x3F) * 2 + 1) * 2, 3);
                    }
                    break;
                case 0x8002:
                    if ((data & 0x80) == 0x80)
                    {
                        cpuMemory.Switch8kPrgRom(((data & 0x3F) * 2 + 1) * 2, 0);
                        cpuMemory.Switch8kPrgRom(((data & 0x3F) * 2 + 1) * 2, 1);
                        cpuMemory.Switch8kPrgRom(((data & 0x3F) * 2 + 1) * 2, 2);
                        cpuMemory.Switch8kPrgRom(((data & 0x3F) * 2 + 1) * 2, 3);
                    }
                    else
                    {
                        cpuMemory.Switch8kPrgRom(((data & 0x3F) * 2 + 0) * 2, 0);
                        cpuMemory.Switch8kPrgRom(((data & 0x3F) * 2 + 0) * 2, 1);
                        cpuMemory.Switch8kPrgRom(((data & 0x3F) * 2 + 0) * 2, 2);
                        cpuMemory.Switch8kPrgRom(((data & 0x3F) * 2 + 0) * 2, 3);
                    }
                    break;
                case 0x8003:
                    if ((data & 0x80) == 0x80)
                    {
                        cpuMemory.Switch8kPrgRom(((data & 0x3F) * 2 + 1) * 2, 2);
                        cpuMemory.Switch8kPrgRom(((data & 0x3F) * 2 + 0) * 2, 3);
                    }
                    else
                    {
                        cpuMemory.Switch8kPrgRom(((data & 0x3F) * 2 + 0) * 2, 2);
                        cpuMemory.Switch8kPrgRom(((data & 0x3F) * 2 + 1) * 2, 3);
                    }
                    if ((data & 0x40) == 0x40)
                        cartridge.Mirroring = Mirroring.ModeHorz;
                    else
                        cartridge.Mirroring = Mirroring.ModeVert;
                    break;
            }
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Switch32kPrgRom(0);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(8);
            cpuMemory.Switch8kChrRom(0);
        }
    }
}
