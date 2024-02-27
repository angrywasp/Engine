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
    class Mapper114 : Mapper
    {
        byte irq_counter = 0;
        byte irq_occur = 0;
        byte reg_a = 0;
        byte[] reg_b = new byte[8];
        byte reg_c = 0;
        byte reg_m = 0;
        public Mapper114(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int Address, byte data)
        {
            if (Address < 0x8000)
            {
                reg_m = data;
                SetBank_CPU();
            }
            else
            {
                if (Address == 0xE003)
                {
                    irq_counter = data;
                }
                else
                    if (Address == 0xE002)
                    {
                        irq_occur = 0;
                        cpu.IRQ(NesCpu.IsrType.External, false);
                    }
                    else
                    {
                        switch (Address & 0xE000)
                        {
                            case 0x8000:
                                if ((data & 0x01) == 0x01)
                                    cartridge.Mirroring = Mirroring.ModeHorz;
                                else
                                    cartridge.Mirroring = Mirroring.ModeVert;
                                break;
                            case 0xA000:
                                reg_c = 1;
                                reg_a = data;
                                break;
                            case 0xC000:
                                if (reg_c == 0)
                                {
                                    break;
                                }
                                reg_b[reg_a & 0x07] = data;
                                switch (reg_a & 0x07)
                                {
                                    case 0:
                                    case 1:
                                    case 2:
                                    case 3:
                                    case 6:
                                    case 7:
                                        SetBank_PPU();
                                        break;
                                    case 4:
                                    case 5:
                                        SetBank_CPU();
                                        break;
                                }
                                reg_c = 0;
                                break;
                        }
                    }
            }
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x4018, 0x7FFF, Poke);
            cpuMemory.Switch16kPrgRom(0, 0);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
        }
        public override void TickScanlineTimer()
        {
            if (irq_counter != 0)
            {
                irq_counter--;
                if (irq_counter == 0)
                {
                    irq_occur = 0xFF;
                    cpu.IRQ(NesCpu.IsrType.External, true);
                }
            }
        }
        void SetBank_CPU()
        {
            if ((reg_m & 0x80) == 0x80)
            {
                cpuMemory.Switch16kPrgRom((reg_m & 0x1F) * 4, 0);
            }
            else
            {
                cpuMemory.Switch8kPrgRom(reg_b[4] * 2, 0);
                cpuMemory.Switch8kPrgRom(reg_b[5] * 2, 1);
            }
        }
        void SetBank_PPU()
        {
            cpuMemory.Switch2kChrRom((reg_b[0] >> 1) * 2, 0);
            cpuMemory.Switch2kChrRom((reg_b[2] >> 1) * 2, 1);
            cpuMemory.Switch1kChrRom(reg_b[6], 4);
            cpuMemory.Switch1kChrRom(reg_b[1], 5);
            cpuMemory.Switch1kChrRom(reg_b[7], 6);
            cpuMemory.Switch1kChrRom(reg_b[3], 7);
        }
        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(irq_counter);
            stateStream.Write(irq_occur);
            stateStream.Write(reg_a);
            stateStream.Write(reg_b);
            stateStream.Write(reg_c);
            stateStream.Write(reg_m);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            irq_counter = stateStream.ReadByte();
            irq_occur = stateStream.ReadByte();
            reg_a = stateStream.ReadByte();
            stateStream.Read(reg_b);
            reg_c = stateStream.ReadByte();
            reg_m = stateStream.ReadByte();
            base.LoadState(stateStream);
        }
    }
}
