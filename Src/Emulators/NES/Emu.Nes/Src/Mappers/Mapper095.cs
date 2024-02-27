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
    class Mapper95 : Mapper
    {
        byte reg = 0x00;
        byte prg0 = 0;
        byte prg1 = 1;
        byte chr01 = 0;
        byte chr23 = 2;
        byte chr4 = 4;
        byte chr5 = 5;
        byte chr6 = 6;
        byte chr7 = 7;

        public Mapper95(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int address, byte data)
        {
            switch (address & 0xE001)
            {
                case 0x8000:
                    reg = data;
                    SetBank_CPU();
                    SetBank_PPU();
                    break;
                case 0x8001:
                    if (reg <= 0x05)
                    {
                        if ((data & 0x20) != 0)
                            cartridge.Mirroring = Mirroring.Mode1ScB;
                        else
                            cartridge.Mirroring = Mirroring.Mode1ScA;

                        data &= 0x1F;
                    }

                    switch (reg & 0x07)
                    {
                        case 0x00:
                            chr01 = (byte)(data & 0xFE);
                            SetBank_PPU();
                            break;
                        case 0x01:
                            chr23 = (byte)(data & 0xFE);
                            SetBank_PPU();
                            break;
                        case 0x02:
                            chr4 = data;
                            SetBank_PPU();
                            break;
                        case 0x03:
                            chr5 = data;
                            SetBank_PPU();
                            break;
                        case 0x04:
                            chr6 = data;
                            SetBank_PPU();
                            break;
                        case 0x05:
                            chr7 = data;
                            SetBank_PPU();
                            break;
                        case 0x06:
                            prg0 = data;
                            SetBank_CPU();
                            break;
                        case 0x07:
                            prg1 = data;
                            SetBank_CPU();
                            break;
                    }
                    break;
            }
        }
        protected override void Initialize(bool initializing)
        {
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);

            SetBank_CPU();
            SetBank_PPU();
        }
        void SetBank_CPU()
        {
            if ((reg & 0x40) != 0)
            {
                cpuMemory.Switch8kPrgRom(((cartridge.PrgPages * 2) - 2) * 2, 0);
                cpuMemory.Switch8kPrgRom(prg1 * 2, 1);
                cpuMemory.Switch8kPrgRom(prg0 * 2, 2);
                cpuMemory.Switch8kPrgRom(((cartridge.PrgPages * 2) - 1) * 2, 3);
            }
            else
            {
                cpuMemory.Switch8kPrgRom(prg0 * 2, 0);
                cpuMemory.Switch8kPrgRom(prg1 * 2, 1);
                cpuMemory.Switch8kPrgRom(((cartridge.PrgPages * 2) - 2) * 2, 2);
                cpuMemory.Switch8kPrgRom(((cartridge.PrgPages * 2) - 1) * 2, 3);
            }
        }
        void SetBank_PPU()
        {
            if ((reg & 0x80) != 0)
            {
                cpuMemory.Switch1kChrRom(chr4, 0);
                cpuMemory.Switch1kChrRom(chr5, 1);
                cpuMemory.Switch1kChrRom(chr6, 2);
                cpuMemory.Switch1kChrRom(chr7, 3);
                cpuMemory.Switch1kChrRom(chr01, 4);
                cpuMemory.Switch1kChrRom(chr01 + 1, 5);
                cpuMemory.Switch1kChrRom(chr23, 6);
                cpuMemory.Switch1kChrRom(chr23 + 1, 7);
            }
            else
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
            reg = stateStream.ReadByte();
            prg0 = stateStream.ReadByte();
            prg1 = stateStream.ReadByte();
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