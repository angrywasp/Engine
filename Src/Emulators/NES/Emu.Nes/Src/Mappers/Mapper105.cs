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

namespace MyNes.Nes
{
    class Mapper105 : Mapper
    {
        byte[] reg = new byte[4];
        byte bits = 0;
        byte write_count = 0;
        int irq_counter = 0;
        byte irq_enable = 0;
        byte init_state = 0;

        public Mapper105(NesSystem nes)
            : base(nes)
        { }

        public override void Poke(int addr, byte data)
        {
            short reg_num = (short)((addr & 0x7FFF) >> 13);

            if ((data & 0x80) == 0x80)
            {
                bits = write_count = 0;
                if (reg_num == 0)
                {
                    reg[reg_num] |= 0x0C;
                }
            }
            else
            {
                bits |= (byte)((data & 1) << write_count++);
                if (write_count == 5)
                {
                    reg[reg_num] = (byte)(bits & 0x1F);
                    bits = write_count = 0;
                }
            }

            if ((reg[0] & 0x02) == 0x02)
            {
                if ((reg[0] & 0x01) == 0x01)
                {
                    cartridge.Mirroring = Mirroring.ModeHorz;
                }
                else
                {
                    cartridge.Mirroring = Mirroring.ModeVert;
                }
            }
            else
            {
                if ((reg[0] & 0x01) == 0x01)
                {
                    cartridge.Mirroring = Mirroring.Mode1ScB;
                }
                else
                {
                    cartridge.Mirroring = Mirroring.Mode1ScA;
                }
            }

            switch (init_state)
            {
                case 0:
                case 1:
                    init_state++;
                    break;
                case 2:
                    if ((reg[1] & 0x08) == 0x08)
                    {
                        if ((reg[0] & 0x08) == 0x08)
                        {
                            if ((reg[0] & 0x04) == 0x04)
                            {
                                cpuMemory.Switch8kPrgRom(((reg[3] & 0x07) * 2 + 16) * 2, 0);
                                cpuMemory.Switch8kPrgRom(((reg[3] & 0x07) * 2 + 17) * 2, 1);
                                cpuMemory.Switch8kPrgRom(30 * 2, 2);
                                cpuMemory.Switch8kPrgRom(31 * 2, 3);
                            }
                            else
                            {
                                cpuMemory.Switch8kPrgRom(16 * 2, 0);
                                cpuMemory.Switch8kPrgRom(17 * 2, 1);
                                cpuMemory.Switch8kPrgRom(((reg[3] & 0x07) * 2 + 16) * 2, 2);
                                cpuMemory.Switch8kPrgRom(((reg[3] & 0x07) * 2 + 17) * 2, 3);
                            }
                        }
                        else
                        {
                            cpuMemory.Switch8kPrgRom(((reg[3] & 0x06) * 2 + 16) * 2, 0);
                            cpuMemory.Switch8kPrgRom(((reg[3] & 0x06) * 2 + 17) * 2, 1);
                            cpuMemory.Switch8kPrgRom(((reg[3] & 0x06) * 2 + 18) * 2, 2);
                            cpuMemory.Switch8kPrgRom(((reg[3] & 0x06) * 2 + 19) * 2, 3);
                        }
                    }
                    else
                    {
                        cpuMemory.Switch8kPrgRom(((reg[1] & 0x06) * 2 + 0) * 2, 0);
                        cpuMemory.Switch8kPrgRom(((reg[1] & 0x06) * 2 + 1) * 2, 1);
                        cpuMemory.Switch8kPrgRom(((reg[1] & 0x06) * 2 + 2) * 2, 2);
                        cpuMemory.Switch8kPrgRom(((reg[1] & 0x06) * 2 + 3) * 2, 3);
                    }

                    if ((reg[1] & 0x10) == 0x10)
                    {
                        irq_counter = 0;
                        irq_enable = 0;
                    }
                    else
                    {
                        irq_enable = 1;
                    }
                    // cpu.Interrupt(NesCpu.IsrType.External, false);
                    break;
                default:
                    break;
            }
            base.Poke(addr, data);
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Switch32kPrgRom(0);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
            base.Initialize(initializing);
        }
        public override void TickScanlineTimer()
        {
            if (ppu.scanline == 0)
            {
                if (irq_enable > 0)
                {
                    irq_counter += 29781;
                }
                if (((irq_counter | 0x21FFFFFF) & 0x3E000000) == 0x3E000000)
                {
                    cpu.IRQ(NesCpu.IsrType.External, true);
                }
            }
            base.TickScanlineTimer();
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
            stateStream.Write(bits);
            stateStream.Write(write_count);
            stateStream.Write(irq_counter);
            stateStream.Write(irq_enable);
            stateStream.Write(init_state);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            stateStream.Read(reg);
            bits = stateStream.ReadByte();
            write_count = stateStream.ReadByte();
            irq_counter = stateStream.ReadInt32();
            irq_enable = stateStream.ReadByte();
            init_state = stateStream.ReadByte();
            base.LoadState(stateStream);
        }
    }
}
