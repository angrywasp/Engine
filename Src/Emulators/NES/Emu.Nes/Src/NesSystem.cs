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
using System.IO;
using System.Threading;
using MyNes.Nes.Input;
using MyNes.Nes.Output.Audio;

namespace MyNes.Nes
{
    public class NesSystem
    {
        private int euAddCycle;

        public Mapper Mapper;
        public NesApu Apu;
        public NesCpu Cpu;
        public NesPpu Ppu;

        public NesCartridge Cartridge;
        public NesCpuMemory CpuMemory;
        public NesPpuMemory PpuMemory;

        public IAudioDevice soundDevice;

        public Stream StateStream;
        public Stream SaveStream;
        public bool Active;
        public bool PAUSE;
        public bool Paused;
        public bool SoundEnabled = true;
        public bool AutoSaveSRAM = true;
        public bool NoLimiter;
        public ushort FPS;
        public double FramePeriod = (1.0 / 60.0);

        public NesSystem()
        {
            Apu = new NesApu(this);
            Cpu = new NesCpu(this);
            Ppu = new NesPpu(this);
            CpuMemory = new NesCpuMemory(this);
            PpuMemory = new NesPpuMemory(this);
            Cartridge = new NesCartridge(this);
        }

        public void SetupAudio(IAudioDevice audioDevice)
        {
            soundDevice = audioDevice;
        }

        public void SetupInput(IJoypad joyPad1, IJoypad joyPad2, IJoypad joyPad3, IJoypad joyPad4)
        {
            CpuMemory.Joypad1 = joyPad1;
            CpuMemory.Joypad2 = joyPad2;
            CpuMemory.Joypad3 = joyPad3;
            CpuMemory.Joypad4 = joyPad4;
        }
        public void SetTVFormat(RegionFormat tvFormat, PaletteFormat paletteFormat)
        {
            //Apply ntsc palette
            NTSCPaletteGenerator.brightness = paletteFormat.Brightness;
            NTSCPaletteGenerator.contrast = paletteFormat.Contrast;
            NTSCPaletteGenerator.gamma = paletteFormat.Gamma;
            NTSCPaletteGenerator.hue_tweak = paletteFormat.Hue;
            NTSCPaletteGenerator.saturation = paletteFormat.Saturation;
            NesPalette.NTSCPalette = NTSCPaletteGenerator.GeneratePalette();
            NesPalette.PALPalette = NTSCPaletteGenerator.GeneratePALPalette();
            //Set palette
            switch (tvFormat)
            {
                case RegionFormat.NTSC:
                    Ppu.scanlinesPerFrame = 261;
                    FramePeriod = 1.0 / 60.0988;
                    Apu.SetNesFrequency(1789773);
                    //Apu.SetNesFrequency(1741950.0f);
                    //Apu.SetNesFrequency(1786830.00f);
                    try
                    {
                        Apu.ChannelDpm.IsPAL = false;
                    }
                    catch { }
                    if (paletteFormat.UseInternalPalette)
                    {
                        switch (paletteFormat.UseInternalPaletteMode)
                        {
                            case UseInternalPaletteMode.Auto:
                                Ppu.Palette = NesPalette.NTSCPalette;
                                break;
                            case UseInternalPaletteMode.NTSC:
                                Ppu.Palette = NesPalette.NTSCPalette;
                                break;
                            case UseInternalPaletteMode.PAL:
                                Ppu.Palette = NesPalette.PALPalette;
                                break;
                        }

                    }
                    else
                    {
                        if (NesPalette.LoadPalette(new FileStream(paletteFormat.ExternalPalettePath, FileMode.Open, FileAccess.Read)) != null)
                        {
                            Ppu.Palette = NesPalette.LoadPalette(new FileStream(paletteFormat.ExternalPalettePath, FileMode.Open, FileAccess.Read));
                        }
                        else
                        {
                            Ppu.Palette = NesPalette.NTSCPalette;
                        }
                    }
                    break;
                case RegionFormat.PAL:
                    Ppu.scanlinesPerFrame = 311;
                    FramePeriod = 1.0 / 50;
                    Apu.SetNesFrequency(1662375.0f);
                    try
                    {
                        Apu.ChannelDpm.IsPAL = true;
                    }
                    catch { }
                    if (paletteFormat.UseInternalPalette)
                    {
                        switch (paletteFormat.UseInternalPaletteMode)
                        {
                            case UseInternalPaletteMode.Auto:
                                Ppu.Palette = NesPalette.PALPalette;
                                break;
                            case UseInternalPaletteMode.NTSC:
                                Ppu.Palette = NesPalette.NTSCPalette;
                                break;
                            case UseInternalPaletteMode.PAL:
                                Ppu.Palette = NesPalette.PALPalette;
                                break;
                        }

                    }
                    else
                    {
                        if (NesPalette.LoadPalette(new FileStream(paletteFormat.ExternalPalettePath, FileMode.Open, FileAccess.Read)) != null)
                        {
                            Ppu.Palette = NesPalette.LoadPalette(new FileStream(paletteFormat.ExternalPalettePath, FileMode.Open, FileAccess.Read));
                        }
                        else
                        {
                            Ppu.Palette = NesPalette.PALPalette;
                        }
                    }
                    break;
            }
            if (Cartridge.IsVSUnisystem)
            {
                Ppu.SetPaletteIndexesTable(VSUnisystem.GetPalette(Cartridge.Sha1));
            }
        }

