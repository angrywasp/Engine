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
    class Mapper248 : Mapper
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
        byte irq_request = 0;
        public Mapper248(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int Address, byte data)
        {
            if (Address < 0x8000)
            {
                cpuMemory.Switch8kPrgRom((2 * data) * 2, 0);
                cpuMemory.Switch8kPrgRom((2 * data + 1) * 2, 1);
                cpuMemory.Switch8kPrgRom(((cartridge.PrgPages * 2) - 2) * 2, 2);
                cpuMemory.Switch8kPrgRom(((cartridge.PrgPages * 2) - 1) * 2, 3);
            }
            else
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

                        switch (reg[0] & 0x07)
                        {
                            case 0x00:
                                chr01 = (byte)(data & 0xFE);
                                SetBank_PPU();
                                break;
                            case 0x01:
                                chr23 = (byte)(data & 0xFE);
                                SetBank_PPU();
                                break;
                            case 0x02:
                                chr4 = data;
                                SetBank_PPU();
                                break;
                            case 0x03:
                                chr5 = data;
                                SetBank_PPU();
                                break;
                            case 0x04:
                                chr6 = data;
                                SetBank_PPU();
                                break;
                            case 0x05:
                                chr7 = data;
                                SetBank_PPU();
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
                    case 0xA000:
                        reg[2] = data;
                        if (cartridge.Mirroring != Mirroring.ModeFull)
                        {
                            if ((data & 0x01) == 0x01)
                            {
                                cartridge.Mirroring = Mirroring.ModeHorz;
                            }
                            else
                            {
                                cartridge.Mirroring = Mirroring.ModeVert;
                            }
                        }
                        break;
                    case 0xC000:
                        irq_enable = 0;
                        irq_latch = 0xBE;
                        irq_counter = 0xBE;
                        cpu.IRQ(NesCpu.IsrType.External, false);
                        break;
                    case 0xC001:
                        irq_enable = 1;
                        irq_latch = 0xBE;
                        irq_counter = 0xBE; 
                        break;
                }

            }
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x6000, 0x7FFF, Poke);
            SetBank_CPU();

            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);

            SetBank_PPU();
        }
        void SetBank_CPU()
        {
            if ((reg[0] & 0x40) != 0x40)
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
            stateStream.Write(irq_request);
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
            irq_request = stateStream.ReadByte();
            base.LoadState(stateStream);
        }
    }
}
