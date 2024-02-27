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

    class Mapper85 : Mapper
    {
        byte irq_latch = 0;
        byte irq_enable = 0;
        byte irq_counter = 0;
        int irq_clock = 0;
        public Mapper85(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int address, byte data)
        {
            switch (address & 0xF038)
            {
                case 0x8000: cpuMemory.Switch8kPrgRom(data * 2, 0); break;
                case 0x8010:
                case 0x8008: cpuMemory.Switch8kPrgRom(data * 2, 1); break;
                case 0x9000: cpuMemory.Switch8kPrgRom(data * 2, 2); break;

                case 0x9010:
                case 0x9030:
                    break;
                case 0xA000: cpuMemory.Switch1kChrRom(data, 0); break;
                case 0xA008:
                case 0xA010: cpuMemory.Switch1kChrRom(data, 1); break;
                case 0xB000: cpuMemory.Switch1kChrRom(data, 2); break;
                case 0xB008:
                case 0xB010: cpuMemory.Switch1kChrRom(data, 3); break;
                case 0xC000: cpuMemory.Switch1kChrRom(data, 4); break;
                case 0xC008:
                case 0xC010: cpuMemory.Switch1kChrRom(data, 5); break;
                case 0xD000: cpuMemory.Switch1kChrRom(data, 6); break;
                case 0xD008:
                case 0xD010: cpuMemory.Switch1kChrRom(data, 7); break;

                case 0xE000:
                    data &= 0x03;
                    if (data == 0)
                        cartridge.Mirroring = Mirroring.ModeVert;
                    else if (data == 1)
                        cartridge.Mirroring = Mirroring.ModeHorz;
                    else if (data == 2)
                        cartridge.Mirroring = Mirroring.Mode1ScA;
                    else
                        cartridge.Mirroring = Mirroring.Mode1ScB;
                    break;
                case 0xE008:
                case 0xE010:
                    irq_latch = data;
                    break;

                case 0xF000:
                    irq_enable = (byte)(data & 0x03);
                    irq_counter = irq_latch;
                    irq_clock = 0;
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;

                case 0xF008:
                case 0xF010:
                    irq_enable = (byte)((irq_enable & 0x01) * 3);
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
            }
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Switch16kPrgRom(0, 0);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
        }
        public override void TickCycleTimer(int cycles)
        {
            if ((irq_enable & 0x02) != 0)
            {
                irq_clock += cycles * 4;
                while (irq_clock >= 455)
                {
                    irq_clock -= 455;
                    irq_counter++;
                    if (irq_counter == 0)
                    {
                        irq_counter = irq_latch;
                        cpu.IRQ(NesCpu.IsrType.External, true);
                    }
                }
            }
        }
        public override bool ScanlineTimerAlwaysActive
        {
            get
            {
                return true;
            }
        }

        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(irq_latch);
            stateStream.Write(irq_enable);
            stateStream.Write(irq_counter); 
            stateStream.Write(irq_clock);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            irq_latch = stateStream.ReadByte();
            irq_enable = stateStream.ReadByte();
            irq_counter = stateStream.ReadByte();
            irq_clock = stateStream.ReadInt32();
            base.LoadState(stateStream);
        }
    }

}
