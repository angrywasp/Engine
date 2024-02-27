/*********************************************************************\
*This file is part of My Nes                                          *
*A Nintendo Entertainment System Emulator.                            *
*                                                                     *
*Copyright © Ala I Hadid 2009 - 2012                                    *
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
using word = System.UInt16;
using System;

namespace MyNes.Nes
{
    public class NesCpu : Component
    {
        public enum IsrType
        {
            Frame = 1,
            Delta = 2,
            External = 4,
        }

        // new variables
        private Action[] codes;
        private Action[] modes;
        private int[] times;
        private PeekAccessor peek;
        private PokeAccessor poke;

        private int addrAddr;
        private int dataAddr;
        byte code;

        public int cycleCounter;
        public int cyclesAdd;

        public int irqRequestFlags;
        bool ExeIRQ;
        bool NmiRequest;
        public bool SetMapperIrqRequest = false;
        public bool SetNmiRequest = false;

        public bool FlagN;
        public bool FlagV;
        public bool FlagD;
        public bool FlagI;
        public bool FlagZ;
        public bool FlagC;

        public byte RegA;
        public byte RegS;
        public byte RegX;
        public byte RegY;
        public ushort RegPC;
        public byte RegSR
        {
            get
            {
                byte data = 0x20;

                if (FlagN) data |= 0x80;
                if (FlagV) data |= 0x40;
                // (FlagR) data |= 0x20;
                // (FlagB) data |= 0x10;
                if (FlagD) data |= 0x08;
                if (FlagI) data |= 0x04;
                if (FlagZ) data |= 0x02;
                if (FlagC) data |= 0x01;

                return data;
            }
            set
            {
                FlagN = (value & 0x80) != 0;
                FlagV = (value & 0x40) != 0;
                FlagD = (value & 0x08) != 0;
                FlagI = (value & 0x04) != 0;
                FlagZ = (value & 0x02) != 0;
                FlagC = (value & 0x01) != 0;
            }
        }

        public NesCpu(NesSystem nesSystem)
            : base(nesSystem) { }

        private void Branch(bool flag)
        {
            if (flag)
            {
                ushort addr = (ushort)(RegPC + (sbyte)peek());

                if ((addr & 0xFF00) != (RegPC & 0xFF00))
                    cycleCounter += 2;
                else
                    cycleCounter++;

                RegPC = addr;
            }
        }
        private word MakeAddr(byte arg1, byte arg2)
        {
            return (word)(arg1 | (arg2 << 8));
        }
        private byte PullByte()
        {
            return cpuMemory[++RegS | 0x0100];
        }
        private word PullWord()
        {
            return MakeAddr(PullByte(), PullByte());
        }
        private void PushByte(int data)
        {
            cpuMemory[RegS-- | 0x0100] = (byte)data;
        }
        private void PushWord(int data)
        {
            PushByte(data >> 8 & 0xFF);
            PushByte(data >> 0 & 0xFF);
        }
        private word ReadWord(word addr)
        {
            return (word)(cpuMemory[addr++] | (cpuMemory[addr++] << 8));
        }

        #region Codes

        private void OpAdc()
        {
            var data = peek();
            var temp = (RegA + data + (FlagC ? 1 : 0));

            FlagN = (temp & 0x80) != 0;
            FlagV = ((temp ^ RegA) & (temp ^ data) & 0x80) != 0;
            FlagZ = (temp & 0xFF) == 0;
            FlagC = (temp >> 0x8) != 0;
            RegA = (byte)(temp);
        }
        private void OpAnd()
        {
            RegA &= peek();
            FlagN = (RegA & 0x80) != 0;
            FlagZ = (RegA & 0xFF) == 0;
        }
        private void OpAsl()
        {
            var data = peek();

            FlagC = (data & 0x80) != 0;

            // TODO: poke();

            data <<= 1;

            FlagN = (data & 0x80) != 0;
            FlagZ = (data & 0xFF) == 0;

            poke(data);
        }
        private void OpBcc()
        {
            Branch(!FlagC);
        }
        private void OpBcs()
        {
            Branch(FlagC);
        }
        private void OpBeq()
        {
            Branch(FlagZ);
        }
        private void OpBit()
        {
            var data = peek();

            FlagN = (data & 0x80) != 0;
            FlagV = (data & 0x40) != 0;
            FlagZ = (data & RegA) == 0;
        }
        private void OpBmi()
        {
            Branch(FlagN);
        }
        private void OpBne()
        {
            Branch(!FlagZ);
        }
        private void OpBpl()
        {
            Branch(!FlagN);
        }
        private void OpBrk()
        {
            PushWord(RegPC + 0x01);
            PushByte(RegSR | 0x10);
            FlagI = true;
            if (!NmiRequest)
            {
                RegPC = ReadWord(0xFFFE);
            }
            else
            {
                SetNmiRequest = NmiRequest = false;
                RegPC = ReadWord(0xFFFA);
            }
        }
        private void OpBvc()
        {
            Branch(!FlagV);
        }
        private void OpBvs()
        {
            Branch(FlagV);
        }
        private void OpClc()
        {
            FlagC = false;
        }
        private void OpCld()
        {
            FlagD = false;
        }
        private void OpCli()
        {
            FlagI = false;
        }
        private void OpClv()
        {
            FlagV = false;
        }
        private void OpCmp()
        {
            var data = (RegA - peek());

            FlagN = (data & 0x80) != 0;
            FlagZ = (data & 0xFF) == 0;
            FlagC = (~data >> 8) != 0;
        }
        private void OpCpx()
        {
            var data = (RegX - peek());

            FlagN = (data & 0x80) != 0;
            FlagZ = (data & 0xFF) == 0;
            FlagC = (~data >> 8) != 0;
        }
        private void OpCpy()
        {
            var data = (RegY - peek());

            FlagN = (data & 0x80) != 0;
            FlagZ = (data & 0xFF) == 0;
            FlagC = (~data >> 8) != 0;
        }
        private void OpDec()
        {
            var data = peek();

            // TODO: poke(data);

            data--;

            FlagN = (data & 0x80) != 0;
            FlagZ = (data & 0xFF) == 0;

            poke(data);
        }
        private void OpDex()
        {
            RegX -= (0x01);
            FlagN = (RegX & 0x80) != 0;
            FlagZ = (RegX & 0xFF) == 0;
        }
        private void OpDey()
        {
            RegY -= (0x01);
            FlagN = (RegY & 0x80) != 0;
            FlagZ = (RegY & 0xFF) == 0;
        }
        private void OpEor()
        {
            RegA ^= peek();
            FlagN = (RegA & 0x80) != 0;
            FlagZ = (RegA & 0xFF) == 0;
        }
        private void OpInc()
        {
            var data = peek();

            // TODO: poke(data);

            data++;

            FlagN = (data & 0x80) != 0;
            FlagZ = (data & 0xFF) == 0;

            poke(data);
        }
        private void OpInx()
        {
            RegX += (0x01);
            FlagN = (RegX & 0x80) != 0;
            FlagZ = (RegX & 0xFF) == 0;
        }
        private void OpIny()
        {
            RegY += (0x01);
            FlagN = (RegY & 0x80) != 0;
            FlagZ = (RegY & 0xFF) == 0;
        }
        private void OpJmp()
        {
            RegPC = (word)dataAddr;
        }
        private void OpJsr()
        {
            PushWord(RegPC - 0x01);
            RegPC = (ushort)dataAddr;
        }
        private void OpLda()
        {
            RegA = peek();

            FlagN = (RegA & 0x80) != 0;
            FlagZ = (RegA & 0xFF) == 0;
        }
        private void OpLdx()
        {
            RegX = peek();
            FlagN = (RegX & 0x80) != 0;
            FlagZ = (RegX & 0xFF) == 0;
        }
        private void OpLdy()
        {
            RegY = peek();
            FlagN = (RegY & 0x80) != 0;
            FlagZ = (RegY & 0xFF) == 0;
        }
        private void OpLsr()
        {
            var data = peek();

            FlagC = (data & 0x01) != 0;

            data >>= 1;

            FlagN = (data & 0x80) != 0;
            FlagZ = (data & 0xFF) == 0;

            poke(data);
        }
        private void OpNop()
        {
        }
        private void OpOra()
        {
            RegA |= peek();
            FlagN = (RegA & 0x80) != 0;
            FlagZ = (RegA & 0xFF) == 0;
        }
        private void OpPha()
        {
            PushByte(RegA);
        }
        private void OpPhp()
        {
            PushByte(RegSR | 0x10);
            //ExeIRQ = (!FlagI && (irqRequestFlags != 0));
        }
        private void OpPla()
        {
            RegA = PullByte();
            FlagN = (RegA & 0x80) != 0;
            FlagZ = (RegA & 0xFF) == 0;
        }
        private void OpPlp()
        {
            RegSR = PullByte();
        }
        private void OpRol()
        {
            var data = peek();
            var temp = (byte)((data << 1) | (FlagC ? 0x01 : 0x00));

            FlagN = (temp & 0x80) != 0;
            FlagZ = (temp & 0xFF) == 0;
            FlagC = (data & 0x80) != 0;

            poke(temp);
        }
        private void OpRor()
        {
            var data = peek();
            var temp = (byte)((data >> 1) | (FlagC ? 0x80 : 0x00));

            FlagN = (temp & 0x80) != 0;
            FlagZ = (temp & 0xFF) == 0;
            FlagC = (data & 0x01) != 0;

            poke(temp);
        }
        private void OpRti()
        {
            RegSR = PullByte();
            RegPC = PullWord();
            ExeIRQ = (!FlagI && (irqRequestFlags != 0));
        }
        private void OpRts()
        {
            RegPC = PullWord();
            RegPC++;
        }
        private void OpSbc()
        {
            var data = peek() ^ 0xFF;
            var temp = (RegA + data + (FlagC ? 1 : 0));

            FlagN = (temp & 0x80) != 0;
            FlagV = ((temp ^ RegA) & (temp ^ data) & 0x80) != 0;
            FlagZ = (temp & 0xFF) == 0;
            FlagC = (temp >> 0x8) != 0;
            RegA = (byte)(temp);
        }
        private void OpSec()
        {
            FlagC = true;
        }
        private void OpSed()
        {
            FlagD = true;
        }
        private void OpSei()
        {
            FlagI = true;
        }
        private void OpSta()
        {
            poke(RegA);
        }
        private void OpStx()
        {
            poke(RegX);
        }
        private void OpSty()
        {
            poke(RegY);
        }
        private void OpTax()
        {
            RegX = RegA;
            FlagN = (RegX & 0x80) != 0;
            FlagZ = (RegX & 0xFF) == 0;
        }
        private void OpTay()
        {
            RegY = RegA;
            FlagN = (RegY & 0x80) != 0;
            FlagZ = (RegY & 0xFF) == 0;
        }
        private void OpTsx()
        {
            RegX = RegS;
            FlagN = (RegX & 0x80) != 0;
            FlagZ = (RegX & 0xFF) == 0;
        }
        private void OpTxa()
        {
            RegA = RegX;
            FlagN = (RegA & 0x80) != 0;
            FlagZ = (RegA & 0xFF) == 0;
        }
        private void OpTxs()
        {
            RegS = RegX;
        }
        private void OpTya()
        {
            RegA = RegY;
            FlagN = (RegA & 0x80) != 0;
            FlagZ = (RegA & 0xFF) == 0;
        }

        /* Unofficial Codes */
        private void OpAnc()
        {
            RegA &= peek();
            FlagN = (RegA & 0x80) != 0;
            FlagZ = (RegA & 0xFF) == 0;
            FlagC = (RegA & 0x80) != 0;
        }
        private void OpArr()
        {
            RegA = (byte)(((peek() & RegA) >> 1) | (FlagC ? 0x80 : 0x00));

            FlagZ = (RegA & 0xFF) == 0;
            FlagN = (RegA & 0x80) != 0;
            FlagC = (RegA & 0x40) != 0;
            FlagV = ((RegA << 1 ^ RegA) & 0x40) != 0;
        }
        private void OpAlr()
        {
            RegA &= peek();

            FlagC = (RegA & 0x01) != 0;

            RegA >>= 1;

            FlagN = (RegA & 0x80) != 0;
            FlagZ = (RegA & 0xFF) == 0;
        }
        private void OpAhx()
        {
            var data = (byte)((RegA & RegX) & 7);

            poke(data);
        }
        private void OpAxs()
        {
            var temp = ((RegA & RegX) - peek());

            FlagN = (temp & 0x80) != 0;
            FlagZ = (temp & 0xFF) == 0;
            FlagC = (~temp >> 8) != 0;

            RegX = (byte)(temp);
        }
        private void OpDcp()
        {
            OpDec();
            OpCmp();
        }
        private void OpDop() { }
        private void OpIsc()
        {
            OpInc();
            OpSbc();
        }
        private void OpJam()
        {
            RegPC++;
        }
        private void OpLar()
        {
            RegS &= peek();
            RegA = RegS;
            RegX = RegS;

            FlagN = (RegS & 0x80) != 0;
            FlagZ = (RegS & 0xFF) == 0;
        }
        private void OpLax()
        {
            OpLda();
            OpLdx();
        }
        private void OpRla()
        {
            OpRol();
            OpAnd();
        }
        private void OpRra()
        {
            OpRor();
            OpAdc();
        }
        private void OpSax()
        {
            poke(
                (byte)(RegX & RegA));
        }
        private void OpShx() { }
        private void OpShy() { }
        private void OpSlo()
        {
            OpAsl();
            OpOra();
        }
        private void OpSre()
        {
            OpLsr();
            OpEor();
        }
        private void OpTop() { }
        private void OpXaa()
        {
            RegA = (byte)(RegX & peek());
            FlagN = (RegA & 0x80) != 0;
            FlagZ = (RegA & 0xFF) == 0;
        }
        private void OpXas()
        {
            RegS = (byte)(RegA & RegX & ((peek() >> 8) + 1));

            poke(RegS);
        }

        #endregion
        #region Modes

        byte PeekMem()
        {
            var data = cpuMemory[dataAddr];
            return data;
        }
        void PokeMem(byte data)
        {
            cpuMemory[dataAddr] = data;
        }

        void AmAbs()
        {
            dataAddr = (cpuMemory[RegPC + 0] | (cpuMemory[RegPC + 1] << 8));
            peek = PeekMem;
            poke = PokeMem;

            RegPC += 2;
        }
        void AmAbX()
        {
            addrAddr = (cpuMemory[RegPC + 0] | (cpuMemory[RegPC + 1] << 8));
            dataAddr = (addrAddr + RegX) & 0xFFFF;

            peek = PeekMem;
            poke = PokeMem;

            byte low = (byte)(addrAddr & 0x00FF);
            byte high = (byte)((addrAddr & 0xFF00) >> 8);
            low = (byte)(low + RegX);
            addrAddr = low | high << 8;

            if (low < RegX)
            {
                byte dummy = cpuMemory[addrAddr];
                high++;
                addrAddr = low | high << 8;
            }

            RegPC += 2;
        }
        void AmAbX_C()
        {
            addrAddr = (cpuMemory[RegPC + 0] | (cpuMemory[RegPC + 1] << 8));
            dataAddr = (addrAddr + RegX) & 0xFFFF;

            peek = PeekMem;
            poke = PokeMem;

            byte low = (byte)(addrAddr & 0x00FF);
            byte high = (byte)((addrAddr & 0xFF00) >> 8);
            low = (byte)(low + RegX);
            addrAddr = low | high << 8;

            if (low < RegX)
            {
                byte dummy = cpuMemory[addrAddr];
                high++;
                addrAddr = low | high << 8;
                cycleCounter++;
            }

            RegPC += 2;
        }
        void AmAbX_D()
        {
            /*Special case for ROL 0x3E, always does dummy reads !!*/
            addrAddr = (cpuMemory[RegPC + 0] | (cpuMemory[RegPC + 1] << 8));
            dataAddr = (addrAddr + RegX) & 0xFFFF;

            peek = PeekMem;
            poke = PokeMem;

            byte low = (byte)(addrAddr & 0x00FF);
            byte high = (byte)((addrAddr & 0xFF00) >> 8);
            low = (byte)(low + RegX);
            addrAddr = low | high << 8;

            byte dummy1 = cpuMemory[addrAddr];
            high++;
            addrAddr = low | high << 8;

            RegPC += 2;
        }
        void AmAbY()
        {
            addrAddr = (cpuMemory[RegPC + 0] | (cpuMemory[RegPC + 1] << 8));
            dataAddr = (addrAddr + RegY) & 0xFFFF;
            peek = PeekMem;
            poke = PokeMem;

            byte low = (byte)(addrAddr & 0x00FF);
            byte high = (byte)((addrAddr & 0xFF00) >> 8);
            low = (byte)(low + RegY);
            addrAddr = low | high << 8;

            if (low < RegY)
            {
                byte dummy = cpuMemory[addrAddr];
                high++;
                addrAddr = low | high << 8;
            }

            RegPC += 2;
        }
        void AmAbY_C()
        {
            addrAddr = (cpuMemory[RegPC + 0] | (cpuMemory[RegPC + 1] << 8));
            dataAddr = (addrAddr + RegY) & 0xFFFF;
            peek = PeekMem;
            poke = PokeMem;

            byte low = (byte)(addrAddr & 0x00FF);
            byte high = (byte)((addrAddr & 0xFF00) >> 8);
            low = (byte)(low + RegY);
            addrAddr = low | high << 8;

            if (low < RegY)
            {
                byte dummy = cpuMemory[addrAddr];
                high++;
                addrAddr = low | high << 8;
                cycleCounter++;
            }

            RegPC += 2;
        }
        void AmAcc()
        {
            peek = () => RegA;
            poke = (data) => RegA = data;
        }
        void AmImm()
        {
            dataAddr = (RegPC + 0);
            peek = PeekMem;
            poke = PokeMem;

            RegPC += 1;
        }
        void AmImp()
        {
        }
        void AmInd()
        {
            addrAddr = (cpuMemory[RegPC + 0] | (cpuMemory[RegPC + 1] << 8));

            dataAddr = (cpuMemory[((addrAddr + 0) & 0xFF) | (addrAddr & 0xFF00)] |
                       (cpuMemory[((addrAddr + 1) & 0xFF) | (addrAddr & 0xFF00)] << 8));
        }
        void AmInX()
        {
            addrAddr = (cpuMemory[RegPC + 0]);
            dataAddr =
                (cpuMemory[(addrAddr + RegX + 0) & 0xFF] |
                (cpuMemory[(addrAddr + RegX + 1) & 0xFF] << 8));
            peek = PeekMem;
            poke = PokeMem;

            RegPC += 1;
        }
        void AmInY()
        {
            addrAddr = (cpuMemory[RegPC + 0]);
            addrAddr =
                (cpuMemory[(addrAddr + 0) & 0xFF] |
                (cpuMemory[(addrAddr + 1) & 0xFF] << 8));
            dataAddr = (addrAddr + RegY) & 0xFFFF;

            peek = PeekMem;
            poke = PokeMem;

            byte low = (byte)(addrAddr & 0x00FF);
            byte high = (byte)((addrAddr & 0xFF00) >> 8);
            low = (byte)(low + RegY);
            addrAddr = low | high << 8;

            if (low < RegY)
            {
                byte dummy = cpuMemory[addrAddr];
                high++;
                addrAddr = low | high << 8;
            }

            RegPC += 1;
        }
        void AmInY_C()
        {
            addrAddr = (cpuMemory[RegPC + 0]);
            addrAddr =
                (cpuMemory[(addrAddr + 0) & 0xFF] |
                (cpuMemory[(addrAddr + 1) & 0xFF] << 8));
            dataAddr = (addrAddr + RegY) & 0xFFFF;

            peek = PeekMem;
            poke = PokeMem;

            byte low = (byte)(addrAddr & 0x00FF);
            byte high = (byte)((addrAddr & 0xFF00) >> 8);
            low = (byte)(low + RegY);
            addrAddr = low | high << 8;

            if (low < RegY)
            {
                byte dummy = cpuMemory[addrAddr];
                high++;
                addrAddr = low | high << 8;
                cycleCounter++;
            }

            RegPC += 1;
        }
        void AmZpg()
        {
            dataAddr = (cpuMemory[RegPC]);
            peek = PeekMem;
            poke = PokeMem;

            RegPC += 1;
        }
        void AmZpX()
        {
            dataAddr = (cpuMemory[RegPC] + RegX) & 0xFF;
            peek = PeekMem;
            poke = PokeMem;

            RegPC += 1;
        }
        void AmZpY()
        {
            dataAddr = (cpuMemory[RegPC] + RegY) & 0xFF;
            peek = PeekMem;
            poke = PokeMem;

            RegPC += 1;
        }

        #endregion

        protected override void Initialize(bool initializing)
        {
            if (initializing)
            {
                CONSOLE.WriteLine(this, "Initializing CPU....", DebugStatus.None);
                codes = new Action[256]
                {
                    OpBrk, OpOra, OpJam, OpSlo, OpDop, OpOra, OpAsl, OpSlo, OpPhp, OpOra, OpAsl, OpAnc, OpTop, OpOra, OpAsl, OpSlo,
                    OpBpl, OpOra, OpJam, OpSlo, OpDop, OpOra, OpAsl, OpSlo, OpClc, OpOra, OpNop, OpSlo, OpTop, OpOra, OpAsl, OpSlo,
                    OpJsr, OpAnd, OpJam, OpRla, OpBit, OpAnd, OpRol, OpRla, OpPlp, OpAnd, OpRol, OpAnc, OpBit, OpAnd, OpRol, OpRla,
                    OpBmi, OpAnd, OpJam, OpRla, OpDop, OpAnd, OpRol, OpRla, OpSec, OpAnd, OpNop, OpRla, OpTop, OpAnd, OpRol, OpRla,
                    OpRti, OpEor, OpJam, OpSre, OpDop, OpEor, OpLsr, OpSre, OpPha, OpEor, OpLsr, OpAlr, OpJmp, OpEor, OpLsr, OpSre,
                    OpBvc, OpEor, OpJam, OpSre, OpDop, OpEor, OpLsr, OpSre, OpCli, OpEor, OpNop, OpSre, OpTop, OpEor, OpLsr, OpSre,
                    OpRts, OpAdc, OpJam, OpRra, OpDop, OpAdc, OpRor, OpRra, OpPla, OpAdc, OpRor, OpArr, OpJmp, OpAdc, OpRor, OpRra,
                    OpBvs, OpAdc, OpJam, OpRra, OpDop, OpAdc, OpRor, OpRra, OpSei, OpAdc, OpNop, OpRra, OpTop, OpAdc, OpRor, OpRra,
                    OpDop, OpSta, OpDop, OpSax, OpSty, OpSta, OpStx, OpSax, OpDey, OpDop, OpTxa, OpXaa, OpSty, OpSta, OpStx, OpSax,
                    OpBcc, OpSta, OpJam, OpAhx, OpSty, OpSta, OpStx, OpSax, OpTya, OpSta, OpTxs, OpXas, OpShy, OpSta, OpShx, OpAhx,
                    OpLdy, OpLda, OpLdx, OpLax, OpLdy, OpLda, OpLdx, OpLax, OpTay, OpLda, OpTax, OpLax, OpLdy, OpLda, OpLdx, OpLax,
                    OpBcs, OpLda, OpJam, OpLax, OpLdy, OpLda, OpLdx, OpLax, OpClv, OpLda, OpTsx, OpLar, OpLdy, OpLda, OpLdx, OpLax,
                    OpCpy, OpCmp, OpDop, OpDcp, OpCpy, OpCmp, OpDec, OpDcp, OpIny, OpCmp, OpDex, OpAxs, OpCpy, OpCmp, OpDec, OpDcp,
                    OpBne, OpCmp, OpJam, OpDcp, OpDop, OpCmp, OpDec, OpDcp, OpCld, OpCmp, OpNop, OpDcp, OpTop, OpCmp, OpDec, OpDcp,
                    OpCpx, OpSbc, OpDop, OpIsc, OpCpx, OpSbc, OpInc, OpIsc, OpInx, OpSbc, OpNop, OpSbc, OpCpx, OpSbc, OpInc, OpIsc,
                    OpBeq, OpSbc, OpJam, OpIsc, OpDop, OpSbc, OpInc, OpIsc, OpSed, OpSbc, OpNop, OpIsc, OpTop, OpSbc, OpInc, OpIsc, 
                };
                modes = new Action[256]
                {
                    //0    1        2      3      4      5      6      7      8      9        A      B      C        D         E       F
                    AmImp, AmInX,   AmImp, AmInX, AmZpg, AmZpg, AmZpg, AmZpg, AmImp, AmImm,   AmAcc, AmImm, AmAbs,   AmAbs,   AmAbs,   AmAbs,//0
                    AmImm, AmInY_C, AmImp, AmInY, AmZpX, AmZpX, AmZpX, AmZpX, AmImp, AmAbY_C, AmImp, AmAbY, AmAbX_C, AmAbX_C, AmAbX,   AmAbX,//1
                    AmAbs, AmInX,   AmImp, AmInX, AmZpg, AmZpg, AmZpg, AmZpg, AmImp, AmImm,   AmAcc, AmImm, AmAbs,   AmAbs,   AmAbs,   AmAbs,//2
                    AmImm, AmInY_C, AmImp, AmInY, AmZpX, AmZpX, AmZpX, AmZpX, AmImp, AmAbY_C, AmImp, AmAbY, AmAbX_C, AmAbX_C, AmAbX_D, AmAbX,//3
                    AmImp, AmInX,   AmImp, AmInX, AmZpg, AmZpg, AmZpg, AmZpg, AmImp, AmImm,   AmAcc, AmImm, AmAbs,   AmAbs,   AmAbs,   AmAbs,//4
                    AmImm, AmInY_C, AmImp, AmInY, AmZpX, AmZpX, AmZpX, AmZpX, AmImp, AmAbY_C, AmImp, AmAbY, AmAbX_C, AmAbX_C, AmAbX,   AmAbX,//5
                    AmImp, AmInX,   AmImp, AmInX, AmZpg, AmZpg, AmZpg, AmZpg, AmImp, AmImm,   AmAcc, AmImm, AmInd,   AmAbs,   AmAbs,   AmAbs,//6
                    AmImm, AmInY_C, AmImp, AmInY, AmZpX, AmZpX, AmZpX, AmZpX, AmImp, AmAbY_C, AmImp, AmAbY, AmAbX_C, AmAbX_C, AmAbX,   AmAbX,//7
                    AmImm, AmInX,   AmImm, AmInX, AmZpg, AmZpg, AmZpg, AmZpg, AmImp, AmImm,   AmImp, AmImm, AmAbs,   AmAbs,   AmAbs,   AmAbs,//8
                    AmImm, AmInY,   AmImp, AmInY, AmZpX, AmZpX, AmZpY, AmZpY, AmImp, AmAbY,   AmImp, AmAbY, AmAbX,   AmAbX,   AmAbY,   AmAbY,//9
                    AmImm, AmInX,   AmImm, AmInX, AmZpg, AmZpg, AmZpg, AmZpg, AmImp, AmImm,   AmImp, AmImm, AmAbs,   AmAbs,   AmAbs,   AmAbs,//A
                    AmImm, AmInY_C, AmImp, AmInY, AmZpX, AmZpX, AmZpY, AmZpY, AmImp, AmAbY_C, AmImp, AmAbY, AmAbX_C, AmAbX_C, AmAbY_C, AmAbY,//B
                    AmImm, AmInX,   AmImm, AmInX, AmZpg, AmZpg, AmZpg, AmZpg, AmImp, AmImm,   AmImp, AmImm, AmAbs,   AmAbs,   AmAbs,   AmAbs,//C
                    AmImm, AmInY_C, AmImp, AmInY, AmZpX, AmZpX, AmZpX, AmZpX, AmImp, AmAbY_C, AmImp, AmAbY, AmAbX_C, AmAbX_C, AmAbX,   AmAbX,//D
                    AmImm, AmInX,   AmImm, AmInX, AmZpg, AmZpg, AmZpg, AmZpg, AmImp, AmImm,   AmImp, AmImm, AmAbs,   AmAbs,   AmAbs,   AmAbs,//E
                    AmImm, AmInY_C, AmImp, AmInY, AmZpX, AmZpX, AmZpX, AmZpX, AmImp, AmAbY_C, AmImp, AmAbY, AmAbX_C, AmAbX_C, AmAbX,   AmAbX,//F
                };
                times = new int[256]
                {
                    7, 6, 0, 8, 3, 3, 5, 5, 3, 2, 2, 2, 4, 4, 6, 6,
                    2, 5, 0, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
                    6, 6, 0, 8, 3, 3, 5, 5, 4, 2, 2, 2, 4, 4, 6, 6,
                    2, 5, 0, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
                    6, 6, 0, 8, 3, 3, 5, 5, 3, 2, 2, 2, 3, 4, 6, 6,
                    2, 5, 0, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
                    6, 6, 0, 8, 3, 3, 5, 5, 4, 2, 2, 2, 5, 4, 6, 6,
                    2, 5, 0, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
                    2, 6, 2, 6, 3, 3, 3, 3, 2, 2, 2, 2, 4, 4, 4, 4,
                    2, 6, 0, 6, 4, 4, 4, 4, 2, 5, 2, 5, 5, 5, 5, 5,
                    2, 6, 2, 6, 3, 3, 3, 3, 2, 2, 2, 2, 4, 4, 4, 4,
                    2, 5, 0, 5, 4, 4, 4, 4, 2, 4, 2, 4, 4, 4, 4, 4,
                    2, 6, 2, 8, 3, 3, 5, 5, 2, 2, 2, 2, 4, 4, 6, 6,
                    2, 5, 0, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
                    2, 6, 2, 8, 3, 3, 5, 5, 2, 2, 2, 2, 4, 4, 6, 6,
                    2, 5, 0, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7, 
                };

            }
            cpuMemory[0x08] = 0xF7;
            cpuMemory[0x09] = 0xEF;
            cpuMemory[0x0A] = 0xDF;
            cpuMemory[0x0F] = 0xBF;

            RegA = 0x00;
            RegS = 0xFD;
            RegX = 0x00;
            RegY = 0x00;

            RegPC = ReadWord(0xFFFC);
            RegSR = 0x34;

            CONSOLE.WriteLine(this, "CPU Initialized OK.", DebugStatus.Cool);
            base.Initialize(initializing);
        }

        public int Execute()
        {
            cycleCounter = cyclesAdd; 
            cyclesAdd = 0;
            //check for irq !?
            ExeIRQ = (!FlagI && (irqRequestFlags != 0));

            //fetch opcode
            code = cpuMemory[RegPC++];
            //do addressing mode
            modes[code]();
            //do opcode
            codes[code]();
            //timing
            cycleCounter += times[code];

            if (NmiRequest)
            {
                SetNmiRequest = NmiRequest = false; 
                PushWord(RegPC);
                PushByte(RegSR & 0xEF);
                FlagI = true;
                RegPC = ReadWord(0xFFFA);
                cycleCounter += 7;
            }
            else if (ExeIRQ)
            {
                PushWord(RegPC);
                PushByte(RegSR & 0xEF);
                FlagI = true;
                RegPC = ReadWord(0xFFFE);
                cycleCounter += 7;
            }

            if (SetNmiRequest)
            {
                NmiRequest = true;
                SetNmiRequest = false;
            }
            if (SetMapperIrqRequest)
            {
                SetMapperIrqRequest = false;
                IRQ(IsrType.External, true);
            }
            return cycleCounter;
        }
        public void SoftReset()
        {
            if (!nesSystem.CpuMemory.ResetTrigger)
            {
                FlagI = true;
                RegS -= 3;
                RegPC = ReadWord(0xFFFC);
                nesSystem.Mapper.SoftReset();
                apu.SoftReset();
            }
            else
            {
                nesSystem.Mapper.SoftReset();
                nesSystem.Mapper.Initialize();
            }
            CONSOLE.WriteLine(this, "SOFT RESET", DebugStatus.Warning);
        }

        /// <summary>
        /// Request IRQ
        /// </summary>
        /// <param name="type"></param>
        /// <param name="asserted"></param>
        public void IRQ(IsrType type, bool asserted)
        {
            if (asserted)
                irqRequestFlags |= (int)type;
            else
                irqRequestFlags &= ~(int)type;
        }

        /// <summary>
        /// Request NMI
        /// </summary>
        public void NMI(bool asserted)
        {
            NmiRequest = asserted;
        }

        public override void SaveState(StateStream stateStream)
        {
            //registers
            stateStream.Write(RegA);
            stateStream.Write(RegPC);
            stateStream.Write(RegS);
            stateStream.Write(RegSR);
            stateStream.Write(RegX);
            stateStream.Write(RegY);

            stateStream.Write(addrAddr);
            stateStream.Write(dataAddr);
            stateStream.Write(code);
            stateStream.Write(cycleCounter);
            stateStream.Write(irqRequestFlags);

            stateStream.Write(NmiRequest);

            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            //registers
            RegA = stateStream.ReadByte();
            RegPC = stateStream.ReadUshort();
            RegS = stateStream.ReadByte();
            RegSR = stateStream.ReadByte();
            RegX = stateStream.ReadByte();
            RegY = stateStream.ReadByte();

            addrAddr = stateStream.ReadInt32();
            dataAddr = stateStream.ReadInt32();
            code = stateStream.ReadByte();
            cycleCounter = stateStream.ReadInt32();
            irqRequestFlags = stateStream.ReadInt32();

            bool[] status = stateStream.ReadBooleans();
            NmiRequest = status[0];

            base.LoadState(stateStream);
        }
    }
}

