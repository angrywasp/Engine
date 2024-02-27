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
    class Mapper249 : Mapper
    {
        byte[] reg = new byte[8];
        byte irq_enable = 0;
        byte irq_counter = 0;
        byte irq_latch = 0;
        byte irq_request = 0;
        byte spdata = 0;
        public Mapper249(NesSystem nes)
            : base(nes)
        { }
        public override void Poke(int addr, byte data)
        {
            if (addr == 0x5000)
            {
                switch (data)
                {
                    case 0x00:
                        spdata = 0;
                        break;
                    case 0x02:
                        spdata = 1;
                        break;
                }
            }
            if (addr >= 0x8000)
            {
                int m0, m1, m2, m3, m4, m5, m6, m7;

                switch (addr & 0xFF01)
                {
                    case 0x8000:
                    case 0x8800:
                        reg[0] = data;
                        break;
                    case 0x8001:
                    case 0x8801:
                        switch (reg[0] & 0x07)
                        {
                            case 0x00:
                                if (spdata == 1)
                                {
                                    m0 = data & 0x1;
                                    m1 = (data & 0x02) >> 1;
                                    m2 = (data & 0x04) >> 2;
                                    m3 = (data & 0x08) >> 3;
                                    m4 = (data & 0x10) >> 4;
                                    m5 = (data & 0x20) >> 5;
                                    m6 = (data & 0x40) >> 6;
                                    m7 = (data & 0x80) >> 7;
                                    data = (byte)((m5 << 7) | (m4 << 6) | (m2 << 5) | (m6 << 4) | (m7 << 3) | (m3 << 2) | (m1 << 1) | m0);
                                }
                                cpuMemory.Switch1kChrRom(data & 0xFE, 0);
                                cpuMemory.Switch1kChrRom(data | 0x01, 1);
                                break;
                            case 0x01:
                                if (spdata == 1)
                                {
                                    m0 = data & 0x1;
                                    m1 = (data & 0x02) >> 1;
                                    m2 = (data & 0x04) >> 2;
                                    m3 = (data & 0x08) >> 3;
                                    m4 = (data & 0x10) >> 4;
                                    m5 = (data & 0x20) >> 5;
                                    m6 = (data & 0x40) >> 6;
                                    m7 = (data & 0x80) >> 7;
                                    data = (byte)((m5 << 7) | (m4 << 6) | (m2 << 5) | (m6 << 4) | (m7 << 3) | (m3 << 2) | (m1 << 1) | m0);
                                }
                                cpuMemory.Switch1kChrRom(data & 0xFE, 2);
                                cpuMemory.Switch1kChrRom(data | 0x01, 3);
                                break;
                            case 0x02:
                                if (spdata == 1)
                                {
                                    m0 = data & 0x1;
                                    m1 = (data & 0x02) >> 1;
                                    m2 = (data & 0x04) >> 2;
                                    m3 = (data & 0x08) >> 3;
                                    m4 = (data & 0x10) >> 4;
                                    m5 = (data & 0x20) >> 5;
                                    m6 = (data & 0x40) >> 6;
                                    m7 = (data & 0x80) >> 7;
                                    data = (byte)((m5 << 7) | (m4 << 6) | (m2 << 5) | (m6 << 4) | (m7 << 3) | (m3 << 2) | (m1 << 1) | m0);
                                }
                                cpuMemory.Switch1kChrRom(data, 4);
                                break;
                            case 0x03:
                                if (spdata == 1)
                                {
                                    m0 = data & 0x1;
                                    m1 = (data & 0x02) >> 1;
                                    m2 = (data & 0x04) >> 2;
                                    m3 = (data & 0x08) >> 3;
                                    m4 = (data & 0x10) >> 4;
                                    m5 = (data & 0x20) >> 5;
                                    m6 = (data & 0x40) >> 6;
                                    m7 = (data & 0x80) >> 7;
                                    data = (byte)((m5 << 7) | (m4 << 6) | (m2 << 5) | (m6 << 4) | (m7 << 3) | (m3 << 2) | (m1 << 1) | m0);
                                }
                                cpuMemory.Switch1kChrRom(data, 5);
                                break;
                            case 0x04:
                                if (spdata == 1)
                                {
                                    m0 = data & 0x1;
                                    m1 = (data & 0x02) >> 1;
                                    m2 = (data & 0x04) >> 2;
                                    m3 = (data & 0x08) >> 3;
                                    m4 = (data & 0x10) >> 4;
                                    m5 = (data & 0x20) >> 5;
                                    m6 = (data & 0x40) >> 6;
                                    m7 = (data & 0x80) >> 7;
                                    data = (byte)((m5 << 7) | (m4 << 6) | (m2 << 5) | (m6 << 4) | (m7 << 3) | (m3 << 2) | (m1 << 1) | m0);
                                }
                                cpuMemory.Switch1kChrRom(data, 6);
                                break;
                            case 0x05:
                                if (spdata == 1)
                                {
                                    m0 = data & 0x1;
                                    m1 = (data & 0x02) >> 1;
                                    m2 = (data & 0x04) >> 2;
                                    m3 = (data & 0x08) >> 3;
                                    m4 = (data & 0x10) >> 4;
                                    m5 = (data & 0x20) >> 5;
                                    m6 = (data & 0x40) >> 6;
                                    m7 = (data & 0x80) >> 7;
                                    data = (byte)((m5 << 7) | (m4 << 6) | (m2 << 5) | (m6 << 4) | (m7 << 3) | (m3 << 2) | (m1 << 1) | m0);
                                }
                                cpuMemory.Switch1kChrRom(data, 7);
                                break;
                            case 0x06:
                                if (spdata == 1)
                                {
                                    if (data < 0x20)
                                    {
                                        m0 = data & 0x1;
                                        m1 = (data & 0x02) >> 1;
                                        m2 = (data & 0x04) >> 2;
                                        m3 = (data & 0x08) >> 3;
                                        m4 = (data & 0x10) >> 4;
                                        m5 = 0;
                                        m6 = 0;
                                        m7 = 0;
                                        data = (byte)((m7 << 7) | (m6 << 6) | (m5 << 5) | (m2 << 4) | (m1 << 3) | (m3 << 2) | (m4 << 1) | m0);
                                    }
                                    else
                                    {
                                        data -= 0x20;
                                        m0 = data & 0x1;
                                        m1 = (data & 0x02) >> 1;
                                        m2 = (data & 0x04) >> 2;
                                        m3 = (data & 0x08) >> 3;
                                        m4 = (data & 0x10) >> 4;
                                        m5 = (data & 0x20) >> 5;
                                        m6 = (data & 0x40) >> 6;
                                        m7 = (data & 0x80) >> 7;
                                        data = (byte)((m5 << 7) | (m4 << 6) | (m2 << 5) | (m6 << 4) | (m7 << 3) | (m3 << 2) | (m1 << 1) | m0);
                                    }
                                }
                                cpuMemory.Switch8kPrgRom(data * 2, 0);
                                break;
                            case 0x07:
                                if (spdata == 1)
                                {
                                    if (data < 0x20)
                                    {
                                        m0 = data & 0x1;
                                        m1 = (data & 0x02) >> 1;
                                        m2 = (data & 0x04) >> 2;
                                        m3 = (data & 0x08) >> 3;
                                        m4 = (data & 0x10) >> 4;
                                        m5 = 0;
                                        m6 = 0;
                                        m7 = 0;
                                        data = (byte)((m7 << 7) | (m6 << 6) | (m5 << 5) | (m2 << 4) | (m1 << 3) | (m3 << 2) | (m4 << 1) | m0);
                                    }
                                    else
                                    {
                                        data -= 0x20;
                                        m0 = data & 0x1;
                                        m1 = (data & 0x02) >> 1;
                                        m2 = (data & 0x04) >> 2;
                                        m3 = (data & 0x08) >> 3;
                                        m4 = (data & 0x10) >> 4;
                                        m5 = (data & 0x20) >> 5;
                                        m6 = (data & 0x40) >> 6;
                                        m7 = (data & 0x80) >> 7;
                                        data = (byte)((m5 << 7) | (m4 << 6) | (m2 << 5) | (m6 << 4) | (m7 << 3) | (m3 << 2) | (m1 << 1) | m0);
                                    }
                                }
                                cpuMemory.Switch8kPrgRom(data * 2, 1);
                                break;
                        }
                        break;
                    case 0xA000:
                    case 0xA800:
                        reg[2] = data;

                        if ((data & 0x01) == 0x01)
                            cartridge.Mirroring = Mirroring.ModeHorz;
                        else
                            cartridge.Mirroring = Mirroring.ModeVert;
                        break;
                    case 0xA001:
                    case 0xA801:
                        reg[3] = data;
                        break;
                    case 0xC000:
                    case 0xC800:
                        reg[4] = data;
                        irq_counter = data;
                        irq_request = 0;
                        cpu.IRQ(NesCpu.IsrType.External, false);
                        break;
                    case 0xC001:
                    case 0xC801:
                        reg[5] = data;
                        irq_latch = data;
                        irq_request = 0;
                        cpu.IRQ(NesCpu.IsrType.External, false);
                        break;
                    case 0xE000:
                    case 0xE800:
                        reg[6] = data;
                        irq_enable = 0;
                        irq_request = 0;
                        cpu.IRQ(NesCpu.IsrType.External, false);
                        break;
                    case 0xE001:
                    case 0xE801:
                        reg[7] = data;
                        irq_enable = 1;
                        irq_request = 0;
                        cpu.IRQ(NesCpu.IsrType.External, false);
                        break;
                }
            }
            base.Poke(addr, data);
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x4018, 0x7FFF, Poke);
            cpuMemory.Switch16kPrgRom(0, 0);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
            base.Initialize(initializing);
        }
        public override void TickScanlineTimer()
        {
            if (irq_enable != 0 && irq_request == 0)
            {
                if (ppu.scanline == 0)
                {
                    if (irq_counter != 0)
                    {
                        irq_counter--;
                    }
                }
                if ((irq_counter--) == 0)
                {
                    irq_request = 0xFF;
                    irq_counter = irq_latch;
                    cpu.IRQ(NesCpu.IsrType.External, true);
                }
            }
            base.TickScanlineTimer();
        }
    }
}
