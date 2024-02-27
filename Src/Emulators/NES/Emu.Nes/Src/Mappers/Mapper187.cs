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
    class Mapper187 : Mapper
    {
        int[] chr = new int[8];
        byte[] bank = new byte[8];
        byte[] prg = new byte[4];
        byte ext_mode = 0;
        byte chr_mode = 0;
        byte ext_enable = 0;

        byte irq_enable = 0;
        byte irq_counter = 0;
        byte irq_latch = 0;
        byte irq_occur = 0;

        byte last_write = 0;
        public Mapper187(NesSystem nes)
            : base(nes)
        { }
        public override void Poke(int addr, byte data)
        {
            if (addr < 0x8000)
            {
                last_write = data;
                if (addr == 0x5000)
                {
                    ext_mode = data;
                    if ((data & 0x80) == 0x80)
                    {
                        if ((data & 0x20) == 0x20)
                        {
                            prg[0] = (byte)(((data & 0x1E) << 1) + 0);
                            prg[1] = (byte)(((data & 0x1E) << 1) + 1);
                            prg[2] = (byte)(((data & 0x1E) << 1) + 2);
                            prg[3] = (byte)(((data & 0x1E) << 1) + 3);
                        }
                        else
                        {
                            prg[2] = (byte)(((data & 0x1F) << 1) + 0);
                            prg[3] = (byte)(((data & 0x1F) << 1) + 1);
                        }
                    }
                    else
                    {
                        prg[0] = bank[6];
                        prg[1] = bank[7];
                        prg[2] = (byte)(cartridge.PRGSizeInKB - 2);
                        prg[3] = (byte)(cartridge.PRGSizeInKB - 1);
                    }
                    SetBank_CPU();
                }
            }
            else
            {
                last_write = data;
                switch (addr)
                {
                    case 0x8003:
                        ext_enable = 0xFF;
                        chr_mode = data;
                        if ((data & 0xF0) == 0)
                        {
                            prg[2] = (byte)(cartridge.PRGSizeInKB - 2);
                            SetBank_CPU();
                        }
                        break;

                    case 0x8000:
                        ext_enable = 0;
                        chr_mode = data;
                        break;

                    case 0x8001:
                        if (ext_enable == 0)
                        {
                            switch (chr_mode & 7)
                            {
                                case 0:
                                    data &= 0xFE;
                                    chr[4] = data + 0x100;
                                    chr[5] = data + 0x100 + 1;
                                    SetBank_PPU();
                                    break;
                                case 1:
                                    data &= 0xFE;
                                    chr[6] = data + 0x100;
                                    chr[7] = data + 0x100 + 1;
                                    SetBank_PPU();
                                    break;
                                case 2:
                                    chr[0] = data;
                                    SetBank_PPU();
                                    break;
                                case 3:
                                    chr[1] = data;
                                    SetBank_PPU();
                                    break;
                                case 4:
                                    chr[2] = data;
                                    SetBank_PPU();
                                    break;
                                case 5:
                                    chr[3] = data;
                                    SetBank_PPU();
                                    break;
                                case 6:
                                    if ((ext_mode & 0xA0) != 0xA0)
                                    {
                                        prg[0] = data;
                                        SetBank_CPU();
                                    }
                                    break;
                                case 7:
                                    if ((ext_mode & 0xA0) != 0xA0)
                                    {
                                        prg[1] = data;
                                        SetBank_CPU();
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            switch (chr_mode)
                            {
                                case 0x2A:
                                    prg[1] = 0x0F;
                                    break;
                                case 0x28:
                                    prg[2] = 0x17;
                                    break;
                                case 0x26:
                                    break;
                                default:
                                    break;
                            }
                            SetBank_CPU();
                        }
                        bank[chr_mode & 7] = data;
                        break;

                    case 0xA000:
                        if ((data & 0x01) == 0x01)
                        {
                            cartridge.Mirroring = Mirroring.ModeHorz;
                        }
                        else
                        {
                            cartridge.Mirroring = Mirroring.ModeVert;
                        }
                        break;
                    case 0xA001:
                        break;

                    case 0xC000:
                        irq_counter = data;
                        irq_occur = 0;
                        cpu.IRQ(NesCpu.IsrType.External, false);
                        break;
                    case 0xC001:
                        irq_latch = data;
                        irq_occur = 0;
                        cpu.IRQ(NesCpu.IsrType.External, false);
                        break;
                    case 0xE000:
                    case 0xE002:
                        irq_enable = 0;
                        irq_occur = 0;
                        cpu.IRQ(NesCpu.IsrType.External, false);
                        break;
                    case 0xE001:
                    case 0xE003:
                        irq_enable = 1;
                        irq_occur = 0;
                        cpu.IRQ(NesCpu.IsrType.External, false);
                        break;
                }
            }
            base.Poke(addr, data);
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x4018, 0x7FFF, Poke);
            cpuMemory.Switch32kPrgRom((cartridge.PrgPages - 1) * 4 - 4);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
            prg[0] = (byte)(cartridge.PRGSizeInKB - 4);
            prg[1] = (byte)(cartridge.PRGSizeInKB - 3);
            prg[2] = (byte)(cartridge.PRGSizeInKB - 2);
            prg[3] = (byte)(cartridge.PRGSizeInKB - 1);
            base.Initialize(initializing);
        }
        void SetBank_CPU()
        {
            cpuMemory.Switch8kPrgRom(prg[0] * 2, 0);
            cpuMemory.Switch8kPrgRom(prg[1] * 2, 1);
            cpuMemory.Switch8kPrgRom(prg[2] * 2, 2);
            cpuMemory.Switch8kPrgRom(prg[3] * 2, 3);
        }
        void SetBank_PPU()
        {
            cpuMemory.Switch1kChrRom(chr[0], 0);
            cpuMemory.Switch1kChrRom(chr[1], 1);
            cpuMemory.Switch1kChrRom(chr[2], 2);
            cpuMemory.Switch1kChrRom(chr[3], 3);
            cpuMemory.Switch1kChrRom(chr[4], 4);
            cpuMemory.Switch1kChrRom(chr[5], 5);
            cpuMemory.Switch1kChrRom(chr[6], 6);
            cpuMemory.Switch1kChrRom(chr[7], 7);
        }
        public override void TickScanlineTimer()
        {
            if (irq_enable != 0)
            {
                if (irq_counter == 0)
                {
                    irq_counter--;
                    irq_enable = 0;
                    irq_occur = 0xFF;
                    cpu.IRQ(NesCpu.IsrType.External, true);
                }
                else
                {
                    irq_counter--;
                }
            }
            base.TickScanlineTimer();
        }
        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(chr);
            stateStream.Write(bank);
            stateStream.Write(prg);
            stateStream.Write(ext_mode);
            stateStream.Write(chr_mode);
            stateStream.Write(ext_enable);
            stateStream.Write(irq_enable);
            stateStream.Write(irq_counter);
            stateStream.Write(irq_latch);
            stateStream.Write(irq_occur);
            stateStream.Write(last_write);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            stateStream.Read(chr);
            stateStream.Read(bank);
            stateStream.Read(prg);
            ext_mode = stateStream.ReadByte();
            chr_mode = stateStream.ReadByte();
            ext_enable = stateStream.ReadByte();
            irq_enable = stateStream.ReadByte();
            irq_counter = stateStream.ReadByte();
            irq_latch = stateStream.ReadByte();
            irq_occur = stateStream.ReadByte();
            last_write = stateStream.ReadByte();
            base.LoadState(stateStream);
        }
    }
}
