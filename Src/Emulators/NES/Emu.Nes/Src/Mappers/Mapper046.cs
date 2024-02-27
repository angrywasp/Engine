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
   
    class Mapper46 : Mapper
    {
        int[] reg = new int[4];
        public Mapper46(NesSystem nesSystem)
            : base(nesSystem) { }

        public override void Poke(int address, byte data)
        {
            if (address < 0x8000)
            {
                reg[0] = data & 0x0F;
                reg[1] = (data & 0xF0) >> 4;
                SetBank();
            }
            else
            {
                reg[2] = data & 0x01;
                reg[3] = (data & 0x70) >> 4;
                SetBank();
            }
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x4018, 0x7FFF, Poke);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
            SetBank();
        }
        void SetBank()
        {
            cpuMemory.Switch8kPrgRom((reg[0] * 8 + reg[2] * 4 + 0) * 2, 0);
            cpuMemory.Switch8kPrgRom((reg[0] * 8 + reg[2] * 4 + 1) * 2, 1);
            cpuMemory.Switch8kPrgRom((reg[0] * 8 + reg[2] * 4 + 2) * 2, 2);
            cpuMemory.Switch8kPrgRom((reg[0] * 8 + reg[2] * 4 + 3) * 2, 3);
            cpuMemory.Switch1kChrRom(reg[1] * 64 + reg[3] * 8 + 0, 0);
            cpuMemory.Switch1kChrRom(reg[1] * 64 + reg[3] * 8 + 1, 1);
            cpuMemory.Switch1kChrRom(reg[1] * 64 + reg[3] * 8 + 2, 2);
            cpuMemory.Switch1kChrRom(reg[1] * 64 + reg[3] * 8 + 3, 3);
            cpuMemory.Switch1kChrRom(reg[1] * 64 + reg[3] * 8 + 4, 4);
            cpuMemory.Switch1kChrRom(reg[1] * 64 + reg[3] * 8 + 5, 5);
            cpuMemory.Switch1kChrRom(reg[1] * 64 + reg[3] * 8 + 6, 6);
            cpuMemory.Switch1kChrRom(reg[1] * 64 + reg[3] * 8 + 7, 7);
        }
        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(reg);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            stateStream.Read(reg);
            base.LoadState(stateStream);
        }
    }
}
