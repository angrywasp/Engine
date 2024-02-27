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
using System.IO;
using System.Security.Cryptography;
using MyNes.Nes.Database;

namespace MyNes.Nes
{
    public class NesCartridge : Component
    {
        private Mirroring mirroring;
        public NesDatabaseGameInfo DataBaseInfo;
        public NesDatabaseCartridgeInfo DataBaseCartInfo;
        public bool HasTrainer;
        public bool HasSaveRam;
        public bool HasCharRam;
        public bool IsPC10 = false;
        public bool IsVSUnisystem = false;

        public byte ChrPages;
        public byte PrgPages;
        public byte Mapper;
        public byte[][] Prg;
        public byte[][] Chr;
        public string Sha1;
        public bool IsPAL;

        public int PRGSizeInKB = 0;
        public int CHRSizeInKB = 0;

        public string FilePath;
        public string SavePath;

        public Mirroring Mirroring
        {
            get { return mirroring; }
            set
            {
                mirroring = value;
                nesSystem.PpuMemory.SwitchNmt(mirroring);
            }
        }

        public NesCartridge(NesSystem nesSystem)
            : base(nesSystem) { }

        protected override void Initialize(bool initializing)
        {
            var type = MapperManager.Fetch(Mapper);

            if (type != null)
            {
                nesSystem.Mapper = Activator.CreateInstance(type, nesSystem) as Mapper;

                if (nesSystem.Mapper != null)
                {
                    CONSOLE.WriteLine(this, "Initializing mapper # " + Mapper + " [" + nesSystem.Mapper.Name + "]...", DebugStatus.None);
                    nesSystem.Mapper.Initialize();
                    CONSOLE.WriteLine(this, "Mapper initialized OK.", DebugStatus.Cool);
                }
                else
                {
                    CONSOLE.WriteLine(this, "Unsupported mapper # " + Mapper + " !!", DebugStatus.Error);
                }
            }
        }
        public bool SupportedMapper()
        {
            return MapperManager.Valid(Mapper);
        }

