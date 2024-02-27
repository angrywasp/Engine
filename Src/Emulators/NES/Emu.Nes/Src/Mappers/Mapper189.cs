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
    class Mapper189 : Mapper
    {
        byte[] reg = new byte[2];
        byte chr01 = 0;
        byte chr23 = 2;
        byte chr4 = 4;
        byte chr5 = 5;
        byte chr6 = 6;
        byte chr7 = 7;
        byte irq_enable = 0;
        byte irq_counter = 0;
        byte irq_latch = 0;
        byte[] protect_dat = new byte[4];

        public Mapper189(NesSystem nesSystem)
            : base(nesSystem) { }

        public override void Poke(int Address, byte data)
        {
            if (Address < 0x8000)
            {
                if ((Address & 0xFF00) == 0x4100)
                {
                    cpuMemory.Switch32kPrgRom(((data & 0x30) >> 4) * 8);
                }
                else if ((Address & 0xFF00) == 0x6100)
                {
                    cpuMemory.Switch32kPrgRom((data & 0x30) * 8);
                }
            }
            else
            {
                switch (Address & 0xE001)
                {
                    case 0x8000:
                        reg[0] = data;
                        SetBank_PPU();
                        break;

                    case 0x8001:
                        reg[1] = data;
                        SetBank_PPU();
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
                        }
                        break;

                    case 0xA000:
                        if ((data & 0x01) == 0x01)
                            cartridge.Mirroring = Mirroring.ModeHorz;
                        else
                            cartridge.Mirroring = Mirroring.ModeVert;
                        break;

                    case 0xC000:
                        irq_counter = data;
                        break;
                    case 0xC001:
                        irq_latch = data;
                        break;
                    case 0xE000:
                        irq_enable = 0;
                        cpu.IRQ(NesCpu.IsrType.External, false);
                        break;
                    case 0xE001:
                        irq_enable = 0xFF;
                        break;
                }
            }
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x4018, 0x7FFF, Poke);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 2) * 4, 0);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            SetBank_PPU();
            if (cartridge.PrgPages == 1)
            {
                cpuMemory.Switch16kPrgRom(0, 1);
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
            else
            {
                if ((reg[0] & 0x80) == 0x80)
                {
                    cpuMemory.Switch1kChrRom((chr01 + 0) & 0x07, 4);
                    cpuMemory.Switch1kChrRom((chr01 + 1) & 0x07, 5);
                    cpuMemory.Switch1kChrRom((chr23 + 0) & 0x07, 6);
                    cpuMemory.Switch1kChrRom((chr23 + 1) & 0x07, 7);
                    cpuMemory.Switch1kChrRom(chr4 & 0x07, 0);
                    cpuMemory.Switch1kChrRom(chr5 & 0x07, 1);
                    cpuMemory.Switch1kChrRom(chr6 & 0x07, 2);
                    cpuMemory.Switch1kChrRom(chr7 & 0x07, 3);
                }
                else
                {
                    cpuMemory.Switch1kChrRom((chr01 + 0) & 0x07, 0);
                    cpuMemory.Switch1kChrRom((chr01 + 1) & 0x07, 1);
                    cpuMemory.Switch1kChrRom((chr23 + 0) & 0x07, 2);
                    cpuMemory.Switch1kChrRom((chr23 + 1) & 0x07, 3);
                    cpuMemory.Switch1kChrRom(chr4 & 0x07, 4);
                    cpuMemory.Switch1kChrRom(chr5 & 0x07, 5);
                    cpuMemory.Switch1kChrRom(chr6 & 0x07, 6);
                    cpuMemory.Switch1kChrRom(chr7 & 0x07, 7);
                }
            }
        }
        public override void TickScanlineTimer()
        {
            if (irq_enable != 0)
            {
                if ((--irq_counter) == 0)
                {
                    irq_counter = irq_latch;
                    cpu.IRQ(NesCpu.IsrType.External, true);
                }
            }
        }
        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(reg);
            stateStream.Write(chr01);
            stateStream.Write(chr23);
            stateStream.Write(chr4);
            stateStream.Write(chr5);
            stateStream.Write(chr6);
            stateStream.Write(chr7);
            stateStream.Write(irq_enable);
            stateStream.Write(irq_latch);
            stateStream.Write(protect_dat);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            stateStream.Read(reg);
            chr01 = stateStream.ReadByte();
            chr23 = stateStream.ReadByte();
            chr4 = stateStream.ReadByte();
            chr5 = stateStream.ReadByte();
            chr6 = stateStream.ReadByte();
            chr7 = stateStream.ReadByte();
            irq_enable = stateStream.ReadByte();
            irq_latch = stateStream.ReadByte();
            stateStream.Read(protect_dat);
            base.LoadState(stateStream);
        }
    }
}