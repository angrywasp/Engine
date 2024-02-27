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
   
    class Mapper17 : Mapper
    {
        public bool IRQEnabled = false;
        public int irq_counter = 0;
        public Mapper17(NesSystem nesSystem)
            : base(nesSystem) { }
        public override void Poke(int address, byte data)
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
                case 0x4503: irq_counter = (short)((data << 8) | (irq_counter & 0x00FF)); IRQEnabled = true; break;

                case 0x4504:
                case 0x4505:
                case 0x4506:
                case 0x4507: cpuMemory.Switch8kPrgRom(data * 2, address - 0x4504); break;

                case 0x4510:
                case 0x4511:
                case 0x4512:
                case 0x4513:
                case 0x4514:
                case 0x4515:
                case 0x4516:
                case 0x4517: cpuMemory.Switch1kChrRom(data, address & 0x07); break;
            }
        }
        protected override void Initialize(bool initializing)
        {
            cpuMemory.Map(0x4018, 0x5FFF, Poke);
            cpuMemory.Switch16kPrgRom(0, 0);
            cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            if (cartridge.HasCharRam)
                cpuMemory.FillChr(16);
            cpuMemory.Switch8kChrRom(0);
        }
        public override void TickCycleTimer(int cycles)
        {
            if (IRQEnabled)
            {
                if (irq_counter >= 0xFFFF - 113)
                {
                    cpu.IRQ(NesCpu.IsrType.External, true);
                    irq_counter &= 0xFFFF;
                }
                else
                    irq_counter += (short)cycles;
            }
        }

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
