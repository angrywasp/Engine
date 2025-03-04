﻿/*********************************************************************\
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
    class VSUnisystem
    {
        static int[] nes =//nes default 512 indexes
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
            0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
            0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F,
            0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F,
            0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F,
            0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A, 0x5B, 0x5C, 0x5D, 0x5E, 0x5F,
            0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x6B, 0x6C, 0x6D, 0x6E, 0x6F,
            0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x7B, 0x7C, 0x7D, 0x7E, 0x7F,
        };
        /*TODO: fix these palettes*/
        static int[] VS1 =
       {
        0x18,0x3f,0x1c,0x3f,0x3f,0x3f,0x0b,0x17,0x10,0x3f,0x14,0x3f,0x36,0x37,0x1a,0x3f,
        0x25,0x3f,0x12,0x3f,0x0f,0x3f,0x3f,0x3f,0x3f,0x3f,0x22,0x19,0x3f,0x3f,0x3a,0x21,
        0x3f,0x3f,0x07,0x3f,0x3f,0x3f,0x00,0x15,0x0c,0x3f,0x3f,0x3f,0x3f,0x3f,0x3f,0x3f,
        0x3f,0x3f,0x07,0x16,0x3f,0x3f,0x30,0x3c,0x3f,0x27,0x3f,0x3f,0x29,0x3f,0x1b,0x09,
       };
        static int[] VS2 =
       {	
         0x0f,0x27,0x18,0x3f,0x3f,0x25,0x3f,0x34,0x16,0x13,0x3f,0x3f,0x20,0x23,0x3f,0x0b,
         0x3f,0x3f,0x06,0x3f,0x1b,0x3f,0x3f,0x22,0x3f,0x24,0x3f,0x3f,0x32,0x3f,0x3f,0x03,
         0x3f,0x37,0x26,0x33,0x11,0x3f,0x10,0x3f,0x14,0x3f,0x00,0x09,0x12,0x0f,0x3f,0x30,
         0x3f,0x3f,0x2a,0x17,0x0c,0x01,0x15,0x19,0x3f,0x3f,0x07,0x37,0x3f,0x05,0x3f,0x3f,
	  };
        static int[] VS3 =
      {	
         0x3f,0x3f,0x3f,0x3f,0x1a,0x30,0x3c,0x09,0x0f,0x0f,0x3f,0x0f,0x3f,0x3f,0x3f,0x30,
         0x32,0x1c,0x3f,0x12,0x3f,0x18,0x17,0x3f,0x0c,0x3f,0x3f,0x02,0x16,0x3f,0x3f,0x3f,
         0x3f,0x3f,0x0f,0x37,0x3f,0x28,0x27,0x3f,0x29,0x3f,0x21,0x3f,0x11,0x3f,0x0f,0x3f,
         0x31,0x3f,0x3f,0x3f,0x0f,0x2a,0x28,0x3f,0x3f,0x3f,0x3f,0x3f,0x13,0x3f,0x3f,0x3f,
	  };
        static int[] VS4 =
      {	
         0x0f,0x3f,0x3f,0x10,0x3f,0x30,0x31,0x3f,0x01,0x0f,0x36,0x3f,0x3f,0x3f,0x3f,0x3c,
         0x3f,0x3f,0x3f,0x12,0x19,0x3f,0x17,0x3f,0x00,0x3f,0x3f,0x02,0x16,0x3f,0x3f,0x3f,
         0x3f,0x3f,0x3f,0x37,0x3f,0x27,0x26,0x20,0x3f,0x04,0x22,0x3f,0x11,0x3f,0x3f,0x3f,
         0x2c,0x3f,0x3f,0x3f,0x07,0x2a,0x3f,0x3f,0x3f,0x3f,0x3f,0x38,0x13,0x3f,0x3f,0x0c,	
	  };
        static int[] VS5 =
      {	
         0x18,0x3f,0x1c,0x3f,0x3f,0x3f,0x01,0x17,0x10,0x3f,0x2a,0x3f,0x36,0x37,0x1a,0x39,
         0x25,0x3f,0x12,0x3f,0x0f,0x3f,0x3f,0x26,0x3f,0x3f,0x22,0x19,0x3f,0x0f,0x3a,0x21,
         0x3f,0x0a,0x07,0x06,0x13,0x3f,0x00,0x15,0x0c,0x3f,0x11,0x3f,0x3f,0x38,0x3f,0x3f,
         0x3f,0x3f,0x07,0x16,0x3f,0x3f,0x30,0x3c,0x0f,0x27,0x3f,0x31,0x29,0x3f,0x11,0x09,
	  };
        static int[] VS6 =
      {	
         0x35,0x3f,0x16,0x3f,0x1c,0x3f,0x3f,0x15,0x3f,0x3f,0x27,0x05,0x04,0x3f,0x3f,0x30,
         0x21,0x3f,0x3f,0x3f,0x3f,0x3f,0x36,0x12,0x3f,0x2b,0x3f,0x3f,0x3f,0x3f,0x3f,0x3f,
         0x3f,0x31,0x3f,0x2a,0x2c,0x0c,0x3f,0x3f,0x3f,0x07,0x34,0x06,0x3f,0x25,0x26,0x0f,
         0x3f,0x19,0x10,0x3f,0x3f,0x3f,0x3f,0x17,0x3f,0x11,0x3f,0x3f,0x3f,0x3f,0x18,0x3f,
	  };
        static int[] VS7 =
 	  {
		 0x35,0x3f,0x16,0x22,0x1c,0x3f,0x3f,0x15,0x3f,0x00,0x27,0x05,0x04,0x3f,0x3f,0x30,
         0x21,0x3f,0x3f,0x29,0x3c,0x3f,0x36,0x12,0x3f,0x2b,0x3f,0x3f,0x3f,0x3f,0x3f,0x01,
         0x3f,0x31,0x3f,0x2a,0x2c,0x0c,0x3f,0x3f,0x3f,0x07,0x34,0x06,0x3f,0x25,0x26,0x0f,
         0x3f,0x19,0x10,0x3f,0x3f,0x3f,0x3f,0x17,0x3f,0x11,0x3f,0x3f,0x3f,0x25,0x18,0x3f,
	  };

        public static int[] GetPalette(string sha1)
        {
            /*We MUST set palette indexes manualy for VS games
             * TODO: finish this list @@
             */

            switch (sha1)
            {
                default:
                    nes = new int[512];
                    for (int i = 0; i < 512; i++)
                        nes[i] = i;
                    return nes;


                case "035cc757cffedefaca2b420e12a2cfcf44409b9f"://Super Skater
                case "b21aa940728ed80c72ee23c251c96e42cc84b2d6"://VS Super Mario Bros
                case "7fd66e0a4cc0e404f404d8164fa221ee2acb7a38"://Clu Clu Land (VS)
                    return VS1;

                case "e0572da111d05bf622ec137df8a658f7b0687ddf"://Battle City VS
                case "1b516cf7688792f5dbd669850c047a7afe9eb59f"://Freedom Force (VS)
                case "e0f7bdbd2c96b14d4b8d2146a900aaad17f9e3b1"://Golf
                case "cde1ecaf212a9f5a5a49f904f87951eda15d54dd"://Ladies Golf (VS)
                case "f8a0f2c5a4b7212cb35f53ea7193b3dd85d6e1cd"://Mach Rider (VS)
                case "68de623b2ad92ba19d18f17eaa0b97ee4523f6df"://VS Slalom (VS)
                    return VS2;

                case "9eb3b75e7b45df51b8bcd29df84689a7e8557f4f"://VS Castlevania (VS)
                    return VS3;

                case "bbb0af27b313d7c838a38fb772a6fe8afbafbb95"://Soccer
                    return VS4;

                case "9f1943aade4233285589cea5bdc96b5380d49337"://Ice Climber (VS)
                    return VS5;

                case "1a4ec64e576bad64daf320aeed0be1b8b50d21df"://Pinball (VS)
                    return VS6;

                case "1a17df593c658f56d71b7026d2771396bff95b36"://Tetris
                    return VS7;
            }
        }
    }
}