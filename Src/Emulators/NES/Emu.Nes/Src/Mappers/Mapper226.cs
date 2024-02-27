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
    class Mapper226 : Mapper
    {
        byte[] reg = new byte[2];
        public Mapper226(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int Address, byte data)
        {
            if ((Address & 0x001) == 0x001)
            {
                reg[1] = data;
            }
            else
            {
                reg[0] = data;
            }

            if ((reg[0] & 0x40) == 0x40)
            {
                cartridge.Mirroring = Mirroring.ModeVert;
            }
            else
            {
                cartridge.Mirroring = Mirroring.ModeHorz;
            }

            byte bank = (byte)(((reg[0] & 0x1E) >> 1) | ((reg[0] & 0x80) >> 3) | ((reg[1] & 0x01) << 5));

            if ((reg[0] & 0x20) == 0x20)
            {
                if ((reg[0] & 0x01) == 0x01)
                {
                    cpuMemory.Switch8kPrgRom((bank * 4 + 2) * 2, 0);
                    cpuMemory.Switch8kPrgRom((bank * 4 + 3) * 2, 1);
                    cpuMemory.Switch8kPrgRom((bank * 4 + 2) * 2, 2);
                    cpuMemory.Switch8kPrgRom((bank * 4 + 3) * 2, 3);

                }
                else
                {
                    cpuMemory.Switch8kPrgRom((bank * 4 + 0) * 2, 0);
                    cpuMemory.Switch8kPrgRom((bank * 4 + 1) * 2, 1);
                    cpuMemory.Switch8kPrgRom((bank * 4 + 0) * 2, 2);
                    cpuMemory.Switch8kPrgRom((bank * 4 + 1) * 2, 3);
                }
            }
            else
            {
                cpuMemory.Switch8kPrgRom((bank * 4 + 0) * 2, 0);
                cpuMemory.Switch8kPrgRom((bank * 4 + 1) * 2, 1);
                cpuMemory.Switch8kPrgRom((bank * 4 + 2) * 2, 2);
                cpuMemory.Switch8kPrgRom((bank * 4 + 3) * 2, 3);
            }
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Switch32kPrgRom(0);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
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
