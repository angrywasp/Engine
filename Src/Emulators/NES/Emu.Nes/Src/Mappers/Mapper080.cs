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
    class Mapper80 : Mapper
    {
        public Mapper80(NesSystem nesSystem)
            : base(nesSystem) { }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x6000, 0x7FFF, Poke);
            ppuMemory.Map(0x2000, 0x3EFF, NmtPeek, NmtPoke);
            cpuMemory.Switch16kPrgRom(0, 0);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(8);
            cpuMemory.Switch8kChrRom(0);
            base.Initialize(initializing);
        }
        public override void Poke(int addr, byte data)
        {
            switch (addr)
            {
                case 0x7EF0:
                    cpuMemory.Switch2kChrRom(((data >> 1) & 0x3F) * 2, 0);
                    if (cartridge.PRGSizeInKB == 256)
                    {
                        if ((data & 0x80) == 0x80)
                        {
                            Switch1kCHRToVRAM(1, 0);
                            Switch1kCHRToVRAM(1, 1);
                        }
                        else
                        {
                            Switch1kCHRToVRAM(0, 0);
                            Switch1kCHRToVRAM(0, 1);
                        }
                    }
                    break;

                case 0x7EF1:
                    cpuMemory.Switch2kChrRom(((data >> 1) & 0x3F) * 2, 1);
                    if (cartridge.PRGSizeInKB == 256)
                    {
                        if ((data & 0x80) == 0x80)
                        {
                            Switch1kCHRToVRAM(1, 2);
                            Switch1kCHRToVRAM(1, 3);
                        }
                        else
                        {
                            Switch1kCHRToVRAM(0, 2);
                            Switch1kCHRToVRAM(0, 3);
                        }
                    }
                    break;

                case 0x7EF2:
                    cpuMemory.Switch1kChrRom(data, 4);
                    break;
                case 0x7EF3:
                    cpuMemory.Switch1kChrRom(data, 5);
                    break;
                case 0x7EF4:
                    cpuMemory.Switch1kChrRom(data, 6);
                    break;
                case 0x7EF5:
                    cpuMemory.Switch1kChrRom(data, 7);
                    break;

                case 0x7EF6:
                    if ((data & 0x01) == 0x01)
                        cartridge.Mirroring = Mirroring.ModeVert;
                    else
                        cartridge.Mirroring = Mirroring.ModeHorz;
                    break;

                case 0x7EFA:
                case 0x7EFB:
                    cpuMemory.Switch8kPrgRom(data * 2, 0);
                    break;
                case 0x7EFC:
                case 0x7EFD:
                    cpuMemory.Switch8kPrgRom(data * 2, 1);
                    break;
                case 0x7EFE:
                case 0x7EFF:
                    cpuMemory.Switch8kPrgRom(data * 2, 2);
                    break;
            }
            base.Poke(addr, data);
        }
        void Switch1kCHRToVRAM(int start, int area)
        {
            switch (cartridge.ChrPages)
            {
                case (2): start = (start & 0xf); break;
                case (4): start = (start & 0x1f); break;
                case (8): start = (start & 0x3f); break;
                case (16): start = (start & 0x7f); break;
                case (32): start = (start & 0xff); break;
                case (64): start = (start & 0x1ff); break;
                case (128): start = (start & 0x1ff); break;
            }
            ppuMemory.nmtBank[area] = (byte)start;
        }
        private byte NmtPeek(int addr)
        {
            if (ppuMemory.nmtBank[(addr & 0x0C00) >> 10] < 4)
                return ppuMemory.nmt[ppuMemory.nmtBank[addr >> 10 & 0x03]][addr & 0x03FF];
            else
                return cartridge.Chr[ppuMemory.nmtBank[(addr & 0x0C00) >> 10]][addr & 0x3FF];
        }
        private void NmtPoke(int addr, byte data)
        {
            if (ppuMemory.nmtBank[(addr & 0x0C00) >> 10] < 4)
                ppuMemory.nmt[ppuMemory.nmtBank[addr >> 10 & 0x03]][addr & 0x03FF] = data;
        }
    }
}
