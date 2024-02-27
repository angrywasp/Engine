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
    class Mapper251 : Mapper
    {
        byte[] reg = new byte[11];
        byte[] breg = new byte[4];
        public Mapper251(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int Address, byte data)
        {
            if (Address < 0x8000)
            {
                if ((Address & 0xE001) == 0x6000)
                {
                    if (reg[9] != 0)
                    {
                        breg[reg[10]++] = data;
                        if (reg[10] == 4)
                        {
                            reg[10] = 0;
                            SetBank();
                        }
                    }
                }
            }
            else
            {
                switch (Address & 0xE001)
                {
                    case 0x8000:
                        reg[8] = data;
                        SetBank();
                        break;
                    case 0x8001:
                        reg[reg[8] & 0x07] = data;
                        SetBank();
                        break;
                    case 0xA001:
                        if ((data & 0x80) == 0x80)
                        {
                            reg[9] = 1;
                            reg[10] = 0;
                        }
                        else
                        {
                            reg[9] = 0;
                        }
                        break;
                }
            }
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x4018, 0x7FFF, Poke);
            cpuMemory.Switch16kPrgRom(0, 0);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
        }
        void SetBank()
        {
            int[] chr = new int[6];
            int[] prg = new int[4];

            for (int i = 0; i < 6; i++)
            {
                chr[i] = (reg[i] | (breg[1] << 4)) & ((breg[2] << 4) | 0x0F);
            }

            if ((reg[8] & 0x80) == 0x80)
            {
                cpuMemory.Switch1kChrRom(chr[2], 0);
                cpuMemory.Switch1kChrRom(chr[3], 1);
                cpuMemory.Switch1kChrRom(chr[4], 2);
                cpuMemory.Switch1kChrRom(chr[5], 3);
                cpuMemory.Switch1kChrRom(chr[0], 4);
                cpuMemory.Switch1kChrRom(chr[0] + 1, 5);
                cpuMemory.Switch1kChrRom(chr[1], 6);
                cpuMemory.Switch1kChrRom(chr[1] + 1, 7);
            }
            else
            {

                cpuMemory.Switch1kChrRom(chr[0], 0);
                cpuMemory.Switch1kChrRom(chr[0] + 1, 1);
                cpuMemory.Switch1kChrRom(chr[1], 2);
                cpuMemory.Switch1kChrRom(chr[1] + 1, 3);
                cpuMemory.Switch1kChrRom(chr[2], 4);
                cpuMemory.Switch1kChrRom(chr[3], 5);
                cpuMemory.Switch1kChrRom(chr[4], 6);
                cpuMemory.Switch1kChrRom(chr[5], 7);
            }

            prg[0] = (reg[6] & ((breg[3] & 0x3F) ^ 0x3F)) | (breg[1]);
            prg[1] = (reg[7] & ((breg[3] & 0x3F) ^ 0x3F)) | (breg[1]);
            prg[2] = prg[3] = ((breg[3] & 0x3F) ^ 0x3F) | (breg[1]);
            prg[2] &= cartridge.PrgPages - 1;

            if ((reg[8] & 0x40) == 0x40)
            {
                cpuMemory.Switch8kPrgRom(prg[2] * 2, 0);
                cpuMemory.Switch8kPrgRom(prg[1] * 2, 1);
                cpuMemory.Switch8kPrgRom(prg[0] * 2, 2);
                cpuMemory.Switch8kPrgRom(prg[3] * 2, 3);
            }
            else
            {
                cpuMemory.Switch8kPrgRom(prg[0] * 2, 0);
                cpuMemory.Switch8kPrgRom(prg[1] * 2, 1);
                cpuMemory.Switch8kPrgRom(prg[2] * 2, 2);
                cpuMemory.Switch8kPrgRom(prg[3] * 2, 3);
            }
        }
        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(reg);
            stateStream.Write(breg);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            stateStream.Read(reg);
            stateStream.Read(breg);
            base.LoadState(stateStream);
        }
    }
}
