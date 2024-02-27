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
    class Mapper50:Mapper
    {
        byte irq_enable = 0;
        public Mapper50(NesSystem nes) : base(nes) { }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x4018, 0x7FFF, Poke);
            Switch8kPrgRomToSRAM(15 * 2);
            cpuMemory.Switch8kPrgRom(8 * 2, 0);
            cpuMemory.Switch8kPrgRom(9 * 2, 1);
            // map.Switch8kPrgRom(0, 2);
            cpuMemory.Switch8kPrgRom(11 * 2, 3);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
            base.Initialize(initializing);
        }
        public override void Poke(int addr, byte data)
        {
            if ((addr & 0xE060) == 0x4020)
            {
                if ((addr & 0x0100) == 0x0100)
                {
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    irq_enable = (byte)(data & 0x01);
                }
                else
                {
                    cpuMemory.Switch8kPrgRom(((data & 0x08) | ((data & 0x01) << 2) | ((data & 0x06) >> 1)) * 2, 2);
                }
            }
            base.Poke(addr, data);
        }
        public override void TickCycleTimer(int cycles)
        {
            if (irq_enable != 0)
            {
                if (ppu.scanline ==  21)
                {
                    cpu.IRQ(NesCpu.IsrType.External, true);
                }
            }
            base.TickCycleTimer(cycles);
        }
        void Switch8kPrgRomToSRAM(int start)
        {
            switch (cartridge.PrgPages)
            {
                case (2): start = (start & 0x7); break;
                case (4): start = (start & 0xf); break;
                case (8): start = (start & 0x1f); break;
                case (16): start = (start & 0x3f); break;
                case (32): start = (start & 0x7f); break;
                case (64): start = (start & 0xff); break;
                case (128): start = (start & 0x1ff); break;
            }
            cartridge.Prg[start].CopyTo(cpuMemory.srm, 0);
            for (int i = 0x1000; i < 0x2000; i++)
            {
                cpuMemory.srm[i] = cartridge.Prg[start + 1][i - 0x1000];
            }
        }

        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(irq_enable);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            irq_enable = stateStream.ReadByte();
            base.LoadState(stateStream);
        }
    }
}
