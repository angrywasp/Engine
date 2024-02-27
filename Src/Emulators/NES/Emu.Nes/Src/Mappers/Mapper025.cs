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
    class Mapper25 : Mapper
    {
        byte[] reg = new byte[11];
        bool SwapMode = false;
        int irq_latch = 0;
        byte irq_enable = 0;
        int irq_counter = 0;
        int irq_clock = 0;

        public Mapper25(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int address, byte data)
        {
            switch (address & 0xF000)
            {
                case 0x8000:
                    if ((reg[10] & 0x02) == 0x02)
                    {
                        reg[9] = data;
                        cpuMemory.Switch8kPrgRom(data * 2, 2);
                    }
                    else
                    {
                        reg[8] = data;
                        cpuMemory.Switch8kPrgRom(data * 2, 0);
                    }
                    break;
                case 0xA000:
                    cpuMemory.Switch8kPrgRom(data * 2, 1);
                    break;
            }
            switch (address & 0xF00F)
            {
                
                case 0x9001:
                case 0x9004:
                    if ((reg[10] & 0x02) != (data & 0x02))
                    {
                        byte swap = reg[8];
                        reg[8] = reg[9];
                        reg[9] = swap;
                        cpuMemory.Switch8kPrgRom(reg[8] * 2, 0);
                        cpuMemory.Switch8kPrgRom(reg[9] * 2, 2);
                    }
                    reg[10] = data;
                    break;
                
                case 0x9000:
                    data &= 0x03;
                    if (data == 0)
                        cartridge.Mirroring = Mirroring.ModeVert;
                    else if (data == 1)
                        cartridge.Mirroring = Mirroring.ModeHorz;
                    else if (data == 2)
                        cartridge.Mirroring = Mirroring.Mode1ScA;
                    else
                        cartridge.Mirroring = Mirroring.Mode1ScB;
                    break;
                
                case 0xB000:
                    reg[0] = (byte)((reg[0] & 0xF0) | (data & 0x0F));
                    cpuMemory.Switch1kChrRom(reg[0], 0);
                    break;
                case 0xB002:
                case 0xB008:
                    reg[0] = (byte)((reg[0] & 0x0F) | ((data & 0x0F) << 4));
                    cpuMemory.Switch1kChrRom(reg[0], 0);
                    break;

                case 0xB001:
                case 0xB004:
                    reg[1] = (byte)((reg[1] & 0xF0) | (data & 0x0F));
                    cpuMemory.Switch1kChrRom(reg[1], 1);
                    break;
                case 0xB003:
                case 0xB00C:
                    reg[1] = (byte)((reg[1] & 0x0F) | ((data & 0x0F) << 4));
                    cpuMemory.Switch1kChrRom(reg[1], 1);
                    break;

                case 0xC000:
                    reg[2] = (byte)((reg[2] & 0xF0) | (data & 0x0F));
                    cpuMemory.Switch1kChrRom(reg[2], 2);
                    break;
                case 0xC002:
                case 0xC008:
                    reg[2] = (byte)((reg[2] & 0x0F) | ((data & 0x0F) << 4));
                    cpuMemory.Switch1kChrRom(reg[2], 2);
                    break;

                case 0xC001:
                case 0xC004:
                    reg[3] = (byte)((reg[3] & 0xF0) | (data & 0x0F));
                    cpuMemory.Switch1kChrRom(reg[3], 3);
                    break;
                case 0xC003:
                case 0xC00C:
                    reg[3] = (byte)((reg[3] & 0x0F) | ((data & 0x0F) << 4));
                    cpuMemory.Switch1kChrRom(reg[3], 3);
                    break;

                case 0xD000:
                    reg[4] = (byte)((reg[4] & 0xF0) | (data & 0x0F));
                    cpuMemory.Switch1kChrRom(reg[4], 4);
                    break;
                case 0xD002:
                case 0xD008:
                    reg[4] = (byte)((reg[4] & 0x0F) | ((data & 0x0F) << 4));
                    cpuMemory.Switch1kChrRom(reg[4], 4);
                    break;

                case 0xD001:
                case 0xD004:
                    reg[5] = (byte)((reg[5] & 0xF0) | (data & 0x0F));
                    cpuMemory.Switch1kChrRom(reg[5], 5);
                    break;
                case 0xD003:
                case 0xD00C:
                    reg[5] = (byte)((reg[5] & 0x0F) | ((data & 0x0F) << 4));
                    cpuMemory.Switch1kChrRom(reg[5], 5);
                    break;

                case 0xE000:
                    reg[6] = (byte)((reg[6] & 0xF0) | (data & 0x0F));
                    cpuMemory.Switch1kChrRom(reg[6], 6);
                    break;
                case 0xE002:
                case 0xE008:
                    reg[6] = (byte)((reg[6] & 0x0F) | ((data & 0x0F) << 4));
                    cpuMemory.Switch1kChrRom(reg[6], 6);
                    break;

                case 0xE001:
                case 0xE004:
                    reg[7] = (byte)((reg[7] & 0xF0) | (data & 0x0F));
                    cpuMemory.Switch1kChrRom(reg[7], 7);
                    break;
                case 0xE003:
                case 0xE00C:
                    reg[7] = (byte)((reg[7] & 0x0F) | ((data & 0x0F) << 4));
                    cpuMemory.Switch1kChrRom(reg[7], 7);
                    break;
                
                case 0xF000:
                    irq_latch = (irq_latch & 0xF0) | (data & 0x0F);   
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;

                case 0xF002:
                case 0xF008:
                    irq_latch = (irq_latch & 0x0F) | ((data & 0x0F) << 4);       
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;

                case 0xF001:
                case 0xF004:
                    irq_enable = (byte)(data & 0x03);
                    irq_counter = irq_latch;
                    irq_clock = 0;
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;

                case 0xF003:
                case 0xF00C:
                    irq_enable = (byte)((irq_enable & 0x01) * 3);
                    break;
            }
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Switch16kPrgRom(0, 0);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            reg[9] = (byte)((cartridge.PrgPages * 2) - 2);
            
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
        }
        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(reg); stateStream.Write(SwapMode);
            stateStream.Write(irq_latch);
            stateStream.Write(irq_enable);
            stateStream.Write(irq_counter);
            stateStream.Write(irq_clock);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            stateStream.Read(reg);
            SwapMode = stateStream.ReadBooleans()[0];
            irq_latch = stateStream.ReadInt32();
            irq_enable = stateStream.ReadByte();
            irq_counter = stateStream.ReadInt32();
            irq_clock = stateStream.ReadInt32();
            base.LoadState(stateStream);
        }
    }
}
