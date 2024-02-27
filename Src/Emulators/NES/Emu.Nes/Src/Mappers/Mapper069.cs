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
    public class Mapper69 : Mapper
    {
        public Mapper69(NesSystem nesSystem)
            : base(nesSystem) { }

        public ushort reg = 0;
        public short timer_irq_counter_69 = 0;
        public bool timer_irq_enabled;
        byte ss5bAddr = 0;
        Ss5BExternalComponent externalUnit;
        void Switch8kPrgRomToSRAM(int start)
        {
            switch (cartridge.PrgPages)
            {
                case (2): start = (start & 0x7); break;
                case (4): start = (start & 0xf); break;
                case (8): start = (start & 0x1f); break;
                case (16): start = (start & 0x3f); break;
                case (32): start = (start & 0x7f); break;
                case (64): start = (start & 0xff); break;
                case (128): start = (start & 0x1ff); break;
            }
            cartridge.Prg[start].CopyTo(cpuMemory.srm, 0);
            for (int i = 0x1000; i < 0x2000; i++)
            {
                cpuMemory.srm[i] = cartridge.Prg[start + 1][i - 0x1000];
            }
        }
        public override void Poke(int addr, byte data)
        {
            switch (addr & 0xE000)
            {
                case 0x8000:
                    reg = data;
                    break;
                case 0xA000:
                    switch (reg & 0x0F)
                    {
                        case 0x00: cpuMemory.Switch1kChrRom(data, 0); break;
                        case 0x01: cpuMemory.Switch1kChrRom(data, 1); break;
                        case 0x02: cpuMemory.Switch1kChrRom(data, 2); break;
                        case 0x03: cpuMemory.Switch1kChrRom(data, 3); break;
                        case 0x04: cpuMemory.Switch1kChrRom(data, 4); break;
                        case 0x05: cpuMemory.Switch1kChrRom(data, 5); break;
                        case 0x06: cpuMemory.Switch1kChrRom(data, 6); break;
                        case 0x07: cpuMemory.Switch1kChrRom(data, 7); break;
                        case 0x08:
                            if ((data & 0x40) == 0)
                                Switch8kPrgRomToSRAM((data & 0x3F) * 2);
                            break;
                        case 0x09:
                            cpuMemory.Switch8kPrgRom(data * 2, 0);
                            break;
                        case 0x0A:
                            cpuMemory.Switch8kPrgRom(data * 2, 1);
                            break;
                        case 0x0B:
                            cpuMemory.Switch8kPrgRom(data * 2, 2);
                            break;

                        case 0x0C:
                            data &= 0x03;
                            if (data == 0) cartridge.Mirroring = Mirroring.ModeVert;
                            if (data == 1) cartridge.Mirroring = Mirroring.ModeHorz;
                            if (data == 2)
                            {
                                cartridge.Mirroring = Mirroring.Mode1ScA;
                            }
                            if (data == 3)
                            {
                                cartridge.Mirroring = Mirroring.Mode1ScB;
                            }
                            break;

                        case 0x0D: 
                            cpu.IRQ(NesCpu.IsrType.External, false);
                            if (data == 0)
                                timer_irq_enabled = false;
                            if (data == 0x81)
                                timer_irq_enabled = true;           
                            break;

                        case 0x0E:
                            timer_irq_counter_69 = (short)((timer_irq_counter_69 & 0xFF00) | data);
                            cpu.IRQ(NesCpu.IsrType.External, false);
                            break;

                        case 0x0F:
                            timer_irq_counter_69 = (short)((timer_irq_counter_69 & 0x00FF) | (data << 8));
                            cpu.IRQ(NesCpu.IsrType.External, false);
                            break;
                    }
                    break;
                //External sound...
                case 0xC000: ss5bAddr = data; break;
                case 0xE000:
                    switch (ss5bAddr)
                    {
                        case 00: externalUnit.ChannelVW1.Poke1(addr, data); break;
                        case 01: externalUnit.ChannelVW1.Poke2(addr, data); break;
                        case 02: externalUnit.ChannelVW2.Poke1(addr, data); break;
                        case 03: externalUnit.ChannelVW2.Poke2(addr, data); break;
                        case 04: externalUnit.ChannelVW3.Poke1(addr, data); break;
                        case 05: externalUnit.ChannelVW3.Poke2(addr, data); break;
                        case 06: break; // Skipped
                        case 07:
                            externalUnit.ChannelVW1.Enabled = (data & 0x01) == 0;
                            externalUnit.ChannelVW2.Enabled = (data & 0x02) == 0;
                            externalUnit.ChannelVW3.Enabled = (data & 0x04) == 0;
                            break;

                        case 08: externalUnit.ChannelVW1.Poke3(addr, data); break;
                        case 09: externalUnit.ChannelVW2.Poke3(addr, data); break;
                        case 10: externalUnit.ChannelVW3.Poke3(addr, data); break;
                    }
                    break;
            }
            base.Poke(addr, data);
        }
        protected override void Initialize(bool initializing)
        {
            apu.External = externalUnit = new Ss5BExternalComponent(nesSystem);
            cpuMemory.Switch8kPrgRom(((cartridge.PrgPages * 2) - 1) * 2, 3);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
            base.Initialize(initializing);
        }
        public override void TickCycleTimer(int cycles)
        {
            if (timer_irq_enabled)
            {
                if (timer_irq_counter_69 > 0)
                    timer_irq_counter_69 -= (short)cycles;
                else
                {
                    cpu.IRQ(NesCpu.IsrType.External, true);
                    timer_irq_enabled = false;
                }
            }
            base.TickCycleTimer(cycles);
        }
    }
}