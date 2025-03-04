﻿/*********************************************************************\
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNes.Nes
{
    public class CpuDisassembler
    {
        public CpuDisassembler(NesSystem nes)
        {
            this.nes = nes; 
            InstallOpcodeHandlers();
        }
        private delegate string OpcodeHandler();
        private OpcodeHandler[] Opcodes = new OpcodeHandler[256];

        private StringBuilder dSB;
        public ushort dPC;
        private NesSystem nes;
        public string GetRegisters()
        {
            dSB = new StringBuilder();
            dSB.AppendFormat(
                "PC:{0:x4} A:{1:x2} X:{2:x2} Y:{3:x2} S:{4:x2} P:",
                nes.Cpu.RegPC, nes.Cpu.RegA, nes.Cpu.RegX, nes.Cpu.RegY, nes.Cpu.RegS);

            string flags = "nv0bdizcNV1BDIZC";

            for (int i = 0; i < 8; i++)
            {
                dSB.Append(((nes.Cpu.RegSR & (1 << (7 - i))) == 0)
                    ? flags[i] : flags[i + 8]);
            }
            return dSB.ToString();
        }

        public string Disassemble(ushort atAddr)
        {
            int numbytes;
            string inst;
            StringBuilder sb1 = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();

            dPC = atAddr;

            // Add address
            sb1.AppendFormat("{0:x4}: ", dPC);

            // Handle vectors
            switch (atAddr)
            {
                case 0xFFFA:
                case 0xFFFC:
                case 0xFFFE:
                    inst = "";
                    numbytes = 2;
                    dPC += 2;
                    break;
                default:
                    // Get the dissaembly
                    inst = Opcodes[nes.CpuMemory.DebugPeek(dPC++)]();
                    numbytes = dPC - atAddr;
                    break;
            }

            // Add the raw bytes to the disassembly
            for (int i = 0; i < numbytes; i++)
            {
                sb1.AppendFormat("{0:x2} ", nes.CpuMemory.DebugPeek(atAddr++));
            }

            // Append the disassebled line
            sb2.AppendFormat("{0,-15:s}{1:s}\n", sb1.ToString(), inst);

            // Trim trailing newline
            if (sb2.Length > 0) sb2.Length--;

            return sb2.ToString();
        }

        public string MemDump(ushort atAddr, ushort untilAddr)
        {
            dSB = new StringBuilder();

            while (atAddr <= untilAddr)
            {
                dSB.AppendFormat("{0:x4}: ", atAddr);
                for (int i = 0; i < 8; i++)
                {
                    dSB.AppendFormat("{0:x2} ", nes.CpuMemory.DebugPeek(atAddr++));
                    if (i == 3)
                    {
                        dSB.Append(" ");
                    }
                }
                dSB.Append("\n");
            }
            if (dSB.Length > 0)
            {
                dSB.Length--;  // Trim trailing newline
            }
            return dSB.ToString();
        }

        // Relative: Bxx $aa  (branch instructions only)
        string D_REL()
        {
            sbyte bo = (sbyte)nes.CpuMemory.DebugPeek(dPC++);
            ushort ea = (ushort)(dPC + bo);
            return String.Format(" ${0:x4}", ea);
        }

        // Zero Page: $aa 
        string D_ZPG()
        {
            return String.Format(" ${0:x2}", nes.CpuMemory.DebugPeek(dPC++));
        }

        // Zero Page Indexed,X: $aa,X
        string D_ZPX()
        {
            return D_ZPG() + ",X";
        }

        // Zero Page Indexed,Y: $aa,Y
        string D_ZPY()
        {
            return D_ZPG() + ",Y";
        }

        // Absolute: $aaaa
        string D_ABS()
        {
            ushort addr = nes.CpuMemory.DebugPeek(dPC++);

            addr |= (ushort)(nes.CpuMemory.DebugPeek(dPC++) << 8);
            return String.Format(" ${0:x4}", addr);
        }

        // Absolute Indexed,X: $aaaa,X
        string D_ABX()
        {
            return D_ABS() + ",X";
        }

        // Absolute Indexed,Y: $aaaa,Y
        string D_ABY()
        {
            return D_ABS() + ",Y";
        }

        // Indexed Indirect: ($aa,X)
        string D_IDX()
        {
            return String.Format(" (${0:x2},X)", nes.CpuMemory.DebugPeek(dPC++));
        }

        // Indirect Indexed: ($aa),Y
        string D_IDY()
        {
            return String.Format(" (${0:x2}),Y", nes.CpuMemory.DebugPeek(dPC++));
        }

        // Indirect Absolute: ($aaaa)    (FYI:only used by JMP)
        string D_IND()
        {
            ushort addr = nes.CpuMemory.DebugPeek(dPC++);

            addr |= (ushort)(nes.CpuMemory.DebugPeek(dPC++) << 8);
            return String.Format(" (${0:x4})", addr);
        }

        // Immediate: #xx
        string D_IMM()
        {
            return String.Format(" #${0:x2}", nes.CpuMemory.DebugPeek(dPC++));
        }

        string opXX() { return "?ILL?"; }
        string op65() { return "ADC" + D_ZPG(); }
        string op75() { return "ADC" + D_ZPX(); }
        string op61() { return "ADC" + D_IDX(); }
        string op71() { return "ADC" + D_IDY(); }
        string op79() { return "ADC" + D_ABY(); }
        string op6d() { return "ADC" + D_ABS(); }
        string op7d() { return "ADC" + D_ABX(); }
        string op69() { return "ADC" + D_IMM(); }

        string op25() { return "AND" + D_ZPG(); }
        string op35() { return "AND" + D_ZPX(); }
        string op21() { return "AND" + D_IDX(); }
        string op31() { return "AND" + D_IDY(); }
        string op2d() { return "AND" + D_ABS(); }
        string op39() { return "AND" + D_ABY(); }
        string op3d() { return "AND" + D_ABX(); }
        string op29() { return "AND" + D_IMM(); }

        string op06() { return "ASL" + D_ZPG(); }
        string op16() { return "ASL" + D_ZPX(); }
        string op0e() { return "ASL" + D_ABS(); }
        string op1e() { return "ASL" + D_ABX(); }
        string op0a() { return "ASL"; }

        string op24() { return "BIT" + D_ZPG(); }
        string op2c() { return "BIT" + D_ABS(); }

        string op10() { return "BPL" + D_REL(); }
        string op30() { return "BMI" + D_REL(); }
        string op50() { return "BVC" + D_REL(); }
        string op70() { return "BVS" + D_REL(); }
        string op90() { return "BCC" + D_REL(); }
        string opb0() { return "BCS" + D_REL(); }
        string opd0() { return "BNE" + D_REL(); }
        string opf0() { return "BEQ" + D_REL(); }

        string op00() { return "BRK"; }

        string op18() { return "CLC"; }

        string opd8() { return "CLD"; }

        string op58() { return "CLI"; }

        string opb8() { return "CLV"; }

        string opc5() { return "CMP" + D_ZPG(); }
        string opd5() { return "CMP" + D_ZPX(); }
        string opc1() { return "CMP" + D_IDX(); }
        string opd1() { return "CMP" + D_IDY(); }
        string opcd() { return "CMP" + D_ABS(); }
        string opdd() { return "CMP" + D_ABX(); }
        string opd9() { return "CMP" + D_ABY(); }
        string opc9() { return "CMP" + D_IMM(); }

        string ope4() { return "CPX" + D_ZPG(); }
        string opec() { return "CPX" + D_ABS(); }
        string ope0() { return "CPX" + D_IMM(); }

        string opc4() { return "CPY" + D_ZPG(); }
        string opcc() { return "CPY" + D_ABS(); }
        string opc0() { return "CPY" + D_IMM(); }

        string opc6() { return "DEC" + D_ZPG(); }
        string opd6() { return "DEC" + D_ZPX(); }
        string opce() { return "DEC" + D_ABS(); }
        string opde() { return "DEC" + D_ABX(); }

        string opca() { return "DEX"; }

        string op88() { return "DEY"; }

        string op45() { return "EOR" + D_ZPG(); }
        string op55() { return "EOR" + D_ZPX(); }
        string op41() { return "EOR" + D_IDX(); }
        string op51() { return "EOR" + D_IDY(); }
        string op4d() { return "EOR" + D_ABS(); }
        string op5d() { return "EOR" + D_ABX(); }
        string op59() { return "EOR" + D_ABY(); }
        string op49() { return "EOR" + D_IMM(); }

        string ope6() { return "INC" + D_ZPG(); }
        string opf6() { return "INC" + D_ZPX(); }
        string opee() { return "INC" + D_ABS(); }
        string opfe() { return "INC" + D_ABX(); }

        string ope8() { return "INX"; }

        string opc8() { return "INY"; }

        string opa5() { return "LDA" + D_ZPG(); }
        string opb5() { return "LDA" + D_ZPX(); }
        string opa1() { return "LDA" + D_IDX(); }
        string opb1() { return "LDA" + D_IDY(); }
        string opad() { return "LDA" + D_ABS(); }
        string opbd() { return "LDA" + D_ABX(); }
        string opb9() { return "LDA" + D_ABY(); }
        string opa9() { return "LDA" + D_IMM(); }

        string opa6() { return "LDX" + D_ZPG(); }
        string opb6() { return "LDX" + D_ZPY(); }
        string opae() { return "LDX" + D_ABS(); }
        string opbe() { return "LDX" + D_ABY(); }
        string opa2() { return "LDX" + D_IMM(); }

        string opa4() { return "LDY" + D_ZPG(); }
        string opb4() { return "LDY" + D_ZPX(); }
        string opac() { return "LDY" + D_ABS(); }
        string opbc() { return "LDY" + D_ABX(); }
        string opa0() { return "LDY" + D_IMM(); }

        string op46() { return "LSR" + D_ZPG(); }
        string op56() { return "LSR" + D_ZPX(); }
        string op4e() { return "LSR" + D_ABS(); }
        string op5e() { return "LSR" + D_ABX(); }
        string op4a() { return "LSR"; }

        string op4c() { return "JMP" + D_ABS(); }
        string op6c() { return "JMP" + D_IND(); }

        string op20() { return "JSR" + D_ABS(); }

        string opea() { return "NOP"; }

        string op05() { return "ORA" + D_ZPG(); }
        string op15() { return "ORA" + D_ZPX(); }
        string op01() { return "ORA" + D_IDX(); }
        string op11() { return "ORA" + D_IDY(); }
        string op0d() { return "ORA" + D_ABS(); }
        string op1d() { return "ORA" + D_ABX(); }
        string op19() { return "ORA" + D_ABY(); }
        string op09() { return "ORA" + D_IMM(); }

        string op48() { return "PHA"; }

        string op68() { return "PLA"; }

        string op08() { return "PHP"; }

        string op28() { return "PLP"; }

        string op26() { return "ROL" + D_ZPG(); }
        string op36() { return "ROL" + D_ZPX(); }
        string op2e() { return "ROL" + D_ABS(); }
        string op3e() { return "ROL" + D_ABX(); }
        string op2a() { return "ROL"; }

        string op66() { return "ROR" + D_ZPG(); }
        string op76() { return "ROR" + D_ZPX(); }
        string op6e() { return "ROR" + D_ABS(); }
        string op7e() { return "ROR" + D_ABX(); }
        string op6a() { return "ROR"; }

        string op40() { return "RTI"; }

        string op60() { return "RTS"; }

        string ope5() { return "SBC" + D_ZPG(); }
        string opf5() { return "SBC" + D_ZPX(); }
        string ope1() { return "SBC" + D_IDX(); }
        string opf1() { return "SBC" + D_IDY(); }
        string oped() { return "SBC" + D_ABS(); }
        string opfd() { return "SBC" + D_ABX(); }
        string opf9() { return "SBC" + D_ABY(); }
        string ope9() { return "SBC" + D_IMM(); }

        string op38() { return "SEC"; }

        string opf8() { return "SED"; }

        string op78() { return "SEI"; }

        string op85() { return "STA" + D_ZPG(); }
        string op95() { return "STA" + D_ZPX(); }
        string op81() { return "STA" + D_IDX(); }
        string op91() { return "STA" + D_IDY(); }
        string op8d() { return "STA" + D_ABS(); }
        string op99() { return "STA" + D_ABY(); }
        string op9d() { return "STA" + D_ABX(); }

        string op86() { return "STX" + D_ZPG(); }
        string op96() { return "STX" + D_ZPY(); }
        string op8e() { return "STX" + D_ABS(); }

        string op84() { return "STY" + D_ZPG(); }
        string op94() { return "STY" + D_ZPX(); }
        string op8c() { return "STY" + D_ABS(); }

        string opaa() { return "TAX"; }

        string opa8() { return "TAY"; }

        string opba() { return "TSX"; }

        string op8a() { return "TXA"; }

        string op9a() { return "TXS"; }

        string op98() { return "TYA"; }

        private void InstallOpcodeHandlers()
        {
            Opcodes[0x65] = new OpcodeHandler(op65);
            Opcodes[0x75] = new OpcodeHandler(op75);
            Opcodes[0x61] = new OpcodeHandler(op61);
            Opcodes[0x71] = new OpcodeHandler(op71);
            Opcodes[0x79] = new OpcodeHandler(op79);
            Opcodes[0x6d] = new OpcodeHandler(op6d);
            Opcodes[0x7d] = new OpcodeHandler(op7d);
            Opcodes[0x69] = new OpcodeHandler(op69);
            Opcodes[0x25] = new OpcodeHandler(op25);
            Opcodes[0x35] = new OpcodeHandler(op35);
            Opcodes[0x21] = new OpcodeHandler(op21);
            Opcodes[0x31] = new OpcodeHandler(op31);
            Opcodes[0x2d] = new OpcodeHandler(op2d);
            Opcodes[0x39] = new OpcodeHandler(op39);
            Opcodes[0x3d] = new OpcodeHandler(op3d);
            Opcodes[0x29] = new OpcodeHandler(op29);
            Opcodes[0x06] = new OpcodeHandler(op06);
            Opcodes[0x16] = new OpcodeHandler(op16);
            Opcodes[0x0e] = new OpcodeHandler(op0e);
            Opcodes[0x1e] = new OpcodeHandler(op1e);
            Opcodes[0x0a] = new OpcodeHandler(op0a);
            Opcodes[0x24] = new OpcodeHandler(op24);
            Opcodes[0x2c] = new OpcodeHandler(op2c);
            Opcodes[0x10] = new OpcodeHandler(op10);
            Opcodes[0x30] = new OpcodeHandler(op30);
            Opcodes[0x50] = new OpcodeHandler(op50);
            Opcodes[0x70] = new OpcodeHandler(op70);
            Opcodes[0x90] = new OpcodeHandler(op90);
            Opcodes[0xb0] = new OpcodeHandler(opb0);
            Opcodes[0xd0] = new OpcodeHandler(opd0);
            Opcodes[0xf0] = new OpcodeHandler(opf0);
            Opcodes[0x00] = new OpcodeHandler(op00);
            Opcodes[0x18] = new OpcodeHandler(op18);
            Opcodes[0xd8] = new OpcodeHandler(opd8);
            Opcodes[0x58] = new OpcodeHandler(op58);
            Opcodes[0xb8] = new OpcodeHandler(opb8);
            Opcodes[0xc5] = new OpcodeHandler(opc5);
            Opcodes[0xd5] = new OpcodeHandler(opd5);
            Opcodes[0xc1] = new OpcodeHandler(opc1);
            Opcodes[0xd1] = new OpcodeHandler(opd1);
            Opcodes[0xcd] = new OpcodeHandler(opcd);
            Opcodes[0xdd] = new OpcodeHandler(opdd);
            Opcodes[0xd9] = new OpcodeHandler(opd9);
            Opcodes[0xc9] = new OpcodeHandler(opc9);
            Opcodes[0xe4] = new OpcodeHandler(ope4);
            Opcodes[0xec] = new OpcodeHandler(opec);
            Opcodes[0xe0] = new OpcodeHandler(ope0);
            Opcodes[0xc4] = new OpcodeHandler(opc4);
            Opcodes[0xcc] = new OpcodeHandler(opcc);
            Opcodes[0xc0] = new OpcodeHandler(opc0);
            Opcodes[0xc6] = new OpcodeHandler(opc6);
            Opcodes[0xd6] = new OpcodeHandler(opd6);
            Opcodes[0xce] = new OpcodeHandler(opce);
            Opcodes[0xde] = new OpcodeHandler(opde);
            Opcodes[0xca] = new OpcodeHandler(opca);
            Opcodes[0x88] = new OpcodeHandler(op88);
            Opcodes[0x45] = new OpcodeHandler(op45);
            Opcodes[0x55] = new OpcodeHandler(op55);
            Opcodes[0x41] = new OpcodeHandler(op41);
            Opcodes[0x51] = new OpcodeHandler(op51);
            Opcodes[0x4d] = new OpcodeHandler(op4d);
            Opcodes[0x5d] = new OpcodeHandler(op5d);
            Opcodes[0x59] = new OpcodeHandler(op59);
            Opcodes[0x49] = new OpcodeHandler(op49);
            Opcodes[0xe6] = new OpcodeHandler(ope6);
            Opcodes[0xf6] = new OpcodeHandler(opf6);
            Opcodes[0xee] = new OpcodeHandler(opee);
            Opcodes[0xfe] = new OpcodeHandler(opfe);
            Opcodes[0xe8] = new OpcodeHandler(ope8);
            Opcodes[0xc8] = new OpcodeHandler(opc8);
            Opcodes[0xa5] = new OpcodeHandler(opa5);
            Opcodes[0xb5] = new OpcodeHandler(opb5);
            Opcodes[0xa1] = new OpcodeHandler(opa1);
            Opcodes[0xb1] = new OpcodeHandler(opb1);
            Opcodes[0xad] = new OpcodeHandler(opad);
            Opcodes[0xbd] = new OpcodeHandler(opbd);
            Opcodes[0xb9] = new OpcodeHandler(opb9);
            Opcodes[0xa9] = new OpcodeHandler(opa9);
            Opcodes[0xa6] = new OpcodeHandler(opa6);
            Opcodes[0xb6] = new OpcodeHandler(opb6);
            Opcodes[0xae] = new OpcodeHandler(opae);
            Opcodes[0xbe] = new OpcodeHandler(opbe);
            Opcodes[0xa2] = new OpcodeHandler(opa2);
            Opcodes[0xa4] = new OpcodeHandler(opa4);
            Opcodes[0xb4] = new OpcodeHandler(opb4);
            Opcodes[0xac] = new OpcodeHandler(opac);
            Opcodes[0xbc] = new OpcodeHandler(opbc);
            Opcodes[0xa0] = new OpcodeHandler(opa0);
            Opcodes[0x46] = new OpcodeHandler(op46);
            Opcodes[0x56] = new OpcodeHandler(op56);
            Opcodes[0x4e] = new OpcodeHandler(op4e);
            Opcodes[0x5e] = new OpcodeHandler(op5e);
            Opcodes[0x4a] = new OpcodeHandler(op4a);
            Opcodes[0x4c] = new OpcodeHandler(op4c);
            Opcodes[0x6c] = new OpcodeHandler(op6c);
            Opcodes[0x20] = new OpcodeHandler(op20);
            Opcodes[0xea] = new OpcodeHandler(opea);
            Opcodes[0x05] = new OpcodeHandler(op05);
            Opcodes[0x15] = new OpcodeHandler(op15);
            Opcodes[0x01] = new OpcodeHandler(op01);
            Opcodes[0x11] = new OpcodeHandler(op11);
            Opcodes[0x0d] = new OpcodeHandler(op0d);
            Opcodes[0x1d] = new OpcodeHandler(op1d);
            Opcodes[0x19] = new OpcodeHandler(op19);
            Opcodes[0x09] = new OpcodeHandler(op09);
            Opcodes[0x48] = new OpcodeHandler(op48);
            Opcodes[0x68] = new OpcodeHandler(op68);
            Opcodes[0x08] = new OpcodeHandler(op08);
            Opcodes[0x28] = new OpcodeHandler(op28);
            Opcodes[0x26] = new OpcodeHandler(op26);
            Opcodes[0x36] = new OpcodeHandler(op36);
            Opcodes[0x2e] = new OpcodeHandler(op2e);
            Opcodes[0x3e] = new OpcodeHandler(op3e);
            Opcodes[0x2a] = new OpcodeHandler(op2a);
            Opcodes[0x66] = new OpcodeHandler(op66);
            Opcodes[0x76] = new OpcodeHandler(op76);
            Opcodes[0x6e] = new OpcodeHandler(op6e);
            Opcodes[0x7e] = new OpcodeHandler(op7e);
            Opcodes[0x6a] = new OpcodeHandler(op6a);
            Opcodes[0x40] = new OpcodeHandler(op40);
            Opcodes[0x60] = new OpcodeHandler(op60);
            Opcodes[0xe5] = new OpcodeHandler(ope5);
            Opcodes[0xf5] = new OpcodeHandler(opf5);
            Opcodes[0xe1] = new OpcodeHandler(ope1);
            Opcodes[0xf1] = new OpcodeHandler(opf1);
            Opcodes[0xed] = new OpcodeHandler(oped);
            Opcodes[0xfd] = new OpcodeHandler(opfd);
            Opcodes[0xf9] = new OpcodeHandler(opf9);
            Opcodes[0xe9] = new OpcodeHandler(ope9);
            Opcodes[0x38] = new OpcodeHandler(op38);
            Opcodes[0xf8] = new OpcodeHandler(opf8);
            Opcodes[0x78] = new OpcodeHandler(op78);
            Opcodes[0x85] = new OpcodeHandler(op85);
            Opcodes[0x95] = new OpcodeHandler(op95);
            Opcodes[0x81] = new OpcodeHandler(op81);
            Opcodes[0x91] = new OpcodeHandler(op91);
            Opcodes[0x8d] = new OpcodeHandler(op8d);
            Opcodes[0x99] = new OpcodeHandler(op99);
            Opcodes[0x9d] = new OpcodeHandler(op9d);
            Opcodes[0x86] = new OpcodeHandler(op86);
            Opcodes[0x96] = new OpcodeHandler(op96);
            Opcodes[0x8e] = new OpcodeHandler(op8e);
            Opcodes[0x84] = new OpcodeHandler(op84);
            Opcodes[0x94] = new OpcodeHandler(op94);
            Opcodes[0x8c] = new OpcodeHandler(op8c);
            Opcodes[0xaa] = new OpcodeHandler(opaa);
            Opcodes[0xa8] = new OpcodeHandler(opa8);
            Opcodes[0xba] = new OpcodeHandler(opba);
            Opcodes[0x8a] = new OpcodeHandler(op8a);
            Opcodes[0x9a] = new OpcodeHandler(op9a);
            Opcodes[0x98] = new OpcodeHandler(op98);
            for (int i = 0; i < Opcodes.Length; i++)
            {
                if (Opcodes[i] == null)
                {
                    Opcodes[i] = new OpcodeHandler(opXX);
                }
            }
        }
    }
}
