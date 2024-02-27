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
    class Mapper91 : Mapper
    {
        public bool IRQEnabled;
        public int IRQCount = 0;
        public Mapper91(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int address, byte data)
        {
            switch (address & 0xF003)
            {
                case 0x6000:
                case 0x6001:
                case 0x6002:
                case 0x6003:
                    cpuMemory.Switch2kChrRom(data * 2, (address & 0x03));
                    break;

                case 0x7000:
                    cpuMemory.Switch8kPrgRom(data * 2, 0);
                    break;
                case 0x7001:
                    cpuMemory.Switch8kPrgRom(data * 2, 1);
                    break;

                case 0x7002:
                    IRQEnabled = false;
                    IRQCount = 0;
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
                case 0x7003:
                    IRQEnabled = true;
                    break;
            }
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x6000, 0x7FFF, Poke);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(8);
            cpuMemory.Switch8kChrRom(0);
        }
        public override void TickScanlineTimer()
        {
            if (IRQCount < 8 & IRQEnabled)
            {
                IRQCount++;
                if (IRQCount >= 8)
                {
                    cpu.IRQ(NesCpu.IsrType.External, true);
                }
            }
        }
        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(IRQEnabled); 
            stateStream.Write(IRQCount);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            IRQEnabled = stateStream.ReadBooleans()[0];
            IRQCount = stateStream.ReadInt32();
            base.LoadState(stateStream);
        }
    }
}
