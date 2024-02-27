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
    class Mapper75 : Mapper
    {
        public int[] reg = new int[2];
        public Mapper75(NesSystem nesSystem)
            : base(nesSystem) { }

        public override void Poke(int address, byte data)
        {
            switch (address & 0xF000)
            {
                case 0x8000:
                    cpuMemory.Switch8kPrgRom((data & 0x0F) * 2, 0);
                    break;

                case 0x9000:
                    if ((data & 0x01) != 0)
                        cartridge.Mirroring = Mirroring.ModeHorz;
                    else
                        cartridge.Mirroring = Mirroring.ModeVert;

                    reg[0] = (reg[0] & 0x0F) | ((data & 0x02) << 3);
                    reg[1] = (reg[1] & 0x0F) | ((data & 0x04) << 2);
                    cpuMemory.Switch4kChrRom(reg[0] * 4, 0);
                    cpuMemory.Switch4kChrRom(reg[1] * 4, 1);
                    break;

                case 0xA000:
                    cpuMemory.Switch8kPrgRom((data & 0x0F) * 2, 1);
                    break;
                case 0xC000:
                    cpuMemory.Switch8kPrgRom((data & 0x0F) * 2, 2);
                    break;

                case 0xE000:
                    reg[0] = (reg[0] & 0x10) | (data & 0x0F);
                    cpuMemory.Switch4kChrRom(reg[0] * 4, 0);
                    break;

                case 0xF000:
                    reg[1] = (reg[1] & 0x10) | (data & 0x0F);
                    cpuMemory.Switch4kChrRom(reg[1] * 4, 1);
                    break;
            }
        }

        protected override void Initialize(bool initializing)
        {
            cpuMemory.Switch16kPrgRom(0, 0);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(8);
            cpuMemory.Switch8kChrRom(0);
        }
    }
}
