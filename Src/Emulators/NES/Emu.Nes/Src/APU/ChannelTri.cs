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
    public class ChannelTri : NesApuChannel
    {
        private const int MIN_FRQ = 3;

        private static readonly byte[] pyramid =
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
            0x0F, 0x0E, 0x0D, 0x0C, 0x0B, 0x0A, 0x09, 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, 0x00,
        };

        private NesApuDuration duration;
        private bool active;
        private bool halted;
        private bool linearControl;
        private int enabled;
        private int linearCounter;
        private int linearCounterLatch;
        private int step;
        private int waveForm;
        private float delay = 1f;
        private float timer;
        bool durationEnabledRequest = false;
        int durationReload = 0;
        bool durationReloadRequest = false;
        public bool Enabled
        {
            get { return duration.Counter != 0; }
            set
            {
                enabled = (value) ? ~0 : 0;
                duration.Counter &= enabled;
            }
        }

        public ChannelTri(NesSystem nes)
            : base(nes) { duration.Enabled = true; durationEnabledRequest = true; }

        private bool CanOutput()
        {
            return (duration.Counter != 0) && (linearCounter != 0) && (waveForm >= MIN_FRQ);
        }

        public override void ClockHalf()
        {
            duration.Clock();
            active = CanOutput();
        }
        public override void ClockQuad()
        {
            if (halted)
            {
                linearCounter = linearCounterLatch;
            }
            else
            {
                if (linearCounter != 0)
                {
                    linearCounter--;
                    active = CanOutput();
                }
            }

            halted &= linearControl;
        }
        public override void ClockCycle(bool isClockingLength)
        {
            if (duration.Enabled != durationEnabledRequest)
                duration.Enabled = durationEnabledRequest;
            if (isClockingLength && duration.Counter > 0)
                durationReloadRequest = false;
            if (durationReloadRequest)
            {
                duration.Counter = durationReload;
                durationReloadRequest = false;
            }
        }
        public override void Poke1(int addr, byte data)
        {
            durationEnabledRequest = (data & 0x80) == 0;

            linearControl = (data & 0x80) != 0;
            linearCounterLatch = (data & 0x7F);
        }
        public override void Poke2(int addr, byte data) { }
        public override void Poke3(int addr, byte data)
        {
            waveForm = (waveForm & 0x0700) | (data << 0 & 0x00FF);
            delay = (waveForm + 1);

            active = CanOutput();
        }
        public override void Poke4(int addr, byte data)
        {
            waveForm = (waveForm & 0x00FF) | (data << 8 & 0x0700);
            delay = (waveForm + 1);

            durationReload = DurationTable[data >> 3] & enabled;
            durationReloadRequest = true;

            halted = true;
            active = CanOutput();
        }
        public override int RenderSample(float rate)
        {
            if (active)
            {
                float sum = timer;
                timer -= rate;

                if (timer >= 0)
                {
                    return pyramid[step];
                }
                else
                {
                    sum *= pyramid[step];

                    do
                    {
                        sum += Math.Min(-timer, delay) * pyramid[step = (byte)((step + 1) & 0x1F)];
                        timer += delay;
                    }
                    while (timer < 0);

                    return (int)(sum / rate);
                }
            }

            return pyramid[step];
        }

        public override void SaveState(StateStream stateStream)
        {
            duration.SaveState(stateStream);
            stateStream.Write(active, halted, linearControl);

            stateStream.Write(enabled);
            stateStream.Write(linearCounter);
            stateStream.Write(linearCounterLatch);
            stateStream.Write(step);
            stateStream.Write(waveForm);
            stateStream.Write(delay);
            stateStream.Write(timer);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            duration.LoadState(stateStream);

            bool[] status = stateStream.ReadBooleans();
            active = status[0];
            halted = status[1];
            linearControl = status[2];

            enabled = stateStream.ReadInt32();
            linearCounter = stateStream.ReadInt32();
            linearCounterLatch = stateStream.ReadInt32();
            step = stateStream.ReadInt32();
            waveForm = stateStream.ReadInt32();
            delay = stateStream.ReadFloat();
            timer = stateStream.ReadFloat();

            base.LoadState(stateStream);
        }
    }
}