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

    class Mapper24 : Mapper
    {
        Vrc6ExternalComponent external;
        int irq_latch = 0;
        int irq_enable = 0;
        int irq_counter = 0;
        int irq_clock = 0;
        public Mapper24(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int address, byte data)
        {
            switch (address)
            {
                case 0x8000: cpuMemory.Switch16kPrgRom(data * 4, 0); break;
                case 0xB003:
                    switch (data & 0x0C)
                    {
                        case 0x00: cartridge.Mirroring = Mirroring.ModeVert; break;
                        case 0x04: cartridge.Mirroring = Mirroring.ModeHorz; break;
                        case 0x08: cartridge.Mirroring = Mirroring.Mode1ScA; break;
                        case 0x0C: cartridge.Mirroring = Mirroring.Mode1ScB; break;
                    }
                    break;

                case 0xC000: cpuMemory.Switch8kPrgRom(data * 2, 2); break;
                case 0xD000: cpuMemory.Switch1kChrRom(data, 0); break;
                case 0xD001: cpuMemory.Switch1kChrRom(data, 1); break;
                case 0xD002: cpuMemory.Switch1kChrRom(data, 2); break;
                case 0xD003: cpuMemory.Switch1kChrRom(data, 3); break;
                case 0xE000: cpuMemory.Switch1kChrRom(data, 4); break;
                case 0xE001: cpuMemory.Switch1kChrRom(data, 5); break;
                case 0xE002: cpuMemory.Switch1kChrRom(data, 6); break;
                case 0xE003: cpuMemory.Switch1kChrRom(data, 7); break;

                case 0xF000:
                    irq_latch = data;
                    break;
                case 0xF001:
                    irq_enable = (data & 0x03);
                    if ((irq_enable & 0x02) == 0x02)
                    {
                        irq_counter = irq_latch;
                        irq_clock = 0;
                    }
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
                case 0xF002:
                    irq_enable = (irq_enable & 0x01) * 3;
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;

                case 0x9000: external.ChannelSq1.Poke1(0x9000, data); break;
                case 0x9001: external.ChannelSq1.Poke2(0x9001, data); break;
                case 0x9002: external.ChannelSq1.Poke3(0x9002, data); break;

                case 0xA000: external.ChannelSq2.Poke1(0xA000, data); break;
                case 0xA001: external.ChannelSq2.Poke2(0xA001, data); break;
                case 0xA002: external.ChannelSq2.Poke3(0xA002, data); break;

                case 0xB000: external.ChannelSaw.Poke1(0xB000, data); break;
                case 0xB001: external.ChannelSaw.Poke2(0xB001, data); break;
                case 0xB002: external.ChannelSaw.Poke3(0xB002, data); break;
            }
        }
        protected override void Initialize(bool initializing)
        {
            apu.External = external = new Vrc6ExternalComponent(nesSystem);

            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);

            cpuMemory.Switch8kChrRom(0);
            cpuMemory.Switch16kPrgRom(0, 0);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
        }
        public override void TickCycleTimer(int cycles)
        {
            if ((irq_enable & 0x02) == 0x02)
            {
                if ((irq_clock += cycles) >= 0x72)
                {
                    irq_clock -= 0x72;
                    if (irq_counter == 0xFF)
                    {
                        irq_counter = irq_latch;
                        cpu.IRQ(NesCpu.IsrType.External, true);
                    }
                    else
                    {
                        irq_counter++;
                    }
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