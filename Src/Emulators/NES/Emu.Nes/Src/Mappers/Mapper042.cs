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

namespace MyNes.Nes
{
    class Mapper42 : Mapper
    {
        public Mapper42(NesSystem nes)
            : base(nes)
        { }
        int irq_enable = 0;
        int irq_counter = 0;
        int[] SramPrgBank = new int[0x08];
        protected override void Initialize(bool initializing)
        {
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(32);
            cpuMemory.Switch8kChrRom(0);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 2) * 4, 0);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);

            cpuMemory.Map(0x6000, 0x7FFF, PeekSrm, PokeSrm);

            base.Initialize(initializing);
        }
        public override void Poke(int addr, byte data)
        {
            switch (addr & 0xE003)
            {
                case 0xE000:
                    //cpuMemory.Switch8kPrgRom(3, data & 0x0F);
                    SwitchPrgToSram((data & 0x0F) * 2);
                    break;

                case 0xE001:
                    if ((data & 0x08) == 0x08)
                        cartridge.Mirroring = Mirroring.ModeHorz;
                    else
                        cartridge.Mirroring = Mirroring.ModeVert;
                    break;

                case 0xE002:
                    if ((data & 0x02) == 0x02)
                    {
                        irq_enable = 0xFF;
                    }
                    else
                    {
                        irq_enable = 0;
                        irq_counter = 0;
                    }
                    cpu.IRQ(NesCpu.IsrType.External, false);
                    break;
            }
            base.Poke(addr, data);
        }
        public override void TickScanlineTimer()
        {
            cpu.IRQ(NesCpu.IsrType.External, false);
            if (irq_enable != 0)
            {
                if (irq_counter < 215)
                {
                    irq_counter++;
                }
                if (irq_counter == 215)
                {
                    irq_enable = 0;
                    cpu.IRQ(NesCpu.IsrType.External, true);
                }
            }
            base.TickScanlineTimer();
        }
        public override bool ScanlineTimerAlwaysActive
        {
            get
            {
                return true;
            }
        }

        void SwitchPrgToSram(int start)
        {
            start = (start & (cartridge.Prg.Length - 1));

            for (int i = 0; i < 2; i++)
                SramPrgBank[2 * 1 + i] = (start++);
        }
        private byte PeekSrm(int addr)
        {
            if (addr < 0x7000)
                return cartridge.Prg[SramPrgBank[0]][addr & 0x0FFF];
            else
                return cartridge.Prg[SramPrgBank[1]][addr & 0x0FFF];
        }
        private void PokeSrm(int addr, byte data)
        {
            cpuMemory.srm[addr & 0x1FFF] = data;
        }
        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(irq_enable);
            stateStream.Write(irq_counter);
            stateStream.Write(SramPrgBank);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            irq_enable = stateStream.ReadInt32();
            irq_counter = stateStream.ReadInt32();
            stateStream.Read(SramPrgBank);
            base.LoadState(stateStream);
        }
    }
}