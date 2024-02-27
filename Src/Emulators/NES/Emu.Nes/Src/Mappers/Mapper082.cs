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
    class Mapper82 : Mapper
    {
        bool Swapped;
        public Mapper82(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int address, byte data)
        {
            if (address == 0x7EF0)
            {
                if (Swapped)
                    cpuMemory.Switch2kChrRom((data >> 1) * 2, 2);
                else
                    cpuMemory.Switch2kChrRom((data >> 1) * 2, 0);
            }
            else if (address == 0x7EF1)
            {
                if (Swapped)
                    cpuMemory.Switch2kChrRom((data >> 1) * 2, 3);
                else
                    cpuMemory.Switch2kChrRom((data >> 1) * 2, 1);
            }
            else if (address == 0x7EF2)
            {
                if (Swapped)
                    cpuMemory.Switch1kChrRom(data, 0);
                else
                    cpuMemory.Switch1kChrRom(data, 4);
            }
            else if (address == 0x7EF3)
            {
                if (Swapped)
                    cpuMemory.Switch1kChrRom(data, 1);
                else
                    cpuMemory.Switch1kChrRom(data, 5);
            }
            else if (address == 0x7EF4)
            {
                if (Swapped)
                    cpuMemory.Switch1kChrRom(data, 2);
                else
                    cpuMemory.Switch1kChrRom(data, 6);
            }
            else if (address == 0x7EF5)
            {
                if (Swapped)
                    cpuMemory.Switch1kChrRom(data, 3);
                else
                    cpuMemory.Switch1kChrRom(data, 7);
            }
            else if (address == 0x7EF6)
            {
                Swapped = ((data & 0x2) >> 1) != 0;
                if ((data & 0x01) == 0)
                    cartridge.Mirroring = Mirroring.ModeHorz;
                else
                    cartridge.Mirroring = Mirroring.ModeVert;
            }
            else if (address == 0x7EFA)
                cpuMemory.Switch8kPrgRom((data >> 2) * 2, 0);
            else if (address == 0x7EFB)
                cpuMemory.Switch8kPrgRom((data >> 2) * 2, 1);
            else if (address == 0x7EFC)
                cpuMemory.Switch8kPrgRom((data >> 2) * 2, 2);
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x6000, 0x7FFF, Poke);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 2) * 4, 0);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(8);
            cpuMemory.Switch8kChrRom(0);
        }
        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(Swapped);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            Swapped = stateStream.ReadBooleans()[0];
            base.LoadState(stateStream);
        }
    }
}
