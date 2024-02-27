using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNes.Nes
{
    class Mapper12 : Mapper
    {
        public Mapper12(NesSystem nes)
            : base(nes)
        { }
        byte[] reg = new byte[8];
        byte prg0 = 0;
        byte prg1 = 1;
        ushort vb0 = 0;
        ushort vb1 = 0;
        byte chr01 = 0;
        byte chr23 = 2;
        byte chr4 = 4;
        byte chr5 = 5;
        byte chr6 = 6;
        byte chr7 = 7;
        byte irq_counter = 0;
        byte irq_latch = 0xFF;

        public override void Poke(int addr, byte data)
        {
            if (addr > 0x4100 && addr < 0x6000)
            {
                vb0 = (ushort)((data & 0x01) << 8);
                vb1 = (ushort)((data & 0x10) << 4);
                SetBank_PPU();
            }
            else
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
                        irq_latch = data;
                        break;
                    case 0xC001:
                        reg[5] = data;
                        if (ppu.scanline >= 0 &
                            ppu.scanline <= 239)
                        {
                            irq_counter |= 0x80;
                        }
                        else
                        {
                            irq_counter |= 0x80;
                        }
                        break;
                    case 0xE000:
                        reg[6] = data;

                        cpu.IRQ(NesCpu.IsrType.External, false);
                        break;
                    case 0xE001:
                        reg[7] = data;
                        break;
                }
            }
            base.Poke(addr, data);
        }
        public override byte Peek(int addr)
        {
            return 0x01;
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x4018, 0x7FFF, Peek, Poke);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(256);
            SetBank_CPU();
            SetBank_PPU();
            base.Initialize(initializing);
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
                    cpuMemory.Switch1kChrRom(vb0 + chr4, 0);
                    cpuMemory.Switch1kChrRom(vb0 + chr5, 1);
                    cpuMemory.Switch1kChrRom(vb0 + chr6, 2);
                    cpuMemory.Switch1kChrRom(vb0 + chr7, 3);
                    cpuMemory.Switch1kChrRom(vb1 + chr01, 4);
                    cpuMemory.Switch1kChrRom(vb1 + chr01 + 1, 5);
                    cpuMemory.Switch1kChrRom(vb1 + chr23, 6);
                    cpuMemory.Switch1kChrRom(vb1 + chr23 + 1, 7);
                }
                else
                {
                    cpuMemory.Switch1kChrRom(vb0 + chr01, 0);
                    cpuMemory.Switch1kChrRom(vb0 + chr01 + 1, 1);
                    cpuMemory.Switch1kChrRom(vb0 + chr23, 2);
                    cpuMemory.Switch1kChrRom(vb0 + chr23 + 1, 3);
                    cpuMemory.Switch1kChrRom(vb1 + chr4, 4);
                    cpuMemory.Switch1kChrRom(vb1 + chr5, 5);
                    cpuMemory.Switch1kChrRom(vb1 + chr6, 6);
                    cpuMemory.Switch1kChrRom(vb1 + chr7, 7);
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
    }
}
