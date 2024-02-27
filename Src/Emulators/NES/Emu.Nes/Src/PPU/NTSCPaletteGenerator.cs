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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNes.Nes
{
    public class NTSCPaletteGenerator
    {
        public static float saturation = 1.0f;
        public static float hue_tweak = 0.0f;
        public static float contrast = 1.0f;
        public static float brightness = 1.0f;
        public static float gamma = 1.8f;

        // Voltage levels, relative to synch voltage
        const float black = .518f;
        const float white = 1.962f;
        const float attenuation = .746f;
        static float[] levels = 
        {
               0.350f, 0.518f, 0.962f, 1.550f,  // Signal low
               1.094f, 1.506f, 1.962f, 1.962f   // Signal high
        };

        public static int MakeRGBcolor(int pixel)
        {
            // The input value is a NES color index (with de-emphasis bits).
            // We need RGB values. Convert the index into RGB.
            // For most part, this process is described at:
            //    http://wiki.nesdev.com/w/index.php/NTSC_video

            // Decode the color index
            int color = (pixel & 0x0F), level = color < 0xE ? (pixel >> 4) & 3 : 1;

            float[] lo_and_hi = { levels[level + 4 *( (color == 0x0)?1:0)],
                               levels[level + 4 * ((color <  0xD)?1:0)] };

            // Calculate the luma and chroma by emulating the relevant circuits:
            float y = 0.0f, i = 0.0f, q = 0.0f;
            for (int p = 0; p < 12; ++p) // 12 clock cycles per pixel.
            {
                // NES NTSC modulator (square wave between two voltage levels):
                float spot = lo_and_hi[wave(p, color)];

                // De-emphasis bits attenuate a part of the signal:
                if ((((pixel & 0x40) == 0x40) && (wave(p, 12) == 1))
                || (((pixel & 0x80) == 0x80) && (wave(p, 4) == 1))
                || (((pixel & 0x100) == 0x100) && (wave(p, 8) == 1)))
                    spot *= attenuation;

                // Normalize:
                float v = (spot - black) / (white - black);

                // Ideal TV NTSC demodulator:
                // Apply contrast/brightness
                v = (v - .5f) * contrast + .5f;
                v *= brightness / 12.0f;

                y += v;
                i += (float)(v * Math.Cos((double)((3.141592653f / 6.0f) * (p + hue_tweak))));
                q += (float)(v * Math.Sin((3.141592653f / 6.0f) * (p + hue_tweak)));
            }

            i *= saturation;
            q *= saturation;

            // Convert YIQ into RGB according to FCC-sanctioned conversion matrix.
            int rgb = 0x10000 * clamp(255 * gammafix(y + 0.946882f * i + 0.623557f * q, gamma))
                         + 0x00100 * clamp(255 * gammafix(y + -0.274788f * i + -0.635691f * q, gamma))
                         + 0x00001 * clamp(255 * gammafix(y + -1.108545f * i + 1.709007f * q, gamma));



            return SwapRedBlueChannel(rgb);
        }
        static int wave(int p, int color)
        {
            return ((color + p + 8) % 12 < 6) ? 1 : 0;
        }
        static float gammafix(float f, float gamma)
        {
            return (float)(f < 0.0f ? 0.0f : Math.Pow(f, 2.2f / gamma));
        }
        static int clamp(float v)
        {
            return (int)(v < 0 ? 0 : v > 255 ? 255 : v);
        }

        public static int[] GeneratePalette()
        {
            int[] pal = new int[512];
            for (int i = 0; i < 512; i++)
                pal[i] = MakeRGBcolor(i);
            return pal;
        }

        public static int[] GeneratePALPalette()
        {
            int[] p = NesPalette.PALPalette;
            int[] pal = new int[512];
            for (int i = 0; i < 512; i++)
                pal[i] = SwapRedBlueChannel(p[i]);
            return pal;
        }

        //bit of a hack, but XNA requires (for reasons i don't fully understand) the Red and Blue channels to be swapped
        //this can be done as a post process, but it is easier to generate the palette this way
        private static int SwapRedBlueChannel(int i)
        {
            byte R = (byte)((i & 0x00FF0000) >> 16);
            byte G = (byte)((i & 0x0000FF00) >> 8);
            byte B = (byte)((i & 0x000000FF));

            return (0xFF << 24) | (B << 16) | (G << 8) | R;
        }
    }
}
