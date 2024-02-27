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
    public class Mmc5ExternalComponent : ExternalComponent
    {
        public ChannelMmc5Pcm ChannelPcm;
        public ChannelMmc5Sqr ChannelSq1;
        public ChannelMmc5Sqr ChannelSq2;

        public Mmc5ExternalComponent(NesSystem nes)
        {
            ChannelPcm = new ChannelMmc5Pcm(nes);
            ChannelSq1 = new ChannelMmc5Sqr(nes);
            ChannelSq2 = new ChannelMmc5Sqr(nes);
            MaxOutput = (15 + 15 + 15);
        }

        public override void ClockHalf()
        {
            ChannelSq1.ClockHalf();
            ChannelSq2.ClockHalf();
            ChannelPcm.ClockHalf();
        }
        public override void ClockQuad()
        {
            ChannelSq1.ClockQuad();
            ChannelSq2.ClockQuad();
            ChannelPcm.ClockQuad();
        }
        public override int RenderSample(float rate)
        {
            int sample = 0;

            if (ChannelPcm.Audible) sample += ChannelPcm.RenderSample();
            if (ChannelSq1.Audible) sample += ChannelSq1.RenderSample();
            if (ChannelSq2.Audible) sample += ChannelSq2.RenderSample();

            return sample;
        }
        public override void SaveState(StateStream stateStream)
        {
            ChannelPcm.SaveState(stateStream);
            ChannelSq1.SaveState(stateStream);
            ChannelSq2.SaveState(stateStream);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            ChannelPcm.LoadState(stateStream);
            ChannelSq1.LoadState(stateStream);
            ChannelSq2.LoadState(stateStream);
            base.LoadState(stateStream);
        }
    }
}