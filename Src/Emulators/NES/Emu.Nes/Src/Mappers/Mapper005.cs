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
/*Re-writen for version 4 by Ala Hadid at : Tuesday, October 25, 2011*/
namespace MyNes.Nes
{
    class Mapper05 : Mapper
    {
        public Mapper05(NesSystem nes)
            : base(nes)
        { }
        public override string Name
        {
            get
            {
                return "Nintendo MMC5";
            }
        }
        const byte EXRAMTABLE = 2;
        const byte FILLMODETABLE = 3;
        int EXRAMMODE = 0;
        //mapping
        int[] BGChrBank = new int[0x08];
        int[] EXChrBank = new int[0x08];
        byte[][] SRAM = new byte[8][];
        int sramPage = 0;

        void Switch01kBGChrRom(int start, int area)
        {
            start = (start & (cartridge.Chr.Length - 1));

            BGChrBank[area] = (start);
        }
        void Switch02kBGChrRom(int start, int area)
        {
            start = (start & (cartridge.Chr.Length - 1));

            for (int i = 0; i < 2; i++)
                BGChrBank[2 * area + i] = (start++);
        }
        void Switch04kBGChrRom(int start, int area)
        {
            start = (start & (cartridge.Chr.Length - 1));

            for (int i = 0; i < 4; i++)
                BGChrBank[4 * area + i] = (start++);
        }
        void Switch08kBGChrRom(int start)
        {
            start = (start & (cartridge.Chr.Length - 1));

            for (int i = 0; i < 8; i++)
                BGChrBank[i] = (start++);
        }
        void Switch04kEXChrRom(int start, int area)
        {
            start = (start & (cartridge.Chr.Length - 1));

            for (int i = 0; i < 4; i++)
                EXChrBank[4 * area + i] = (start++);
        }
        void Switch04kCHRExtra(int address, int bank)
        {
            if (address < 0x1000)
                Switch04kEXChrRom(bank * 4, 0);
            else
                Switch04kEXChrRom(bank * 4, 1);
        }

        byte prgBankSize = 3;
        byte chrBankSize = 3;
        byte PRGRAMprotect1 = 0;
        byte PRGRAMprotect2 = 0;
        byte ExtendedRAMmode = 0;
        int IRQStatus = 0;
        int irq_scanline = 0;
        int irq_line = 0;
        int irq_clear = 0;
        int irq_enable = 0;

        byte Multiplier_A = 0;
        byte Multiplier_B = 0;

        int split_scroll = 0;
        int split_control = 0;
        int split_page = 0;
        int lastAccessVRAM = 0;

