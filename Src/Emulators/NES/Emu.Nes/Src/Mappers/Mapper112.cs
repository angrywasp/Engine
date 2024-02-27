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
    class Mapper112 : Mapper
    {
        byte[] reg = new byte[4];
        int prg0 = 0;
        int prg1 = 1;
        byte chr01 = 0;
        byte chr23 = 2;
        byte chr4 = 4;
        byte chr5 = 5;
        byte chr6 = 6;
        byte chr7 = 7;
        public Mapper112(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int address, byte data)
        {
            switch (address)
            {
                case 0x8000:
                    reg[0] = data;
                    SetPRG();
                    SetCHR();
                    break;
                case 0xA000:
                    reg[1] = data;
                    switch (reg[0] & 0x07)
                    {
                        case 0x00:
                            prg0 = (data & ((cartridge.PrgPages * 4) - 1));
                            SetPRG();
                            break;
                        case 0x01:
                            prg1 = (data & ((cartridge.PrgPages * 4) - 1));
                            SetPRG();
                            break;
                        case 0x02:
                            chr01 = (byte)(data & 0xFE);
                            SetCHR();
                            break;
                        case 0x03:
                            chr23 = (byte)(data & 0xFE);
                            SetCHR();
                            break;
                        case 0x04:
                            chr4 = data;
                            SetCHR();
                            break;
                        case 0x05:
                            chr5 = data;
                            SetCHR();
                            break;
                        case 0x06:
                            chr6 = data;
                            SetCHR();
                            break;
                        case 0x07:
                            chr7 = data;
                            SetCHR();
                            break;
                    }
                    break;

                case 0xC000:
                    reg[3] = data;
                    SetCHR();
                    break;

                case 0xE000:
                    reg[2] = data;
                    if (cartridge.Mirroring != Mirroring.ModeFull)
                    {
                        if ((data & 0x01) != 0)
                            cartridge.Mirroring = Mirroring.ModeHorz;
                        else
                            cartridge.Mirroring = Mirroring.ModeVert;
                    }
                    SetCHR();
                    break;
            }
        }
        protected override void Initialize(bool initializing)
        {
            SetPRG();
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            SetCHR();
        }
        void SetPRG()
        {
            cpuMemory.Switch8kPrgRom(prg0 * 2, 0);
            cpuMemory.Switch8kPrgRom(prg1 * 2, 1);
            cpuMemory.Switch8kPrgRom(((cartridge.PrgPages * 2) - 2) * 2, 2);
            cpuMemory.Switch8kPrgRom(((cartridge.PrgPages * 2) - 1) * 2, 3);

        }
        void SetCHR()
        {
            if ((reg[2] & 0x02) != 0)
            {
                cpuMemory.Switch1kChrRom(chr01, 0);
                cpuMemory.Switch1kChrRom(chr01 + 1, 1);
                cpuMemory.Switch1kChrRom(chr23, 2);
                cpuMemory.Switch1kChrRom(chr23 + 1, 3);
                cpuMemory.Switch1kChrRom(chr4, 4);
                cpuMemory.Switch1kChrRom(chr5, 5);
                cpuMemory.Switch1kChrRom(chr6, 6);
                cpuMemory.Switch1kChrRom(chr7, 7);
            }
            else
            {
                cpuMemory.Switch1kChrRom(((reg[3] << 6) & 0x100) + chr01, 0);
                cpuMemory.Switch1kChrRom(((reg[3] << 6) & 0x100) + chr01 + 1, 1);
                cpuMemory.Switch1kChrRom(((reg[3] << 5) & 0x100) + chr23, 2);
                cpuMemory.Switch1kChrRom(((reg[3] << 5) & 0x100) + chr23 + 1, 3);
                cpuMemory.Switch1kChrRom(((reg[3] << 4) & 0x100) + chr4, 4);
                cpuMemory.Switch1kChrRom(((reg[3] << 3) & 0x100) + chr5, 5);
                cpuMemory.Switch1kChrRom(((reg[3] << 2) & 0x100) + chr6, 6);
                cpuMemory.Switch1kChrRom(((reg[3] << 1) & 0x100) + chr7, 7);
            }
        }
        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(reg);
            stateStream.Write(prg0);
            stateStream.Write(prg1);
            stateStream.Write(chr01);
            stateStream.Write(chr23);
            stateStream.Write(chr4);
            stateStream.Write(chr5);
            stateStream.Write(chr6);
            stateStream.Write(chr7);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            stateStream.Read(reg);
            prg0 = stateStream.ReadInt32();
            prg1 = stateStream.ReadInt32();
            chr01 = stateStream.ReadByte();
            chr23 = stateStream.ReadByte();
            chr4 = stateStream.ReadByte();
            chr5 = stateStream.ReadByte();
            chr6 = stateStream.ReadByte();
            chr7 = stateStream.ReadByte();
            base.LoadState(stateStream);
        }
    }
}
