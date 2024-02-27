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

    class Mapper44 : Mapper
    {
        byte[] reg = new byte[8];
        int bank = 0;
        int prg0 = 0;
        int prg1 = 1;

        int chr01 = 0;
        int chr23 = 2;
        int chr4 = 4;
        int chr5 = 5;
        int chr6 = 6;
        int chr7 = 7;

        int irq_enable = 0;
        int irq_counter = 0;
        int irq_latch = 0;

        public Mapper44(NesSystem nesSystem)
            : base(nesSystem) { }

        public override void Poke(int address, byte data)
        {
            if (address == 0x6000)
            {
                bank = (data & 0x06) >> 1;
                SetPRG();
                SetCHR();
            }
            switch (address & 0xE001)
            {
                case 0x8000:
                    reg[0] = data;
                    SetPRG();
                    SetCHR();
                    break;
                case 0x8001:
                    reg[1] = data;
                    switch (reg[0] & 0x07)
                    {
                        case 0x00:
                            chr01 = data & 0xFE;
                            SetCHR();
                            break;
                        case 0x01:
                            chr23 = data & 0xFE;
                            SetCHR();
                            break;
                        case 0x02:
                            chr4 = data;
                            SetCHR();
                            break;
                        case 0x03:
                            chr5 = data;
                            SetCHR();
                            break;
                        case 0x04:
                            chr6 = data;
                            SetCHR();
                            break;
                        case 0x05:
                            chr7 = data;
                            SetCHR();
                            break;
                        case 0x06:
                            prg0 = data;
                            SetPRG();
                            break;
                        case 0x07:
                            prg1 = data;
                            SetPRG();
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
                    bank = data & 0x07;
                    if (bank == 7)
                    {
                        bank = 6;
                    }
                    SetPRG();
                    SetCHR();
                    break;
                case 0xC000:
                    reg[4] = data;
                    irq_counter = data;
                    break;
                case 0xC001:
                    reg[5] = data;
                    irq_latch = data;
                    break;
                case 0xE000:
                    reg[6] = data;
                    irq_enable = 0;
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
                case 0xE001:
                    reg[7] = data;
                    irq_enable = 1;
                    break;
            }
        }
        protected override void Initialize(bool initializing)
        {
            if (cartridge.HasCharRam)
            {
                chr01 = chr23 = chr4 = chr5 = chr6 = chr7 = 0;
                cpuMemory.FillChr(16);
            }
            cpuMemory.Map(0x6000, 0x7FFF, Peek, Poke);
            SetPRG();
            SetCHR();
        }
        public override void TickScanlineTimer()
        {
            if (irq_enable != 0)
            {
                if ((--irq_counter) == 0)
                {
                    irq_counter = irq_latch;
                    cpu.IRQ(NesCpu.IsrType.External, true);
                }
            }
        }

        void SetPRG()
        {
            if ((reg[0] & 0x40) == 0x40)
            {
                cpuMemory.Switch8kPrgRom((((bank == 6) ? 0x1e : 0x0e) | (bank << 4)) * 2, 0);
                cpuMemory.Switch8kPrgRom((((bank == 6) ? 0x1f & prg1 : 0x0f & prg1) | (bank << 4)) * 2, 1);
                cpuMemory.Switch8kPrgRom((((bank == 6) ? 0x1f & prg0 : 0x0f & prg0) | (bank << 4)) * 2, 2);
                cpuMemory.Switch8kPrgRom((((bank == 6) ? 0x1f : 0x0f) | (bank << 4)) * 2, 3);
            }
            else
            {
                cpuMemory.Switch8kPrgRom((((bank == 6) ? 0x1f & prg0 : 0x0f & prg0) | (bank << 4)) * 2, 0);
                cpuMemory.Switch8kPrgRom((((bank == 6) ? 0x1f & prg1 : 0x0f & prg1) | (bank << 4)) * 2, 1);
                cpuMemory.Switch8kPrgRom((((bank == 6) ? 0x1e : 0x0e) | (bank << 4)) * 2, 2);
                cpuMemory.Switch8kPrgRom((((bank == 6) ? 0x1f : 0x0f) | (bank << 4)) * 2, 3);
            }
        }
        void SetCHR()
        {
            if ((reg[0] & 0x80) == 0x80)
            {
                cpuMemory.Switch1kChrRom(((bank == 6) ? 0xff & chr4 : 0x7f & chr4) | (bank << 7), 0);
                cpuMemory.Switch1kChrRom(((bank == 6) ? 0xff & chr5 : 0x7f & chr5) | (bank << 7), 1);
                cpuMemory.Switch1kChrRom(((bank == 6) ? 0xff & chr6 : 0x7f & chr6) | (bank << 7), 2);
                cpuMemory.Switch1kChrRom(((bank == 6) ? 0xff & chr7 : 0x7f & chr7) | (bank << 7), 3);
                cpuMemory.Switch1kChrRom(((bank == 6) ? 0xff & chr01 : 0x7f & chr01) | (bank << 7), 4);
                cpuMemory.Switch1kChrRom(((bank == 6) ? 0xff & (chr01 + 1) : 0x7f & (chr01 + 1)) | (bank << 7), 5);
                cpuMemory.Switch1kChrRom(((bank == 6) ? 0xff & chr23 : 0x7f & chr23) | (bank << 7), 6);
                cpuMemory.Switch1kChrRom(((bank == 6) ? 0xff & (chr23 + 1) : 0x7f & (chr23 + 1)) | (bank << 7), 7);
            }
            else
            {
                cpuMemory.Switch1kChrRom(((bank == 6) ? 0xff & chr01 : 0x7f & chr01) | (bank << 7), 0);
                cpuMemory.Switch1kChrRom(((bank == 6) ? 0xff & (chr01 + 1) : 0x7f & (chr01 + 1)) | (bank << 7), 1);
                cpuMemory.Switch1kChrRom(((bank == 6) ? 0xff & chr23 : 0x7f & chr23) | (bank << 7), 2);
                cpuMemory.Switch1kChrRom(((bank == 6) ? 0xff & (chr23 + 1) : 0x7f & (chr23 + 1)) | (bank << 7), 3);
                cpuMemory.Switch1kChrRom(((bank == 6) ? 0xff & chr4 : 0x7f & chr4) | (bank << 7), 4);
                cpuMemory.Switch1kChrRom(((bank == 6) ? 0xff & chr5 : 0x7f & chr5) | (bank << 7), 5);
                cpuMemory.Switch1kChrRom(((bank == 6) ? 0xff & chr6 : 0x7f & chr6) | (bank << 7), 6);
                cpuMemory.Switch1kChrRom(((bank == 6) ? 0xff & chr7 : 0x7f & chr7) | (bank << 7), 7);
            }
        }

        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(reg);
            stateStream.Write(bank);
            stateStream.Write(prg0);
            stateStream.Write(prg1);
            stateStream.Write(chr01);
            stateStream.Write(chr23);
            stateStream.Write(chr4);
            stateStream.Write(chr5);
            stateStream.Write(chr6);
            stateStream.Write(chr7);

            stateStream.Write(irq_enable);
            stateStream.Write(irq_counter);
            stateStream.Write(irq_latch);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            stateStream.Read(reg);
            bank = stateStream.ReadInt32();
            prg0 = stateStream.ReadInt32();
            prg1 = stateStream.ReadInt32();
            chr01 = stateStream.ReadInt32();
            chr23 = stateStream.ReadInt32();
            chr4 = stateStream.ReadInt32();
            chr5 = stateStream.ReadInt32();
            chr6 = stateStream.ReadInt32();
            chr7 = stateStream.ReadInt32();

            irq_enable = stateStream.ReadInt32();
            irq_counter = stateStream.ReadInt32();
            irq_latch = stateStream.ReadInt32();
            base.LoadState(stateStream);
        }
    }
}
