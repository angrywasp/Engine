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
    class Mapper68 : Mapper
    {
        byte[] reg = new byte[4];
        byte coin = 0;
        public Mapper68(NesSystem nes)
            : base(nes)
        { }

        public override void Poke(int addr, byte data)
        {
            switch (addr & 0xF000)
            {
                case 0x8000:
                    cpuMemory.Switch2kChrRom(data * 2, 0);
                    break;
                case 0x9000:
                    cpuMemory.Switch2kChrRom(data * 2, 1);
                    break;
                case 0xA000:
                    cpuMemory.Switch2kChrRom(data * 2, 2);
                    break;
                case 0xB000:
                    cpuMemory.Switch2kChrRom(data * 2, 3);
                    break;

                case 0xC000:
                    reg[2] = data;
                    SetBank();
                    break;
                case 0xD000:
                    reg[3] = data;
                    SetBank();
                    break;
                case 0xE000:
                    reg[0] = (byte)((data & 0x10) >> 4);
                    reg[1] = (byte)(data & 0x03);
                    SetBank();
                    break;

                case 0xF000:
                    cpuMemory.Switch16kPrgRom(data * 4, 0);
                    break;
            }
            base.Poke(addr, data);
        }
        protected override void Initialize(bool initializing)
        {
            ppuMemory.Map(0x2000, 0x3EFF, NmtPeek, NmtPoke);
            cpuMemory.Map(0x4020, Peek4020, Poke4020);
            cpuMemory.Switch16kPrgRom(0, 0);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            // cpuMemory.FillCRAM(32);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
            base.Initialize(initializing);
        }
        void SetBank()
        {
            if (reg[0] != 0)
            {
                switch (reg[1])
                {
                    case 0:
                        Switch1kCHRToVRAM(reg[2] + 0x80, 0);
                        Switch1kCHRToVRAM(reg[3] + 0x80, 1);
                        Switch1kCHRToVRAM(reg[2] + 0x80, 2);
                        Switch1kCHRToVRAM(reg[3] + 0x80, 3);
                        break;
                    case 1:
                        Switch1kCHRToVRAM(reg[2] + 0x80, 0);
                        Switch1kCHRToVRAM(reg[2] + 0x80, 1);
                        Switch1kCHRToVRAM(reg[3] + 0x80, 2);
                        Switch1kCHRToVRAM(reg[3] + 0x80, 3);
                        break;
                    case 2:
                        Switch1kCHRToVRAM(reg[2] + 0x80, 0);
                        Switch1kCHRToVRAM(reg[2] + 0x80, 1);
                        Switch1kCHRToVRAM(reg[2] + 0x80, 2);
                        Switch1kCHRToVRAM(reg[2] + 0x80, 3);
                        break;
                    case 3:
                        Switch1kCHRToVRAM(reg[3] + 0x80, 0);
                        Switch1kCHRToVRAM(reg[3] + 0x80, 1);
                        Switch1kCHRToVRAM(reg[3] + 0x80, 2);
                        Switch1kCHRToVRAM(reg[3] + 0x80, 3);
                        break;
                }
            }
            else
            {
                switch (reg[1])
                {
                    case 0:
                        cartridge.Mirroring = Mirroring.ModeVert;
                        break;
                    case 1:
                        cartridge.Mirroring = Mirroring.ModeHorz;
                        break;
                    case 2:
                        cartridge.Mirroring = Mirroring.Mode1ScA;
                        break;
                    case 3:
                        cartridge.Mirroring = Mirroring.Mode1ScB;
                        break;
                }
            }
        }
        void Switch1kCHRToVRAM(int start, int area)
        {
            switch (cartridge.ChrPages)
            {
                case (2): start = (start & 0xf); break;
                case (4): start = (start & 0x1f); break;
                case (8): start = (start & 0x3f); break;
                case (16): start = (start & 0x7f); break;
                case (32): start = (start & 0xff); break;
                case (64): start = (start & 0x1ff); break;
                case (128): start = (start & 0x1ff); break;
            }
            ppuMemory.nmtBank[area] = (byte)start;
        }
        private byte NmtPeek(int addr)
        {
            if (ppuMemory.nmtBank[(addr & 0x0C00) >> 10] < 4)
                return ppuMemory.nmt[ppuMemory.nmtBank[addr >> 10 & 0x03]][addr & 0x03FF];
            else
                return cartridge.Chr[ppuMemory.nmtBank[(addr & 0x0C00) >> 10]][addr & 0x3FF];
        }
        private void NmtPoke(int addr, byte data)
        {
            if (ppuMemory.nmtBank[(addr & 0x0C00) >> 10] < 4)
                ppuMemory.nmt[ppuMemory.nmtBank[addr >> 10 & 0x03]][addr & 0x03FF] = data;
        }
        private void Poke4020(int addr, byte data)
        { coin = data; }
        private byte Peek4020(int addr)
        { return coin; }
        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(reg); stateStream.Write(coin);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            stateStream.Read(reg);
            coin = stateStream.ReadByte();
            base.LoadState(stateStream);
        }
    }
}
