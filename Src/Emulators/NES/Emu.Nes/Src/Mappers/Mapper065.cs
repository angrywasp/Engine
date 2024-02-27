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
    class Mapper65 : Mapper
    {
        int timer_irq_counter_65 = 0;
        int timer_irq_Latch_65 = 0;
        int timer_irq_enabled;
        int patch = 0;

        public Mapper65(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int address, byte data)
        {
            switch (address)
            {
                case 0x8000: cpuMemory.Switch8kPrgRom(data * 2, 0); break;

                case 0x9000:
                    if (patch == 0)
                    {
                        if ((data & 0x40) == 0x40)
                            cartridge.Mirroring = Mirroring.ModeVert;
                        else
                            cartridge.Mirroring = Mirroring.ModeHorz;
                    }
                    break;
                case 0x9001:
                    if (patch == 1)
                    {
                        if ((data & 0x40) == 0x40)
                            cartridge.Mirroring = Mirroring.ModeHorz;
                        else
                            cartridge.Mirroring = Mirroring.ModeVert;
                    }
                    break;
                case 0x9003:
                    if (patch == 0)
                    {
                        timer_irq_enabled = data & 0x80;
                        cpu.IRQ(NesCpu.IsrType.External, false);
                    }
                    break;
                case 0x9004:
                    if (patch == 0)
                    {
                        timer_irq_counter_65 = timer_irq_Latch_65;
                    }
                    break;
                case 0x9005:
                    if (patch == 1)
                    {
                        timer_irq_counter_65 = (byte)(data << 1);
                        timer_irq_enabled = data;
                        cpu.IRQ(NesCpu.IsrType.External, false);
                    }
                    else
                    {
                        timer_irq_Latch_65 = (timer_irq_Latch_65 & 0x00FF) | (data << 8);
                    }
                    break;
                case 0x9006:
                    if (patch == 1)
                    {
                        timer_irq_enabled = 1;
                    }
                    else
                    {
                        timer_irq_Latch_65 = (timer_irq_Latch_65 & 0xFF00) | data;
                    }
                    break;

                case 0xB000:
                case 0xB001:
                case 0xB002:
                case 0xB003:
                case 0xB004:
                case 0xB005:
                case 0xB006:
                case 0xB007: cpuMemory.Switch1kChrRom(data, address & 0x7); break;

                case 0xA000: cpuMemory.Switch8kPrgRom(data * 2, 1); break;
                case 0xC000: cpuMemory.Switch8kPrgRom(data * 2, 2); break;
            }
        }
        protected override void Initialize(bool initializing)
        {
            if (cartridge.Sha1.ToLower() == "7eb88e7678ab484593168328f63125330da64c6f")//Kaiketsu Yanchamaru 3(J)
            { patch = 1; }
            cpuMemory.Switch16kPrgRom(0, 0);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
        }
        public override void TickScanlineTimer()
        {
            if (patch == 1)
            {
                if (timer_irq_enabled != 0)
                {
                    if (timer_irq_counter_65 == 0)
                    {
                        cpu.IRQ(NesCpu.IsrType.External, true);
                    }
                    else
                    {
                        timer_irq_counter_65--;
                    }
                }
            }
            base.TickScanlineTimer();
        }
        public override void TickCycleTimer(int cycles)
        {
            if (patch == 0)
            {
                if (timer_irq_enabled != 0)
                {
                    if (timer_irq_counter_65 <= 0)
                    {
                        cpu.IRQ(NesCpu.IsrType.External, true);
                    }
                    else
                    {
                        timer_irq_counter_65 -= cycles;
                    }
                }
            }
        }
        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(timer_irq_counter_65);
            stateStream.Write(timer_irq_Latch_65);
            stateStream.Write(timer_irq_enabled);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            timer_irq_counter_65 = stateStream.ReadInt32();
            timer_irq_Latch_65 = stateStream.ReadInt32();
            timer_irq_enabled = stateStream.ReadInt32();
            base.LoadState(stateStream);
        }
    }
}
