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
    class Mapper45 : Mapper
    {
        public Mapper45(NesSystem nes)
            : base(nes)
        { }
        int patch = 0;
        int[] reg = new int[8];
        int prg0 = 0;
        int prg1 = 1;
        int prg2 = 0;
        int prg3 = 0;
        int[] p = new int[4];
        int[] c = new int[8];
        int chr0 = 0;
        int chr1 = 1;
        int chr2 = 2;
        int chr3 = 3;
        int chr4 = 4;
        int chr5 = 5;
        int chr6 = 6;
        int chr7 = 7;

        int irq_enable = 0;
        int irq_counter = 0;
        int irq_latch = 0;
        int irq_latched = 0;
        int irq_reset = 0;

        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x6000, 0x7FFF, Poke);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(32);
            prg0 = 0;
            prg1 = 1;
            prg2 = ((cartridge.PrgPages * 2) - 2);
            prg3 = ((cartridge.PrgPages * 2) - 1);

            if (cartridge.Sha1.ToLower() == "b0fa48af696b7b7d39799b86564131291b202122"  // HIK 7-in-1 (Pirate Cart)
                | cartridge.Sha1.ToLower() == "e10696cca08f43467bcf1ec886a7e29ae20eed6e"// Super 8-in-1 (Pirate Cart)

                )
            {
                patch = 1;
                prg2 = 62;
                prg3 = 63;
            }
            else if (cartridge.Sha1.ToLower() == "0d16de52add9efec448f4bb7ddb98aa104ce3744") // Super 3-in-1 (Pirate Cart)
            {
                patch = 2;
            }
            cpuMemory.Switch8kPrgRom(prg0 * 2, 0);
            cpuMemory.Switch8kPrgRom(prg1 * 2, 1);
            cpuMemory.Switch8kPrgRom(prg2 * 2, 2);
            cpuMemory.Switch8kPrgRom(prg3 * 2, 3);

            p[0] = prg0;
            p[1] = prg1;
            p[2] = prg2;
            p[3] = prg3;

            cpuMemory.Switch8kChrRom(0);

            chr0 = c[0] = 0;
            chr1 = c[1] = 1;
            chr2 = c[2] = 2;
            chr3 = c[3] = 3;
            chr4 = c[4] = 4;
            chr5 = c[5] = 5;
            chr6 = c[6] = 6;
            chr7 = c[7] = 7;

            base.Initialize(initializing);
        }

        public override void Poke(int addr, byte data)
        {
            if (addr < 0x8000)
            {
                if ((reg[3] & 0x40) == 0)
                {
                    reg[reg[5]] = data;
                    reg[5] = (reg[5] + 1) & 0x03;

                    SetBank_CPU_0(prg0);
                    SetBank_CPU_1(prg1);
                    SetBank_CPU_2(prg2);
                    SetBank_CPU_3(prg3);
                    SetBank_PPU();
                }
            }
            else
            {
                switch (addr & 0xE001)
                {
                    case 0x8000:
                        if ((data & 0x40) != (reg[6] & 0x40))
                        {
                            int swp;
                            swp = prg0; prg0 = prg2; prg2 = swp;
                            swp = p[0]; p[0] = p[2]; p[2] = swp;
                            SetBank_CPU_0(p[0]);
                            SetBank_CPU_1(p[1]);
                        }
                        if (!cartridge.HasCharRam)
                        {
                            if ((data & 0x80) != (reg[6] & 0x80))
                            {
                                int swp;
                                swp = chr4; chr4 = chr0; chr0 = swp;
                                swp = chr5; chr5 = chr1; chr1 = swp;
                                swp = chr6; chr6 = chr2; chr2 = swp;
                                swp = chr7; chr7 = chr3; chr3 = swp;
                                swp = c[4]; c[4] = c[0]; c[0] = swp;
                                swp = c[5]; c[5] = c[1]; c[1] = swp;
                                swp = c[6]; c[6] = c[2]; c[2] = swp;
                                swp = c[7]; c[7] = c[3]; c[3] = swp;

                                cpuMemory.Switch1kChrRom(c[0], 0);
                                cpuMemory.Switch1kChrRom(c[1], 1);
                                cpuMemory.Switch1kChrRom(c[2], 2);
                                cpuMemory.Switch1kChrRom(c[3], 3);
                                cpuMemory.Switch1kChrRom(c[4], 4);
                                cpuMemory.Switch1kChrRom(c[5], 5);
                                cpuMemory.Switch1kChrRom(c[6], 6);
                                cpuMemory.Switch1kChrRom(c[7], 7);
                            }
                        }
                        reg[6] = data;
                        break;
                    case 0x8001:
                        switch (reg[6] & 0x07)
                        {
                            case 0x00:
                                chr0 = (data & 0xFE) + 0;
                                chr1 = (data & 0xFE) + 1;
                                SetBank_PPU();
                                break;
                            case 0x01:
                                chr2 = (data & 0xFE) + 0;
                                chr3 = (data & 0xFE) + 1;
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
                                if ((reg[6] & 0x40) == 0x40)
                                {
                                    prg2 = data & 0x3F;
                                    SetBank_CPU_2(data);
                                }
                                else
                                {
                                    prg0 = data & 0x3F;
                                    SetBank_CPU_0(data);
                                }
                                break;
                            case 0x07:
                                prg1 = data & 0x3F;
                                SetBank_CPU_1(data);
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
                        if (patch == 2)
                        {
                            if (data == 0x29 || data == 0x70)
                                data = 0x07;
                        }
                        irq_latch = data;
                        irq_latched = 1;
                        if (irq_reset != 0)
                        {
                            irq_counter = data;
                            irq_latched = 0;
                        }
                        break;
                    case 0xC001:
                        irq_counter = irq_latch;
                        break;
                    case 0xE000:
                        irq_enable = 0;
                        irq_reset = 1;
                        cpu.IRQ(NesCpu.IsrType.External, false);
                        break;
                    case 0xE001:
                        irq_enable = 1;
                        if (irq_latched != 0)
                        {
                            irq_counter = irq_latch;
                        }
                        break;
                }
            }
            base.Poke(addr, data);
        }
        public override void TickScanlineTimer()
        {
            if (irq_counter > 0)
            {
                irq_counter--;
                if (irq_counter == 0)
                {
                    if (irq_enable != 0)
                    {
                        cpu.IRQ(NesCpu.IsrType.External, true);
                    }
                }
            }
            base.TickScanlineTimer();
        }

        void SetBank_CPU_0(int data)
        {
            data &= (reg[3] & 0x3F) ^ 0xFF;
            data &= 0x3F;
            data |= reg[1];
            cpuMemory.Switch8kPrgRom(data * 2, 0);
            p[0] = data;
        }
        void SetBank_CPU_1(int data)
        {
            data &= (reg[3] & 0x3F) ^ 0xFF;
            data &= 0x3F;
            data |= reg[1];
            cpuMemory.Switch8kPrgRom(data * 2, 1);
            p[1] = data;
        }
        void SetBank_CPU_2(int data)
        {
            data &= (reg[3] & 0x3F) ^ 0xFF;
            data &= 0x3F;
            data |= reg[1];
            cpuMemory.Switch8kPrgRom(data * 2, 2);
            p[2] = data;
        }
        void SetBank_CPU_3(int data)
        {
            data &= (reg[3] & 0x3F) ^ 0xFF;
            data &= 0x3F;
            data |= reg[1];
            cpuMemory.Switch8kPrgRom(data * 2, 3);
            p[3] = data;
        }
        void SetBank_PPU()
        {
            byte[] table =
            {
	        	0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
	        	0x01,0x03,0x07,0x0F,0x1F,0x3F,0x7F,0xFF
        	};

            c[0] = chr0;
            c[1] = chr1;
            c[2] = chr2;
            c[3] = chr3;
            c[4] = chr4;
            c[5] = chr5;
            c[6] = chr6;
            c[7] = chr7;

            for (int i = 0; i < 8; i++)
            {
                c[i] &= table[reg[2] & 0x0F];
                c[i] |= reg[0] & ((patch != 1) ? 0xFF : 0xC0);
                c[i] += (reg[2] & ((patch != 1) ? 0x10 : 0x30)) << 4;
            }

            if ((reg[6] & 0x80) == 0x80)
            {
                cpuMemory.Switch1kChrRom(c[4], 0);
                cpuMemory.Switch1kChrRom(c[5], 1);
                cpuMemory.Switch1kChrRom(c[6], 2);
                cpuMemory.Switch1kChrRom(c[7], 3);
                cpuMemory.Switch1kChrRom(c[0], 4);
                cpuMemory.Switch1kChrRom(c[1], 5);
                cpuMemory.Switch1kChrRom(c[2], 6);
                cpuMemory.Switch1kChrRom(c[3], 7);

            }
            else
            {
                cpuMemory.Switch1kChrRom(c[0], 0);
                cpuMemory.Switch1kChrRom(c[1], 1);
                cpuMemory.Switch1kChrRom(c[2], 2);
                cpuMemory.Switch1kChrRom(c[3], 3);
                cpuMemory.Switch1kChrRom(c[4], 4);
                cpuMemory.Switch1kChrRom(c[5], 5);
                cpuMemory.Switch1kChrRom(c[6], 6);
                cpuMemory.Switch1kChrRom(c[7], 7);
            }
        }

        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(reg);
            stateStream.Write(prg0);
            stateStream.Write(prg1);
            stateStream.Write(prg2);
            stateStream.Write(prg3);
            stateStream.Write(p);
            stateStream.Write(c);
            stateStream.Write(chr0);
            stateStream.Write(chr1);
            stateStream.Write(chr2);
            stateStream.Write(chr3);
            stateStream.Write(chr4);
            stateStream.Write(chr5);
            stateStream.Write(chr6);
            stateStream.Write(chr7);
            stateStream.Write(irq_enable);
            stateStream.Write(irq_counter);
            stateStream.Write(irq_latch);
            stateStream.Write(irq_latched);
            stateStream.Write(irq_reset);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            stateStream.Read(reg);
            prg0 = stateStream.ReadInt32();
            prg1 = stateStream.ReadInt32();
            prg2 = stateStream.ReadInt32();
            prg3 = stateStream.ReadInt32();
            stateStream.Read(p);
            stateStream.Read(c);
            chr0 = stateStream.ReadInt32();
            chr1 = stateStream.ReadInt32();
            chr2 = stateStream.ReadInt32();
            chr3 = stateStream.ReadInt32();
            chr4 = stateStream.ReadInt32();
            chr5 = stateStream.ReadInt32();
            chr6 = stateStream.ReadInt32();
            chr7 = stateStream.ReadInt32();
            irq_enable = stateStream.ReadInt32();
            irq_counter = stateStream.ReadInt32();
            irq_latch = stateStream.ReadInt32();
            irq_latched = stateStream.ReadInt32();
            irq_reset = stateStream.ReadInt32();
            base.LoadState(stateStream);
        }
    }
}