        public void TurnOn()
        {
            Active = true;
            InitializeComponents();
        }
        /// <summary>
        /// Execute the emulation while it's active (ON), this should called using thread
        /// </summary>
        /*public void Execute()
        {
            while (Active)
            {
                if (!PAUSE)
                {
                    Paused = false;
                    int cycles = Cpu.Execute();
                   
                    while (cycles-- != 0)
                    {
                        Apu.Execute();

                        Mapper.TickCycleTimer(1);

                        for (int i = 0; i < 3; i++)
                            Ppu.Execute();

                        if (Ppu.Region == RegionFormat.PAL)
                        {
                            euAddCycle++;
                            if (euAddCycle == 5)
                            {
                                Ppu.Execute();
                                euAddCycle = 0;
                            }
                        }
                        if (Ppu.FrameDone)
                        {
                            currFrameTime = Timer.GetCurrentTime() - lastFrameTime;
                            DeadTime = FramePeriod - currFrameTime;
                            //sound
                            if (SoundEnabled)
                                if (soundDevice != null)
                                    soundDevice.UpdateBuffer();
                            //graphics
                            graphicsDevice.RenderFrame(Ppu.ColorBuffer);
                            Ppu.FrameDone = false;

                            if (!NoLimiter)
                            {
                                if (DeadTime > 0)
                                    Thread.Sleep((int)(DeadTime * 1000));

                                while (currFrameTime < FramePeriod)
                                {
                                    if ((Timer.GetCurrentTime() - lastFrameTime) > FramePeriod)
                                    {
                                        break;
                                    }
                                }
                            }

                            lastFrameTime = Timer.GetCurrentTime();
                            FPS++;
                        }
                    }
                
                    if (soundDevice != null)
                        soundDevice.Play();//When paused
                }
                else
                {
                    Paused = true;
                    if (soundDevice != null)
                        soundDevice.Stop();
                    Apu.Stop();
                    if (stateSaveRequest)
                        SaveState();
                    if (stateLoadRequest)
                        LoadState();
                    Thread.Sleep(100);
                }
            }
        }*/