        public override void Poke(int addr, byte data)
        {
            switch (addr)
            {
                //AUDIO
                case 0x5000: ((Mmc5ExternalComponent)apu.External).ChannelSq1.Poke1(0x5000, data); break;
                //case 0x5001:
                case 0x5002: ((Mmc5ExternalComponent)apu.External).ChannelSq1.Poke3(0x5002, data); break;
                case 0x5003: ((Mmc5ExternalComponent)apu.External).ChannelSq1.Poke4(0x5003, data); break;
                case 0x5004: ((Mmc5ExternalComponent)apu.External).ChannelSq2.Poke1(0x5004, data); break;
                //case 0x5005:
                case 0x5006: ((Mmc5ExternalComponent)apu.External).ChannelSq2.Poke3(0x5006, data); break;
                case 0x5007: ((Mmc5ExternalComponent)apu.External).ChannelSq2.Poke4(0x5007, data); break;
                //     case 0x5010: break;//??!!
                case 0x5011: ((Mmc5ExternalComponent)apu.External).ChannelPcm.Poke1(0x5011, data); break;
                case 0x5015: apu.Poke5015(data); break;
                //PRG mode ($5100) : 0 = 32k, 1 = 16k, 2 = 16k/8k, 3 = 8k
                case 0x5100: prgBankSize = (byte)(data & 0x3); break;
                //CHR mode ($5101) : 0 = 8k, 1 = 4k, 2 = 2k, 3 = 1k
                case 0x5101: chrBankSize = (byte)(data & 0x3); break;
                //PRG RAM Protect  ?
                case 0x5102: PRGRAMprotect1 = (byte)(data & 0x3); break;
                case 0x5103: PRGRAMprotect2 = (byte)(data & 0x3); break;
                //Extended RAM mode 
                case 0x5104:
                    ExtendedRAMmode = (byte)(data & 0x3);
                    EXRAMMODE = data & 0x3;
                    break;
                //Nametable mapping
                case 0x5105:
                    ppuMemory.SwitchNmt(data);
                    break;
                //Fill-mode tile
                case 0x5106:
                    for (int i = 0; i < 0x3C0; i++)
                        ppuMemory.nmt[FILLMODETABLE][i] = data;
                    break;
                //Fill-mode color
                case 0x5107:
                    for (int i = 0x3C0; i < (0x3C0 + 0x40); i++)
                    {
                        byte value = (byte)((2 << (data & 0x03)) | (data & 0x03));
                        value |= (byte)((value & 0x0F) << 4);
                        ppuMemory.nmt[FILLMODETABLE][i] = value;
                    }
                    break;
                //PRG RAM bank
                case 0x5113:
                    sramPage = data & 0x7;
                    break;
                //PRG bank 0
                case 0x5114:
                    if ((data & 0x80) == 0x80)
                    {
                        if (prgBankSize == 3)
                            cpuMemory.Switch8kPrgRom((data & 0x7F) * 2, 0);
                    }
                    else if (prgBankSize == 3)
                    {
                        //cpuMemory.Switch8kSRAMToPRG((data & 0x07) * 2, 0);
                    }

                    break;
                //PRG bank 1
                case 0x5115:
                    if ((data & 0x80) == 0x80)
                    {
                        if (prgBankSize == 1)
                            cpuMemory.Switch16kPrgRom(((data & 0x7E) >> 1) * 4, 0);
                        else if (prgBankSize == 2)
                            cpuMemory.Switch16kPrgRom(((data & 0x7E) >> 1) * 4, 0);
                        else if (prgBankSize == 3)
                            cpuMemory.Switch8kPrgRom((data & 0x7F) * 2, 1);
                    }
                    else
                    {
                        if (prgBankSize == 1 || prgBankSize == 2)
                        {
                            //cpuMemory.Switch8kSRAMToPRG(((data & 0x06) + 0) * 2, 0);
                            //cpuMemory.Switch8kSRAMToPRG(((data & 0x06) + 1) * 2, 1);
                        }
                        else if (prgBankSize == 3)
                        {
                            //map.Switch8kSRAMToPRG((data & 0x07) * 2, 1);
                        }
                    }
                    break;
                //PRG bank 2 
                case 0x5116:
                    if ((data & 0x80) == 0x80)
                    {
                        if (prgBankSize == 2)
                            cpuMemory.Switch8kPrgRom((data & 0x7f) * 2, 2);
                        else if (prgBankSize == 3)
                            cpuMemory.Switch8kPrgRom((data & 0x7f) * 2, 2);
                    }
                    else if (prgBankSize == 2 || prgBankSize == 3)
                    {
                        //cpuMemory.Switch8kSRAMToPRG((data & 0x07) * 2, 1);
                    }
                    break;
                //PRG bank 3
                case 0x5117:
                    if (prgBankSize == 0)
                        cpuMemory.Switch32kPrgRom(((data & 0x7C) >> 2) * 8);
                    else if (prgBankSize == 1)
                        cpuMemory.Switch16kPrgRom(((data & 0x7E) >> 1) * 4, 1);
                    else if (prgBankSize == 2)
                        cpuMemory.Switch8kPrgRom((data & 0x7f) * 2, 3);
                    else if (prgBankSize == 3)
                        cpuMemory.Switch8kPrgRom((data & 0x7f) * 2, 3);
                    break;
                //Sprite CHR bank 0
                case 0x5120:
                    if (chrBankSize == 3)
                        cpuMemory.Switch1kChrRom(data, 0);
                    break;
                //Sprite CHR bank 1
                case 0x5121:
                    if (chrBankSize == 3)
                        cpuMemory.Switch1kChrRom(data, 1);
                    else if (chrBankSize == 2)
                        cpuMemory.Switch2kChrRom(data * 2, 0);
                    break;
                //Sprite CHR bank 2
                case 0x5122:
                    if (chrBankSize == 3)
                        cpuMemory.Switch1kChrRom(data, 2);
                    break;
                //Sprite CHR bank 3
                case 0x5123:
                    if (chrBankSize == 3)
                        cpuMemory.Switch1kChrRom(data, 3);
                    else if (chrBankSize == 2)
                        cpuMemory.Switch2kChrRom(data * 2, 1);
                    else if (chrBankSize == 1)
                        cpuMemory.Switch4kChrRom(data * 4, 0);
                    break;
                //Sprite CHR bank 4
                case 0x5124:
                    if (chrBankSize == 3)
                        cpuMemory.Switch1kChrRom(data, 4);
                    break;
                //Sprite CHR bank 5
                case 0x5125:
                    if (chrBankSize == 3)
                        cpuMemory.Switch1kChrRom(data, 5);
                    else if (chrBankSize == 2)
                        cpuMemory.Switch2kChrRom(data * 2, 2);
                    break;
                //Sprite CHR bank 6
                case 0x5126:
                    if (chrBankSize == 3)
                        cpuMemory.Switch1kChrRom(data, 6);
                    break;
                //Sprite CHR bank 7
                case 0x5127:
                    if (chrBankSize == 3)
                        cpuMemory.Switch1kChrRom(data, 7);
                    else if (chrBankSize == 2)
                        cpuMemory.Switch2kChrRom(data * 2, 3);
                    else if (chrBankSize == 1)
                        cpuMemory.Switch4kChrRom(data * 4, 1);
                    else if (chrBankSize == 0)
                        cpuMemory.Switch8kChrRom(data * 8);
                    break;

                //Background CHR bank 0
                case 0x5128:
                    Switch01kBGChrRom(data, 0);
                    Switch01kBGChrRom(data, 4);
                    break;
                //Background CHR bank 1
                case 0x5129:
                    if (chrBankSize == 3)
                    {
                        Switch01kBGChrRom(data, 1);
                        Switch01kBGChrRom(data, 5);
                    }
                    else if (chrBankSize == 2)
                    {
                        Switch02kBGChrRom(data * 2, 0);
                        Switch02kBGChrRom(data * 2, 2);
                    }
                    break;
                //Background CHR bank 2
                case 0x512A:
                    if (chrBankSize == 3)
                    {
                        Switch01kBGChrRom(data, 2);
                        Switch01kBGChrRom(data, 6);
                    }
                    break;
                //Background CHR bank 2
                case 0x512B:
                    if (chrBankSize == 3)
                    {
                        Switch01kBGChrRom(data, 3);
                        Switch01kBGChrRom(data, 7);
                    }
                    else if (chrBankSize == 2)
                    {
                        Switch02kBGChrRom(data * 2, 1);
                        Switch02kBGChrRom(data * 2, 3);
                    }
                    else if (chrBankSize == 1)
                    {
                        Switch04kBGChrRom(data * 4, 0);
                        Switch04kBGChrRom(data * 4, 1);
                    }
                    else if (chrBankSize == 0)
                    {
                        Switch08kBGChrRom(data * 8);
                    }
                    break;
                //Vertical Split Mode
                case 0x5200:
                    split_control = data;
                    break;
                //Vertical Split Scroll  
                case 0x5201:
                    split_scroll = data;
                    break;
                //Vertical Split Bank
                case 0x5202:
                    split_page = data & 0x3F;
                    break;
                //IRQ Counter
                case 0x5203: irq_line = data; cpu.IRQ(NesCpu.IsrType.External, false); break;
                //IRQ Status 
                case 0x5204: irq_enable = data; cpu.IRQ(NesCpu.IsrType.External, false); break;
                //Multiplier a
                case 0x5205: Multiplier_A = data; break;
                //Multiplier b
                case 0x5206: Multiplier_B = data; break;

                default:
                    if (addr >= 0x5C00 && addr <= 0x5FFF)
                    {
                        if (ExtendedRAMmode == 2)
                        {
                            ppuMemory.nmt[EXRAMTABLE][(ushort)(addr & 0x3FF)] = data;
                        }
                        else if (ExtendedRAMmode != 3)
                        {
                            if ((IRQStatus & 0x40) == 0x40)
                            {
                                ppuMemory.nmt[EXRAMTABLE][(ushort)(addr & 0x3FF)] = data;
                            }
                            else
                            {
                                ppuMemory.nmt[EXRAMTABLE][(ushort)(addr & 0x3FF)] = 0;
                            }
                        }
                    }
                    else if (addr >= 0x6000 && addr <= 0x7FFF)
                    {
                        if ((PRGRAMprotect1 == 0x02) && (PRGRAMprotect2 == 0x01))
                            SRAM[sramPage][addr & 0x1FFF] = data;
                    }
                    break;
            }
            base.Poke(addr, data);
        }
        public override byte Peek(int addr)
        {
            byte data = 0;
            switch (addr)
            {
                case 0x5015:
                    data = apu.Peek5015();
                    break;
                case 0x5204:
                    data = (byte)IRQStatus;
                    IRQStatus &= ~0x80;
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
                case 0x5205:
                    data = (byte)(Multiplier_A * Multiplier_B);
                    break;
                case 0x5206:
                    data = (byte)((Multiplier_A * Multiplier_B) >> 8);
                    break;
            }
            if (addr >= 0x5C00 && addr <= 0x5FFF)
            {
                if (ExtendedRAMmode >= 2)
                {
                    data = ppuMemory.nmt[EXRAMTABLE][(ushort)(addr & 0x3FF)];
                }
            }
            else if (addr >= 0x6000 & addr < 0x8000)
            {
                data = SRAM[sramPage][addr & 0x1FFF];
            }
            return data;
        }
        public override void TickScanlineTimer()
        {
            if (ppu.drawBkg | ppu.drawObj)
            {
                if (ppu.scanline > 0 & ppu.scanline <= 239)
                {
                    irq_scanline++;
                    IRQStatus |= 0x40;
                    irq_clear = 0;
                }
            }

            if (irq_scanline == irq_line)
            {
                IRQStatus |= 0x80;
            }

            if (++irq_clear > 2)
            {
                irq_scanline = 0;
                IRQStatus &= ~0x80;
                IRQStatus &= ~0x40;

                cpu.IRQ(NesCpu.IsrType.External, false);
            }

            if ((irq_enable & 0x80) == 0x80 && (IRQStatus & 0x80) == 0x80 && (IRQStatus & 0x40) == 0x40)
                cpu.IRQ(NesCpu.IsrType.External, true);
            base.TickScanlineTimer();
        }
        protected override void Initialize(bool initializing)
        {
            if (initializing)
            {
                //map memory
                cpuMemory.Map(0x4018, 0x7FFF, Peek, Poke);
                ppuMemory.Map(0x0000, 0x1FFF, ChrPeek, ChrPoke);//make chr access done here
                ppuMemory.Map(0x2000, 0x3EFF, NmtPeek, NmtPoke);

                apu.External = new Mmc5ExternalComponent(nesSystem);
                cpuMemory.Switch8kPrgRom((cartridge.PrgPages * 4) - 2, 0);
                cpuMemory.Switch8kPrgRom((cartridge.PrgPages * 4) - 2, 1);
                cpuMemory.Switch8kPrgRom((cartridge.PrgPages * 4) - 2, 2);
                cpuMemory.Switch8kPrgRom((cartridge.PrgPages * 4) - 2, 3);

                for (int i = 0; i < 8; i++)
                    SRAM[i] = new byte[0x2000];

                if (cartridge.HasCharRam)
                    cpuMemory.FillChr(16);
                cpuMemory.Switch8kChrRom(0);
                Switch08kBGChrRom(0);
            }
            base.Initialize(initializing);
        }
        public override bool ScanlineTimerAlwaysActive
        {
            get
            {
                return true;
            }
        }

