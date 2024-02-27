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
namespace MyNes.Nes
{
    public static class MapperManager
    {
        private static Type[] mappers = new Type[256];

        public static void Cache()
        {
            var assembly = typeof(MapperManager).Assembly;

            for (int i = 0; i < 256; i++)
            {
                Type t = assembly.GetType("MyNes.Nes.Mapper" + i.ToString("D2"), false, false);

                mappers[i] = t;
            }
        }
        public static Type Fetch(byte number)
        {
            return mappers[number];
        }
        public static bool Valid(byte number)
        {
            return mappers[number] != null;
        }
        public static string[] SupportedMappers
        {
            get 
            {
                List<string> list = new List<string>();
                for (int i = 0; i < mappers.Length; i++)
                {
                    if (mappers[i] != null)
                    {
                        Mapper mapper = Activator.CreateInstance(mappers[i],new NesSystem()) as Mapper;
                        list.Add("Mapper # " + i.ToString() + " [ " + mapper.Name + " ]");
                    }
                }
                return list.ToArray();
            }
        }
    }
}