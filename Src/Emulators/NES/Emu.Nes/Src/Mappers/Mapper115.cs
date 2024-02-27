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
    class Mapper115 : Mapper
    {
        byte[] reg = new byte[8];
        byte prg0 = 0;
        byte prg0L = 0;
        byte prg1 = 0;
        byte prg1L = 0;
        byte prg2 = 0;
        byte prg3 = 0;
        byte ExPrgSwitch = 0;
        byte ExChrSwitch = 0;
        byte chr0 = 0;
        byte chr1 = 1;
        byte chr2 = 2;
        byte chr3 = 3;
        byte chr4 = 4;
        byte chr5 = 5;
        byte chr6 = 6;
        byte chr7 = 7;
        byte irq_enable = 0;
        byte irq_counter = 0;
        byte irq_latch = 0;

        public Mapper115(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int Address, byte data)
        {
            if (Address < 0x8000)
            {
                switch (Address)
                {
                    case 0x6000:
                        ExPrgSwitch = data; 
                        SetBank_CPU();
                        break;
                    case 0x6001:
                        ExChrSwitch = (byte)(data & 0x1);
                        SetBank_PPU();
                        break;
                }
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
                                chr0 = (byte)(data & 0xFE);
                                chr1 = (byte)(chr0 + 1);
                                SetBank_PPU();
                                break;
                            case 0x01:
                                chr2 = (byte)(data & 0xFE);
                                chr3 = (byte)(chr2 + 1);
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
                                prg0 = prg0L = data;
                                SetBank_CPU();
                                break;
                            case 0x07:
                                prg1 = prg1L = data;
                                SetBank_CPU();
                                break;
                        }
                        break;
                    case 0xA000:
                        reg[2] = data;
                        if (cartridge.Mirroring != Mirroring.ModeFull)
                        {
                            if ((data & 0x01) == 0x01)
                                cartridge.Mirroring = Mirroring.ModeHorz;
                            else
                                cartridge.Mirroring = Mirroring.ModeVert;
                        }
                        break;
                    case 0xA001:
                        reg[3] = data;
                        break;
                    case 0xC000:
                        reg[4] = data;
                        irq_counter = data;
                        irq_enable = 0xFF;
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
                        irq_enable = 0xFF;
                        break;
                }
            }
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x4018, 0x7FFF, Poke);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            prg0 = prg0L = 0;
            prg1 = prg1L = 1;
            prg2 = (byte)((cartridge.PrgPages * 2) - 2);
            prg3 = (byte)((cartridge.PrgPages * 2) - 1);

            ExPrgSwitch = 0;
            ExChrSwitch = 0;
            if (!cartridge.HasCharRam)
            {
                chr0 = 0;
                chr1 = 1;
                chr2 = 2;
                chr3 = 3;
                chr4 = 4;
                chr5 = 5;
                chr6 = 6;
                chr7 = 7;
            }
            else
            {
                chr0 = chr2 = chr4 = chr5 = chr6 = chr7 = 0;
                chr1 = chr3 = 1;
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
            if ((ExPrgSwitch & 0x80) == 0x80)
            {
                prg0 = (byte)((ExPrgSwitch << 1) & 0x1e);
                prg1 = (byte)(prg0 + 1);

                cpuMemory.Switch8kPrgRom(prg0 * 2, 0);
                cpuMemory.Switch8kPrgRom(prg1 * 2, 1);
                cpuMemory.Switch8kPrgRom((prg0 + 2) * 2, 2);
                cpuMemory.Switch8kPrgRom((prg1 + 2) * 2, 3);
            }
            else
            {
                prg0 = prg0L;
                prg1 = prg1L;
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
        }
        void SetBank_PPU()
        {
            if (!cartridge.HasCharRam)
            {
                if ((reg[0] & 0x80) == 0x80)
                {
                    cpuMemory.Switch1kChrRom((ExChrSwitch << 8) + chr4, 0);
                    cpuMemory.Switch1kChrRom((ExChrSwitch << 8) + chr5, 1);
                    cpuMemory.Switch1kChrRom((ExChrSwitch << 8) + chr6, 2);
                    cpuMemory.Switch1kChrRom((ExChrSwitch << 8) + chr7, 3);
                    cpuMemory.Switch1kChrRom((ExChrSwitch << 8) + chr0, 4);
                    cpuMemory.Switch1kChrRom((ExChrSwitch << 8) + chr1, 5);
                    cpuMemory.Switch1kChrRom((ExChrSwitch << 8) + chr2, 6);
                    cpuMemory.Switch1kChrRom((ExChrSwitch << 8) + chr3, 7);
                }
                else
                {
                    cpuMemory.Switch1kChrRom((ExChrSwitch << 8) + chr0, 0);
                    cpuMemory.Switch1kChrRom((ExChrSwitch << 8) + chr1, 1);
                    cpuMemory.Switch1kChrRom((ExChrSwitch << 8) + chr2, 2);
                    cpuMemory.Switch1kChrRom((ExChrSwitch << 8) + chr3, 3);
                    cpuMemory.Switch1kChrRom((ExChrSwitch << 8) + chr4, 4);
                    cpuMemory.Switch1kChrRom((ExChrSwitch << 8) + chr5, 5);
                    cpuMemory.Switch1kChrRom((ExChrSwitch << 8) + chr6, 6);
                    cpuMemory.Switch1kChrRom((ExChrSwitch << 8) + chr7, 7);
                }
            }
        }
    }
}