        private byte ChrPeek(int addr)
        {
            if (EXRAMMODE != 1)
            {
                if ((ppu.HClock < 256 | ppu.HClock >= 320) & ppu.oamSize)//bk fetch
                {
                    return cartridge.Chr[BGChrBank[addr >> 10 & 0x07]][addr & 0x03FF];
                }
                else
                    return cartridge.Chr[cpuMemory.ChrBank[addr >> 10 & 0x07]][addr & 0x03FF];
            }
            else
            {
                if ((ppu.HClock < 256 | ppu.HClock >= 320))//bk fetch
                {
                    int EXtilenumber = ppuMemory.nmt[EXRAMTABLE][lastAccessVRAM] & 0x3F;
                    Switch04kCHRExtra(addr, EXtilenumber);
                    return cartridge.Chr[EXChrBank[(addr & 0x1C00) >> 10]][addr & 0x3FF]; 
                }
                else//Sprites are not effected
                    return cartridge.Chr[cpuMemory.ChrBank[addr >> 10 & 0x07]][addr & 0x03FF];
            }
        }
        private void ChrPoke(int addr, byte data)
        {
            if (cartridge.HasCharRam)
            {
                if ((ppu.HClock < 256 | ppu.HClock >= 320))//bk write
                {
                    cartridge.Chr[BGChrBank[addr >> 10 & 0x07]][addr & 0x03FF] = data;
                }
                else
                    cartridge.Chr[cpuMemory.ChrBank[addr >> 10 & 0x07]][addr & 0x03FF] = data;
            }
        }
        private byte NmtPeek(int addr)
        {
            if (EXRAMMODE == 1)
            {
                if ((addr & 0x03FF) <= 0x3BF)
                {
                    lastAccessVRAM = addr & 0x03FF;
                }
                else
                {
                    int paletteNo = ppuMemory.nmt[EXRAMTABLE][lastAccessVRAM] & 0xC0;
                    int shift = ((lastAccessVRAM >> 4 & 0x04) | (lastAccessVRAM & 0x02));
                    switch (shift)
                    {
                        case 0: return (byte)(paletteNo >> 6);
                        case 2: return (byte)(paletteNo >> 4);
                        case 4: return (byte)(paletteNo >> 2);
                        case 6: return (byte)(paletteNo >> 0);
                    }
                }
            }
            return ppuMemory.nmt[ppuMemory.nmtBank[addr >> 10 & 0x03]][addr & 0x03FF];
        }
        private void NmtPoke(int addr, byte data)
        {
            if (EXRAMMODE == 1)
            {
                if ((addr & 0x03FF) <= 0x3BF)
                {
                    lastAccessVRAM = addr & 0x03FF;
                }
            }
            ppuMemory.nmt[ppuMemory.nmtBank[addr >> 10 & 0x03]][addr & 0x03FF] = data;
        }

        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(EXRAMMODE);
            stateStream.Write(BGChrBank);
            stateStream.Write(EXChrBank);
            for (int i = 0; i < 8; i++)
                stateStream.Write(SRAM[i]);
            stateStream.Write(sramPage);
            stateStream.Write(prgBankSize);
            stateStream.Write(chrBankSize);
            stateStream.Write(PRGRAMprotect1);
            stateStream.Write(PRGRAMprotect2);
            stateStream.Write(ExtendedRAMmode);
            stateStream.Write(IRQStatus);
            stateStream.Write(irq_scanline);
            stateStream.Write(irq_line);
            stateStream.Write(irq_clear);
            stateStream.Write(irq_enable);
            stateStream.Write(Multiplier_A);
            stateStream.Write(Multiplier_B);

