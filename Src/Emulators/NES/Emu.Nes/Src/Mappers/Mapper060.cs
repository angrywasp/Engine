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
    class Mapper60 : Mapper
    {
        byte patch = 0;
        byte game_sel = 1;
        public Mapper60(NesSystem nes)
            : base(nes)
        { }
        public override void Poke(int addr, byte data)
        {
            if (patch == 1)
            {
                if ((addr & 0x80) == 0x80)
                {
                    cpuMemory.Switch16kPrgRom(((addr & 0x70) >> 4) * 4, 0);
                    cpuMemory.Switch16kPrgRom(((addr & 0x70) >> 4) * 4, 1);
                }
                else
                {
                    cpuMemory.Switch32kPrgRom(((addr & 0x70) >> 5) * 8);
                }

                cpuMemory.Switch8kChrRom((addr & 0x07) * 8);

                if ((data & 0x08) == 0x08)
                    cartridge.Mirroring = Mirroring.ModeVert;
                else
                    cartridge.Mirroring = Mirroring.ModeHorz;
            }
            base.Poke(addr, data);
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.ResetTrigger = true;
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);

            if (cartridge.Sha1.ToLower() == "4984fb582737ccc6f2b6d6f587bcaad889c7a934")//reset based 4-in-1
            {
                cpuMemory.Switch16kPrgRom(game_sel * 4, 0);
                cpuMemory.Switch16kPrgRom(game_sel * 4, 1);
                cpuMemory.Switch8kChrRom(game_sel * 8);

            }
            else
            {
                patch = 1;
                cpuMemory.Switch32kPrgRom(0);
                cpuMemory.Switch8kChrRom(0);
            }

            base.Initialize(initializing);
        }
        public override void SoftReset()
        {
            game_sel++;
            game_sel &= 3;
            base.SoftReset();
        }
        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(game_sel);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            game_sel = stateStream.ReadByte();
            base.LoadState(stateStream);
        }
    }
}
