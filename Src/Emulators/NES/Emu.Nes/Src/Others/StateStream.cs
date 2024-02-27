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
/*Written by Ala Hadid at Saturday, October 8, 2011*/
/*Updated by Ala Hadid at Monday, April 23, 2012: Updated to use BinaryWriter/BinaryReader*/
using System;
using System.IO;

namespace MyNes.Nes
{
    /*
     * To create new save state file, simply create instance of this class with "Write access, Create mode" stream
     * then use WriteHeader() method to write the header after that you're ready to write the other values.
     * To read save state file, simply create instance of this class with "Read access, Open mode" stream
     * then use ReadHeader() method to read and check the header after that you're ready to read the other values.
     * When you save values then you want to read them, they MUST read as same as they saved, for example:
     * //cpu registers
     * Write(RegA)
     * Write(RegX)
     * ....
     * //Then you MUST read cpu registers like this
     * RegA= ReadByte()
     * RegX= ReadByte()
     * ....
     */
    /// <summary>
    /// Stream for saving/loading My Nes State
    /// </summary>
    public class StateStream
    {
        //Stream stream;
        BinaryWriter writer;
        BinaryReader reader;
        bool isRead = false;

        /// <summary>
        /// Stream for saving/loading My Nes State
        /// </summary>
        /// <param name="stream">The file stream</param>
        /// <param name="isRead">True=this stream will be used for reading state,False=this stream will be used for writing</param>
        public StateStream(Stream stream, bool isRead)
        {
            this.isRead = isRead;
            if (!isRead)
                writer = new BinaryWriter(stream);
            else
                reader = new BinaryReader(stream);
        }

        /// <summary>
        /// Write My Nes State file header
        /// </summary>
        /// <param name="SHA1">The rom sha1, used to determine that this state file match the rom</param>
        public void WriteHeader(string SHA1)
        {
            /*My Nes State*/
            writer.Write((byte)0x4D);//M
            writer.Write((byte)0x4E);//N
            writer.Write((byte)0x53);//S
            //Write sha1
            for (int i = 0; i < SHA1.Length; i += 2)
            {
                string v = SHA1.Substring(i, 2).ToUpper();
                writer.Write(Convert.ToByte(v, 16));
            }
        }
        /// <summary>
        /// Read My Nes State file header and check it, then check the sha1
        /// </summary>
        /// <param name="SHA1">The rom sha1, used to determine that this state file match the rom</param>
        /// <returns>True= read ok and this file is state file for the rom</returns>
        public bool ReadHeader(string SHA1)
        {
            byte[] header = new byte[3];
            reader.Read(header, 0, 3);
            //check header
            if (header[0] != 0x4D | header[1] != 0x4E | header[2] != 0x53)//MNS
            {
                return false;
            }
            string sha1 = "";
            //Read sha1
            for (int i = 0; i < SHA1.Length; i += 2)
            {
                sha1 += (reader.ReadByte()).ToString("X2");
            }
            if (sha1.ToLower() != SHA1.ToLower())
                return false;
            return true;
        }

        #region General Write/Read
        /// <summary>
        /// Write a value into stream (position +1)
        /// </summary>
        /// <param name="value">The byte value to write</param>
        public void Write(byte value)
        { writer.Write(value); }
        /// <summary>
        /// Write a value into stream (position +2)
        /// </summary>
        /// <param name="value">The ushort value to write</param>
        public void Write(ushort value)
        {
            writer.Write(value);
        }
        /// <summary>
        /// Write a value into stream (position +4)
        /// </summary>
        /// <param name="value">The int value to write</param>
        public void Write(int value)
        {
            writer.Write(value);
        }
        /// <summary>
        /// Write a value into stream (position +4)
        /// </summary>
        /// <param name="value">The uint value to write</param>
        public void Write(uint value)
        {
            writer.Write(value);
        }
        /// <summary>
        /// Write a value into stream (position +8)
        /// </summary>
        /// <param name="value">The float value to write</param>
        public void Write(float value)
        {
            writer.Write(value);
        }
        /// <summary>
        /// Write a value into stream (position +8)
        /// </summary>
        /// <param name="value">The double value to write</param>
        public void Write(double value)
        {
            writer.Write(value);
        }

