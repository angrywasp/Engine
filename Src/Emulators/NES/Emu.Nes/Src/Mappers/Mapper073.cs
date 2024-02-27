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
    class Mapper73 : Mapper
    {
        int irq_latch = 0;
        int irq_enable = 0;
        int irq_counter = 0;
        int irq_clock = 0;
        public Mapper73(NesSystem nesSystem)
            : base(nesSystem) { }

        public override void Poke(int address, byte data)
        {
            switch (address)
            {
                case 0xF000:
                    cpuMemory.Switch16kPrgRom((data & 0x0F) * 4, 0);
                    break;

                case 0x8000:
                    irq_counter = (irq_counter & 0xFFF0) | (data & 0x0F);
                    break;
                case 0x9000:
                    irq_counter = (irq_counter & 0xFF0F) | ((data & 0x0F) << 4);
                    break;
                case 0xA000:
                    irq_counter = (irq_counter & 0xF0FF) | ((data & 0x0F) << 8);
                    break;
                case 0xB000:
                    irq_counter = (irq_counter & 0x0FFF) | ((data & 0x0F) << 12);
                    break;
                case 0xC000:
                    irq_enable = data & 0x02;
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
                case 0xD000:
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
            }
        }

        protected override void Initialize(bool initializing)
        {
            cpuMemory.Switch16kPrgRom(0, 0);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(8);
            cpuMemory.Switch8kChrRom(0);
        }

        public override void TickCycleTimer(int cycles)
        {
            if (irq_enable > 0)
            {
                if ((irq_counter += cycles) >= 0xFFFF)
                {
                    irq_enable = 0;
                    irq_counter &= 0xFFFF;
                    cpu.IRQ(NesCpu.IsrType.External, true);
                }
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
             irq_latch = stateStream.ReadInt32();
             irq_enable = stateStream.ReadInt32();
             irq_counter = stateStream.ReadInt32();
             irq_clock = stateStream.ReadInt32();
            base.LoadState(stateStream);
        }
    }
}
