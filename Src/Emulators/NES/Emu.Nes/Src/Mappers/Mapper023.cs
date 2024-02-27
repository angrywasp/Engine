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
    class Mapper23 : Mapper
    {
        public Mapper23(NesSystem nes)
            : base(nes)
        { }
        byte[] REG = new byte[8];
        int irq_latch = 0;
        int irq_enable = 0;
        int irq_counter = 0;
        int irq_clock = 0;
        int mask = 0xFFFF;

        public override void Poke(int addr, byte data)
        {
            switch (addr & mask)
            {
                case 0x8000:
                case 0x8004:
                case 0x8008:
                case 0x800C: cpuMemory.Switch8kPrgRom(data * 2, 0); break;
                case 0x9000:
                    if (data != 0xFF)
                    {
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
                    }
                    break;
                case 0xA000:
                case 0xA004:
                case 0xA008:
                case 0xA00C: cpuMemory.Switch8kPrgRom(data * 2, 1);
                    break;

                case 0xB000:
                    REG[0] = (byte)((REG[0] & 0xF0) | (data & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[0], 0);
                    break;
                case 0xB001:
                case 0xB004:
                    REG[0] = (byte)(((data & 0x0F) << 4) | (REG[0] & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[0], 0);
                    break;

                case 0xB002:
                case 0xB008:
                    REG[1] = (byte)((REG[1] & 0xF0) | (data & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[1], 1);
                    break;
                case 0xB003:
                case 0xB00C:
                    REG[1] = (byte)(((data & 0x0F) << 4) | (REG[1] & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[1], 1);
                    break;

                case 0xC000:
                    REG[2] = (byte)((REG[2] & 0xF0) | (data & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[2], 2);
                    break;
                case 0xC001:
                case 0xC004:
                    REG[2] = (byte)(((data & 0x0F) << 4) | (REG[2] & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[2], 2);
                    break;

                case 0xC002:
                case 0xC008:
                    REG[3] = (byte)((REG[3] & 0xF0) | (data & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[3], 3);
                    break;
                case 0xC003:
                case 0xC00C:
                    REG[3] = (byte)(((data & 0x0F) << 4) | (REG[3] & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[3], 3);
                    break;

                case 0xD000:
                    REG[4] = (byte)((REG[4] & 0xF0) | (data & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[4], 4);
                    break;
                case 0xD001:
                case 0xD004:
                    REG[4] = (byte)(((data & 0x0F) << 4) | (REG[4] & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[4], 4);
                    break;

                case 0xD002:
                case 0xD008:
                    REG[5] = (byte)((REG[5] & 0xF0) | (data & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[5], 5);
                    break;
                case 0xD003:
                case 0xD00C:
                    REG[5] = (byte)(((data & 0x0F) << 4) | (REG[5] & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[5], 5);
                    break;

                case 0xE000:
                    REG[6] = (byte)((REG[6] & 0xF0) | (data & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[6], 6);
                    break;
                case 0xE001:
                case 0xE004:
                    REG[6] = (byte)(((data & 0x0F) << 4) | (REG[6] & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[6], 6);
                    break;

                case 0xE002:
                case 0xE008:
                    REG[7] = (byte)((REG[7] & 0xF0) | (data & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[7], 7);
                    break;
                case 0xE003:
                case 0xE00C:
                    REG[7] = (byte)(((data & 0x0F) << 4) | (REG[7] & 0x0F));
                    cpuMemory.Switch1kChrRom(REG[7], 7);
                    break;

                case 0xF000:
                    irq_latch = (irq_latch & 0xF0) | (data & 0x0F);
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
                case 0xF004:
                    irq_latch = (irq_latch & 0x0F) | ((data & 0x0F) << 4);
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
                case 0xF008:
                    irq_enable = data & 0x03;
                    irq_counter = irq_latch;
                    irq_clock = 0;
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
                case 0xF00C:
                    irq_enable = (irq_enable & 0x01) * 3;
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
            }
            base.Poke(addr, data);
        }
        protected override void Initialize(bool initializing)
        {
            for (int i = 0; i < 7; i++)
            {
                REG[i] = (byte)i;
            }
            cpuMemory.Switch16kPrgRom(0, 0);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);

            cpuMemory.Switch8kChrRom(0);
            //This is not a fix, this game is special case for this mapper.
            if (cartridge.Sha1.ToLower() == "8c8faf50f14f944ed296bf2ef473a58d421f7707")//Akumajou Special - Boku Dracula Kun
            {
                mask = 0xF00C;
            }
            base.Initialize(initializing);
        }
        public override void TickCycleTimer(int cycles)
        {
            if ((irq_enable & 0x02) != 0)
            {
                irq_clock += cycles * 3;
                while (irq_clock >= 341)
                {
                    irq_clock -= 341;
                    irq_counter++;
                    if (irq_counter == 0xFF)
                    {
                        irq_counter = irq_latch;
                        cpu.IRQ(NesCpu.IsrType.External, true);
                    }
                }
            }
            base.TickCycleTimer(cycles);
        }
        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(REG);
            stateStream.Write(irq_latch);
            stateStream.Write(irq_enable);
            stateStream.Write(irq_counter);
            stateStream.Write(irq_clock);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            stateStream.Read(REG);
            irq_latch = stateStream.ReadInt32();
            irq_enable = stateStream.ReadInt32();
            irq_counter = stateStream.ReadInt32();
            irq_clock = stateStream.ReadInt32();
            base.LoadState(stateStream);
        }
    }
}
