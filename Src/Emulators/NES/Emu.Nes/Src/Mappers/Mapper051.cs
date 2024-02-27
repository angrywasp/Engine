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
    class Mapper51:Mapper
    {
        int bank = 0;
        int mode = 1;
        public Mapper51(NesSystem nes) : base(nes) { }

        public override void Poke(int addr, byte data)
        {
            if (addr < 08000)
            {
                mode = ((data & 0x10) >> 3) | ((data & 0x02) >> 1);
                SetBank_CPU();
            }
            else
            {
                bank = (data & 0x0f) << 2;
                if (0xC000 <= addr && addr <= 0xDFFF)
                {
                    mode = (mode & 0x01) | ((data & 0x10) >> 3);
                }
                SetBank_CPU();
            } 
            base.Poke(addr, data);
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x6000, 0x7FFF, Poke);
            SetBank_CPU();
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
            base.Initialize(initializing);
        }
        void SetBank_CPU()
        {
            switch (mode)
            {
                case 0:
                    cartridge.Mirroring = Mirroring.ModeVert;
                    cpuMemory.Switch8kPrgRomToSRAM((bank | 0x2c | 3) * 2);
                    cpuMemory.Switch8kPrgRom((bank | 0x00 | 0) * 2, 0);
                    cpuMemory.Switch8kPrgRom((bank | 0x00 | 1) * 2, 1);
                    cpuMemory.Switch8kPrgRom((bank | 0x0c | 2) * 2, 2);
                    cpuMemory.Switch8kPrgRom((bank | 0x0c | 3) * 2, 3);
                    break;
                case 1:
                    cartridge.Mirroring = Mirroring.ModeVert;
                    cpuMemory.Switch8kPrgRomToSRAM((bank | 0x20 | 3) * 2);
                    cpuMemory.Switch8kPrgRom((bank | 0x00 | 0) * 2, 0);
                    cpuMemory.Switch8kPrgRom((bank | 0x00 | 1) * 2, 1);
                    cpuMemory.Switch8kPrgRom((bank | 0x00 | 2) * 2, 2);
                    cpuMemory.Switch8kPrgRom((bank | 0x00 | 3) * 2, 3);
                    break;
                case 2:
                    cartridge.Mirroring = Mirroring.ModeVert;
                    cpuMemory.Switch8kPrgRomToSRAM((bank | 0x2e | 3) * 2);
                    cpuMemory.Switch8kPrgRom((bank | 0x02 | 0) * 2, 0);
                    cpuMemory.Switch8kPrgRom((bank | 0x02 | 1) * 2, 1);
                    cpuMemory.Switch8kPrgRom((bank | 0x0e | 2) * 2, 2);
                    cpuMemory.Switch8kPrgRom((bank | 0x0e | 3) * 2, 3);
                    break;
                case 3:
                    cartridge.Mirroring = Mirroring.ModeHorz;
                    cpuMemory.Switch8kPrgRomToSRAM((bank | 0x20 | 3) * 2);
                    cpuMemory.Switch8kPrgRom((bank | 0x00 | 0) * 2, 0);
                    cpuMemory.Switch8kPrgRom((bank | 0x00 | 1) * 2, 1);
                    cpuMemory.Switch8kPrgRom((bank | 0x00 | 2) * 2, 2);
                    cpuMemory.Switch8kPrgRom((bank | 0x00 | 3) * 2, 3);
                    break;
            }
        }

        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(bank);
            stateStream.Write(mode);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            bank = stateStream.ReadInt32();
            mode = stateStream.ReadInt32();
            base.LoadState(stateStream);
        }
    }
}
