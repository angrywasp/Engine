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
    class Mapper243 : Mapper
    {
        byte[] reg = new byte[4];
        public Mapper243(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int Address, byte data)
        {
            if ((Address & 0x4101) == 0x4100)
            {
                reg[0] = data;
            }
            else if ((Address & 0x4101) == 0x4101)
            {
                switch (reg[0] & 0x07)
                {
                    case 0:
                        reg[1] = 0;
                        reg[2] = 3;
                        break;
                    case 4:
                        reg[2] = (byte)((reg[2] & 0x06) | (data & 0x01));
                        break;
                    case 5:
                        reg[1] = (byte)(data & 0x01);
                        break;
                    case 6:
                        reg[2] = (byte)((reg[2] & 0x01) | ((data & 0x03) << 1));
                        break;
                    case 7:
                        reg[3] = (byte)(data & 0x01);
                        break;
                    default:
                        break;
                }

                cpuMemory.Switch32kPrgRom(reg[1]*8);

                cpuMemory.Switch1kChrRom(reg[2] * 8 + 0,0);
                cpuMemory.Switch1kChrRom(reg[2] * 8 + 1, 1);
                cpuMemory.Switch1kChrRom(reg[2] * 8 + 2, 2);
                cpuMemory.Switch1kChrRom(reg[2] * 8 + 3, 3);
                cpuMemory.Switch1kChrRom(reg[2] * 8 + 4, 4);
                cpuMemory.Switch1kChrRom(reg[2] * 8 + 5, 5);
                cpuMemory.Switch1kChrRom(reg[2] * 8 + 6, 6);
                cpuMemory.Switch1kChrRom(reg[2] * 8 + 7, 7);

                if (reg[3]!=0)
                {
                    cartridge.Mirroring = Mirroring.ModeVert;
                }
                else
                {
                    cartridge.Mirroring = Mirroring.ModeHorz;
                }
            }
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x4018, 0x7FFF, Poke);
            cpuMemory.Switch32kPrgRom(0);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);

            reg[0] = 0;
            reg[1] = 0;
            reg[2] = 3;
            reg[3] = 0;
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
