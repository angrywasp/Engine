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
    class Mapper182 : Mapper
    {
        byte reg = 0;
        byte irq_enable = 0;
        byte irq_counter = 0;
        public Mapper182(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int Address, byte data)
        {
            switch (Address & 0xF003)
            {
                case 0x8001:
                    if ((data & 0x01) == 0x01)
                        cartridge.Mirroring = Mirroring.ModeHorz;
                    else
                        cartridge.Mirroring = Mirroring.ModeVert;
                    break;
                case 0xA000:
                    reg = (byte)(data & 0x07);
                    break;
                case 0xC000:
                    switch (reg)
                    {
                        case 0:
                            cpuMemory.Switch1kChrRom((data & 0xFE) + 0, 0);
                            cpuMemory.Switch1kChrRom((data & 0xFE) + 1, 1);
                            break;
                        case 1:
                            cpuMemory.Switch1kChrRom(data, 5);
                            break;
                        case 2:
                            cpuMemory.Switch1kChrRom((data & 0xFE) + 0, 2);
                            cpuMemory.Switch1kChrRom((data & 0xFE) + 1, 3);
                            break;
                        case 3:
                            cpuMemory.Switch1kChrRom(data, 7);
                            break;
                        case 4:
                            cpuMemory.Switch8kPrgRom(data * 2, 0);
                            break;
                        case 5:
                            cpuMemory.Switch8kPrgRom(data * 2, 1);
                            break;
                        case 6:
                            cpuMemory.Switch1kChrRom(data, 4);
                            break;
                        case 7:
                            cpuMemory.Switch1kChrRom(data, 6);
                            break;
                    }
                    break;
                case 0xE003:
                    irq_enable = data;
                    irq_counter = data;
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
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
            if ((--irq_counter) == 0)
            {
                irq_enable = 0;
                irq_counter = 0;
                cpu.IRQ(NesCpu.IsrType.External, true);
            }
        }
    }
}
