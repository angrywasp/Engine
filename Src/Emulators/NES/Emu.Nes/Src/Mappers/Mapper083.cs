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
    class Mapper83 : Mapper
    {
        byte[] reg = new byte[3];
        int chr_bank = 0;
        int irq_enable = 0;
        int irq_counter = 0;

        public Mapper83(NesSystem nes)
            : base(nes)
        { }
        public override void Poke(int addr, byte data)
        {
            switch (addr)
            {
                case 0x5101:
                case 0x5102:
                case 0x5103:
                    reg[2] = data;
                    break;
                case 0x8000:
                case 0xB000:
                case 0xB0FF:
                case 0xB1FF:
                    reg[0] = data;
                    chr_bank = (data & 0x30) << 4;
                    cpuMemory.Switch16kPrgRom(data * 4, 0);
                    cpuMemory.Switch16kPrgRom(((data & 0x30) | 0x0F) * 4, 1);
                    break;

                case 0x8100:
                    reg[1] = (byte)(data & 0x80);
                    data &= 0x03;
                    if (data == 0)
                        cartridge.Mirroring = Mirroring.ModeVert;
                    else if (data == 1)
                        cartridge.Mirroring = Mirroring.ModeHorz;
                    else if (data == 2)
                    {
                        cartridge.Mirroring = Mirroring.Mode1ScA;
                    }
                    else
                    {
                        cartridge.Mirroring = Mirroring.Mode1ScB;
                    }
                    break;

                case 0x8200:
                    irq_counter = (irq_counter & 0xFF00) | data;  
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
                case 0x8201:
                    irq_counter = (irq_counter & 0x00FF) | (data << 8);
                    irq_enable = reg[1];
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;

                case 0x8300:
                    cpuMemory.Switch8kPrgRom(data * 2, 0);
                    break;
                case 0x8301:
                    cpuMemory.Switch8kPrgRom(data * 2, 1);
                    break;
                case 0x8302:
                    cpuMemory.Switch8kPrgRom(data * 2, 2);
                    break;

                case 0x8310:
                    if (cartridge.CHRSizeInKB == 512)
                    {
                        cpuMemory.Switch2kChrRom((chr_bank | data) * 2, 0);
                    }
                    else
                    {
                        cpuMemory.Switch1kChrRom(chr_bank | data, 0);
                    }
                    break;
                case 0x8311:
                    if (cartridge.CHRSizeInKB == 512)
                    {
                        cpuMemory.Switch2kChrRom((chr_bank | data) * 2, 1);
                    }
                    else
                    {
                        cpuMemory.Switch1kChrRom(chr_bank | data, 1);
                    }
                    break;
                case 0x8312:
                    cpuMemory.Switch1kChrRom(chr_bank | data, 2);
                    break;
                case 0x8313:
                    cpuMemory.Switch1kChrRom(chr_bank | data, 3);
                    break;
                case 0x8314:
                    cpuMemory.Switch1kChrRom(chr_bank | data, 4);
                    break;
                case 0x8315:
                    cpuMemory.Switch1kChrRom(chr_bank | data, 5);
                    break;
                case 0x8316:
                    if (cartridge.CHRSizeInKB == 512)
                    {
                        cpuMemory.Switch2kChrRom((chr_bank | data) * 2, 2);
                    }
                    else
                    {
                        cpuMemory.Switch1kChrRom(chr_bank | data, 6);
                    }
                    break;
                case 0x8317:
                    if (cartridge.CHRSizeInKB == 512)
                    {
                        cpuMemory.Switch2kChrRom((chr_bank | data) * 2, 3);
                    }
                    else
                    {
                        cpuMemory.Switch1kChrRom(chr_bank | data, 7);
                    }
                    break;

                case 0x8318:
                    cpuMemory.Switch16kPrgRom(((reg[0] & 0x30) | data) * 4, 0);
                    break;
            }
            base.Poke(addr, data);
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x4018, 0x7FFF, Poke);
            if (cartridge.PRGSizeInKB >= 256)
            {
                reg[1] = 0x30;
                cpuMemory.Switch16kPrgRom(0, 0);
                cpuMemory.Switch8kPrgRom(30 * 2, 2);
                cpuMemory.Switch8kPrgRom(31 * 2, 3);
            }
            else
            {
                cpuMemory.Switch16kPrgRom(0, 0);
                cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            }
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
            base.Initialize(initializing);
        }
        public override void TickScanlineTimer()
        {
            if (irq_enable != 0)
            {
                if (irq_counter <= 113)
                {
                    irq_enable = 0;
                    cpu.IRQ(NesCpu.IsrType.External, true);
                }
                else
                {
                    irq_counter -= 113;
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
            stateStream.Write(chr_bank);
            stateStream.Write(irq_enable); 
            stateStream.Write(irq_counter);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            stateStream.Read(reg);
            chr_bank = stateStream.ReadInt32();
            irq_enable = stateStream.ReadInt32();
            irq_counter = stateStream.ReadInt32();
            base.LoadState(stateStream);
        }
    }
}
