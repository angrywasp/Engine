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
    public class ChannelSqr : NesApuChannel
    {
        private static readonly int[][] DutyForms =
        {
            new int[] {  0, ~0,  0,  0,  0,  0,  0,  0 }, // 12.5%
            new int[] {  0, ~0, ~0,  0,  0,  0,  0,  0 }, // 25.0%
            new int[] {  0, ~0, ~0, ~0, ~0,  0,  0,  0 }, // 50.0%
            new int[] { ~0,  0,  0, ~0, ~0, ~0, ~0, ~0 }, // 75.0% (25.0% negated)
        };

        private NesApuDuration duration;
        private NesApuEnvelope envelope;
        private bool active;
        private bool sweepReload;
        private bool validFrequency;
        private byte dutyForm;
        private byte dutyStep;
        private int enabled;
        private int sweepCount;
        private int sweepRate;
        private int sweepShift;
        private int sweepIncrease;
        private int waveLength;
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
                enabled = value ? ~0 : 0;
                duration.Counter &= enabled;
                active = CanOutput();
            }
        }

        public ChannelSqr(NesSystem nes)
            : base(nes) { duration.Enabled = true; durationEnabledRequest = true; }

        private void UpdateFrequency()
        {
            if (waveLength >= 0x08 && waveLength + (sweepIncrease & waveLength >> sweepShift) <= 0x7FF)
            {
                delay = (waveLength + 1) << 1;
                validFrequency = true;
            }
            else
            {
                validFrequency = false;
            }

            active = CanOutput();
        }
        private bool CanOutput()
        {
            return (duration.Counter != 0) && (envelope.Sound != 0) && validFrequency;
        }

        public void UpdateSweep(int complement)
        {
            if (sweepRate != 0 && --sweepCount == 0)
            {
                sweepCount = sweepRate;

                if (waveLength >= 0x0008)
                {
                    int shifted = (waveLength >> sweepShift);

                    if (sweepIncrease == 0)
                    {
                        waveLength -= shifted + complement;
                        UpdateFrequency();
                    }
                    else if (waveLength + shifted <= 0x07FF)
                    {
                        waveLength += shifted;
                        UpdateFrequency();
                    }
                }
            }

            if (sweepReload)
            {
                sweepReload = false;
                sweepCount = sweepRate;
            }
        }

        public override void ClockHalf()
        {
            duration.Clock();
            active = CanOutput();
        }
        public override void ClockQuad()
        {
            envelope.Clock();
            active = CanOutput();
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
            dutyForm = (byte)((data & 0xC0) >> 6);
            durationEnabledRequest = (data & 0x20) == 0;
            envelope.Looping = (data & 0x20) != 0;
            envelope.Enabled = (data & 0x10) != 0;
            envelope.Sound = (data & 0x0F);
        }
        public override void Poke2(int addr, byte data)
        {
            sweepIncrease = (data & 0x08) != 0 ? 0 : ~0;
            sweepShift = (data & 0x07);
            sweepRate = 0;

            if ((data & (0x80 | 0x07)) > 0x80)
            {
                sweepRate = (data >> 4 & 0x07) + 1;
                sweepReload = true;
            }

            UpdateFrequency();
        }
        public override void Poke3(int addr, byte data)
        {
            waveLength = (waveLength & 0x0700) | (data << 0 & 0x00FF);
            UpdateFrequency();
        }
        public override void Poke4(int addr, byte data)
        {
            waveLength = (waveLength & 0x00FF) | (data << 8 & 0x0700);
            UpdateFrequency();

            durationReload = DurationTable[(data & 0xF8) >> 3] & enabled;
            durationReloadRequest = true;

            envelope.Refresh = true;
            dutyStep = 0;

            active = CanOutput();
        }
        public override int RenderSample(float rate)
        {
            if (active)
            {
                float sum = timer;
                timer -= rate;

                int[] form = DutyForms[dutyForm];

                if (timer >= 0)
                {
                    return (envelope.Sound & form[dutyStep]);
                }
                else
                {
                    sum *= (envelope.Sound & form[dutyStep]);

                    do
                    {
                        sum += Math.Min(-timer, delay) * (envelope.Sound & form[dutyStep = (byte)((dutyStep - 1) & 0x07)]);
                        timer += delay;
                    }
                    while (timer < 0);

                    return (int)(sum / rate);
                }
            }

            return 0;
        }

        public override void SoftReset()
        {
            durationEnabledRequest = true;
        }

        public override void SaveState(StateStream stateStream)
        {
            duration.SaveState(stateStream);
            envelope.SaveState(stateStream);
            stateStream.Write(sweepReload, validFrequency);

            stateStream.Write(dutyForm);
            stateStream.Write(dutyStep);
            stateStream.Write(enabled);
            stateStream.Write(sweepCount);
            stateStream.Write(sweepRate);
            stateStream.Write(sweepShift);
            stateStream.Write(sweepIncrease);
            stateStream.Write(waveLength);
            stateStream.Write(delay);
            stateStream.Write(timer);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            duration.LoadState(stateStream);
            envelope.LoadState(stateStream);

            bool[] status = stateStream.ReadBooleans();
            sweepReload = status[0];
            validFrequency = status[1];

            dutyForm = stateStream.ReadByte();
            dutyStep = stateStream.ReadByte();
            enabled = stateStream.ReadInt32();
            sweepCount = stateStream.ReadInt32();
            sweepRate = stateStream.ReadInt32();
            sweepShift = stateStream.ReadInt32();
            sweepIncrease = stateStream.ReadInt32();
            waveLength = stateStream.ReadInt32();
            delay = stateStream.ReadFloat();
            timer = stateStream.ReadFloat();
            base.LoadState(stateStream);
        }
    }
}