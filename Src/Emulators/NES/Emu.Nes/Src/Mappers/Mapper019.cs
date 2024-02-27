using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNes.Nes
{
    class Mapper19 : Mapper
    {
        public Mapper19(NesSystem nes)
            : base(nes)
        { }
        byte[] reg = new byte[3];
        byte[] exram = new byte[128];
        int irq_enable = 0;
        int irq_counter = 0;
        public byte[][] CRAM = new byte[32][];
        public int[] CRAM_PAGE = new int[8];
        //area 0,1,2,3,4,5,6,7
        void Switch1kCRAM(int start, int area)
        {
            CRAM_PAGE[area] = (start - cartridge.Chr.Length);
            cpuMemory.Switch1kChrRom(start, area);
        }
        void Switch1kCHRToVRAM(int start, int area)
        {
            switch (cartridge.ChrPages)
            {
                case (2): start = (start & 0xf); break;
                case (4): start = (start & 0x1f); break;
                case (8): start = (start & 0x3f); break;
                case (16): start = (start & 0x7f); break;
                case (32): start = (start & 0xff); break;
                case (64): start = (start & 0x1ff); break;
                case (128): start = (start & 0x1ff); break;
            }
            ppuMemory.nmtBank[area] = (byte)start;
        }

        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x4018, 0x7FFF, Peek, Poke);
            ppuMemory.Map(0x0000, 0x1FFF, ChrPeek, ChrPoke);
            ppuMemory.Map(0x2000, 0x3EFF, NmtPeek, NmtPoke);
            cpuMemory.Switch16kPrgRom(0, 0);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            for (int i = 0; i < 32; i++)
                CRAM[i] = new byte[1024];

            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
            base.Initialize(initializing);
        }
        public override void Poke(int addr, byte data)
        {
            if (addr >= 0x6000 & addr < 0x8000)
                cpuMemory.srm[addr - 0x6000] = data;
            else
                switch (addr & 0xF800)
                {
                    case 0x4800:
                        if (addr == 0x4800)
                        {
                            if ((reg[2] & 0x80) == 0x80)
                                reg[2] = (byte)((reg[2] + 1) | 0x80);
                        }
                        break;
                    case 0x5000:
                        irq_counter = (irq_counter & 0xFF00) | data;
                        break;
                    case 0x5800:
                        irq_counter = (irq_counter & 0x00FF) | ((data & 0x7F) << 8);
                        irq_enable = data & 0x80;
                        cpu.IRQ(NesCpu.IsrType.External, false);
                        break;

                    case 0x8000:
                        if ((data < 0xE0) || (reg[0] != 0))
                        {
                            cpuMemory.Switch1kChrRom(data, 0);
                        }
                        else
                        {
                            Switch1kCRAM(data & 0x1F, 0);
                        }
                        break;
                    case 0x8800:
                        if ((data < 0xE0) || (reg[0] != 0))
                        {
                            cpuMemory.Switch1kChrRom(data, 1);
                        }
                        else
                        {
                            Switch1kCRAM(data & 0x1F, 1);
                        }
                        break;
                    case 0x9000:
                        if ((data < 0xE0) || (reg[0] != 0))
                        {
                            cpuMemory.Switch1kChrRom(data, 2);
                        }
                        else
                        {
                            Switch1kCRAM(data & 0x1F, 2);
                        }
                        break;
                    case 0x9800:
                        if ((data < 0xE0) || (reg[0] != 0))
                        {
                            cpuMemory.Switch1kChrRom(data, 3);
                        }
                        else
                        {
                            Switch1kCRAM(data & 0x1F, 3);
                        }
                        break;
                    case 0xA000:
                        if ((data < 0xE0) || (reg[1] != 0))
                        {
                            cpuMemory.Switch1kChrRom(data, 4);
                        }
                        else
                        {
                            Switch1kCRAM(data & 0x1F, 4);
                        }
                        break;
                    case 0xA800:
                        if ((data < 0xE0) || (reg[1] != 0))
                        {
                            cpuMemory.Switch1kChrRom(data, 5);
                        }
                        else
                        {
                            Switch1kCRAM(data & 0x1F, 5);
                        }
                        break;
                    case 0xB000:
                        if ((data < 0xE0) || (reg[1] != 0))
                        {
                            cpuMemory.Switch1kChrRom(data, 6);
                        }
                        else
                        {
                            Switch1kCRAM(data & 0x1F, 6);
                        }
                        break;
                    case 0xB800:
                        if ((data < 0xE0) || (reg[1] != 0))
                        {
                            cpuMemory.Switch1kChrRom(data, 7);
                        }
                        else
                        {
                            Switch1kCRAM(data & 0x1F, 7);
                        }
                        break;
                    case 0xC000:
                        if (data <= 0xDF)
                        {
                            Switch1kCHRToVRAM(data, 0);
                        }
                        else
                        {
                            ppuMemory.nmtBank[0] = (byte)(data & 0x01);
                        }
                        break;
                    case 0xC800:

                        if (data <= 0xDF)
                        {
                            Switch1kCHRToVRAM(data, 1);
                        }
                        else
                        {
                            ppuMemory.nmtBank[1] = (byte)(data & 0x01);
                        }
                        break;
                    case 0xD000:
                        if (data <= 0xDF)
                        {
                            Switch1kCHRToVRAM(data, 2);
                        }
                        else
                        {
                            ppuMemory.nmtBank[2] = (byte)(data & 0x01);
                        }
                        break;
                    case 0xD800:
                        if (data <= 0xDF)
                        {
                            Switch1kCHRToVRAM(data, 3);
                        }
                        else
                        {
                            ppuMemory.nmtBank[3] = (byte)(data & 0x01);
                        }
                        break;
                    case 0xE000:
                        cpuMemory.Switch8kPrgRom((data & 0x3F) * 2, 0);
                        break;
                    case 0xE800:
                        reg[0] = (byte)(data & 0x40);
                        reg[1] = (byte)(data & 0x80);
                        cpuMemory.Switch8kPrgRom((data & 0x3F) * 2, 1);
                        break;
                    case 0xF000:
                        cpuMemory.Switch8kPrgRom((data & 0x3F) * 2, 2);
                        break;
                    case 0xF800:
                        if (addr == 0xF800)
                        {
                            reg[2] = data;
                        }
                        break;
                }
            base.Poke(addr, data);
        }
        public override byte Peek(int addr)
        {
            switch (addr & 0xF800)
            {
                case 0x4800:
                    if (addr == 0x4800)
                    {
                        if ((reg[2] & 0x80) == 0x80)
                            reg[2] = (byte)((reg[2] + 1) | 0x80);
                        //return data;
                    }
                    break;
                case 0x5000:
                    return (byte)(irq_counter & 0x00FF);
                case 0x5800:
                    return (byte)((irq_counter >> 8) & 0x7F);
            }
            return base.Peek(addr);
        }
        public override void TickCycleTimer(int cycles)
        {
            if (irq_enable > 0)
            {
                if ((irq_counter += cycles) >= 0x7FFF)
                {
                    irq_enable = 0;
                    irq_counter = 0x7FFF;
                    cpu.IRQ(NesCpu.IsrType.External, true);
                }
            }
            base.TickCycleTimer(cycles);
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
        private byte NmtPeek(int addr)
        {
            if (ppuMemory.nmtBank[(addr & 0x0C00) >> 10] < 4)
                return ppuMemory.nmt[ppuMemory.nmtBank[addr >> 10 & 0x03]][addr & 0x03FF];
            else
                return cartridge.Chr[ppuMemory.nmtBank[(addr & 0x0C00) >> 10]][addr & 0x3FF];
        }
        private void NmtPoke(int addr, byte data)
        {
            if (ppuMemory.nmtBank[(addr & 0x0C00) >> 10] < 4)
                ppuMemory.nmt[ppuMemory.nmtBank[addr >> 10 & 0x03]][addr & 0x03FF] = data;
        }
    }
}
