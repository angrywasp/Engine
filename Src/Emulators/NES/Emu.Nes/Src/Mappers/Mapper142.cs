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
    class Mapper142 : Mapper
    {
        byte prg_sel = 0;
        byte irq_enable = 0;
        int irq_counter = 0;
        public Mapper142(NesSystem nes)
            : base(nes)
        { }
        public override void Poke(int addr, byte data)
        {
            switch (addr & 0xF000)
            {
                case 0x8000:
                    irq_counter = (irq_counter & 0xFFF0) | ((data & 0x0F) << 0);
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
                    irq_enable = (byte)(data & 0x0F);   
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
                case 0xE000:
                    prg_sel = (byte)(data & 0x0F);
                    break;
                case 0xF000:
                    switch (prg_sel)
                    {
                        case 1: cpuMemory.Switch8kPrgRom((data & 0x0F) * 2, 0); break;
                        case 2: cpuMemory.Switch8kPrgRom((data & 0x0F) * 2, 1); break;
                        case 3: cpuMemory.Switch8kPrgRom((data & 0x0F) * 2, 2); break;
                        case 4: cpuMemory.Switch8kPrgRomToSRAM((data & 0x0F) * 2); break;
                    }
                    break;
            }
            base.Poke(addr, data);
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Switch8kPrgRomToSRAM(0);
            cpuMemory.Switch8kPrgRom(0x0F * 2, 3);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
            base.Initialize(initializing);
        }
        public override void TickScanlineTimer()
        {
            if (irq_enable != 0)
            {
                if (irq_counter > (0xFFFF - 113))
                {
                    irq_counter = 0;
                    cpu.IRQ(NesCpu.IsrType.External, true);
                }
                else
                {
                    irq_counter += 113;
                }
            }
            base.TickScanlineTimer();
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
            stateStream.Write(prg_sel);
            stateStream.Write(irq_enable);
            stateStream.Write(irq_counter);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            prg_sel = stateStream.ReadByte();
            irq_enable = stateStream.ReadByte();
            irq_counter = stateStream.ReadInt32();
            base.LoadState(stateStream);
        }
    }
}
