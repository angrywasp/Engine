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
    class Mapper21 : Mapper
    {
        public Mapper21(NesSystem nes)
            : base(nes)
        { }
        bool PRGMode = true;
        byte[] REG = new byte[8];
        int irq_latch = 0;
        int irq_enable = 0;
        int irq_counter = 0;
        int irq_clock = 0;

        public override void Poke(int addr, byte data)
        {
            switch (addr)
            {
                case 0x8000:
                    cpuMemory.Switch8kPrgRom(data * 2, 0);
                    break;
                case 0xA000:
                    cpuMemory.Switch8kPrgRom(data * 2, 1);
                    break;
                case 0x9000:
                    switch (data & 0x3)
                    {
                        case 0: cartridge.Mirroring = Mirroring.ModeVert; break;
                        case 1: cartridge.Mirroring = Mirroring.ModeHorz; break;
                        case 2:
                            cartridge.Mirroring = Mirroring.Mode1ScA;
                            break;
                        case 3:
                            cartridge.Mirroring = Mirroring.Mode1ScB;
                            break;
                    }
                    break;

                case 0x9080:
                case 0x9002:
                    PRGMode = (data & 0x2) == 0x2;
                    break;

                case 0xB000:
                    REG[0] = (byte)((REG[0] & 0xF0) | (data & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[0], 0);
                    break;
                case 0xB002:
                case 0xB040:
                    REG[0] = (byte)(((data & 0x0F) << 4) | (REG[0] & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[0], 0);
                    break;

                case 0xB001:
                case 0xB004:
                case 0xB080:
                    REG[1] = (byte)((REG[1] & 0xF0) | (data & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[1], 1);
                    break;
                case 0xB003:
                case 0xB006:
                case 0xB0C0:
                    REG[1] = (byte)(((data & 0x0F) << 4) | (REG[1] & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[1], 1);
                    break;

                case 0xC000:
                    REG[2] = (byte)((REG[2] & 0xF0) | (data & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[2], 2);
                    break;
                case 0xC002:
                case 0xC040:
                    REG[2] = (byte)(((data & 0x0F) << 4) | (REG[2] & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[2], 2);
                    break;

                case 0xC001:
                case 0xC004:
                case 0xC080:
                    REG[3] = (byte)((REG[3] & 0xF0) | (data & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[3], 3);
                    break;
                case 0xC003:
                case 0xC006:
                case 0xC0C0:
                    REG[3] = (byte)(((data & 0x0F) << 4) | (REG[3] & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[3], 3);
                    break;

                case 0xD000:
                    REG[4] = (byte)((REG[4] & 0xF0) | (data & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[4], 4);
                    break;
                case 0xD002:
                case 0xD040:
                    REG[4] = (byte)(((data & 0x0F) << 4) | (REG[4] & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[4], 4);
                    break;

                case 0xD001:
                case 0xD004:
                case 0xD080:
                    REG[5] = (byte)((REG[5] & 0xF0) | (data & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[5], 5);
                    break;
                case 0xD003:
                case 0xD006:
                case 0xD0C0:
                    REG[5] = (byte)(((data & 0x0F) << 4) | (REG[5] & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[5], 5);
                    break;

                case 0xE000:
                    REG[6] = (byte)((REG[6] & 0xF0) | (data & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[6], 6);
                    break;
                case 0xE002:
                case 0xE040:
                    REG[6] = (byte)(((data & 0x0F) << 4) | (REG[6] & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[6], 6);
                    break;

                case 0xE001:
                case 0xE004:
                case 0xE080:
                    REG[7] = (byte)((REG[7] & 0xF0) | (data & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[7], 7);
                    break;
                case 0xE003:
                case 0xE006:
                case 0xE0C0:
                    REG[7] = (byte)(((data & 0x0F) << 4) | (REG[7] & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[7], 7);
                    break;
                case 0xF000:
                    irq_latch = (irq_latch & 0xF0) | (data & 0x0F);
                    break;
                case 0xF002:
                case 0xF040:
                    irq_latch = (irq_latch & 0x0F) | ((data & 0x0F) << 4);
                    break;
                case 0xF003:
                case 0xF0C0:
                case 0xF006:
                    irq_enable = (irq_enable & 0x01) * 3;
                    irq_clock = 0;
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
                case 0xF004:
                case 0xF080:
                    irq_enable = data & 0x03;
                    if ((irq_enable & 0x02) != 0)
                    {
                        irq_counter = irq_latch;
                        irq_clock = 0;
                    }
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

            base.Initialize(initializing);
        }
        public override void TickCycleTimer(int cycles)
        {
            if ((irq_enable & 0x02) != 0)
            {
                irq_clock -= cycles;
                if (irq_clock <= 0)
                {
                    irq_clock += ppu.Region == RegionFormat.NTSC ? 113 : 106;
                    if (irq_counter == 0xFF)
                    {
                        irq_counter = irq_latch;
                        cpu.IRQ(NesCpu.IsrType.External, true);
                        irq_enable = 0;
                    }
                    else
                    {
                        irq_counter++;
                    }
                }
            }
            base.TickCycleTimer(cycles);
        }
        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(PRGMode);
            stateStream.Write(REG);
            stateStream.Write(irq_latch);
            stateStream.Write(irq_enable);
            stateStream.Write(irq_counter);
            stateStream.Write(irq_clock);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            PRGMode = stateStream.ReadBooleans()[0];
            stateStream.Read(REG);
            irq_latch = stateStream.ReadInt32();
            irq_enable = stateStream.ReadInt32();
            irq_counter = stateStream.ReadInt32();
            irq_clock = stateStream.ReadInt32();
            base.LoadState(stateStream);
        }
    }
}
