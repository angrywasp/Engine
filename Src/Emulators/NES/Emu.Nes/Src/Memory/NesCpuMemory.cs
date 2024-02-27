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
using MyNes.Nes.Input;
using MyNes.Nes.GameGenie;

namespace MyNes.Nes
{
    public class NesCpuMemory : NesMemory
    {
        public const int AddrBusLines = 16;

        private uint inputData1;
        private uint inputData2;
        private byte inputData3;
        private byte inputData4;
        private byte inputStrobe;
        private byte[] ram = new byte[0x0800];
        public byte[] srm = new byte[0x2000];
        //input devices
        public IJoypad Joypad1;
        public IJoypad Joypad2;
        public IJoypad Joypad3;
        public IJoypad Joypad4;
        public IZapper Zapper;
        public IVSunisystemDIP VSunisystemDIP;

        public bool IsFourPlayers = true;
        public bool IsZapperConnected = false;
        public bool IsVSunisystem = false;
        public bool ResetTrigger;
        public bool SRamReadOnly = false;
        public bool SRamEnabled = true;
        public int[] ChrBank = new int[0x08];
        public int[] PrgBank = new int[0x08];

        public bool IsGameGenieActive;
        public GameGenieCode[] GameGenieCodes;

        public NesCpuMemory(NesSystem nesSystem)
            : base(nesSystem, 1 << AddrBusLines) { }

        /*$4016*/
        private byte PeekPad1(int addr)
        {
            byte data = (byte)(inputData1 & 1);
            inputData1 >>= 1;
            data |= (byte)((inputData3 & 1) << 1);
            inputData3 >>= 1;

            if (IsVSunisystem)
            {
                data |= VSunisystemDIP.GetData4016();
            }
            return data;
        }
        /*$4017*/
        private byte PeekPad2(int addr)
        {
            byte data = (byte)(inputData2 & 1);
            inputData2 >>= 1;
            data |= (byte)((inputData4 & 1) << 1);
            inputData4 >>= 1;
            //zapper
            if (IsZapperConnected)
            {
                data |= (byte)(Zapper.LightDetected ? 0x08 : 0);
                data |= (byte)(Zapper.Trigger ? 0x10 : 0);
            }
            //VS unisystem //TODO: is possible to connect both zapper and vs unisystem ?
            if (IsVSunisystem)
            {
                data |= VSunisystemDIP.GetData4017();
            }
            return data;
        }
        private void PokePad(int addr, byte data)
        {
            if (inputStrobe > (data & 0x01))
            {
                
                if (IsFourPlayers)
                {
                    inputData1 = (uint)(Joypad1.GetData() | (Joypad3.GetData() << 8) | 0x00080000);
                    inputData2 = (uint)(Joypad2.GetData() | (Joypad4.GetData() << 8) | 0x00040000);
                }
                else
                {
                    inputData1 = (uint)Joypad1.GetData();
                    inputData2 = (uint)Joypad2.GetData();
                }
            }

            inputStrobe = (byte)(data & 0x01);

            if (IsVSunisystem)
            {
                Switch8kChrRom(((data & 0x4) >> 2) * 8);
            }
        }
        private byte PeekPrg(int addr)
        {
            int address = PrgBank[addr >> 12 & 0x07];
            byte value = cartridge.Prg[address][addr & 0x0FFF];
            if (IsGameGenieActive && GameGenieCodes != null)
            {
                foreach (GameGenieCode code in GameGenieCodes)
                {
                    if (code.Address == addr)
                    {
                        if (code.Enabled)
                        {
                            if (code.IsCompare)
                            {
                                if (code.Compare == value)
                                    return code.Value;
                            }
                            else
                                return code.Value;
                        }
                        break;
                    }
                }
            }
            return value;
        }
        private void PokePrg(int addr, byte data)
        {
            nesSystem.Mapper.Poke(addr, data);
        }
        private byte PeekRam(int addr)
        {
            return ram[addr & 0x07FF];
        }
        private void PokeRam(int addr, byte data)
        {
            ram[addr & 0x07FF] = data;
        }
        private byte PeekSrm(int addr)
        {
            if (SRamEnabled)
                return srm[addr & 0x1FFF];
            return 0;
        }
        private void PokeSrm(int addr, byte data)
        {
            if (!SRamReadOnly)
                srm[addr & 0x1FFF] = data;
        }

