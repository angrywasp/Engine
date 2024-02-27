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
namespace MyNes.Nes
{
    class Mapper159 : Mapper
    {
        public int timer_irq_counter_16 = 0;
        public int timer_irq_Latch_16 = 0;
        public bool timer_irq_enabled;
        public Mapper159(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int address, byte data)
        {
            switch (address & 0x0F)
            {
                case 0: cpuMemory.Switch1kChrRom(data, 0); break;
                case 1: cpuMemory.Switch1kChrRom(data, 1); break;
                case 2: cpuMemory.Switch1kChrRom(data, 2); break;
                case 3: cpuMemory.Switch1kChrRom(data, 3); break;
                case 4: cpuMemory.Switch1kChrRom(data, 4); break;
                case 5: cpuMemory.Switch1kChrRom(data, 5); break;
                case 6: cpuMemory.Switch1kChrRom(data, 6); break;
                case 7: cpuMemory.Switch1kChrRom(data, 7); break;
                case 8: cpuMemory.Switch16kPrgRom(data * 4, 0); break;
                case 9:
                    switch (data & 0x03)
                    {
                        case 0: cartridge.Mirroring = Mirroring.ModeVert; break;
                        case 1: cartridge.Mirroring = Mirroring.ModeHorz; break;
                        case 2: cartridge.Mirroring = Mirroring.Mode1ScA; break;
                        case 3: cartridge.Mirroring = Mirroring.Mode1ScB; break;
                    }
                    break;
                case 0xA:
                    timer_irq_enabled = ((data & 0x1) != 0);
                    timer_irq_counter_16 = timer_irq_Latch_16;
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
                case 0xB:
                    timer_irq_Latch_16 = ((timer_irq_Latch_16 & 0xFF00) | data);
                    break;
                case 0xC:
                    timer_irq_Latch_16 = ((data << 8) | (timer_irq_Latch_16 & 0x00FF));
                    break;
                case 0xD: break;
            }
        }
        protected override void Initialize(bool initializing)
        {
            timer_irq_enabled = false;
            cpuMemory.Map(0x6000, 0x7FFF, Poke);
            cpuMemory.Switch16kPrgRom(0, 0);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
        }
        public override void TickCycleTimer(int cycles)
        {
            if (timer_irq_enabled)
            {
                if (timer_irq_counter_16 > 0)
                    timer_irq_counter_16 -= cycles;
                else
                {
                    cpu.IRQ(NesCpu.IsrType.External, true);
                    timer_irq_enabled = false;
                }
            }
        }
        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(timer_irq_counter_16);
            stateStream.Write(timer_irq_Latch_16);
            stateStream.Write(timer_irq_enabled);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            timer_irq_counter_16 = stateStream.ReadInt32();
            timer_irq_Latch_16 = stateStream.ReadInt32();
            timer_irq_enabled = stateStream.ReadBooleans()[0];
            base.LoadState(stateStream);
        }
    }
}