        public LoadRomStatus Load(string fileName, Stream fileStream, Stream saveStream, bool HeaderOnly)
        {
            if (fileStream == null || !fileStream.CanRead)
                throw new InvalidOperationException();

            try
            {
                //Read the header
                byte[] header = new byte[16];
                fileStream.Read(header, 0, 16);
                //Check out
                if (header[0] != 0x4E | header[1] != 0x45 | header[2] != 0x53 | header[3] != 0x1A)
                {
                    fileStream.Close();
                    return LoadRomStatus.InvalidHeader;
                }
                //Flags
                PrgPages = header[4];
                ChrPages = header[5];
                if ((header[6] & 0x1) == 0)
                    this.mirroring = Nes.Mirroring.ModeHorz;
                else
                    this.mirroring = Nes.Mirroring.ModeVert;

                if ((header[6] & 0x8) != 0)
                    this.mirroring = Nes.Mirroring.ModeFull;

                HasSaveRam = (header[6] & 0x2) != 0x0;
                HasTrainer = (header[6] & 0x4) != 0x0;

                if ((header[7] & 0x0F) == 0)
                    Mapper = (byte)((header[7] & 0xF0) | (header[6] & 0xF0) >> 4);
                else
                    Mapper = (byte)((header[6] & 0xF0) >> 4);

                IsVSUnisystem = ((header[7] & 0x1) == 0x1);
                IsPC10 = ((header[7] & 0x2) == 0x2);

                if (!SupportedMapper())
                {
                    fileStream.Close();
                    return LoadRomStatus.InvalidMapper;
                }

                if (!HeaderOnly)
                {
                    CONSOLE.WriteLine(this, "Reading from ROM ....", DebugStatus.None);
                    //Read rom sha1, info if database found
                    CONSOLE.WriteLine(this, "Computing SHA1", DebugStatus.None);
                    byte[] prgBuffer = new byte[fileStream.Length - 16];
                    fileStream.Read(prgBuffer, 0, (int)(fileStream.Length - 16));

                    SHA1 managedSHA1 = SHA1.Create();
                    byte[] shaBuffer = managedSHA1.ComputeHash(prgBuffer);

                    foreach (byte b in shaBuffer)
                        Sha1 += b.ToString("x2").ToLower();
                    CONSOLE.WriteLine(this, "SHA1= " + Sha1, DebugStatus.None);
                    DataBaseInfo = NesDatabase.Find(Sha1);
                    //set cart info
                    if (DataBaseInfo.Cartridges != null)
                    {
                        foreach (NesDatabaseCartridgeInfo cartinf in DataBaseInfo.Cartridges)
                            if (cartinf.SHA1.ToLower() == Sha1.ToLower())
                            { DataBaseCartInfo = cartinf; break; }
                        IsPAL = DataBaseCartInfo.System.Contains("PAL");
                        CONSOLE.WriteLine(this, "ROM info loaded from database !!", DebugStatus.Cool);
                        CONSOLE.WriteLine(this, "System= " + DataBaseCartInfo.System, DebugStatus.Cool);
                    }

                    //seek to begaining
                    fileStream.Position = 16;
                    //Trainer
                    if (HasTrainer)
                    {
                        fileStream.Read(nesSystem.CpuMemory.srm, 0x1000, 512);
                        CONSOLE.WriteLine(this, "Trainer loaded!", DebugStatus.None);
                    }

                    //PRG
                    int prg_roms = PrgPages * 4;
                    PRGSizeInKB = prg_roms * 4;
                    CONSOLE.WriteLine(this, "PRG's= " + PrgPages + " [" + PRGSizeInKB + " kb]", DebugStatus.None);
                    Prg = new byte[prg_roms][];
                    for (int i = 0; i < prg_roms; i++)
                    {
                        Prg[i] = new byte[4096];
                        fileStream.Read(Prg[i], 0, 4096);
                    }

                    //CHR
                    int chr_roms = ChrPages * 8;
                    CHRSizeInKB = chr_roms;
                    CONSOLE.WriteLine(this, "CHR's= " + ChrPages + " [" + CHRSizeInKB + " kb]", DebugStatus.None);
                    if (ChrPages != 0)
                    {
                        Chr = new byte[chr_roms][];
                        for (int i = 0; i < (chr_roms); i++)
                        {
                            Chr[i] = new byte[1024];
                            fileStream.Read(Chr[i], 0, 1024);
                        }
                        HasCharRam = false;
                    }
                    else
                    {
                        HasCharRam = true;
                    }
                    //SRAM
                    if (HasSaveRam)
                    {
                        CONSOLE.WriteLine(this, "Trying to read SRAM from file : " + SavePath, DebugStatus.None);
                        if (File.Exists(SavePath))
                        {
                            try
                            {
                                saveStream.Read(nesSystem.CpuMemory.srm, 0, 0x2000);
                            }
                            catch (Exception ex)
                            { CONSOLE.WriteLine(this, "Faild to read SRAM\nex: " + ex.Message, DebugStatus.Error); }
                        }
                        else
                        {
                            CONSOLE.WriteLine(this, "No SRAM file found for this rom", DebugStatus.Warning);
                        }
                    }
                    //Connect and active
                    nesSystem.CpuMemory.IsVSunisystem = IsVSUnisystem;
                    nesSystem.PpuMemory.SwitchNmt(mirroring);

                    //finish
                    FilePath = fileName;
                    CONSOLE.WriteLine(this, "ROM read OK.", DebugStatus.Cool);
                }
                //Finish
                fileStream.Close();
                return LoadRomStatus.LoadSuccess;
            }
            catch (Exception Ex)
            {
                CONSOLE.WriteLine(this, "Faild to read cart, ex: " + Ex.Message, DebugStatus.Error);
                CONSOLE.WriteLine(this, "Line " + Ex.StackTrace, DebugStatus.Error);
            }
            return LoadRomStatus.LoadFailure;
        }

        public override void SaveState(StateStream stateStream)
        {
            if (HasCharRam)
                foreach (byte[] chr in Chr)
                    stateStream.Write(chr);
            base.SaveState(stateStream);
        }
        public override void LoadState(StateStream stateStream)
        {
            if (HasCharRam)
                foreach (byte[] chr in Chr)
                    stateStream.Read(chr);
            base.LoadState(stateStream);
        }
    }

    public enum LoadRomStatus
    {
        InvalidHeader,
        InvalidMapper,
        LoadFailure,
        LoadSuccess,
    }

    public enum Hardware
    {
        NES = (1 << 0), // NES
        FMC = (1 << 1), // Famicom
        PCT = (1 << 2), // PC10
        VSU = (1 << 3), // Vs Unisystem

        Eu = (1 << 30), // Indicates Pal hardware
        Us = (1 << 31), // Indicates Ntsc hardware

        // Example:
        // if (us && nes) {
        //     flag = System.Us | System.NES;
        // }
    }

    public enum Mirroring : byte
    {
        /// <summary>
        /// Vertical
        /// </summary>
        ModeVert = 0x11,
        /// <summary>
        /// Horizontal
        /// </summary>
        ModeHorz = 0x05,
        /// <summary>
        /// One screen - low
        /// </summary>
        Mode1ScA = 0x00,
        /// <summary>
        /// One screen - high
        /// </summary>
        Mode1ScB = 0x55,
        /// <summary>
        /// Four screen
        /// </summary>
        ModeFull = 0x1B,
    }
}