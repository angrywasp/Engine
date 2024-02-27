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
    class Mapper245 : Mapper
    {
        byte[] reg = new byte[8];
        byte prg0 = 0;
        byte prg1 = 1;
        byte we_sram = 0;
        byte irq_enable = 0;
        byte irq_counter = 0;
        byte irq_latch = 0;
        byte irq_request = 0;
        public Mapper245(NesSystem nes)
            : base(nes)
        { }
        public override void Poke(int addr, byte data)
        {
            switch (addr & 0xF7FF)
            {
                case 0x8000:
                    reg[0] = data;
                    break;
                case 0x8001:
                    reg[1] = data;
                    switch (reg[0])
                    {
                        case 0x00:
                            reg[3] = (byte)((data & 2) << 5);
                            cpuMemory.Switch8kPrgRom((0x3E | reg[3]) * 2, 2);
                            cpuMemory.Switch8kPrgRom((0x3F | reg[3]) * 2, 3);
                            break;
                        case 0x06:
                            prg0 = data;
                            break;
                        case 0x07:
                            prg1 = data;
                            break;
                    }

                    cpuMemory.Switch8kPrgRom((prg0 | reg[3]) * 2, 0);
                    cpuMemory.Switch8kPrgRom((prg1 | reg[3]) * 2, 1);
                    break;
                case 0xA000:
                    reg[2] = data;
                    //if (map.cartridge.Mirroring != Mirroring.Four_Screen)
                    {
                        if ((data & 0x01) == 0x01)
                            cartridge.Mirroring = Mirroring.ModeHorz;
                        else
                            cartridge.Mirroring = Mirroring.ModeVert;
                    }
                    break;
                case 0xA001:

                    break;
                case 0xC000:
                    reg[4] = data;
                    irq_counter = data;
                    irq_request = 0;
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
                case 0xC001:
                    reg[5] = data;
                    irq_latch = data;
                    irq_request = 0;
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
                case 0xE000:
                    reg[6] = data;
                    irq_enable = 0;
                    irq_request = 0;
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
                case 0xE001:
                    reg[7] = data;
                    irq_enable = 1;
                    irq_request = 0;
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
            }
            base.Poke(addr, data);
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Switch16kPrgRom(0, 0);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
            base.Initialize(initializing);
        }
        public override void TickCycleTimer(int cycles)
        {
            if (irq_enable != 0 && irq_request == 0)
            {
                if (ppu.scanline == 0)
                {
                    if (irq_counter > 0)
                    {
                        irq_counter--;
                    }
                }
                if ((irq_counter--) == 0)
                {
                    irq_request = 0xFF;
                    irq_counter = irq_latch;
                    cpu.IRQ(NesCpu.IsrType.External, true);
                }
            }
            base.TickCycleTimer(cycles);
        }
        public override bool ScanlineTimerAlwaysActive
        {
            get
            {
                return true;
            }
        }
        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(reg);
            stateStream.Write(prg0);
            stateStream.Write(prg1);
            stateStream.Write(we_sram);
            stateStream.Write(irq_enable);
            stateStream.Write(irq_counter);
            stateStream.Write(irq_latch);
            stateStream.Write(irq_request);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            stateStream.Read(reg);
            prg0 = stateStream.ReadByte();
            prg1 = stateStream.ReadByte();
            we_sram = stateStream.ReadByte();
            irq_enable = stateStream.ReadByte();
            irq_counter = stateStream.ReadByte();
            irq_latch = stateStream.ReadByte();
            irq_request = stateStream.ReadByte();
            base.LoadState(stateStream);
        }
    }
}
