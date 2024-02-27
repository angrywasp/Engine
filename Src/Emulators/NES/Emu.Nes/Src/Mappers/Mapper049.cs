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

    class Mapper49 : Mapper
    {
        byte commandNumber;
        byte prgAddressSelect;
        byte chrAddressSelect;
        byte irq_enabled;
        byte irq_count, irq_reload = 0xff;
        byte prg0 = 0;
        byte prg1 = 1;
        byte chr0, chr1, chr2, chr3, chr4, chr5 = 0;
        int mode = 0;

        public Mapper49(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int address, byte data)
        {
            if (address <0x8000 & !cpuMemory.SRamReadOnly)
            {
                mode = data;
                SetPRG(); SetCHR();
            }
            else
            {
                address &= 0xE001;
                if (address == 0x8000)
                {
                    commandNumber = (byte)(data & 0x7);
                    prgAddressSelect = (byte)(data & 0x40);
                    chrAddressSelect = (byte)(data & 0x80);
                    SetPRG(); SetCHR();
                }
                else if (address == 0x8001)
                {
                    if (commandNumber == 0)
                    {
                        chr0 = (byte)(data - (data % 2)); SetCHR();
                    }
                    else if (commandNumber == 1)
                    {
                        chr1 = (byte)(data - (data % 2)); SetCHR();
                    }
                    else if (commandNumber == 2)
                    {
                        chr2 = (byte)(data & (cartridge.ChrPages * 8 - 1)); SetCHR();
                    }
                    else if (commandNumber == 3)
                    {
                        chr3 = data; SetCHR();
                    }
                    else if (commandNumber == 4)
                    {
                        chr4 = data; SetCHR();
                    }
                    else if (commandNumber == 5)
                    {
                        chr5 = data; SetCHR();
                    }
                    else if (commandNumber == 6)
                    {
                        prg0 = data; SetPRG();
                    }
                    else if (commandNumber == 7)
                    {
                        prg1 = data; SetPRG();
                    }
                }
                else if (address == 0xA000)
                {
                    if ((data & 0x1) == 0)
                    {
                        cartridge.Mirroring = Mirroring.ModeVert;
                    }
                    else
                    {
                        cartridge.Mirroring = Mirroring.ModeHorz;
                    }
                }
                else if (address == 0xA001)
                {
                    cpuMemory.SRamReadOnly = ((data & 0x80) == 0);
                }
                
                else if (address == 0xC000)
                {
                    irq_reload = data;
                }
                else if (address == 0xC001)
                {
                    irq_count = 0;
                }
                else if (address == 0xE000)
                {
                    irq_enabled = 0;
                    cpu.IRQ(NesCpu.IsrType.External, false);
                }
                else if (address == 0xE001)
                {
                    irq_enabled = 1;
                }
            }
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x4018, 0x7FFF, Poke);
            prgAddressSelect = 0;
            chrAddressSelect = 0;
            SetPRG();
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(8);
            cpuMemory.Switch4kChrRom(0, 0);
        }
        public override void TickScanlineTimer()
        {
            if (irq_count > 0)
                irq_count--;
            else
                irq_count = irq_reload;
            if (irq_count == 0)
            {
                if (irq_enabled == 1)
                {
                    cpu.IRQ(NesCpu.IsrType.External, true);
                }
            }
        }

        void SetPRG()
        {
            if ((mode & 0x1) == 1)
            {
                int r = mode >> 2 & 0x30;

                if (prgAddressSelect != 0)
                {
                    cpuMemory.Switch8kPrgRom(((((((cartridge.PrgPages * 2) - 2))) & 0x0F) | r) * 2, 0);
                    cpuMemory.Switch8kPrgRom(((((prg1)) & 0x0F) | r) * 2, 1);
                    cpuMemory.Switch8kPrgRom(((((prg0)) & 0x0F) | r) * 2, 2);
                    cpuMemory.Switch8kPrgRom(((((((cartridge.PrgPages * 2) - 1))) & 0x0F )| r) * 2, 3);
                }
                else
                {
                    cpuMemory.Switch8kPrgRom(((((prg0)) & 0x0F )| r) * 2, 0);
                    cpuMemory.Switch8kPrgRom(((((prg1)) & 0x0F) | r) * 2, 1);
                    cpuMemory.Switch8kPrgRom(((((((cartridge.PrgPages * 2) - 2))) & 0x0F) | r) * 2, 2);
                    cpuMemory.Switch8kPrgRom(((((((cartridge.PrgPages * 2) - 1))) & 0x0F )| r) * 2, 3);
                }
            }
            else
            {
                cpuMemory.Switch32kPrgRom((mode >> 4 & 0x3) * 8);
            }
        }
        void SetCHR()
        {
            int or = mode & 0xC0;
            if (chrAddressSelect == 0)
            {
                cpuMemory.Switch2kChrRom(chr0 | or, 0);
                cpuMemory.Switch2kChrRom(chr1 | or, 1);
                or <<= 1;
                cpuMemory.Switch1kChrRom(chr2 | or, 4);
                cpuMemory.Switch1kChrRom(chr3 | or, 5);
                cpuMemory.Switch1kChrRom(chr4 | or, 6);
                cpuMemory.Switch1kChrRom(chr5 | or, 7);
            }
            else
            {
                cpuMemory.Switch2kChrRom(chr0 | or, 2);
                cpuMemory.Switch2kChrRom(chr1 | or, 3);
                or <<= 1;
                cpuMemory.Switch1kChrRom(chr2 | or, 0);
                cpuMemory.Switch1kChrRom(chr3 | or, 1);
                cpuMemory.Switch1kChrRom(chr4 | or, 2);
                cpuMemory.Switch1kChrRom(chr5 | or, 3);
            }
        }
    }
}
