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
    class Mapper202 : Mapper
    {
        public Mapper202(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int Address, byte data)
        {
            if (Address >= 0x4020)
            {
                int bank = (Address >> 1) & 0x07;

                cpuMemory.Switch16kPrgRom( bank*4,0);
                if ((Address & 0x0C) == 0x0C)
                {
                    cpuMemory.Switch16kPrgRom((bank + 1) * 4, 1);
                }
                else
                {
                    cpuMemory.Switch16kPrgRom(bank * 4, 1);
                }

                cpuMemory.Switch8kChrRom(bank * 8);

                if ((Address & 0x01)==0x1)
                {
                    cartridge.Mirroring = Mirroring.ModeHorz;
                }
                else
                {
                    cartridge.Mirroring = Mirroring.ModeVert;
                }
            }
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x4018, 0x7FFF, Poke);
            cpuMemory.Switch16kPrgRom(6*4, 0);
            cpuMemory.Switch16kPrgRom(7*4, 1);

            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
        }
    }
}
