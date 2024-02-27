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
    public class NesPpu : Component
    {
        private static int[] reverseLookup =
        {
            0x00, 0x80, 0x40, 0xC0, 0x20, 0xA0, 0x60, 0xE0, 0x10, 0x90, 0x50, 0xD0, 0x30, 0xB0, 0x70, 0xF0,
            0x08, 0x88, 0x48, 0xC8, 0x28, 0xA8, 0x68, 0xE8, 0x18, 0x98, 0x58, 0xD8, 0x38, 0xB8, 0x78, 0xF8,
            0x04, 0x84, 0x44, 0xC4, 0x24, 0xA4, 0x64, 0xE4, 0x14, 0x94, 0x54, 0xD4, 0x34, 0xB4, 0x74, 0xF4,
            0x0C, 0x8C, 0x4C, 0xCC, 0x2C, 0xAC, 0x6C, 0xEC, 0x1C, 0x9C, 0x5C, 0xDC, 0x3C, 0xBC, 0x7C, 0xFC,
            0x02, 0x82, 0x42, 0xC2, 0x22, 0xA2, 0x62, 0xE2, 0x12, 0x92, 0x52, 0xD2, 0x32, 0xB2, 0x72, 0xF2,
            0x0A, 0x8A, 0x4A, 0xCA, 0x2A, 0xAA, 0x6A, 0xEA, 0x1A, 0x9A, 0x5A, 0xDA, 0x3A, 0xBA, 0x7A, 0xFA,
            0x06, 0x86, 0x46, 0xC6, 0x26, 0xA6, 0x66, 0xE6, 0x16, 0x96, 0x56, 0xD6, 0x36, 0xB6, 0x76, 0xF6,
            0x0E, 0x8E, 0x4E, 0xCE, 0x2E, 0xAE, 0x6E, 0xEE, 0x1E, 0x9E, 0x5E, 0xDE, 0x3E, 0xBE, 0x7E, 0xFE,
            0x01, 0x81, 0x41, 0xC1, 0x21, 0xA1, 0x61, 0xE1, 0x11, 0x91, 0x51, 0xD1, 0x31, 0xB1, 0x71, 0xF1,
            0x09, 0x89, 0x49, 0xC9, 0x29, 0xA9, 0x69, 0xE9, 0x19, 0x99, 0x59, 0xD9, 0x39, 0xB9, 0x79, 0xF9,
            0x05, 0x85, 0x45, 0xC5, 0x25, 0xA5, 0x65, 0xE5, 0x15, 0x95, 0x55, 0xD5, 0x35, 0xB5, 0x75, 0xF5,
            0x0D, 0x8D, 0x4D, 0xCD, 0x2D, 0xAD, 0x6D, 0xED, 0x1D, 0x9D, 0x5D, 0xDD, 0x3D, 0xBD, 0x7D, 0xFD,
            0x03, 0x83, 0x43, 0xC3, 0x23, 0xA3, 0x63, 0xE3, 0x13, 0x93, 0x53, 0xD3, 0x33, 0xB3, 0x73, 0xF3,
            0x0B, 0x8B, 0x4B, 0xCB, 0x2B, 0xAB, 0x6B, 0xEB, 0x1B, 0x9B, 0x5B, 0xDB, 0x3B, 0xBB, 0x7B, 0xFB,
            0x07, 0x87, 0x47, 0xC7, 0x27, 0xA7, 0x67, 0xE7, 0x17, 0x97, 0x57, 0xD7, 0x37, 0xB7, 0x77, 0xF7,
            0x0F, 0x8F, 0x4F, 0xCF, 0x2F, 0xAF, 0x6F, 0xEF, 0x1F, 0x9F, 0x5F, 0xDF, 0x3F, 0xBF, 0x7F, 0xFF,
        };
        public int[] paletteIndexes =
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
            0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
            0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F,
            0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F,
            0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F,
            0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A, 0x5B, 0x5C, 0x5D, 0x5E, 0x5F,
            0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x6B, 0x6C, 0x6D, 0x6E, 0x6F,
            0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x7B, 0x7C, 0x7D, 0x7E, 0x7F,
        };

        // new variables for cycle accuracy
        private int ioAddr;
        private int ioData;
        private int attr;
        private int bit0;
        private int bit1;
        private int[] bkgPixels = new int[272];
        private int[] objPixels = new int[256];

        // new variables for object evaluation
        private NesPpuObj[] objects = new NesPpuObj[8];
        private Action OamFetch;
        private Action OamPhase;
        private byte oamData;
        private byte[] oamBuffer = new byte[32];
        private int oamSlot;

        private bool chrSwap;
        private bool clipBkg;
        private bool clipObj;
        public bool oamSize;
        private bool obj0Hit;
        private bool oddSwap;
        private bool pullNmi;
        private bool vblFlag;
        private byte chrData;
        private byte oamAddr;
        private int bkgAddr;
        public int chrAddr;
        private int chrFine;
        private int chrStep = 1;
        private int chrTemp;
        private int objAddr;
        private bool objFlow;
        private int palEmph;
        private int palMask;
        public int scanline = -1;
        public int scanlinesPerFrame = 261;

        public bool drawBkg;
        public bool drawObj;

        public RegionFormat Region = RegionFormat.NTSC;
        public bool FrameDone = false;
        public int HClock;
        public int[] ColorBuffer = new int[256 * 240];
        public int[] Palette = NesPalette.NTSCPalette;
        public Action<int> AddressLineUpdating;
        int oamCount = 0;

        bool suppressVbl = false;
        bool suppressNmi = false;

        //for state
        byte value2000;
        byte value2001;

        public NesPpu(NesSystem nesSystem)
            : base(nesSystem)
        { }

        private void MergeBkgBits(int index)
        {
            for (int i = 0; i < 8; i++)
            {
                bkgPixels[index++] = 0x3F00 | attr | (bit0 >> 7 & 0x01) | (bit1 >> 6 & 0x02);

                bit0 = (bit0 << 1);
                bit1 = (bit1 << 1);
            }
        }
        private void MergeObjBits(int index, bool object0, bool infront)
        {
            if (index == 255)
                return;

            for (int i = 0; i < 8; i++, index++)
            {
                if (index > 255)
                    break;

                var pixel = 0x3F10 | attr | (bit0 >> 7 & 0x01) | (bit1 >> 6 & 0x02) | (object0 ? 0x4000 : 0x0000) | (infront ? 0x8000 : 0x0000);

                if ((objPixels[index] & 0x03) == 0 && (pixel & 0x03) != 0)
                    objPixels[index] = pixel;

                bit0 = (bit0 << 1);
                bit1 = (bit1 << 1);
            }
        }

        private void ClockX()
        {
            if ((chrAddr & 0x001F) != 0x001F)
                chrAddr += 0x0001;
            else
                chrAddr ^= 0x041F;
        }
        private void ClockY()
        {
            if ((chrAddr & 0x7000) != 0x7000)
            {
                chrAddr += 0x1000;
            }
            else
            {
                chrAddr ^= 0x7000;

                switch (chrAddr & 0x03E0)
                {
                    case 0x03A0: chrAddr ^= 0x0BA0; break;
                    case 0x03E0: chrAddr ^= 0x03E0; break;
                    default: chrAddr += 0x0020; break;
                }
            }
        }
        private void ResetX()
        {
            chrAddr = (chrAddr & 0x7BE0) | (chrTemp & 0x041F);
        }
        private void ResetY()
        {
            if (scanline == -1)
                chrAddr = chrTemp;
        }

        //bkg fetches
        private void FetchBkgAttr()
        {
            attr = ppuMemory[ioAddr] >> ((chrAddr >> 4 & 0x04) | (chrAddr & 0x02));
            attr = (attr << 2 & 0x0C);
        }
        private void FetchBkgBit0()
        {
            bit0 = ppuMemory[ioAddr];
        }
        private void FetchBkgBit1()
        {
            bit1 = ppuMemory[ioAddr];

            switch (HClock)
            {
                case 327: MergeBkgBits(0x00); break;
                case 335: MergeBkgBits(0x08); break;
                default: MergeBkgBits(HClock + 0x09); break;
            }
        }
        private void FetchBkgName()
        {
            ioData = bkgAddr | (ppuMemory[ioAddr] << 4) | (chrAddr >> 12 & 0x07);
        }
        private void PointBkgAttr()
        {
            ioAddr = 0x23C0 | (chrAddr & 0x0C00) | (chrAddr >> 4 & 0x38) | (chrAddr >> 2 & 0x07);
            AddressLineUpdating(ioAddr);
        }
        private void PointBkgBit0()
        {
            ioAddr = (ioData | 0x00);
            AddressLineUpdating(ioAddr);
        }
        private void PointBkgBit1()
        {
            ioAddr = (ioData | 0x08);
            AddressLineUpdating(ioAddr);
        }
        private void PointBkgName()
        {
            ioAddr = 0x2000 | (chrAddr & 0x0FFF);
            AddressLineUpdating(ioAddr);
        }
        //obj fetches
        private void FetchObjBit0(int obj)
        {
            bit0 = ppuMemory[ioAddr];

            if ((objects[obj].Attr & 0x40) != 0)
                bit0 = reverseLookup[bit0];
        }
        private void FetchObjBit1(int obj)
        {
            bit1 = ppuMemory[ioAddr];

            if ((objects[obj].Attr & 0x40) != 0)
                bit1 = reverseLookup[bit1];

            attr = (objects[obj].Attr << 2 & 0x0C);

            MergeObjBits(objects[obj].XPos, objects[obj].Zero, (objects[obj].Attr & 0x20) == 0);
        }
        private void PointObjBit0(int obj)
        {
            int comparator = (scanline - objects[obj].YPos) ^ ((objects[obj].Attr & 0x80) != 0 ? 0x0F : 0x00);

            if (oamSize)
            {
                ioData =
                    (objects[obj].Name << 0x0C & 0x1000) |
                    (objects[obj].Name << 0x04 & 0x0FE0) |
                    (comparator << 0x01 & 0x0010) |
                    (comparator << 0x00 & 0x0007);
            }
            else
            {
                ioData = objAddr |
                    (objects[obj].Name << 0x04 & 0x0FF0) |
                    (comparator << 0x00 & 0x0007);
            }

            ioAddr = (ioData | 0x00);
            AddressLineUpdating(ioAddr);
        }
        private void PointObjBit1(int obj)
        {
            ioAddr = (ioData | 0x08);
            AddressLineUpdating(ioAddr);
        }

        private void EvaluateFetch()
        {
            oamData = ppuMemory.ObjRam[oamAddr];
        }
        private void EvaluateReset()
        {
            oamData = 0xFF;
        }

        private void EvaluatePhase0()
        {
            if (HClock <= 64)
            {
                switch (HClock >> 1 & 0x03)
                {
                    case 0: objects[HClock >> 3].YPos = 0xFF; break;
                    case 1: objects[HClock >> 3].Name = 0xFF; break;
                    case 2: objects[HClock >> 3].Attr = 0xFF; break;
                    case 3: objects[HClock >> 3].XPos = 0xFF; break;
                }
            }
        }
        private void EvaluatePhase1()
        {
            if (scanline == -1)
                return;

            oamCount++;

            var comparator = (scanline - oamData) & int.MaxValue;

            if (comparator >= (oamSize ? 16 : 8))
            {
                if (oamCount != 64)
                {
                    oamAddr = (byte)(oamCount != 2 ? oamAddr + 4 : 8);
                }
                else
                {
                    oamAddr = 0;
                    OamPhase = EvaluatePhase9;
                }
            }
            else
            {
                oamAddr++;
                OamPhase = EvaluatePhase2;
                objects[oamSlot].YPos = oamData;
                objects[oamSlot].Zero = oamCount == 1;
            }
        }
        private void EvaluatePhase2()
        {
            oamAddr++;
            OamPhase = EvaluatePhase3;
            objects[oamSlot].Name = oamData;
        }
        private void EvaluatePhase3()
        {
            oamAddr++;
            OamPhase = EvaluatePhase4;
            objects[oamSlot].Attr = oamData;
        }
        private void EvaluatePhase4()
        {
            objects[oamSlot].XPos = oamData;
            oamSlot++;

            if (oamCount != 64)
            {
                OamPhase = (oamSlot != 8 ? new Action(EvaluatePhase1) : new Action(EvaluatePhase5));

                if (oamCount != 2)
                {
                    oamAddr++;
                }
                else
                {
                    oamAddr = 8;
                }
            }
            else
            {
                oamAddr = 0;
                OamPhase = EvaluatePhase9;
            }
        }
        private void EvaluatePhase5()
        {
            if (scanline == -1)
                return;

            var comparator = (scanline - oamData) & int.MaxValue;

            if (comparator >= (oamSize ? 16 : 8))
            {
                oamAddr = (byte)(((oamAddr + 4) & 0xFC) + ((oamAddr + 1) & 0x03));

                if (oamAddr <= 5)
                {
                    OamPhase = EvaluatePhase9;
                    oamAddr &= 0xFC;
                }
            }
            else
            {
                OamPhase = EvaluatePhase6;
                oamAddr += (0x01) & 0xFF;
                objFlow = true;
            }
        }
        private void EvaluatePhase6()
        {
            OamPhase = EvaluatePhase7;
            oamAddr += (0x01);
        }
        private void EvaluatePhase7()
        {
            OamPhase = EvaluatePhase8;
            oamAddr += (0x01);
        }
        private void EvaluatePhase8()
        {
            OamPhase = EvaluatePhase9;
            oamAddr += (0x01);

            if ((oamAddr & 0x03) == 0x03)
                oamAddr += (0x01);

            oamAddr &= 0xFC;
        }
        private void EvaluatePhase9()
        {
            oamAddr += (0x04);
        }

        private void EvaluationBegin()
        {
            this.OamFetch = this.EvaluateFetch;
            this.OamPhase = this.EvaluatePhase1;
            this.oamSlot = 0;
            this.oamCount = 0;
        }
        private void EvaluationReset()
        {
            this.OamFetch = this.EvaluateReset;
            this.OamPhase = this.EvaluatePhase0;
            this.oamSlot = 0;
            this.oamAddr = 0;
            this.oamCount = 0;

            for (int i = 0; i < 256; i++)
                this.objPixels[i] = 0;
        }

        private void RenderPixel()
        {
            if (scanline == -1)
                return;
            //Render is enabled here, draw background color at 0x3F00
            byte bkg = (byte)(ppuMemory[0x3F00]);
            ColorBuffer[HClock | (scanline << 8)] = Palette[paletteIndexes[bkg]];

            var bkgPixel = drawBkg ? bkgPixels[HClock + chrFine] : 0;
            var objPixel = drawObj ? objPixels[HClock] : 0;
            var pixel = 0;
            //clipping
            if ((HClock < 8 & clipBkg))
                bkgPixel = 0;
            if ((HClock < 8 & clipObj))
                objPixel = 0;

            if ((bkgPixel & 0x03) == 0)
            {
                pixel = objPixel & 0x3F1F;
                goto render;
            }

            if ((objPixel & 0x03) == 0)
            {
                pixel = bkgPixel;
                goto render;
            }

            if ((objPixel & 0x8000) != 0)
                pixel = objPixel & 0x3F1F;
            else
                pixel = bkgPixel;

            if ((objPixel & 0x4000) != 0 & HClock < 255)
                obj0Hit = true;

        render:
            if ((pixel & 0x03) != 0)
                ColorBuffer[HClock | (scanline << 8)] = Palette[paletteIndexes[ppuMemory[pixel] & palMask | palEmph]];
        }

        protected override void Initialize(bool initializing)
        {
            if (initializing)
            {
                CONSOLE.WriteLine(this, "Initializing PPU.....", DebugStatus.None);
                for (int i = 0x0000; i < 0x2000; i += 8)
                {
                    base.cpuMemory.Map(0x2000 + i, /*******/ Poke2000);
                    base.cpuMemory.Map(0x2001 + i, /*******/ Poke2001);
                    base.cpuMemory.Map(0x2002 + i, Peek2002 /*******/);
                    base.cpuMemory.Map(0x2003 + i, /*******/ Poke2003);
                    base.cpuMemory.Map(0x2004 + i, Peek2004, Poke2004);
                    base.cpuMemory.Map(0x2005 + i, /*******/ Poke2005);
                    base.cpuMemory.Map(0x2006 + i, /*******/ Poke2006);
                    base.cpuMemory.Map(0x2007 + i, Peek2007, Poke2007);
                }

                base.cpuMemory.Map(0x4014, Poke4014);
                if (AddressLineUpdating == null)
                    AddressLineUpdating = new Action<int>(AddressLineupdating);
                paletteIndexes = new int[512];
                for (int i = 0; i < 512; i++)
                    paletteIndexes[i] = i;
            }

            this.EvaluationReset();
            CONSOLE.WriteLine(this, "PPU initialized OK.", DebugStatus.Cool);
            base.Initialize(initializing);
        }
        public void SetPaletteIndexesTable(int[] indexesTable)
        {
            paletteIndexes = indexesTable;
        }

        public void Execute()
        {
            #region Render is enabled
            if (scanline < 240)
            {
                if (drawBkg || drawObj)
                {
                    switch (HClock)
                    {
                        case 000: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 001: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 002: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 003: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 004: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 005: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 006: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 007: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 008: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 009: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 010: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 011: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 012: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 013: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 014: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 015: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 016: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 017: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 018: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 019: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 020: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 021: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 022: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 023: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 024: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 025: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 026: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 027: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 028: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 029: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 030: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 031: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 032: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 033: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 034: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 035: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 036: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 037: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 038: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 039: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 040: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 041: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 042: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 043: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 044: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 045: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 046: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 047: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 048: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 049: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 050: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 051: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 052: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 053: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 054: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 055: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 056: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 057: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 058: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 059: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 060: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 061: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 062: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 063: FetchBkgBit1(); OamPhase(); RenderPixel(); EvaluationBegin(); break;
                        case 064: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 065: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 066: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 067: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 068: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 069: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 070: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 071: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 072: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 073: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 074: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 075: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 076: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 077: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 078: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 079: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 080: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 081: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 082: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 083: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 084: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 085: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 086: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 087: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 088: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 089: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 090: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 091: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 092: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 093: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 094: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 095: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 096: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 097: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 098: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 099: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 100: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 101: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 102: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 103: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 104: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 105: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 106: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 107: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 108: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 109: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 110: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 111: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 112: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 113: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 114: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 115: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 116: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 117: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 118: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 119: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 120: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 121: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 122: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 123: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 124: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 125: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 126: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 127: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 128: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 129: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 130: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 131: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 132: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 133: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 134: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 135: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 136: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 137: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 138: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 139: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 140: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 141: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 142: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 143: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 144: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 145: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 146: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 147: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 148: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 149: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 150: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 151: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 152: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 153: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 154: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 155: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 156: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 157: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 158: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 159: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 160: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 161: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 162: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 163: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 164: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 165: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 166: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 167: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 168: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 169: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 170: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 171: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 172: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 173: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 174: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 175: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 176: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 177: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 178: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 179: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 180: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 181: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 182: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 183: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 184: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 185: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 186: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 187: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 188: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 189: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 190: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 191: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 192: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 193: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 194: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 195: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 196: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 197: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 198: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 199: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 200: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 201: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 202: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 203: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 204: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 205: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 206: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 207: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 208: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 209: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 210: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 211: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 212: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 213: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 214: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 215: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 216: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 217: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 218: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 219: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 220: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 221: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 222: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 223: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 224: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 225: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 226: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 227: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 228: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 229: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 230: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 231: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 232: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 233: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 234: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 235: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 236: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 237: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 238: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 239: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 240: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 241: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 242: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 243: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockX(); break;
                        case 244: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 245: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 246: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 247: FetchBkgBit1(); OamPhase(); RenderPixel(); break;
                        case 248: PointBkgName(); OamFetch(); RenderPixel(); break;
                        case 249: FetchBkgName(); OamPhase(); RenderPixel(); break;
                        case 250: PointBkgAttr(); OamFetch(); RenderPixel(); break;
                        case 251: FetchBkgAttr(); OamPhase(); RenderPixel(); ClockY(); break;
                        case 252: PointBkgBit0(); OamFetch(); RenderPixel(); break;
                        case 253: FetchBkgBit0(); OamPhase(); RenderPixel(); break;
                        case 254: PointBkgBit1(); OamFetch(); RenderPixel(); break;
                        case 255: FetchBkgBit1(); OamPhase(); RenderPixel(); EvaluationReset(); break;

                        case 256: PointBkgName(); break; // garbage
                        case 257: FetchBkgName(); ResetX(); break; // garbage
                        case 258: PointBkgName(); break; // garbage
                        case 259: FetchBkgName(); break; // garbage
                        case 260: PointObjBit0(0x00); break;
                        case 261: FetchObjBit0(0x00); break;
                        case 262: PointObjBit1(0x00); break;
                        case 263: FetchObjBit1(0x00); break;

                        case 264: PointBkgName(); break; // garbage
                        case 265: FetchBkgName(); break; // garbage
                        case 266: PointBkgName(); break; // garbage
                        case 267: FetchBkgName(); break; // garbage
                        case 268: PointObjBit0(0x01); break;
                        case 269: FetchObjBit0(0x01); break;
                        case 270: PointObjBit1(0x01); break;
                        case 271: FetchObjBit1(0x01); break;

                        case 272: PointBkgName(); break; // garbage
                        case 273: FetchBkgName(); break; // garbage
                        case 274: PointBkgName(); break; // garbage
                        case 275: FetchBkgName(); break; // garbage
                        case 276: PointObjBit0(0x02); break;
                        case 277: FetchObjBit0(0x02); break;
                        case 278: PointObjBit1(0x02); break;
                        case 279: FetchObjBit1(0x02); break;

                        case 280: PointBkgName(); break; // garbage
                        case 281: FetchBkgName(); break; // garbage
                        case 282: PointBkgName(); break; // garbage
                        case 283: FetchBkgName(); break; // garbage
                        case 284: PointObjBit0(0x03); break;
                        case 285: FetchObjBit0(0x03); break;
                        case 286: PointObjBit1(0x03); break;
                        case 287: FetchObjBit1(0x03); break;

                        case 288: PointBkgName(); break; // garbage
                        case 289: FetchBkgName(); break; // garbage
                        case 290: PointBkgName(); break; // garbage
                        case 291: FetchBkgName(); break; // garbage
                        case 292: PointObjBit0(0x04); break;
                        case 293: FetchObjBit0(0x04); break;
                        case 294: PointObjBit1(0x04); break;
                        case 295: FetchObjBit1(0x04); break;

                        case 296: PointBkgName(); break; // garbage
                        case 297: FetchBkgName(); break; // garbage
                        case 298: PointBkgName(); break; // garbage
                        case 299: FetchBkgName(); break; // garbage
                        case 300: PointObjBit0(0x05); break;
                        case 301: FetchObjBit0(0x05); break;
                        case 302: PointObjBit1(0x05); break;
                        case 303: FetchObjBit1(0x05); break;

                        case 304: PointBkgName(); ResetY(); break; // garbage
                        case 305: FetchBkgName(); break; // garbage
                        case 306: PointBkgName(); break; // garbage
                        case 307: FetchBkgName(); break; // garbage
                        case 308: PointObjBit0(0x06); break;
                        case 309: FetchObjBit0(0x06); break;
                        case 310: PointObjBit1(0x06); break;
                        case 311: FetchObjBit1(0x06); break;

                        case 312: PointBkgName(); break; // garbage
                        case 313: FetchBkgName(); break; // garbage
                        case 314: PointBkgName(); break; // garbage
                        case 315: FetchBkgName(); break; // garbage
                        case 316: PointObjBit0(0x07); break;
                        case 317: FetchObjBit0(0x07); break;
                        case 318: PointObjBit1(0x07); break;
                        case 319: FetchObjBit1(0x07); break;

                        case 320: PointBkgName(); break;
                        case 321: FetchBkgName(); break;
                        case 322: PointBkgAttr(); break;
                        case 323: FetchBkgAttr(); ClockX(); break;
                        case 324: PointBkgBit0(); break;
                        case 325: FetchBkgBit0(); break;
                        case 326: PointBkgBit1(); break;
                        case 327: FetchBkgBit1(); break;

                        case 328: PointBkgName(); break;
                        case 329: FetchBkgName(); break;
                        case 330: PointBkgAttr(); break;
                        case 331: FetchBkgAttr(); ClockX(); break;
                        case 332: PointBkgBit0(); break;
                        case 333: FetchBkgBit0(); break;
                        case 334: PointBkgBit1(); break;
                        case 335: FetchBkgBit1(); break;
                        // Dummy fetches go here, since they don't provide any real benefit, they don't need to be emulated.
                        // This comment is to acknowledge the existence of these dummy reads.
                        //   336: PointBkgName(); break;
                        //   337: FetchBkgName(); break;
                        //   338: PointBkgName(); break;
                        //   339: FetchBkgName(); break;
                        //   340: /* This is the 'idle' cycle that is skipped on odd frames. */ break;
                    }
                }
            #endregion
                else
                {
                    // Rendering is disabled, draw color at vram address if it in range 0x3F00 - 0x3FFF
                    if (HClock < 255 & scanline >= 0)
                    {
                        int pixel = 0;//this will clear the previous line
                        if ((chrAddr & 0x3F00) == 0x3F00)
                            pixel = Palette[paletteIndexes[ppuMemory[chrAddr & 0x3FFF] & palMask | palEmph]];
                        else
                            pixel = Palette[paletteIndexes[ppuMemory[0x3F00] & palMask | palEmph]];
                        ColorBuffer[HClock | (scanline << 8)] = pixel;
                    }
                }
            }

            nesSystem.Mapper.TickPPUCylceTimer();//Timer tick each ppu cycle.
            HClock++;

            if (HClock == 341)
            {
                HClock = 0;
                scanline++;

                if (nesSystem.Mapper.ScanlineTimerAlwaysActive)
                    nesSystem.Mapper.TickScanlineTimer();

                if (scanline >= 0 && scanline <= 239)
                {
                    if (!nesSystem.Mapper.ScanlineTimerAlwaysActive & (drawBkg || drawObj))
                        nesSystem.Mapper.TickScanlineTimer();
                }

                //Start of vblank
                /*VBL timing based on http://wiki.nesdev.com/w/index.php/PPU_rendering */
                if (scanline == 241)
                {
                    if (!suppressVbl)
                        vblFlag = true;
                    else
                        suppressVbl = false;
                }
                //End of vblank, End of frame
                if (scanline == scanlinesPerFrame)
                {
                    obj0Hit = false;
                    vblFlag = false;
                    objFlow = false;

                    scanline = -1;
                    FrameDone = true;
                }
            }
            //Nmi occur after 2 cycles of vblank
            if (scanline == 241 && HClock == 2)
            {
                if (!suppressNmi)
                {
                    if (pullNmi)
                    {
                        nesSystem.Cpu.NMI(true);
                    }
                }
                else
                {
                    suppressNmi = false;
                }
            }
            //odd frame ?
            if (scanline == -1 && HClock == 339)
            {
                if (Region == RegionFormat.NTSC)
                {
                    oddSwap = !oddSwap;

                    if (!oddSwap & drawBkg)
                    {
                        HClock++;
                    }
                }
            }
        }
        private byte Peek2002(int addr)
        {
            //Read 1 cycle before vblank should suppress setting flag
            if (scanline == 240 & HClock == 340)
            {
                suppressVbl = true; suppressNmi = true;
            }
            //Read 1 cycle before/after vblank should suppress nmi
            if ((scanline == 241 & HClock < 2))
            {
                suppressNmi = true;
            }
            byte status = 0;

            if (vblFlag) status |= 0x80;
            if (obj0Hit) status |= 0x40;
            if (objFlow) status |= 0x20;

            vblFlag = false;
            chrSwap = false;

            return status;
        }
        private byte Peek2004(int addr)
        {
            return ppuMemory.ObjRam[oamAddr];
        }
        private byte Peek2007(int addr)
        {
            byte data = ((chrAddr & 0x3F00) != 0x3F00) ? chrData : (byte)(ppuMemory.PalPeek((chrAddr & 0x3FFF)) & palMask);

            chrData = chrAddr >= 0x2000 ? ppuMemory.NmtPeek(chrAddr) : ppuMemory.ChrPeek(chrAddr);

            chrAddr = (chrAddr + chrStep) & 0x7FFF;

            AddressLineUpdating(chrAddr);

            return data;
        }
        private void Poke2000(int addr, byte data)
        {
            chrTemp = (chrTemp & 0x73FF) | (data << 10 & 0x0C00);
            chrStep = (data & 0x04) != 0 ? 32 : 1;
            objAddr = (data & 0x08) != 0 ? 0x1000 : 0;
            bkgAddr = (data & 0x10) != 0 ? 0x1000 : 0;
            oamSize = (data & 0x20) != 0;
            pullNmi = (data & 0x80) != 0;

            if (pullNmi && ((value2000 & 0x80) == 0) && vblFlag)
                cpu.SetNmiRequest = true;
            if ((scanline == 241 & HClock < 2) && !pullNmi)
            {
                cpu.SetNmiRequest = false;
                cpu.NMI(false);
            }
            value2000 = data;
        }
        private void Poke2001(int addr, byte data)
        {
            palMask = (data & 0x01) != 0 ? 0x30 : 0x3F;
            clipBkg = (data & 0x02) == 0;
            clipObj = (data & 0x04) == 0;
            drawBkg = (data & 0x08) != 0;
            drawObj = (data & 0x10) != 0;
            palEmph = (data & 0xE0) << 1;
            value2001 = data;

        }
        private void Poke2003(int addr, byte data)
        {
            oamAddr = data;
        }
        private void Poke2004(int addr, byte data)
        {
            //ppuMemory.ObjRam[oamAddr] = data;
            //oamAddr++;
            if ((oamAddr & 0x03) == 0x02)
            {
                ppuMemory.ObjRam[oamAddr++] = (byte)(data & 0xE3);
            }
            else
            {
                ppuMemory.ObjRam[oamAddr++] = data;
            }
        }
        private void Poke2005(int addr, byte data)
        {
            if (chrSwap = !chrSwap)
            {
                chrTemp = (chrTemp & 0x7FE0) | (data >> 3 & 0x001F);
                chrFine = (data & 0x0007);
            }
            else
            {
                chrTemp = (chrTemp & 0x0C1F) | (data << 0x0C & 0x7000) | (data << 0x02 & 0x03E0);
            }
        }
        private void Poke2006(int addr, byte data)
        {
            if (chrSwap = !chrSwap)
            {
                chrTemp = (chrTemp & 0x00FF) | (data << 8 & 0x3F00);
            }
            else
            {
                chrTemp = (chrTemp & 0x7F00) | (data << 0 & 0x00FF);
                chrAddr = (chrTemp);
                AddressLineUpdating(chrAddr);
            }
        }
        private void Poke2007(int addr, byte data)
        {
            ppuMemory[(chrAddr & 0x3FFF)] = data;

            int previuos = chrAddr;

            chrAddr = (chrAddr + chrStep) & 0x7FFF;

            AddressLineUpdating(chrAddr);
        }
        private void Poke4014(int addr, byte data)
        {
            addr = (data << 8) & 0xFFFF;
            for (int i = 0; i < 256; i++)
            {
                Poke2004(0x2004, cpuMemory[addr++]);
            }
            nesSystem.Cpu.cyclesAdd += 513;
        }
        //null
        void AddressLineupdating(int test)
        { }

        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(value2000);
            stateStream.Write(value2001);
            stateStream.Write(ioAddr);
            stateStream.Write(ioData);
            stateStream.Write(attr);
            stateStream.Write(bit0);
            stateStream.Write(bit1);
            stateStream.Write(bkgPixels);
            stateStream.Write(objPixels);
            stateStream.Write(oamData);
            stateStream.Write(oamBuffer);
            stateStream.Write(oamSlot);
            stateStream.Write(chrData);
            stateStream.Write(oamAddr);
            stateStream.Write(bkgAddr);
            stateStream.Write(chrAddr);
            stateStream.Write(chrFine);
            stateStream.Write(chrStep);
            stateStream.Write(chrTemp);
            stateStream.Write(objAddr);
            stateStream.Write(scanline);
            stateStream.Write(HClock);
            stateStream.Write(oamCount);

            stateStream.Write(suppressVbl, suppressNmi, chrSwap, vblFlag, objFlow);

            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            value2000 = stateStream.ReadByte();
            Poke2000(0, value2000);
            value2001 = stateStream.ReadByte();
            Poke2001(0, value2001);
            ioAddr = stateStream.ReadInt32();
            ioData = stateStream.ReadInt32();
            attr = stateStream.ReadInt32();
            bit0 = stateStream.ReadInt32();
            bit1 = stateStream.ReadInt32();
            stateStream.Read(bkgPixels);
            stateStream.Read(objPixels);
            oamData = stateStream.ReadByte();
            stateStream.Read(oamBuffer);
            oamSlot = stateStream.ReadInt32();
            chrData = stateStream.ReadByte();
            oamAddr = stateStream.ReadByte();
            bkgAddr = stateStream.ReadInt32();
            chrAddr = stateStream.ReadInt32();
            chrFine = stateStream.ReadInt32();
            chrStep = stateStream.ReadInt32();
            chrTemp = stateStream.ReadInt32();
            objAddr = stateStream.ReadInt32();
            scanline = stateStream.ReadInt32();
            HClock = stateStream.ReadInt32();
            oamCount = stateStream.ReadInt32();

            bool[] status = stateStream.ReadBooleans();
            suppressVbl = status[0];
            suppressNmi = status[1];
            chrSwap = status[2];
            vblFlag = status[3];
            objFlow = status[4];
            base.LoadState(stateStream);
        }
    }
}
