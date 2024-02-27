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

    class Mapper06 : Mapper
    {
        public bool IRQEnabled = false;
        public int irq_counter = 0;
        public Mapper06(NesSystem nesSystem)
            : base(nesSystem) { }
        public override string Name
        {
            get
            {
                return "FFE F4xxx";
            }
        }
        public override void Poke(int address, byte data)
        {
            if (address >= 0x8000 & address <= 0xFFFF)
            {
                cpuMemory.Switch16kPrgRom(((data & 0x3C) >> 2) * 4, 0);
                cpuMemory.Switch8kChrRom((data & 0x3) * 8);
            }
            else
            {
                if (address < 0x6000)
                {
                    switch (address)
                    {
                        case 0x42FE:
                            if ((data & 0x10) != 0)
                                cartridge.Mirroring = Mirroring.Mode1ScB;
                            else
                                cartridge.Mirroring = Mirroring.Mode1ScA;
                            break;
                        case 0x42FF:
                            if ((data & 0x10) != 0)
                                cartridge.Mirroring = Mirroring.ModeHorz;
                            else
                                cartridge.Mirroring = Mirroring.ModeVert;
                            break;
                        case 0x4501: IRQEnabled = false; cpu.IRQ(NesCpu.IsrType.External, false); break;
                        case 0x4502: irq_counter = (short)((irq_counter & 0xFF00) | data); break;
                        case 0x4503: irq_counter = (short)((data << 8) | (irq_counter & 0x00FF)); IRQEnabled = true; cpu.IRQ(NesCpu.IsrType.External, false); break;
                    }
                }
                else
                {
                    cpuMemory.srm[address - 0x6000] = data;
                }
            }
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x4018, 0x7FFF, Poke);

            cpuMemory.Switch16kPrgRom(0, 0);
            cpuMemory.Switch16kPrgRom(7 * 4, 1);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(512);
            cpuMemory.Switch8kChrRom(0);
        }
        public override void TickScanlineTimer()
        {
            if (IRQEnabled)
            {
                irq_counter += 133;
                if (irq_counter >= 0xFFFF)
                {
                    irq_counter = 0;
                    cpu.IRQ(NesCpu.IsrType.External, true);
                }
            }
        }
        public bool ScanlineTimerNotPauseAtVBLANK { get { return true; } }
        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(IRQEnabled);
            stateStream.Write(irq_counter);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            IRQEnabled = stateStream.ReadBooleans()[0];
            irq_counter = stateStream.ReadInt32();
            base.LoadState(stateStream);
        }
    }
}