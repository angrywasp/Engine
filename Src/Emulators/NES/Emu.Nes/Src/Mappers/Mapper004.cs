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
using System;
namespace MyNes.Nes
{
    class Mapper04 : Mapper
    {
        byte commandNumber;
        byte prgAddressSelect;
        byte chrAddressSelect;
        bool irqEnabled;
        int irqCounter;
        int irqRefresh = 0xFF;
        bool mmc3_alt_behavior = false;//true=rev A, false= rev B
        bool clear = false;
        private int oldA12;
        private int newA12;
        private int timer;

        byte prg0 = 0;
        byte prg1 = 1;
        byte chr0, chr1, chr2, chr3, chr4, chr5 = 0;

        public Mapper04(NesSystem nesSystem)
            : base(nesSystem) { }
        public override string Name
        {
            get
            {
                return "Nintendo MMC3";
            }
        }
        public override void Poke(int address, byte data)
        {
            address &= 0xE001;

            if (address == 0x8000)
            {
                commandNumber = (byte)(data & 0x7);
                prgAddressSelect = (byte)(data & 0x40);
                chrAddressSelect = (byte)(data & 0x80);
                SetPRG(); SetCHR();
            }
            else if (address == 0x8001)
            {
                if (commandNumber == 0)
                {

                    chr0 = (byte)(data & 0xFE);
                    SetCHR();
                }
                else if (commandNumber == 1)
                {

                    chr1 = (byte)(data & 0xFE);
                    SetCHR();
                }
                else if (commandNumber == 2)
                {
                    chr2 = (byte)(data & (cartridge.ChrPages * 8 - 1)); SetCHR();
                }
                else if (commandNumber == 3)
                {
                    chr3 = data; SetCHR();
                }
                else if (commandNumber == 4)
                {
                    chr4 = data; SetCHR();
                }
                else if (commandNumber == 5)
                {
                    chr5 = data; SetCHR();
                }
                else if (commandNumber == 6)
                {
                    prg0 = data; SetPRG();
                }
                else if (commandNumber == 7)
                {
                    prg1 = data; SetPRG();
                }
            }
            else if (address == 0xA000)
            {
                if (cartridge.Mirroring != Mirroring.ModeFull)
                {
                    if ((data & 0x1) == 0)
                        cartridge.Mirroring = Mirroring.ModeVert;
                    else
                        cartridge.Mirroring = Mirroring.ModeHorz;
                }
            }
            else if (address == 0xA001)
            {
                cpuMemory.SRamReadOnly = ((data & 0x40) == 0x40);
                cpuMemory.SRamEnabled = ((data & 0x80) == 0x80);
            }

            else if (address == 0xC000)
            {
                irqRefresh = data;
            }
            else if (address == 0xC001)
            {
                if (mmc3_alt_behavior)
                    clear = true;
                irqCounter = 0;
            }
            else if (address == 0xE000)
            {
                irqEnabled = false;
                cpu.IRQ(NesCpu.IsrType.External, false);
            }
            else if (address == 0xE001)
            {
                irqEnabled = true;
            }
        }
        protected override void Initialize(bool initializing)
        {
            prgAddressSelect = 0;
            chrAddressSelect = 0;

            if (cartridge.HasCharRam)
                cpuMemory.FillChr(8);

            ppu.AddressLineUpdating = PPU_AddressLineUpdating;
            cpuMemory.Switch4kChrRom(0, 0);
            SetPRG();

            //set irq type depending on chip type
            if (cartridge.DataBaseInfo.Game_Name != null)
            {
                switch (cartridge.DataBaseInfo.chip_type[0].ToLower())
                {
                    case "mmc3a": mmc3_alt_behavior = true; CONSOLE.WriteLine(this, "Chip= MMC3 A, MMC3 IQR mode switched to RevA", DebugStatus.Warning); break;
                    case "mmc3b": mmc3_alt_behavior = false; CONSOLE.WriteLine(this, "Chip= MMC3 B, MMC3 IQR mode switched to RevB", DebugStatus.Warning); break;
                    case "mmc3c": mmc3_alt_behavior = false; CONSOLE.WriteLine(this, "Chip= MMC3 C, MMC3 IQR mode switched to RevB", DebugStatus.Warning); break;
                }
            }
        }
        public override void TickPPUCylceTimer()
        {
            timer++;
            base.TickPPUCylceTimer();
        }
        void SetPRG()
        {
            if (prgAddressSelect != 0)
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
        void SetCHR()
        {
            if (cartridge.HasCharRam)
            {
                if (chrAddressSelect == 0)
                {
                    cpuMemory.Switch1kChrRom((chr0 + 0) & 0x07, 0);
                    cpuMemory.Switch1kChrRom((chr0 + 1) & 0x07, 1);
                    cpuMemory.Switch1kChrRom((chr1 + 0) & 0x07, 2);
                    cpuMemory.Switch1kChrRom((chr1 + 1) & 0x07, 3);
                    cpuMemory.Switch1kChrRom(chr2 & 0x07, 4);
                    cpuMemory.Switch1kChrRom(chr3 & 0x07, 5);
                    cpuMemory.Switch1kChrRom(chr4 & 0x07, 6);
                    cpuMemory.Switch1kChrRom(chr5 & 0x07, 7);
                }
                else
                {
                    cpuMemory.Switch1kChrRom((chr0 + 0) & 0x07, 4);
                    cpuMemory.Switch1kChrRom((chr0 + 1) & 0x07, 5);
                    cpuMemory.Switch1kChrRom((chr1 + 0) & 0x07, 6);
                    cpuMemory.Switch1kChrRom((chr1 + 1) & 0x07, 7);
                    cpuMemory.Switch1kChrRom(chr2 & 0x07, 0);
                    cpuMemory.Switch1kChrRom(chr3 & 0x07, 1);
                    cpuMemory.Switch1kChrRom(chr4 & 0x07, 2);
                    cpuMemory.Switch1kChrRom(chr5 & 0x07, 3);
                }
            }
            else
            {
                if (chrAddressSelect == 0)
                {
                    cpuMemory.Switch1kChrRom(chr0, 0);
                    cpuMemory.Switch1kChrRom(chr0 + 1, 1);
                    cpuMemory.Switch1kChrRom(chr1, 2);
                    cpuMemory.Switch1kChrRom(chr1 + 1, 3);
                    cpuMemory.Switch1kChrRom(chr2, 4);
                    cpuMemory.Switch1kChrRom(chr3, 5);
                    cpuMemory.Switch1kChrRom(chr4, 6);
                    cpuMemory.Switch1kChrRom(chr5, 7);
                }
                else
                {
                    cpuMemory.Switch1kChrRom(chr2, 0);
                    cpuMemory.Switch1kChrRom(chr3, 1);
                    cpuMemory.Switch1kChrRom(chr4, 2);
                    cpuMemory.Switch1kChrRom(chr5, 3);
                    cpuMemory.Switch1kChrRom(chr0, 4);
                    cpuMemory.Switch1kChrRom(chr0 + 1, 5);
                    cpuMemory.Switch1kChrRom(chr1, 6);
                    cpuMemory.Switch1kChrRom(chr1 + 1, 7);
                }
            }
        }

        private void PPU_AddressLineUpdating(int addr)
        {
            oldA12 = newA12;
            newA12 = addr & 0x1000;

            if (oldA12 < newA12)
            {
                if (timer > 16)
                {
                    int old = irqCounter;

                    if (irqCounter == 0 || clear)
                        irqCounter = irqRefresh;
                    else
                        irqCounter = irqCounter - 1;

                    if ((!mmc3_alt_behavior || old != 0 || clear) && irqCounter == 0 && irqEnabled)
                        cpu.SetMapperIrqRequest = true;

                    clear = false;
                }

                timer = 0;
            }
        }

        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(commandNumber);
            stateStream.Write(prgAddressSelect);
            stateStream.Write(chrAddressSelect);
            stateStream.Write(irqEnabled, clear, false, false, false, false, false, false);
            stateStream.Write(irqCounter);
            stateStream.Write(irqRefresh);
            stateStream.Write(oldA12);
            stateStream.Write(newA12);
            stateStream.Write(timer);
            stateStream.Write(prg0);
            stateStream.Write(prg1);
            stateStream.Write(chr0);
            stateStream.Write(chr1);
            stateStream.Write(chr2);
            stateStream.Write(chr3);
            stateStream.Write(chr4);
            stateStream.Write(chr5);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            commandNumber = stateStream.ReadByte();
            prgAddressSelect = stateStream.ReadByte();
            chrAddressSelect = stateStream.ReadByte();
            bool[] vals = stateStream.ReadBooleans();
            irqEnabled = vals[0];
            clear = vals[1];

            irqCounter = stateStream.ReadInt32();
            irqRefresh = stateStream.ReadInt32();
            oldA12 = stateStream.ReadInt32();
            newA12 = stateStream.ReadInt32();
            timer = stateStream.ReadInt32();
            prg0 = stateStream.ReadByte();
            prg1 = stateStream.ReadByte();
            chr0 = stateStream.ReadByte();
            chr1 = stateStream.ReadByte();
            chr2 = stateStream.ReadByte();
            chr3 = stateStream.ReadByte();
            chr4 = stateStream.ReadByte();
            chr5 = stateStream.ReadByte();

            base.LoadState(stateStream);
        }
    }
}