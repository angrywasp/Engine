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

namespace MyNes.Nes
{
    class Mapper117 : Mapper
    {
        byte irq_counter = 0;
        byte irq_enable = 0;
        public Mapper117(NesSystem nes)
            : base(nes)
        { }

        public override void Poke(int addr, byte data)
        {
            switch (addr)
            {
                case 0x8000:
                    cpuMemory.Switch8kPrgRom(data * 2, 0);
                    break;
                case 0x8001:
                    cpuMemory.Switch8kPrgRom(data * 2, 1);
                    break;
                case 0x8002:
                    cpuMemory.Switch8kPrgRom(data * 2, 2);
                    break;
                case 0xA000:
                    cpuMemory.Switch1kChrRom(data, 0);
                    break;
                case 0xA001:
                    cpuMemory.Switch1kChrRom(data, 1);
                    break;
                case 0xA002:
                    cpuMemory.Switch1kChrRom(data, 2);
                    break;
                case 0xA003:
                    cpuMemory.Switch1kChrRom(data, 3);
                    break;
                case 0xA004:
                    cpuMemory.Switch1kChrRom(data, 4);
                    break;
                case 0xA005:
                    cpuMemory.Switch1kChrRom(data, 5);
                    break;
                case 0xA006:
                    cpuMemory.Switch1kChrRom(data, 6);
                    break;
                case 0xA007:
                    cpuMemory.Switch1kChrRom(data, 7);
                    break;
                case 0xC001:
                case 0xC002:
                case 0xC003:
                    irq_counter = data; cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
                case 0xE000:
                    irq_enable = (byte)(data & 1);

                    break;
            }
            base.Poke(addr, data);
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Switch16kPrgRom(0, 0);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
            base.Initialize(initializing);
        }
        public override void TickScanlineTimer()
        {
            if (irq_enable != 0)
            {
                if (irq_counter == ppu.scanline)
                {
                    irq_counter = 0;
                    cpu.IRQ(NesCpu.IsrType.External, true);
                }
            }
            base.TickScanlineTimer();
        }
        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(irq_counter); stateStream.Write(irq_enable);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            irq_counter = stateStream.ReadByte();
            irq_enable = stateStream.ReadByte();
            base.LoadState(stateStream);
        }
    }
}
