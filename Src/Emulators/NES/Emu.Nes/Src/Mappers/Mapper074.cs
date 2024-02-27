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
    class Mapper74 : Mapper
    {
        public Mapper74(NesSystem nes)
            : base(nes)
        { }

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
        public byte[][] CRAM = new byte[32][];
        public int[] CRAM_PAGE = new int[8];


        public override void Poke(int addr, byte data)
        {
            switch (addr & 0xE001)
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
                    if ((data & 0x01) == 0x01)
                        cartridge.Mirroring = Mirroring.ModeHorz;
                    else
                        cartridge.Mirroring = Mirroring.ModeVert;
                    break;
                case 0xA001:
                    reg[3] = data;
                    break;
                case 0xC000:
                    reg[4] = data;
                    irq_counter = data;
                    irq_request = 0;
                    break;
                case 0xC001:
                    reg[5] = data;
                    irq_latch = data;
                    irq_request = 0;
                    break;
                case 0xE000:
                    reg[6] = data;
                    irq_enable = 0;
                    irq_request = 0;
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
                case 0xE001:
                    reg[7] = data;
                    irq_enable = 1;
                    irq_request = 0;
                    break;
            }

            base.Poke(addr, data);
        }
        protected override void Initialize(bool initializing)
        {
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            CloneCHRtoCRAM();
            SetBank_PPU();
            SetBank_CPU();
            base.Initialize(initializing);
        }
        public override void TickScanlineTimer()
        {
            if (irq_enable != 0 && irq_request == 0)
            {
                if (ppu.scanline == 0)
                {
                    if (irq_counter > 0)
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
                    SetBank_PPUSUB(4, chr01 + 0);
                    SetBank_PPUSUB(5, chr01 + 1);
                    SetBank_PPUSUB(6, chr23 + 0);
                    SetBank_PPUSUB(7, chr23 + 1);
                    SetBank_PPUSUB(0, chr4);
                    SetBank_PPUSUB(1, chr5);
                    SetBank_PPUSUB(2, chr6);
                    SetBank_PPUSUB(3, chr7);
                }
                else
                {
                    SetBank_PPUSUB(0, chr01 + 0);
                    SetBank_PPUSUB(1, chr01 + 1);
                    SetBank_PPUSUB(2, chr23 + 0);
                    SetBank_PPUSUB(3, chr23 + 1);
                    SetBank_PPUSUB(4, chr4);
                    SetBank_PPUSUB(5, chr5);
                    SetBank_PPUSUB(6, chr6);
                    SetBank_PPUSUB(7, chr7);
                }
            }
            else
            {
                if ((reg[0] & 0x80) == 0x80)
                {
                    Switch1kCRAMEX((chr01 + 0) & 0x07, 4);
                    Switch1kCRAMEX((chr01 + 1) & 0x07, 5);
                    Switch1kCRAMEX((chr23 + 0) & 0x07, 6);
                    Switch1kCRAMEX((chr23 + 1) & 0x07, 7);
                    Switch1kCRAMEX(chr4 & 0x07, 0);
                    Switch1kCRAMEX(chr5 & 0x07, 1);
                    Switch1kCRAMEX(chr6 & 0x07, 2);
                    Switch1kCRAMEX(chr7 & 0x07, 3);
                }
                else
                {
                    Switch1kCRAMEX((chr01 + 0) & 0x07, 0);
                    Switch1kCRAMEX((chr01 + 1) & 0x07, 1);
                    Switch1kCRAMEX((chr23 + 0) & 0x07, 2);
                    Switch1kCRAMEX((chr23 + 1) & 0x07, 3);
                    Switch1kCRAMEX(chr4 & 0x07, 4);
                    Switch1kCRAMEX(chr5 & 0x07, 5);
                    Switch1kCRAMEX(chr6 & 0x07, 6);
                    Switch1kCRAMEX(chr7 & 0x07, 7);
                }
            }
        }
        void SetBank_PPUSUB(int bank, int page)
        {
            if ((page == 8 || page == 9))
            {
                Switch1kCRAMEX(page & 7, bank);
            }
            // else if (patch == 1 && page >= 128)
            // {
            //      SetCRAM_1K_Bank(bank, page & 7);
            //  }
            else
            {
                cpuMemory.Switch1kChrRom(page, bank);
            }
        }

        void Switch1kCRAMEX(int start, int area)
        {
            if (start >= cartridge.Chr.Length)
                start -= cartridge.Chr.Length;
            CRAM_PAGE[area] = start;
            cpuMemory.Switch1kChrRom(start + cartridge.Chr.Length, area);
        }
        private byte ChrPeek(int addr)
        {
            if (cpuMemory.ChrBank[(addr & 0x1C00) >> 10] < cartridge.Chr.Length)
                return cartridge.Chr[cpuMemory.ChrBank[addr >> 10 & 0x07]][addr & 0x03FF];
            else
                return CRAM[CRAM_PAGE[(addr & 0x1C00) >> 10]][addr & 0x3FF];
        }
        private void ChrPoke(int addr, byte data)
        {
            if (cartridge.HasCharRam)
            {
                cartridge.Chr[cpuMemory.ChrBank[addr >> 10 & 0x07]][addr & 0x03FF] = data;
            }
            else
            {
                CRAM[CRAM_PAGE[(addr & 0x1C00) >> 10]][addr & 0x3FF] = data;
            }
        }
        void CloneCHRtoCRAM()
        {
            CRAM = new byte[cartridge.Chr.Length][];
            for (int i = 0; i < cartridge.Chr.Length; i++)
                CRAM[i] = cartridge.Chr[i];
            for (int i = cartridge.Chr.Length; i < cartridge.Chr.Length; i++)
                CRAM[i] = new byte[1024];
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
            for (int i = 0; i < CRAM.Length; i++)
                stateStream.Write(CRAM[i]);
            stateStream.Write(CRAM_PAGE);
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
            for (int i = 0; i < CRAM.Length; i++)
                stateStream.Read(CRAM[i]);
            stateStream.Read(CRAM_PAGE);
            base.LoadState(stateStream);
        }
    }
}
