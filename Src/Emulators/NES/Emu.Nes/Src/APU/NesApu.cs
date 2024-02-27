/*********************************************************************\
*This file is part of My Nes                                          *
*A Nintendo Entertainment System Emulator.                            *
*                                                                     *
*Copyright © Ala I Hadid 2009 - 2012                                  *
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
using MyNes.Nes.Output.Audio;

namespace MyNes.Nes
{
    public class NesApu : Component
    {
        private const float OutputMax = sbyte.MaxValue;
        private const float OutputMin = sbyte.MinValue;
        private const float OutputMul = 256;
        private const float OutputSub = 128;
        private int[] SequenceMode0 = { 7459, 7456, 7458, 7457, 1, 1, 1, 7456 };
        private int[] SequenceMode1 = { 1, 7458, 7456, 7458, 14910 };

        public int rPos;
        public int wPos;
        private int[] soundBuffer = new int[44100];
        public float sampleDelay = (1789772f / 44100.00f);
        public float ClockFreq = 1789772f;
        private float sampleTimer;

        public ChannelSqr ChannelSq1;
        public ChannelSqr ChannelSq2;
        public ChannelTri ChannelTri;
        public ChannelNoi ChannelNoi;
        public ChannelDpm ChannelDpm;
        public ExternalComponent External;
        public bool FrameIrqEnabled;
        public bool FrameIrqFlag;
        public bool SequencingMode;
        public int Cycles = 0;
        byte CurrentSeq = 0;
        bool oddCycle = false;

        public NesApu(NesSystem nesSystem)
            : base(nesSystem)
        {
            ChannelSq1 = new ChannelSqr(nesSystem);
            ChannelSq2 = new ChannelSqr(nesSystem);
            ChannelTri = new ChannelTri(nesSystem);
            ChannelNoi = new ChannelNoi(nesSystem);
            ChannelDpm = new ChannelDpm(nesSystem);
        }

        public void SetNesFrequency(float frequency)
        {
            ClockFreq = frequency;
            sampleDelay = (frequency / 44100.00f);
        }
        private void AddSample(float sampleRate)
        {
            int tndSample = 0;
            int sqrSample = 0;

            if (this.ChannelSq1.Audible) sqrSample += this.ChannelSq1.RenderSample(sampleRate);
            if (this.ChannelSq2.Audible) sqrSample += this.ChannelSq2.RenderSample(sampleRate);

            if (this.ChannelTri.Audible) tndSample += this.ChannelTri.RenderSample(sampleRate) * 3;
            if (this.ChannelNoi.Audible) tndSample += this.ChannelNoi.RenderSample(sampleRate) * 2;
            if (this.ChannelDpm.Audible) tndSample += this.ChannelDpm.RenderSample() * 1;

            float output = NesApuMixer.MixSamples(sqrSample, tndSample) * OutputMul;

            if (this.External != null)
            {
                output += this.External.RenderSample(sampleRate);
                output *= (OutputMul / (OutputMul + External.MaxOutput));
            }

            if (output > OutputMul)
                output = OutputMul;
            if (output < 0)
                output = 0;

            this.soundBuffer[wPos++ % this.soundBuffer.Length] = (int)(output - OutputSub);
            /*int output = (int)(NesApuMixer.MixSamples(sqrSample, tndSample) * 128);

            if (this.External != null)
                output += this.External.RenderSample(sampleRate);

            soundBuffer[wPos++ % soundBuffer.Length] = output;*/
         
        }
        private void ClockHalf()
        {
            ChannelSq1.UpdateSweep(1);
            ChannelSq2.UpdateSweep(0);
            ChannelSq1.ClockHalf();
            ChannelSq2.ClockHalf();
            ChannelNoi.ClockHalf();
            ChannelTri.ClockHalf();

            if (External != null)
                External.ClockHalf();
        }
        private void ClockQuad()
        {
            ChannelSq1.ClockQuad();
            ChannelSq2.ClockQuad();
            ChannelNoi.ClockQuad();
            ChannelTri.ClockQuad();

            if (External != null)
                External.ClockQuad();
        }
        void ClockChannelsCycle(bool isClockingLength)
        {
            ChannelSq1.ClockCycle(isClockingLength);
            ChannelSq2.ClockCycle(isClockingLength);
            ChannelNoi.ClockCycle(isClockingLength);
            ChannelTri.ClockCycle(isClockingLength);
            ChannelDpm.ClockCycle(isClockingLength);
        }

        protected override void Initialize(bool initializing)
        {
            if (initializing)
            {
                CONSOLE.WriteLine(this, "Initializing pAPU...", DebugStatus.None);
                // Channel Sq1
                cpuMemory.Map(0x4000, ChannelSq1.Poke1);
                cpuMemory.Map(0x4001, ChannelSq1.Poke2);
                cpuMemory.Map(0x4002, ChannelSq1.Poke3);
                cpuMemory.Map(0x4003, ChannelSq1.Poke4);

                // Channel Sq2
                cpuMemory.Map(0x4004, ChannelSq2.Poke1);
                cpuMemory.Map(0x4005, ChannelSq2.Poke2);
                cpuMemory.Map(0x4006, ChannelSq2.Poke3);
                cpuMemory.Map(0x4007, ChannelSq2.Poke4);

                // Channel Tri
                cpuMemory.Map(0x4008, ChannelTri.Poke1);
                cpuMemory.Map(0x4009, ChannelTri.Poke2);
                cpuMemory.Map(0x400A, ChannelTri.Poke3);
                cpuMemory.Map(0x400B, ChannelTri.Poke4);

                // Channel Noi
                cpuMemory.Map(0x400C, ChannelNoi.Poke1);
                cpuMemory.Map(0x400D, ChannelNoi.Poke2);
                cpuMemory.Map(0x400E, ChannelNoi.Poke3);
                cpuMemory.Map(0x400F, ChannelNoi.Poke4);

                // Channel Dpm
                cpuMemory.Map(0x4010, ChannelDpm.Poke1);
                cpuMemory.Map(0x4011, ChannelDpm.Poke2);
                cpuMemory.Map(0x4012, ChannelDpm.Poke3);
                cpuMemory.Map(0x4013, ChannelDpm.Poke4);

                cpuMemory.Map(0x4015, Peek4015, Poke4015);
                cpuMemory.Map(0x4017, /*******/ Poke4017);

                FrameIrqEnabled = true;
                Cycles = SequenceMode0[0] - 11;

                CONSOLE.WriteLine(this, "pAPU initialized OK.", DebugStatus.Cool);
            }

            base.Initialize(initializing);
        }

        public void Execute()
        {
            Cycles--;
            oddCycle = !oddCycle;
            bool isClockingLength = false;
            if (Cycles == 0)
            {
                if (!SequencingMode)
                {
                    switch (CurrentSeq)
                    {
                        case 0:
                        case 2: ClockQuad(); break;

                        case 1: ClockQuad(); ClockHalf(); isClockingLength = true; break;

                        case 3:
                        case 5: if (FrameIrqEnabled) FrameIrqFlag = true; break;

                        case 4: if (FrameIrqEnabled) FrameIrqFlag = true; ClockQuad(); ClockHalf(); isClockingLength = true; break;

                        case 6: if (FrameIrqFlag) cpu.IRQ(NesCpu.IsrType.Frame, true); break;
                    }
                    CurrentSeq++;
                    Cycles = SequenceMode0[CurrentSeq];
                    if (CurrentSeq == 7)
                        CurrentSeq = 0;
                }
                else
                {
                    switch (CurrentSeq)
                    {
                        case 0:
                        case 2: ClockQuad(); ClockHalf(); isClockingLength = true; break;
                        case 1:
                        case 3: ClockQuad(); break;
                    } 
                    CurrentSeq++;
                    Cycles = SequenceMode1[CurrentSeq];
                    if (CurrentSeq == 4)
                        CurrentSeq = 0;
                }
            }
            ClockChannelsCycle(isClockingLength);
            if (sampleTimer > 0)
            {
                sampleTimer--;
            }
            else
            {
                sampleTimer += sampleDelay;
                AddSample(sampleDelay);
            }
        }
        public int PullSample()
        {
            while (rPos >= wPos)
            {
                AddSample(sampleDelay);
            }
            return soundBuffer[rPos++ % soundBuffer.Length];
        }
        public void Shutdown()
        {
            wPos = 0;
            rPos = 0;
            sampleTimer = 0;
        }
        public void SoftReset()
        {
            ChannelSq1.SoftReset();
            ChannelSq2.SoftReset();
            ChannelTri.SoftReset();
            ChannelNoi.SoftReset();
            ChannelDpm.SoftReset();

            ChannelDpm.DeltaIrqOccur = false;
            FrameIrqFlag = false;
            FrameIrqEnabled = true;
            Cycles = SequenceMode0[0] - 11;
            SequencingMode = false;
            CurrentSeq = 0; 
            oddCycle = false;
        }
        public void Stop()
        {
            rPos = 0;
            wPos = 0;
        }
        public byte PeekNull(int addr)
        {
            return 0;
        }
        public byte Peek4015(int addr)
        {
            byte data = 0;

            if (ChannelSq1.Enabled) data |= 0x01;
            if (ChannelSq2.Enabled) data |= 0x02;
            if (ChannelTri.Enabled) data |= 0x04;
            if (ChannelNoi.Enabled) data |= 0x08;
            if (ChannelDpm.Enabled) data |= 0x10;
            if (FrameIrqFlag) data |= 0x40;
            if (ChannelDpm.DeltaIrqOccur) data |= 0x80;

            FrameIrqFlag = false;
            cpu.IRQ(NesCpu.IsrType.Frame, false);

            return data;
        }
        public void Poke4015(int addr, byte data)
        {
            ChannelSq1.Enabled = (data & 0x01) != 0;
            ChannelSq2.Enabled = (data & 0x02) != 0;
            ChannelTri.Enabled = (data & 0x04) != 0;
            ChannelNoi.Enabled = (data & 0x08) != 0;
            ChannelDpm.Enabled = (data & 0x10) != 0;
        }
        public void Poke4017(int addr, byte data)
        {
            SequencingMode = (data & 0x80) != 0;
            FrameIrqEnabled = (data & 0x40) == 0;

            CurrentSeq = 0;

            if (!SequencingMode)
                Cycles = SequenceMode0[0];
            else
                Cycles = SequenceMode1[0];

            if (!oddCycle)
                Cycles++;
            else
                Cycles += 2;

            if (!FrameIrqEnabled)
            {
                FrameIrqFlag = false;
                cpu.IRQ(NesCpu.IsrType.Frame, false);
            }
        }

        public void Poke5015(byte data)
        {
            if (External != null)
            {
                ((Mmc5ExternalComponent)External).ChannelSq1.Enabled = (data & 0x01) != 0;
                ((Mmc5ExternalComponent)External).ChannelSq2.Enabled = (data & 0x02) != 0;
            }
        }
        public byte Peek5015()
        {
            byte rt = 0;
            if (External != null)
            {
                if (((Mmc5ExternalComponent)External).ChannelSq1.Enabled)
                    rt |= 0x01;
                if (((Mmc5ExternalComponent)External).ChannelSq2.Enabled)
                    rt |= 0x02;
            }
            return rt;
        }

        public override void SaveState(StateStream stateStream)
        {
            stateStream.Write(sampleTimer);
            stateStream.Write(Cycles);
            stateStream.Write(ChannelDpm.DeltaIrqOccur, FrameIrqEnabled, FrameIrqFlag, SequencingMode, oddCycle);

            ChannelSq1.SaveState(stateStream);
            ChannelSq2.SaveState(stateStream);
            ChannelTri.SaveState(stateStream);
            ChannelNoi.SaveState(stateStream);
            ChannelDpm.SaveState(stateStream);
            if (External != null)
                External.SaveState(stateStream);

            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            sampleTimer = stateStream.ReadFloat();
            Cycles = stateStream.ReadInt32();

            bool[] status = stateStream.ReadBooleans();
            ChannelDpm.DeltaIrqOccur = status[0];
            FrameIrqEnabled = status[1];
            FrameIrqFlag = status[2];
            SequencingMode = status[3];
            oddCycle = status[4];

            ChannelSq1.LoadState(stateStream);
            ChannelSq2.LoadState(stateStream);
            ChannelTri.LoadState(stateStream);
            ChannelNoi.LoadState(stateStream);
            ChannelDpm.LoadState(stateStream);
            if (External != null)
                External.LoadState(stateStream);

            base.LoadState(stateStream);
        }
    }
}