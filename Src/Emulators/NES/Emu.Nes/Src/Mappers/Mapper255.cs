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
namespace MyNes.Nes
{
    class Mapper255 : Mapper
    {
        byte Mapper225_reg0 = 0x0F;
        byte Mapper225_reg1 = 0x0F;
        byte Mapper225_reg2 = 0x0F;
        byte Mapper225_reg3 = 0x0F;

        public Mapper255(NesSystem nesSystem)
            : base(nesSystem) { }

        public override void Poke(int address, byte data)
        {
            if ((address >= 0x8000) && (address <= 0xFFFF))
            {
                int banks = ((address & 0x4000) != 0) ? 1 : 0;
                ushort wher = (ushort)((address & 0x0Fc0) >> 7);
                cpuMemory.Switch8kChrRom(((address & 0x003F) + (banks << 6)) * 8);
                if ((address & 0x1000) != 0)//A12
                {
                    if ((address & 0x0040) != 0)//A6
                    {
                        cpuMemory.Switch16kPrgRom((((wher + (banks << 5)) << 1) + 1) * 4, 0);
                        cpuMemory.Switch16kPrgRom((((wher + (banks << 5)) << 1) + 1) * 4, 1);
                    }
                    else
                    {
                        cpuMemory.Switch16kPrgRom(((wher + (banks << 5)) << 1) * 4, 0);
                        cpuMemory.Switch16kPrgRom(((wher + (banks << 5)) << 1) * 4, 1);
                    }
                }
                else
                {
                    cpuMemory.Switch32kPrgRom((wher + (banks << 5)) * 8);//ignore A6
                }
                cartridge.Mirroring = ((address >> 13) & 1) == 0 ? Mirroring.ModeVert : Mirroring.ModeHorz;
            }
            else if ((address >= 0x5800) && (address <= 0x5FFF))
            {
                switch (address & 0x3)
                {
                    case 0: Mapper225_reg0 = (byte)(data & 0xf); break;
                    case 1: Mapper225_reg1 = (byte)(data & 0xf); break;
                    case 2: Mapper225_reg2 = (byte)(data & 0xf); break;
                    case 3: Mapper225_reg3 = (byte)(data & 0xf); break;
                }
            }
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x4018, 0x7FFF, Peek, Poke);
            cpuMemory.Switch32kPrgRom(0);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(8);
            cpuMemory.Switch8kChrRom(0);
            Mapper225_reg0 = 0xF;
            Mapper225_reg1 = 0xF;
            Mapper225_reg2 = 0xF;
            Mapper225_reg3 = 0xF;
        }
        public override byte Peek(int addr)
        {
            if ((addr >= 0x5800) && (addr <= 0x5FFF))
            {
                switch (addr & 0x3)
                {
                    case 0: return Mapper225_reg0;
                    case 1: return Mapper225_reg1;
                    case 2: return Mapper225_reg2;
                    case 3: return Mapper225_reg3;
                }
            }
            return 0xF;
        }
    }
}