            stateStream.Write(split_scroll);
            stateStream.Write(split_control);
            stateStream.Write(split_page);
            stateStream.Write(lastAccessVRAM);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            EXRAMMODE = stateStream.ReadInt32();
            stateStream.Read(BGChrBank);
            stateStream.Read(EXChrBank);
            for (int i = 0; i < 8; i++)
                stateStream.Read(SRAM[i]);
            sramPage = stateStream.ReadInt32();
            prgBankSize = stateStream.ReadByte();
            chrBankSize = stateStream.ReadByte();
            PRGRAMprotect1 = stateStream.ReadByte();
            PRGRAMprotect2 = stateStream.ReadByte();
            ExtendedRAMmode = stateStream.ReadByte();
            IRQStatus = stateStream.ReadInt32();
            irq_scanline = stateStream.ReadInt32();
            irq_line = stateStream.ReadInt32();
            irq_clear = stateStream.ReadInt32();
            irq_enable = stateStream.ReadInt32();
            Multiplier_A = stateStream.ReadByte();
            Multiplier_B = stateStream.ReadByte();
            split_scroll = stateStream.ReadInt32();
            split_control = stateStream.ReadInt32();
            split_page = stateStream.ReadInt32();
            lastAccessVRAM = stateStream.ReadInt32();
            base.LoadState(stateStream);
        }
    }
}
