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
    class Mapper67 : Mapper
    {
        public Mapper67(NesSystem nesSystem)
            : base(nesSystem) { }
        int irq_enable = 0;
        int irq_counter = 0;
        int irq_occur = 0;
        int irq_toggle = 0;
        public override void Poke(int address, byte data)
        {
            switch (address & 0xF800)
            {
                case 0x8800:
                    cpuMemory.Switch2kChrRom(data * 2, 0);
                    break;
                case 0x9800:
                    cpuMemory.Switch2kChrRom(data * 2, 1);
                    break;
                case 0xA800:
                    cpuMemory.Switch2kChrRom(data * 2, 2);
                    break;
                case 0xB800:
                    cpuMemory.Switch2kChrRom(data * 2, 3);
                    break;

                case 0xC800:
                    if (irq_toggle == 0)
                    {
                        irq_counter = (irq_counter & 0x00FF) | (data << 8);
                    }
                    else
                    {
                        irq_counter = (irq_counter & 0xFF00) | (data & 0xFF);
                    }
                    irq_toggle ^= 1;
                    irq_occur = 0;
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
                case 0xD800:
                    irq_enable = data & 0x10;
                    irq_toggle = 0;
                    irq_occur = 0;
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;

                case 0xE800:
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

                case 0xF800:
                    cpuMemory.Switch16kPrgRom(data * 4, 0);
                    break;
            }
        }

        protected override void Initialize(bool initializing)
        {
            cpuMemory.Switch16kPrgRom(0, 0);
            cpuMemory.Switch16kPrgRom((cartridge.ChrPages - 1) * 4, 1);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
        }

        public override void TickCycleTimer(int cycles)
        {
            if (irq_enable != 0)
            {
                if ((irq_counter -= cycles) <= 0)
                {
                    irq_enable = 0;
                    irq_occur = 0xFF;
                    irq_counter = 0xFFFF;
                    cpu.IRQ(NesCpu.IsrType.External, true);
                }
            }
        }
        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(irq_enable);
            stateStream.Write(irq_counter);
            stateStream.Write(irq_occur);
            stateStream.Write(irq_toggle);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            irq_enable = stateStream.ReadInt32();
            irq_counter = stateStream.ReadInt32();
            irq_occur = stateStream.ReadInt32();
            irq_toggle = stateStream.ReadInt32();
            base.LoadState(stateStream);
        }
    }
}