        protected override void Initialize(bool initializing)
        {
            if (initializing)
            {
                CONSOLE.WriteLine(this, "Initializing CPU Memory....", DebugStatus.None);
                base.Map(0x0000, 0x1FFF, PeekRam, PokeRam);
                base.Map(0x6000, 0x7FFF, PeekSrm, PokeSrm);
                base.Map(0x8000, 0xFFFF, PeekPrg, PokePrg);

                base.Map(0x4016, PeekPad1, PokePad);
                base.Map(0x4017, PeekPad2);
                CONSOLE.WriteLine(this, "CPU Memory Initialized OK.", DebugStatus.Cool);
            }

            base.Initialize(initializing);
        }

        public void FillChr(int count)
        {
            cartridge.Chr = new byte[count][];

            for (int i = 0; i < count; i++)
                cartridge.Chr[i] = new byte[1024];
        }

        public void Switch1kChrRom(int start, int area)
        {
            start = (start & (cartridge.Chr.Length - 1));

            ChrBank[area] = (start);
        }
        public void Switch2kChrRom(int start, int area)
        {
            start = (start & (cartridge.Chr.Length - 1));

            for (int i = 0; i < 2; i++)
                ChrBank[2 * area + i] = (start++);
        }
        public void Switch4kChrRom(int start, int area)
        {
            start = (start & (cartridge.Chr.Length - 1));

            for (int i = 0; i < 4; i++)
                ChrBank[4 * area + i] = (start++);
        }
        public void Switch8kChrRom(int start)
        {
            start = (start & (cartridge.Chr.Length - 1));

            for (int i = 0; i < 8; i++)
                ChrBank[i] = (start++);
        }

        public void Switch8kPrgRom(int start, int area)
        {
            start = (start & (cartridge.Prg.Length - 1));

            for (int i = 0; i < 2; i++)
                PrgBank[2 * area + i] = (start++);
        }
        public void Switch16kPrgRom(int start, int area)
        {
            start = (start & (cartridge.Prg.Length - 1));

            for (int i = 0; i < 4; i++)
                PrgBank[4 * area + i] = (start++);
        }
        public void Switch32kPrgRom(int start)
        {
            start = (start & (cartridge.Prg.Length - 1));

            for (int i = 0; i < 8; i++)
                PrgBank[i] = (start++);
        }
        public void Switch8kPrgRomToSRAM(int start)
        {
            switch (cartridge.PrgPages)
            {
                case (2): start = (start & 0x7); break;
                case (4): start = (start & 0xf); break;
                case (8): start = (start & 0x1f); break;
                case (16): start = (start & 0x3f); break;
                case (32): start = (start & 0x7f); break;
                case (64): start = (start & 0xff); break;
                case (128): start = (start & 0x1ff); break;
            }
            cartridge.Prg[start].CopyTo(srm, 0);
            for (int i = 0x1000; i < 0x2000; i++)
            {
                srm[i] = cartridge.Prg[start + 1][i - 0x1000];
            }
        }

        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(inputData1);
            stateStream.Write(inputData2);
            stateStream.Write(inputData3);
            stateStream.Write(inputData4);
            stateStream.Write(inputStrobe);
            stateStream.Write(ram);
            stateStream.Write(srm);
            stateStream.Write(IsFourPlayers, IsZapperConnected, IsVSunisystem, ResetTrigger, SRamReadOnly, SRamEnabled);
            stateStream.Write(ChrBank);
            stateStream.Write(PrgBank);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            inputData1 = stateStream.ReadUInt32();
            inputData2 = stateStream.ReadUInt32();
            inputData3 = stateStream.ReadByte();
            inputData4 = stateStream.ReadByte();
            inputStrobe = stateStream.ReadByte();
            stateStream.Read(ram);
            stateStream.Read(srm);
            bool[] states = stateStream.ReadBooleans();
            IsFourPlayers = states[0];
            IsZapperConnected = states[1];
            IsVSunisystem = states[2];
            ResetTrigger = states[3];
            SRamReadOnly = states[4];
            SRamEnabled = states[5];
            stateStream.Read(ChrBank);
            stateStream.Read(PrgBank);
            base.LoadState(stateStream);
        }

        public override byte DebugPeek(int addr)
        {
            if (addr >= 0x2000 & addr <= 0x4017)
            {
                return 0;
            }
            return base.DebugPeek(addr);
        }
        /// <summary>
        /// Write value to prg !! use this only for Game Genie
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="data"></param>
        public void PRGPoke(int addr, byte data)
        {
            nesSystem.Cartridge.Prg[PrgBank[addr >> 12 & 0x07]][addr & 0x0FFF] = data;
        }
    }
}