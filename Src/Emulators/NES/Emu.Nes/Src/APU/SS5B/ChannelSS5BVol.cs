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
    public class ChannelSs5BVol : NesApuChannel
    {
        private static readonly int[] DutyForm =
        {
             0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            ~0, ~0, ~0, ~0, ~0, ~0, ~0, ~0, ~0, ~0, ~0, ~0, ~0, ~0, ~0, ~0,
        };

        private int volume;
        private int dutyCount;
        private int waveLength;
        private float timer;
        private new float frequency = 1;

        public bool Enabled;

        public ChannelSs5BVol(NesSystem nes)
            : base(nes) { }

        public override void ClockHalf() { }
        public override void ClockQuad() { }
        public override void Poke1(int addr, byte data)
        {
            this.waveLength = (this.waveLength & 0x0F00) | (data << 0 & 0x00FF);
            this.frequency = (this.waveLength + 1);
        }
        public override void Poke2(int addr, byte data)
        {
            this.waveLength = (this.waveLength & 0x00FF) | (data << 8 & 0x0F00);
            this.frequency = (this.waveLength + 1);
        }
        public override void Poke3(int addr, byte data)
        {
            this.volume = (data & 0x0F);
        }
        public override void Poke4(int addr, byte data) { }
        public override int RenderSample(float rate)
        {
            if (Enabled)
            {
                float sum = timer;
                timer -= rate;

                if (timer >= 0)
                {
                    return (volume & DutyForm[dutyCount]);
                }
                else
                {
                    sum *= (volume & DutyForm[dutyCount]);

                    do
                    {
                        sum += Math.Min(-timer, frequency) * (volume & DutyForm[dutyCount = (dutyCount + 1) & 0x1F]);
                        timer += frequency;
                    }
                    while (timer < 0);

                    return (int)(sum / rate);
                }
            }
            else
            {
                return 0;
            }
        }

        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(volume);
            stateStream.Write(dutyCount);
            stateStream.Write(waveLength);
            stateStream.Write(timer);
            stateStream.Write(frequency);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            volume = stateStream.ReadInt32();
            dutyCount = stateStream.ReadInt32();
            waveLength = stateStream.ReadInt32();
            timer = stateStream.ReadFloat();
            frequency = stateStream.ReadFloat();
            base.LoadState(stateStream);
        }
    }
}