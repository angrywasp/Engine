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
    class Mapper90 : Mapper
    {
        byte[] prg_reg = new byte[4];
        byte[] ntl_reg = new byte[4];
        byte[] nth_reg = new byte[4];
        byte[] chl_reg = new byte[8];
        byte[] chh_reg = new byte[8];
        byte irq_enable = 0;
        byte irq_counter = 0;
        byte irq_latch = 0;
        byte irq_preset = 0;
        byte irq_offset = 0;

        byte prg_6000 = 0;
        byte prg_E000 = 0;
        byte prg_size = 0;
        byte chr_size = 0;
        byte mir_mode = 0;
        byte mir_type = 0;

        byte key_val = 0;
        byte mul_val1, mul_val2 = 0;
        byte sw_val = 0;

        public Mapper90(NesSystem nes)
            : base(nes)
        { }
        protected override void Initialize(bool initializing)
        {
            ppuMemory.Map(0x2000, 0x3EFF, NmtPeek, NmtPoke);
            cpuMemory.Map(0x4018, 0x7FFF, Peek, Poke);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 2) * 4, 0);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);

            for (int i = 0; i < 4; i++)
            {
                prg_reg[i] = (byte)(cartridge.PRGSizeInKB - 4 + i);
                ntl_reg[i] = 0;
                nth_reg[i] = 0;
                chl_reg[i] = (byte)i;
                chh_reg[i] = 0;
                chl_reg[i + 4] = (byte)(i + 4);
                chh_reg[i + 4] = 0;
            }

            if (sw_val != 0)
                sw_val = 0x00;
            else
                sw_val = 0xFF;
            base.Initialize(initializing);
        }
        public override void Poke(int addr, byte data)
        {
            if (addr == 0x5800)
            {
                mul_val1 = data;
            }
            else if (addr == 0x5801)
            {
                mul_val2 = data;
            }
            else if (addr == 0x5803)
            {
                key_val = data;
            }
            else
                switch (addr & 0xF007)
                {
                    case 0x8000:
                    case 0x8001:
                    case 0x8002:
                    case 0x8003:
                        prg_reg[addr & 3] = data;
                        SetPRG();
                        break;

                    case 0x9000:
                    case 0x9001:
                    case 0x9002:
                    case 0x9003:
                    case 0x9004:
                    case 0x9005:
                    case 0x9006:
                    case 0x9007:
                        chl_reg[addr & 7] = data;
                        SetCHR();
                        break;

                    case 0xA000:
                    case 0xA001:
                    case 0xA002:
                    case 0xA003:
                    case 0xA004:
                    case 0xA005:
                    case 0xA006:
                    case 0xA007:
                        chh_reg[addr & 7] = data;
                        SetCHR();
                        break;

                    case 0xB000:
                    case 0xB001:
                    case 0xB002:
                    case 0xB003:
                        ntl_reg[addr & 3] = data;
                        SetVRAM();
                        break;

                    case 0xB004:
                    case 0xB005:
                    case 0xB006:
                    case 0xB007:
                        nth_reg[addr & 3] = data;
                        SetVRAM();
                        break;

                    case 0xC002:
                        irq_enable = 0;
                        cpu.IRQ(NesCpu.IsrType.External, false);
                        break;
                    case 0xC003:
                        irq_enable = 0xFF;
                        irq_preset = 0xFF;
                        break;
                    case 0xC004:
                        break;
                    case 0xC005:
                        if ((irq_offset & 0x80) != 0)
                        {
                            irq_latch = (byte)(data ^ (irq_offset | 1));
                        }
                        else
                        {
                            irq_latch = (byte)(data | (irq_offset & 0x27));
                        }
                        irq_preset = 0xFF;
                        break;
                    case 0xC006:
                        irq_offset = data;
                        break;

                    case 0xD000:
                        prg_6000 = (byte)(data & 0x80);
                        prg_E000 = (byte)(data & 0x04);
                        prg_size = (byte)(data & 0x03);
                        chr_size = (byte)((data & 0x18) >> 3);
                        mir_mode = (byte)(data & 0x20);
                        SetPRG();
                        SetCHR();
                        SetVRAM();
                        break;

                    case 0xD001:
                        mir_type = (byte)(data & 0x03);
                        SetVRAM();
                        break;

                    case 0xD003:
                        break;
                }
            base.Poke(addr, data);
        }
        public override byte Peek(int addr)
        {
            switch (addr)
            {
                case 0x5000:
                    return (byte)((sw_val != 0) ? 0x00 : 0xFF);
                case 0x5800:
                    return (byte)(mul_val1 * mul_val2);
                case 0x5801:
                    return (byte)((mul_val1 * mul_val2) >> 8);
                case 0x5803:
                    return key_val;
            }
            return base.Peek(addr);
        }
        void SetPRG()
        {
            if (prg_size == 0)
            {
                cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 2) * 4, 0);
                cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            }
            else if (prg_size == 1)
            {
                cpuMemory.Switch8kPrgRom((prg_reg[1] * 2) * 2, 0);
                cpuMemory.Switch8kPrgRom((prg_reg[1] * 2 + 1) * 2, 1);
                cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            }
            else if (prg_size == 2)
            {
                if (prg_E000 != 0)
                {
                    cpuMemory.Switch8kPrgRom(prg_reg[0] * 2, 0);
                    cpuMemory.Switch8kPrgRom(prg_reg[1] * 2, 1);
                    cpuMemory.Switch8kPrgRom(prg_reg[2] * 2, 2);
                    cpuMemory.Switch8kPrgRom(prg_reg[3] * 2, 3);
                }
                else
                {
                    if (prg_6000 != 0)
                    {
                        cpuMemory.Switch8kPrgRomToSRAM(prg_reg[3] * 2);
                    }
                    cpuMemory.Switch8kPrgRom(prg_reg[0] * 2, 0);
                    cpuMemory.Switch8kPrgRom(prg_reg[1] * 2, 1);
                    cpuMemory.Switch8kPrgRom(prg_reg[2] * 2, 2);
                    cpuMemory.Switch8kPrgRom(((cartridge.PrgPages * 2) - 1) * 2, 3);
                }
            }
            else
            {
                cpuMemory.Switch8kPrgRom(prg_reg[3] * 2, 0);
                cpuMemory.Switch8kPrgRom(prg_reg[2] * 2, 1);
                cpuMemory.Switch8kPrgRom(prg_reg[1] * 2, 2);
                cpuMemory.Switch8kPrgRom(prg_reg[0] * 2, 3);
            }
        }
        void SetCHR()
        {
            int[] bank = new int[8];

            for (int i = 0; i < 8; i++)
            {
                bank[i] = (chh_reg[i] << 8) | (chl_reg[i]);
            }

            if (chr_size == 0)
            {
                cpuMemory.Switch8kChrRom(bank[0] * 8);
            }
            else if (chr_size == 1)
            {
                cpuMemory.Switch4kChrRom(bank[0] * 4, 0);
                cpuMemory.Switch4kChrRom(bank[4] * 4, 1);
            }
            else if (chr_size == 2)
            {
                cpuMemory.Switch2kChrRom(bank[0] * 2, 0);
                cpuMemory.Switch2kChrRom(bank[2] * 2, 1);
                cpuMemory.Switch2kChrRom(bank[4] * 2, 2);
                cpuMemory.Switch2kChrRom(bank[6] * 2, 3);
            }
            else
            {
                cpuMemory.Switch1kChrRom(bank[0], 0);
                cpuMemory.Switch1kChrRom(bank[1], 1);
                cpuMemory.Switch1kChrRom(bank[2], 2);
                cpuMemory.Switch1kChrRom(bank[3], 3);
                cpuMemory.Switch1kChrRom(bank[4], 4);
                cpuMemory.Switch1kChrRom(bank[5], 5);
                cpuMemory.Switch1kChrRom(bank[6], 6);
                cpuMemory.Switch1kChrRom(bank[7], 7);
            }
        }
        void SetVRAM()
        {
            if (mir_type == 0)
            {
                cartridge.Mirroring = Mirroring.ModeVert;
            }
            else if (mir_type == 1)
            {
                cartridge.Mirroring = Mirroring.ModeHorz;
            }
            else
            {
                cartridge.Mirroring = Mirroring.Mode1ScA;
            }
        }

        public override void TickScanlineTimer()
        {
            if (irq_preset != 0)
            {
                irq_counter = irq_latch;
                irq_preset = 0;
            }
            if (irq_counter > 0)
            {
                irq_counter--;
            }
            if (irq_counter == 0)
            {
                if (irq_enable != 0)
                {
                    cpu.IRQ(NesCpu.IsrType.External, true);
                }
            }
            base.TickScanlineTimer();
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

        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(prg_reg);
            stateStream.Write(ntl_reg);
            stateStream.Write(nth_reg);
            stateStream.Write(chl_reg);
            stateStream.Write(chh_reg);
            stateStream.Write(irq_enable);
            stateStream.Write(irq_counter);
            stateStream.Write(irq_latch);
            stateStream.Write(irq_preset);
            stateStream.Write(irq_offset);
            stateStream.Write(prg_6000);
            stateStream.Write(prg_E000);
            stateStream.Write(prg_size);
            stateStream.Write(chr_size);
            stateStream.Write(mir_mode);
            stateStream.Write(mir_type);
            stateStream.Write(key_val);
            stateStream.Write(mul_val1);
            stateStream.Write(mul_val2);
            stateStream.Write(sw_val);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            stateStream.Read(prg_reg);
            stateStream.Read(ntl_reg);
            stateStream.Read(nth_reg);
            stateStream.Read(chl_reg);
            stateStream.Read(chh_reg);
            irq_enable = stateStream.ReadByte();
            irq_counter = stateStream.ReadByte();
            irq_latch = stateStream.ReadByte();
            irq_preset = stateStream.ReadByte();
            irq_offset = stateStream.ReadByte();
            prg_6000 = stateStream.ReadByte();
            prg_E000 = stateStream.ReadByte();
            prg_size = stateStream.ReadByte();
            chr_size = stateStream.ReadByte();
            mir_mode = stateStream.ReadByte();
            mir_type = stateStream.ReadByte();
            key_val = stateStream.ReadByte();
            mul_val1 = stateStream.ReadByte();
            mul_val2 = stateStream.ReadByte();
            sw_val = stateStream.ReadByte();
            base.LoadState(stateStream);
        }
    }
}