        /// <summary>
        /// Execute one cpu instruction then clock components depending on executed instruction cycles
        /// </summary>
        public void ExecuteInstruction()
        {
            int cycles = Cpu.Execute();

            while (cycles-- != 0)
            {
                Apu.Execute();

                Mapper.TickCycleTimer(1);

                for (int i = 0; i < 3; i++)
                    Ppu.Execute();

                if (Ppu.Region == RegionFormat.PAL)
                {
                    euAddCycle++;
                    if (euAddCycle == 5)
                    {
                        Ppu.Execute();
                        euAddCycle = 0;
                    }
                }
                if (Ppu.FrameDone)
                {
                    if (SoundEnabled)
                        if (soundDevice != null)
                            soundDevice.UpdateBuffer();
                }
            }
        }
        /// <summary>
        /// Execute the emulation until a complete frame is finished
        /// </summary>
        public void ExecuteFrame()
        {
            PAUSE = true;
            while (!Ppu.FrameDone)
            {
                int cycles = Cpu.Execute();

                while (cycles-- != 0)
                {
                    Apu.Execute();

                    Mapper.TickCycleTimer(1);

                    for (int i = 0; i < 3; i++)
                        Ppu.Execute();

                    if (Ppu.Region == RegionFormat.PAL)
                    {
                        euAddCycle++;
                        if (euAddCycle == 5)
                        {
                            Ppu.Execute();
                            euAddCycle = 0;
                        }
                    }
                }
            }
            Ppu.FrameDone = false;
            if (SoundEnabled)
                if (soundDevice != null)
                    soundDevice.UpdateBuffer();
            //graphicsDevice.RenderFrame(Ppu.ColorBuffer);
        }

        public void InitializeComponents()
        {
            CpuMemory.Initialize();
            PpuMemory.Initialize();
            Cartridge.Initialize();
            Apu.Initialize();
            Cpu.Initialize();
            Ppu.Initialize();
        }
        public void ShutDown()
        {
            PAUSE = true;
            //sound
            Apu.Shutdown();
            if (soundDevice != null)
            {
                soundDevice.Shutdown();
                soundDevice = null;
            }

            //s-ram
            if (Cartridge.HasSaveRam & AutoSaveSRAM)
                SaveSRAM(Cartridge.SavePath);
            SaveStream.Close();

            Active = false;
            CONSOLE.WriteLine(this, "SHUTDOWN", DebugStatus.Error);
            CONSOLE.WriteSeparateLine(this, DebugStatus.None);
        }
        public void SaveSRAM(string FilePath)
        {
            //If we have save RAM, try to save it
            try
            {
                SaveStream.Position = 0;
                SaveStream.Write(CpuMemory.srm, 0, 0x2000);
                CONSOLE.WriteLine(this, "SRAM saved !!", DebugStatus.Cool);
            }
            catch
            {
                CONSOLE.WriteLine(this, "Could not save S-RAM.", DebugStatus.Error);
            }
        }
        public void SaveStateRequest(string fileName, Stream stateStream)
        {
            this.StateStream = stateStream;
            PAUSE = true;
        }
        public void LoadStateRequest(string fileName, Stream stateStream)
        {
            this.StateStream = stateStream;
            PAUSE = true;
        }

        void LoadState()
        {
            StateStream st = new StateStream(this.StateStream,true);
            //check header
            if (!st.ReadHeader(Cartridge.Sha1))
            {
                PAUSE = false;
                CONSOLE.WriteLine(this, "Unable to load state file, unknown file format", DebugStatus.Notification);
                CONSOLE.WriteLine(this, "Unable to load state file, unknown file format", DebugStatus.Error);
                st.Close();
                return;
            }
            //load
            Cpu.LoadState(st); 
            Cartridge.LoadState(st); 
            CpuMemory.LoadState(st); 
            PpuMemory.LoadState(st); 
            Ppu.LoadState(st);
            Apu.LoadState(st); 
            Mapper.LoadState(st); 
            //close   
            CONSOLE.WriteLine(this, "State loaded", DebugStatus.Notification);
            st.Close();
            PAUSE = false;
        }
        void SaveState()
        {
            StateStream st = new StateStream(this.StateStream,false);
            st.WriteHeader(Cartridge.Sha1); 
           
            //save 
            Cpu.SaveState(st); 
            Cartridge.SaveState(st); 
            CpuMemory.SaveState(st); 
            PpuMemory.SaveState(st);
            Ppu.SaveState(st);
            Apu.SaveState(st);
            Mapper.SaveState(st);

            //close
            CONSOLE.WriteLine(this, "State saved", DebugStatus.Notification);
            st.Close();
            PAUSE = false;
        }

        public LoadRomStatus LoadRom(string fileName, Stream fileStream, Stream saveStream)
        {
            this.SaveStream = saveStream;
            return Cartridge.Load(fileName, fileStream, saveStream, false);
        }
    }

    public enum RegionFormat
    {
        PAL,
        NTSC
    }
}