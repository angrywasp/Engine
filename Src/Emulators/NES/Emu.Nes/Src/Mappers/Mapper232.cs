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
    class Mapper232 : Mapper
    {
        byte[] reg = new byte[2];
        public Mapper232(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int Address, byte data)
        {
            if (Address <= 0x9FFF)
            {
                reg[0] = (byte)((data & 0x18) >> 1);
            }
            else
            {
                reg[1] = (byte)(data & 0x03);
            }

            cpuMemory.Switch8kPrgRom(((reg[0] | reg[1]) * 2 + 0) * 2, 0);
            cpuMemory.Switch8kPrgRom(((reg[0] | reg[1]) * 2 + 1) * 2, 1);
            cpuMemory.Switch8kPrgRom(((reg[0] | 0x03) * 2 + 0) * 2, 2);
            cpuMemory.Switch8kPrgRom(((reg[0] | 0x03) * 2 + 1) * 2, 3);
        }
        protected override void Initialize(bool initializing)
        {
            reg[0] = 0x0C;
            reg[1] = 0x00;
            cpuMemory.Map(0x4018, 0x7FFF, Poke);
            cpuMemory.Switch16kPrgRom(0, 0);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
        }
    }
}
