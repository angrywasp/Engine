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
    class Mapper64 : Mapper
    {
        int[] reg = new int[3];
        int irq_enable = 0;
        int irq_mode = 0;
        int irq_counter = 0;
        int irq_counter2 = 0;
        int irq_latch = 0;
        int irq_reset = 0;
        public Mapper64(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int address, byte data)
        {
            switch (address & 0xF003)
            {
                case 0x8000:
                    reg[0] = data & 0x0F;
                    reg[1] = data & 0x40;
                    reg[2] = data & 0x80;
                    break;

                case 0x8001:
                    switch (reg[0])
                    {
                        case 0x00:
                            if (reg[2] != 0)
                            {
                                cpuMemory.Switch1kChrRom(data + 0, 4);
                                cpuMemory.Switch1kChrRom(data + 1, 5);
                            }
                            else
                            {
                                cpuMemory.Switch1kChrRom(data + 0, 0);
                                cpuMemory.Switch1kChrRom(data + 1, 1);
                            }
                            break;
                        case 0x01:
                            if (reg[2] != 0)
                            {
                                cpuMemory.Switch1kChrRom(data + 0, 6);
                                cpuMemory.Switch1kChrRom(data + 1, 7);
                            }
                            else
                            {
                                cpuMemory.Switch1kChrRom(data + 0, 2);
                                cpuMemory.Switch1kChrRom(data + 1, 3);
                            }
                            break;
                        case 0x02:
                            if (reg[2] != 0)
                            {
                                cpuMemory.Switch1kChrRom(data, 0);
                            }
                            else
                            {
                                cpuMemory.Switch1kChrRom(data, 4);
                            }
                            break;
                        case 0x03:
                            if (reg[2] != 0)
                            {
                                cpuMemory.Switch1kChrRom(data, 1);
                            }
                            else
                            {
                                cpuMemory.Switch1kChrRom(data, 5);
                            }
                            break;
                        case 0x04:
                            if (reg[2] != 0)
                            {
                                cpuMemory.Switch1kChrRom(data, 2);
                            }
                            else
                            {
                                cpuMemory.Switch1kChrRom(data, 6);
                            }
                            break;
                        case 0x05:
                            if (reg[2] != 0)
                            {
                                cpuMemory.Switch1kChrRom(data, 3);
                            }
                            else
                            {
                                cpuMemory.Switch1kChrRom(data, 7);
                            }
                            break;
                        case 0x06:
                            if (reg[1] != 0)
                            {
                                cpuMemory.Switch8kPrgRom(data * 2, 1);
                            }
                            else
                            {
                                cpuMemory.Switch8kPrgRom(data * 2, 0);
                            }
                            break;
                        case 0x07:
                            if (reg[1] != 0)
                            {
                                cpuMemory.Switch8kPrgRom(data * 2, 2);
                            }
                            else
                            {
                                cpuMemory.Switch8kPrgRom(data * 2, 1);
                            }
                            break;
                        case 0x08:
                            cpuMemory.Switch1kChrRom(data, 1);
                            break;
                        case 0x09:
                            cpuMemory.Switch1kChrRom(data, 3);
                            break;
                        case 0x0F:
                            if (reg[1] != 0)
                            {
                                cpuMemory.Switch8kPrgRom(data * 2, 0);
                            }
                            else
                            {
                                cpuMemory.Switch8kPrgRom(data * 2, 2);
                            }
                            break;
                    }
                    break;

                case 0xA000:
                    if ((data & 0x01) == 0x01)
                        cartridge.Mirroring = Mirroring.ModeHorz;
                    else
                        cartridge.Mirroring = Mirroring.ModeVert;
                    break;

                case 0xC000:
                    irq_latch = data;
                    if (irq_reset != 0)
                    {
                        irq_counter = irq_latch;
                    }
                    break;
                case 0xC001:
                    irq_reset = 0xFF;
                    irq_counter = irq_latch;
                    irq_mode = data & 0x01;
                    break;
                case 0xE000:
                    irq_enable = 0;
                    if (irq_reset != 0)
                    {
                        irq_counter = irq_latch;
                    }
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
                case 0xE001:
                    irq_enable = 0xFF;
                    if (irq_reset != 0)
                    {
                        irq_counter = irq_latch;
                    }
                    break;
            }
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Switch8kPrgRom(((cartridge.PrgPages * 2) - 1) * 2, 0);
            cpuMemory.Switch8kPrgRom(((cartridge.PrgPages * 2) - 1) * 2, 1);
            cpuMemory.Switch8kPrgRom(((cartridge.PrgPages * 2) - 1) * 2, 2);
            cpuMemory.Switch8kPrgRom(((cartridge.PrgPages * 2) - 1) * 2, 3);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
        }
        public override void TickScanlineTimer()
        {
            if (irq_mode == 1)
                return;
            irq_reset = 0;
            if (irq_counter >= 0)
            {
                irq_counter--;
                if (irq_counter < 0)
                {
                    if (irq_enable != 0)
                    {
                        irq_reset = 1;
                        cpu.IRQ(NesCpu.IsrType.External, true);
                    }
                }
            }
        }
        public override void TickCycleTimer(int cycles)
        {
            if (irq_mode == 0)
                return;

            irq_counter2 += cycles;
            while (irq_counter2 >= 4)
            {
                irq_counter2 -= 4;
                if (irq_counter >= 0)
                {
                    irq_counter--;
                    if (irq_counter < 0)
                    {
                        if (irq_enable != 0)
                        {
                            cpu.IRQ(NesCpu.IsrType.External, true);
                        }
                    }
                }
            }
        }

        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(reg);
            stateStream.Write(irq_enable);
            stateStream.Write(irq_mode);
            stateStream.Write(irq_counter);
            stateStream.Write(irq_counter2);
            stateStream.Write(irq_latch);
            stateStream.Write(irq_reset);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            stateStream.Read(reg);
            irq_enable = stateStream.ReadInt32();
            irq_mode = stateStream.ReadInt32();
            irq_counter = stateStream.ReadInt32();
            irq_counter2 = stateStream.ReadInt32();
            irq_latch = stateStream.ReadInt32();
            irq_reset = stateStream.ReadInt32();
            base.LoadState(stateStream);
        }
    }
}
