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
    public class Ss5BExternalComponent : ExternalComponent
    {
        public ChannelSs5BVol ChannelVW1;
        public ChannelSs5BVol ChannelVW2;
        public ChannelSs5BVol ChannelVW3;

        public Ss5BExternalComponent(NesSystem nes)
        {
            ChannelVW1 = new ChannelSs5BVol(nes);
            ChannelVW2 = new ChannelSs5BVol(nes);
            ChannelVW3 = new ChannelSs5BVol(nes);

            //MaxOutput = (15 + 15 + 15);
            MaxOutput = (int)System.Math.Pow(System.Math.Pow(10, 0.15), 15) * 3;
        }

        public override void ClockHalf() { }
        public override void ClockQuad() { }
        public override int RenderSample(float rate)
        {
            int sample = 0;

            if (ChannelVW1.Audible) sample += ChannelVW1.RenderSample(rate);
            if (ChannelVW2.Audible) sample += ChannelVW2.RenderSample(rate);
            if (ChannelVW3.Audible) sample += ChannelVW3.RenderSample(rate);

            return sample;
        }
        public override void SaveState(StateStream stateStream)
        {
            ChannelVW1.SaveState(stateStream);
            ChannelVW2.SaveState(stateStream);
            ChannelVW3.SaveState(stateStream);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            ChannelVW1.LoadState(stateStream);
            ChannelVW2.LoadState(stateStream);
            ChannelVW3.LoadState(stateStream);
            base.LoadState(stateStream);
        }
    }
}