        /// <summary>
        /// Read a byte from stream (position +1)
        /// </summary>
        /// <returns>Byte value</returns>
        public byte ReadByte()
        {
            return reader.ReadByte();
        }
        /// <summary>
        /// Read a ushort from stream (position +2)
        /// </summary>
        /// <returns>ushort value</returns>
        public ushort ReadUshort()
        {
            return reader.ReadUInt16();
        }
        /// <summary>
        /// Read an int32 from stream (position +4)
        /// </summary>
        /// <returns>int value</returns>
        public int ReadInt32()
        {
            return reader.ReadInt32();
        }
        /// <summary>
        /// Read an uint32 from stream (position +4)
        /// </summary>
        /// <returns>int value</returns>
        public uint ReadUInt32()
        {
            return reader.ReadUInt32();
        }
        /// <summary>
        /// Read a float from stream (position +8)
        /// </summary>
        /// <returns>float value</returns>
        public float ReadFloat()
        {
            return reader.ReadSingle();
        }
        /// <summary>
        /// Read a double from stream (position +8)
        /// </summary>
        /// <returns>double value</returns>
        public double ReadDouble()
        {
            return reader.ReadDouble();
        }
        #endregion
        #region Arrays Write/Read
        /// <summary>
        /// Write a byte array
        /// </summary>
        /// <param name="valueArray">The byte array to write</param>
        public void Write(byte[] valueArray)
        {
            writer.Write(valueArray, 0, valueArray.Length);
        }
        /// <summary>
        /// Write an int array
        /// </summary>
        /// <param name="valueArray">The int array to write</param>
        public void Write(int[] valueArray)
        {
            for (int i = 0; i < valueArray.Length; i++)
                Write(valueArray[i]);
        }
        /// <summary>
        /// Write an ushort array
        /// </summary>
        /// <param name="valueArray">The ushort array to write</param>
        public void Write(ushort[] valueArray)
        {
            for (int i = 0; i < valueArray.Length; i++)
                Write(valueArray[i]);
        }
        /// <summary>
        /// Write a double array
        /// </summary>
        /// <param name="valueArray">The double array to write</param>
        public void Write(double[] valueArray)
        {
            for (int i = 0; i < valueArray.Length; i++)
                Write(valueArray[i]);
        }
        /// <summary>
        /// Write a float array
        /// </summary>
        /// <param name="valueArray">The float array to write</param>
        public void Write(float[] valueArray)
        {
            for (int i = 0; i < valueArray.Length; i++)
                Write(valueArray[i]);
        }

        /// <summary>
        /// Read for a array
        /// </summary>
        /// <param name="valueArray">The byte array to fill</param>
        public void Read(byte[] valueArray)
        {
            reader.Read(valueArray, 0, valueArray.Length);
        }

