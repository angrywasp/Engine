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
    class Mapper118 : Mapper
    {
        byte[] reg = new byte[8];
        byte prg0 = 0;
        byte prg1 = 1;
        byte chr01 = 0;
        byte chr23 = 2;
        byte chr4 = 4;
        byte chr5 = 5;
        byte chr6 = 6;
        byte chr7 = 7;
        byte we_sram = 0;
        byte irq_enable = 0;
        byte irq_counter = 0;
        byte irq_latch = 0;

        public Mapper118(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int Address, byte data)
        {
            switch (Address & 0xE001)
            {
                case 0x8000:
                    reg[0] = data;
                    SetBank_CPU();
                    SetBank_PPU();
                    break;
                case 0x8001:
                    reg[1] = data;

                    if ((reg[0] & 0x80) != 0)
                    {
                        if ((reg[0] & 0x07) == 2)
                        {
                            if ((data & 0x80) != 0)
                                cartridge.Mirroring = Mirroring.Mode1ScA;
                            else
                                cartridge.Mirroring = Mirroring.Mode1ScB;
                        }
                    }
                    else
                    {
                        if ((reg[0] & 0x07) == 0)
                        {
                            if ((data & 0x80) != 0)
                                cartridge.Mirroring = Mirroring.Mode1ScA;
                            else
                                cartridge.Mirroring = Mirroring.Mode1ScB;
                        }
                    }

                    switch (reg[0] & 0x07)
                    {
                        case 0x00:
                            if (!cartridge.HasCharRam)
                            {
                                chr01 = (byte)(data & 0xFE);
                                SetBank_PPU();
                            }
                            break;
                        case 0x01:
                            if (!cartridge.HasCharRam)
                            {
                                chr23 = (byte)(data & 0xFE);
                                SetBank_PPU();
                            }
                            break;
                        case 0x02:
                            if (!cartridge.HasCharRam)
                            {
                                chr4 = data;
                                SetBank_PPU();
                            }
                            break;
                        case 0x03:
                            if (!cartridge.HasCharRam)
                            {
                                chr5 = data;
                                SetBank_PPU();
                            }
                            break;
                        case 0x04:
                            if (!cartridge.HasCharRam)
                            {
                                chr6 = data;
                                SetBank_PPU();
                            }
                            break;
                        case 0x05:
                            if (!cartridge.HasCharRam)
                            {
                                chr7 = data;
                                SetBank_PPU();
                            }
                            break;
                        case 0x06:
                            prg0 = data;
                            SetBank_CPU();
                            break;
                        case 0x07:
                            prg1 = data;
                            SetBank_CPU();
                            break;
                    }
                    break;

                case 0xC000:
                    reg[4] = data;
                    irq_counter = data;
                    break;
                case 0xC001:
                    reg[5] = data;
                    irq_latch = data;
                    break;
                case 0xE000:
                    reg[6] = data;
                    irq_enable = 0;
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
                case 0xE001:
                    reg[7] = data;
                    irq_enable = 1;
                    break;
            }
        }
        protected override void Initialize(bool initializing)
        {

            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
            if (!cartridge.HasCharRam)
            {
                chr01 = 0;
                chr23 = 2;
                chr4 = 4;
                chr5 = 5;
                chr6 = 6;
                chr7 = 7;

            }
            else
            {
                chr01 = 0;
                chr23 = 0;
                chr4 = 0;
                chr5 = 0;
                chr6 = 0;
                chr7 = 0;
            }
            SetBank_PPU();
            SetBank_CPU();
        }
        public override void TickScanlineTimer()
        {
            if (irq_enable != 0)
            {
                if ((irq_counter--) == 0)
                {
                    irq_counter = irq_latch;
                    cpu.IRQ(NesCpu.IsrType.External, true);
                }
            }
        }
        void SetBank_CPU()
        {
            if ((reg[0] & 0x40) == 0x40)
            {
                cpuMemory.Switch8kPrgRom(((cartridge.PrgPages * 2) - 2) * 2, 0);
                cpuMemory.Switch8kPrgRom(prg1 * 2, 1);
                cpuMemory.Switch8kPrgRom(prg0 * 2, 2);
                cpuMemory.Switch8kPrgRom(((cartridge.PrgPages * 2) - 1) * 2, 3);
            }
            else
            {
                cpuMemory.Switch8kPrgRom(prg0 * 2, 0);
                cpuMemory.Switch8kPrgRom(prg1 * 2, 1);
                cpuMemory.Switch8kPrgRom(((cartridge.PrgPages * 2) - 2) * 2, 2);
                cpuMemory.Switch8kPrgRom(((cartridge.PrgPages * 2) - 1) * 2, 3);
            }
        }
        void SetBank_PPU()
        {
            if (!cartridge.HasCharRam)
            {
                if ((reg[0] & 0x80) == 0x80)
                {
                    cpuMemory.Switch1kChrRom(chr4, 0);
                    cpuMemory.Switch1kChrRom(chr5, 1);
                    cpuMemory.Switch1kChrRom(chr6, 2);
                    cpuMemory.Switch1kChrRom(chr7, 3);
                    cpuMemory.Switch1kChrRom(chr01, 4);
                    cpuMemory.Switch1kChrRom(chr01 + 1, 5);
                    cpuMemory.Switch1kChrRom(chr23, 6);
                    cpuMemory.Switch1kChrRom(chr23 + 1, 7);
                }
                else
                {
                    cpuMemory.Switch1kChrRom(chr01, 0);
                    cpuMemory.Switch1kChrRom(chr01 + 1, 1);
                    cpuMemory.Switch1kChrRom(chr23, 2);
                    cpuMemory.Switch1kChrRom(chr23 + 1, 3);
                    cpuMemory.Switch1kChrRom(chr4, 4);
                    cpuMemory.Switch1kChrRom(chr5, 5);
                    cpuMemory.Switch1kChrRom(chr6, 6);
                    cpuMemory.Switch1kChrRom(chr7, 7);
                }
            }
        }
        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(reg);
            stateStream.Write(prg0);
            stateStream.Write(prg1);
            stateStream.Write(chr01);
            stateStream.Write(chr23);
            stateStream.Write(chr4);
            stateStream.Write(chr5);
            stateStream.Write(chr6);
            stateStream.Write(chr7);
            stateStream.Write(we_sram);
            stateStream.Write(irq_enable);
            stateStream.Write(irq_counter);
            stateStream.Write(irq_latch);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            stateStream.Read(reg);
            prg0 = stateStream.ReadByte();
            prg1 = stateStream.ReadByte();
            chr01 = stateStream.ReadByte();
            chr23 = stateStream.ReadByte();
            chr4 = stateStream.ReadByte();
            chr5 = stateStream.ReadByte();
            chr6 = stateStream.ReadByte();
            chr7 = stateStream.ReadByte();
            we_sram = stateStream.ReadByte();
            irq_enable = stateStream.ReadByte();
            irq_counter = stateStream.ReadByte();
            irq_latch = stateStream.ReadByte();
            base.LoadState(stateStream);
        }
    }
}
