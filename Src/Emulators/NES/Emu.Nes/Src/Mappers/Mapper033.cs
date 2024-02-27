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
    class Mapper33 : Mapper
    {
        bool type1 = true; 
        bool IRQEabled;
        byte IRQCounter = 0;
        byte IRQLatch = 0;
        public Mapper33(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int address, byte data)
        {
            if (address == 0x8000)
            {
                cpuMemory.Switch8kPrgRom((data & 0x1F) * 2, 0);
                if (type1)
                {
                    if ((data & 0x40) == 0x40)
                    {
                        cartridge.Mirroring = Mirroring.ModeHorz;

                    }
                    else
                    {
                        cartridge.Mirroring = Mirroring.ModeVert;
                    }
                }
            }
            else if (address == 0x8001)
            {
                cpuMemory.Switch8kPrgRom(data * 2, 1);
            }
            else if (address == 0x8002)
            {
                cpuMemory.Switch2kChrRom(data * 2, 0);
            }
            else if (address == 0x8003)
            {
                cpuMemory.Switch2kChrRom(data * 2, 1);
            }
            else if (address == 0xA000)
            {
                cpuMemory.Switch1kChrRom(data, 4);
            }
            else if (address == 0xA001)
            {
                cpuMemory.Switch1kChrRom(data, 5);
            }
            else if (address == 0xA002)
            {
                cpuMemory.Switch1kChrRom(data, 6);
            }
            else if (address == 0xA003)
            {
                cpuMemory.Switch1kChrRom(data, 7);
            }
            
            else if (address == 0xC000)
            {
                type1 = false;
                IRQLatch = data;
                IRQCounter = IRQLatch; 
                cpu.IRQ(NesCpu.IsrType.External, false);
            }
            else if (address == 0xC001)
            {
                type1 = false;
                IRQCounter = IRQLatch; cpu.IRQ(NesCpu.IsrType.External, false);
            }
            else if (address == 0xC002)
            {
                type1 = false;
                IRQEabled = true; cpu.IRQ(NesCpu.IsrType.External, false);
            }
            else if (address == 0xC003)
            {
                type1 = false;
                IRQEabled = false; cpu.IRQ(NesCpu.IsrType.External, false);
            }
            else if (address == 0xE000)
            {
                type1 = false;
                if ((data & 0x40) == 0x40)
                {
                    cartridge.Mirroring = Mirroring.ModeHorz;

                }
                else
                {
                    cartridge.Mirroring = Mirroring.ModeVert;
                }
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
        public override void TickScanlineTimer()
        {
            if (IRQEabled)
            {
                if (++IRQCounter == 0)
                {
                    IRQEabled = false;
                    IRQCounter = 0;
                    cpu.IRQ(NesCpu.IsrType.External, true);
                }
            }
        }

        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(type1, IRQEabled);
            stateStream.Write(IRQCounter);
            stateStream.Write(IRQLatch);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            bool[] status = stateStream.ReadBooleans();
            type1 = status[0];
            IRQEabled = status[1];
            IRQCounter = stateStream.ReadByte();
            IRQLatch = stateStream.ReadByte();
            base.LoadState(stateStream);
        }
    }
}