        public void Read(ushort[] valueArray)
        {
            for (int i = 0; i < valueArray.Length; i++)
                valueArray[i] = ReadUshort();
        }
        /// <summary>
        /// Read for a array
        /// </summary>
        /// <param name="valueArray">The int array to fill</param>
        public void Read(int[] valueArray)
        {
            for (int i = 0; i < valueArray.Length; i++)
                valueArray[i] = ReadInt32();
        }
        /// <summary>
        /// Read for a array
        /// </summary>
        /// <param name="valueArray">The double array to fill</param>
        public void Read(double[] valueArray)
        {
            for (int i = 0; i < valueArray.Length; i++)
                valueArray[i] = ReadDouble();
        }
        /// <summary>
        /// Read for a array
        /// </summary>
        /// <param name="valueArray">The float array to fill</param>
        public void Read(float[] valueArray)
        {
            for (int i = 0; i < valueArray.Length; i++)
                valueArray[i] = ReadFloat();
        }
        #endregion
        #region Boolean Write/Read !!
        /// <summary>
        /// Write one byte to save up to 8 boolean values. the other 7 values will be false
        /// </summary>
        /// <param name="v1">The first boolean, in bit 0</param>
        public void Write(bool v1)
        { Write(v1, false, false, false, false, false, false, false); }
        /// <summary>
        /// Write one byte to save up to 8 boolean values. the other 6 values will be false
        /// </summary>
        /// <param name="v1">The first boolean, in bit 0</param>
        /// <param name="v2">The second boolean, in bit 1</param>
        public void Write(bool v1, bool v2)
        { Write(v1, v2, false, false, false, false, false, false); }
        /// <summary>
        /// Write one byte to save up to 8 boolean values. the other 5 values will be false
        /// </summary>
        /// <param name="v1">The first boolean, in bit 0</param>
        /// <param name="v2">The second boolean, in bit 1</param>
        /// <param name="v3">The third boolean, in bit 2</param>
        public void Write(bool v1, bool v2, bool v3)
        { Write(v1, v2, v3, false, false, false, false, false); }
        /// <summary>
        /// Write one byte to save up to 8 boolean values. the other 4 values will be false
        /// </summary>
        /// <param name="v1">The first boolean, in bit 0</param>
        /// <param name="v2">The second boolean, in bit 1</param>
        /// <param name="v3">The third boolean, in bit 2</param>
        /// <param name="v4">The fourth boolean, in bit 3</param>
        public void Write(bool v1, bool v2, bool v3, bool v4)
        { Write(v1, v2, v3, v4, false, false, false, false); }
        /// <summary>
        /// Write one byte to save up to 8 boolean values. the other 3 values will be false
        /// </summary>
        /// <param name="v1">The first boolean, in bit 0</param>
        /// <param name="v2">The second boolean, in bit 1</param>
        /// <param name="v3">The third boolean, in bit 2</param>
        /// <param name="v4">The fourth boolean, in bit 3</param>
        /// <param name="v5">The fifth boolean, in bit 4</param>
        public void Write(bool v1, bool v2, bool v3, bool v4, bool v5)
        { Write(v1, v2, v3, v4, v5, false, false, false); }
        /// <summary>
        /// Write one byte to save up to 8 boolean values. the other 2 values will be false
        /// </summary>
        /// <param name="v1">The first boolean, in bit 0</param>
        /// <param name="v2">The second boolean, in bit 1</param>
        /// <param name="v3">The third boolean, in bit 2</param>
        /// <param name="v4">The fourth boolean, in bit 3</param>
        /// <param name="v5">The fifth boolean, in bit 4</param>
        /// <param name="v6">The sixth boolean, in bit 5</param>
        public void Write(bool v1, bool v2, bool v3, bool v4, bool v5, bool v6)
        { Write(v1, v2, v3, v4, v5, v6, false, false); }
        /// <summary>
        /// Write one byte to save up to 8 boolean values. the other 1 values will be false
        /// </summary>
        /// <param name="v1">The first boolean, in bit 0</param>
        /// <param name="v2">The second boolean, in bit 1</param>
        /// <param name="v3">The third boolean, in bit 2</param>
        /// <param name="v4">The fourth boolean, in bit 3</param>
        /// <param name="v5">The fifth boolean, in bit 4</param>
        /// <param name="v6">The sixth boolean, in bit 5</param>
        /// <param name="v7">The seventh boolean, in bit 6</param>
        public void Write(bool v1, bool v2, bool v3, bool v4, bool v5, bool v6, bool v7)
        { Write(v1, v2, v3, v4, v5, v6, v7, false); }
        /// <summary>
        /// Write one byte to save up to 8 boolean values. Make the values you don't need as false
        /// </summary>
        /// <param name="v1">The first boolean, in bit 0</param>
        /// <param name="v2">The second boolean, in bit 1</param>
        /// <param name="v3">The third boolean, in bit 2</param>
        /// <param name="v4">The fourth boolean, in bit 3</param>
        /// <param name="v5">The fifth boolean, in bit 4</param>
        /// <param name="v6">The sixth boolean, in bit 5</param>
        /// <param name="v7">The seventh boolean, in bit 6</param>
        /// <param name="v8">The eighth boolean, in bit 7</param>
        public void Write(bool v1, bool v2, bool v3, bool v4, bool v5, bool v6, bool v7, bool v8)
        {
            byte status = 0;
            if (v1)
                status |= 0x01;
            if (v2)
                status |= 0x02;
            if (v3)
                status |= 0x04;
            if (v4)
                status |= 0x08;
            if (v5)
                status |= 0x10;
            if (v6)
                status |= 0x20;
            if (v7)
                status |= 0x40;
            if (v8)
                status |= 0x80;
            writer.Write(status);
        }
        /// <summary>
        /// Read a BYTE that may include up to 8 boolean values
        /// </summary>
        /// <returns>A boolean array with 8 values, read from byte</returns>
        public bool[] ReadBooleans()
        {
            bool[] val = new bool[8];
            byte status = reader.ReadByte();
            val[0] = (status & 0x01) == 0x01;
            val[1] = (status & 0x02) == 0x02;
            val[2] = (status & 0x04) == 0x04;
            val[3] = (status & 0x08) == 0x08;
            val[4] = (status & 0x10) == 0x10;
            val[5] = (status & 0x20) == 0x20;
            val[6] = (status & 0x40) == 0x40;
            val[7] = (status & 0x80) == 0x80;
            return val;
        }
        #endregion

        /// <summary>
        /// Get the file length
        /// </summary>
        public long Length
        { get { return isRead ? reader.BaseStream.Length : writer.BaseStream.Length; } }
        /// <summary>
        /// Get or set the strem position
        /// </summary>
        public long Position
        {
            get { return isRead ? reader.BaseStream.Position : writer.BaseStream.Position; }
            set
            {
                if (isRead)
                    reader.BaseStream.Position = value;
                else
                    writer.BaseStream.Position = value;
            }
        }
        /// <summary>
        /// Close the stream
        /// </summary>
        public void Close()
        {
            if (isRead)
                reader.Close();
            else
                writer.Close();
        }
    }
}
