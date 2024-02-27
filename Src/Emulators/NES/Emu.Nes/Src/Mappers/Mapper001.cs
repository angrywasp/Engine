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
   
    class Mapper01 : Mapper
    {
        public Mapper01(NesSystem nesSystem)
            : base(nesSystem) { }
        public override string Name
        {
            get
            {
                return "Nintendo MMC1";
            }
        }
        byte[] reg = new byte[4];
        byte SHIFT = 0;
        byte BUFFER = 0;
        bool MODE = false;

        public override void Poke(int address, byte data)
        {
            
            if ((data & 0x80) == 0x80)
            {
                reg[0] |= 0x0C;
                SHIFT = BUFFER = 0;
                return;
            }
            
            if ((data & 0x01) == 0x01)
                BUFFER |= (byte)(1 << SHIFT);
            if (++SHIFT < 5)
                return;

            address = (ushort)((address & 0x7FFF) >> 13);
            reg[address] = BUFFER;

            
            SHIFT = BUFFER = 0;

            if (!MODE)
            {
                switch (address)
                {
                    case 0:
                        if ((reg[0] & 0x02) == 0x02)
                        {
                            if ((reg[0] & 0x01) != 0)
                                cartridge.Mirroring = Mirroring.ModeHorz;
                            else
                                cartridge.Mirroring = Mirroring.ModeVert;
                        }
                        else
                        {
                            if ((reg[0] & 0x01) != 0)
                                cartridge.Mirroring = Mirroring.Mode1ScB;
                            else
                                cartridge.Mirroring = Mirroring.Mode1ScA;
                        }
                        break;
                    case 1:
                        
                        if (!cartridge.HasCharRam)
                        {
                            if ((reg[0] & 0x10) != 0)
                            {
                                cpuMemory.Switch4kChrRom(reg[1] * 4, 0);
                                cpuMemory.Switch4kChrRom(reg[2] * 4, 1);
                            }
                            else
                            {
                                cpuMemory.Switch8kChrRom((reg[1] >> 1) * 8);
                            }
                        }
                        else
                        {
                            if ((reg[0] & 0x10) != 0)
                            {
                                cpuMemory.Switch4kChrRom(reg[1] * 4, 0);
                            }
                        }
                        break;
                    case 2:
                        
                        if (!cartridge.HasCharRam)
                        {
                            if ((reg[0] & 0x10) != 0)
                            {

                                cpuMemory.Switch4kChrRom(reg[1] * 4, 0);
                                cpuMemory.Switch4kChrRom(reg[2] * 4, 1);
                            }
                            else
                            {
                                cpuMemory.Switch8kChrRom((reg[1] >> 1) * 8);
                            }
                        }
                        else
                        {
                            if ((reg[0] & 0x10) != 0)
                            {
                                cpuMemory.Switch4kChrRom(reg[2] * 4, 1);
                            }
                        }
                        break;
                    case 3:
                        if ((reg[0] & 0x08) == 0)
                        {
                            cpuMemory.Switch32kPrgRom((reg[3] >> 1) * 8);
                        }
                        else
                        {
                            if ((reg[0] & 0x04) != 0)
                            {
                                cpuMemory.Switch16kPrgRom(reg[3] * 4, 0);
                                cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
                            }
                            else
                            {
                                cpuMemory.Switch16kPrgRom(0, 0);
                                cpuMemory.Switch16kPrgRom(reg[3] * 4, 1);
                            }
                        }
                        break;
                }
            }
            else
            {
                int BASE = reg[1] & 0x10;

                
                if (address == 0)
                {
                    if ((reg[0] & 0x02) == 0x02)
                    {
                        if ((reg[0] & 0x01) == 0x01)
                            cartridge.Mirroring = Mirroring.ModeHorz;
                        else
                            cartridge.Mirroring = Mirroring.ModeVert;
                    }
                    else
                    {
                        if ((reg[0] & 0x01) != 0)
                            cartridge.Mirroring = Mirroring.Mode1ScB;
                        else
                            cartridge.Mirroring = Mirroring.Mode1ScA;
                    }
                }
                
                if (!cartridge.HasCharRam)
                {
                    if ((reg[0] & 0x10) == 0x10)
                    {
                        cpuMemory.Switch4kChrRom(reg[1] * 4, 0);
                    }
                    else
                    {
                        cpuMemory.Switch8kChrRom((reg[1] >> 1) * 8);
                    }
                }
                else
                {
                    if ((reg[0] & 0x10) == 0x10)
                    {
                        cpuMemory.Switch4kChrRom(reg[1] * 4, 0);
                    }
                }
                
                if (!cartridge.HasCharRam)
                {
                    if ((reg[0] & 0x10) == 0x10)
                    {
                        cpuMemory.Switch4kChrRom(reg[2] * 4, 1);
                    }
                }
                else
                {
                    if ((reg[0] & 0x10) == 0x10)
                    {
                        cpuMemory.Switch4kChrRom(reg[2] * 4, 1);
                    }
                }
                
                if ((reg[0] & 0x08) == 0)
                {
                    cpuMemory.Switch32kPrgRom(((reg[3] & (0xF + BASE)) >> 1) * 8);
                }
                else
                {
                    if ((reg[0] & 0x04) == 0x04)
                    {
                        cpuMemory.Switch16kPrgRom((BASE + (reg[3] & 0x0F)) * 4, 0);
                        cpuMemory.Switch16kPrgRom((BASE + 16 - 1) * 4, 1);
                    }
                    else
                    {

                        cpuMemory.Switch16kPrgRom(BASE * 4, 0);
                        cpuMemory.Switch16kPrgRom((BASE + (reg[3] & 0x0F)) * 4, 1);
                    }
                }
            }
        }

        protected override void Initialize(bool initializing)
        {
            reg[0] = 0x0C;
            reg[1] = reg[2] = reg[3] = 0;

            if (cartridge.PrgPages < 32)
            {
                cpuMemory.Switch16kPrgRom(0, 0);
                cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            }
            else
            {
                cpuMemory.Switch16kPrgRom(0, 0);
                cpuMemory.Switch16kPrgRom((15) * 4, 1); MODE = true;
            }

            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
        }

        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(reg); 
            stateStream.Write(SHIFT); 
            stateStream.Write(BUFFER);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            stateStream.Read(reg);
            SHIFT = stateStream.ReadByte();
            BUFFER = stateStream.ReadByte();
            base.LoadState(stateStream);
        }
    }
}
