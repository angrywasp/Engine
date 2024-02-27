/*********************************************************************\
*This file is part of My Nes                                          *
*A Nintendo Entertainment System Emulator.                            *
*                                                                     *
*Copyright © Ala I.Hadid 2009 - 2012                                  *
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNes.Nes
{
    class Mapper233 : Mapper
    {
        public Mapper233(NesSystem nes)
            : base(nes)
        { }
        public override void Poke(int addr, byte data)
        {
            if ((data & 0x20) == 0x20)
            {
                cpuMemory.Switch8kPrgRom(((data & 0x1F) * 2 + 0) * 2, 0);
                cpuMemory.Switch8kPrgRom(((data & 0x1F) * 2 + 1) * 2, 1);
                cpuMemory.Switch8kPrgRom(((data & 0x1F) * 2 + 0) * 2, 2);
                cpuMemory.Switch8kPrgRom(((data & 0x1F) * 2 + 1) * 2, 3);
            }
            else
            {
                byte bank = (byte)((data & 0x1E) >> 1);

                cpuMemory.Switch8kPrgRom((bank * 4 + 0) * 2, 0);
                cpuMemory.Switch8kPrgRom((bank * 4 + 1) * 2, 1);
                cpuMemory.Switch8kPrgRom((bank * 4 + 2) * 2, 2);
                cpuMemory.Switch8kPrgRom((bank * 4 + 3) * 2, 3);
            }

            if ((data & 0xC0) == 0x00)
            {
                //cartridge.Mirroring = Mirroring.Mode1ScA;
                ppuMemory.SwitchNmt(0x40);// 0 0 0 1
            }
            else if ((data & 0xC0) == 0x40)
            {
                cartridge.Mirroring = Mirroring.ModeVert;
            }
            else if ((data & 0xC0) == 0x80)
            {
                cartridge.Mirroring = Mirroring.ModeHorz;
            }
            else
            {
                cartridge.Mirroring = Mirroring.Mode1ScB;
            }
            base.Poke(addr, data);
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Switch32kPrgRom(0);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(32);
            cpuMemory.Switch8kChrRom(0);
            base.Initialize(initializing);
        }
    }
}
