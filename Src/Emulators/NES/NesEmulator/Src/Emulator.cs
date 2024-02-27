using System.Diagnostics;
using System.IO;
using EmulatorCore;
using Engine;
using Engine.Configuration;
using Engine.Helpers;
using Engine.Scripting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyNes.Nes;
using NesEmulator.Input;
using Keyboard = NesEmulator.Input.Keyboard;

namespace NesEmulator
{
	public class Emulator : EmulatorGame
	{

        //todo: move to settings
        public const bool AUTO_SAVE_NVRAM = true;
        public const bool AUDIO_ENABLE = true;
        public const int VOLUME = 50;

		private const int OFFSET_X = 0;
		private const int OFFSET_W = 0;
		private const int OFFSET_Y = 1;
		private const int OFFSET_H = 0;
		
		private Rectangle renderRegion;
		
		private NesSystem nes;

        public override string WindowTitle => "NES Emulator";

        protected override void Initialize()
		{
            base.Initialize();
            
			rt = new RenderTarget2D(graphicsDeviceManager.GraphicsDevice, 256, 224, false, SurfaceFormat.Rgba64, DepthFormat.None);

            MapperManager.Cache();

            LoadRom("Emulator/Roms/NES/SuperMarioBros.nes");
            
			renderRegion = new Rectangle(OFFSET_X, OFFSET_Y, rt.Width - OFFSET_W, rt.Height - OFFSET_H);
		}
		
		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

            if (running)
                nes.ExecuteFrame();
            else
            	nes.ShutDown();
		}

		protected override void Draw(GameTime gameTime)
		{
			rt.SetData<int>(0, nes.Ppu.ColorBuffer, 256 * 8, rt.Width * rt.Height);
            uiRenderer.DrawRectangle(rt, Color.White, Vector2i.Zero, ui.Size, renderRegion);
            ui.Draw();
		}

		public void LoadRom(string filename)
        {
            filename = EngineFolders.ContentPathVirtualToReal(filename);
            NesCartridge cart = new NesCartridge(null);
            var fs = File.OpenRead(filename);
            LoadRomStatus status = cart.Load(filename, fs, null, true);

            switch (status)
            {
                case LoadRomStatus.LoadSuccess:
                    break;
                default:
                    Debugger.Break();
                    break;
            }

            if (nes != null)
                nes.ShutDown();

            nes = new NesSystem();
            Stream romStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            nes.Cartridge.SavePath = filename.Substring(0, filename.Length - 3) + "sav";
            Stream sramStream = Stream.Null;
            if (File.Exists(nes.Cartridge.SavePath))
                sramStream = new FileStream(nes.Cartridge.SavePath, FileMode.Open, FileAccess.ReadWrite);

            nes.LoadRom(filename, romStream, sramStream);
            nes.TurnOn();

            ApplyVideoSettings();
            ApplyAudioSettings();
            ApplyKeyboardInputSettings();
            nes.AutoSaveSRAM = AUTO_SAVE_NVRAM;

            running = true;
        }
        
        private void ApplyVideoSettings()
        {
            if (nes == null)
                return;

            nes.PAUSE = true;

            //todo: load custom *.pal file for palette
            PaletteFormat pf = new PaletteFormat();
            pf.UseInternalPalette = true;
            
            if (nes.Cartridge.IsPAL)
            {
                pf.UseInternalPaletteMode = UseInternalPaletteMode.PAL;
                nes.SetTVFormat(RegionFormat.PAL, pf);
            }
            else
            {
                pf.UseInternalPaletteMode = UseInternalPaletteMode.NTSC;
                nes.SetTVFormat(RegionFormat.NTSC, pf);
            }

            nes.PAUSE = false;
        }
        
        private void ApplyAudioSettings()
        {
            if (nes == null)
                return;

            nes.PAUSE = true;

            if (nes.soundDevice != null)
                nes.soundDevice.Shutdown();

            AudioDevice ad = new AudioDevice(nes.Apu);
            ad.Initialize();
            nes.soundDevice = ad;

            nes.SoundEnabled = AUDIO_ENABLE;
            nes.Apu.ChannelSq1.Audible = true;
            nes.Apu.ChannelSq2.Audible = true;
            nes.Apu.ChannelTri.Audible = true;
            nes.Apu.ChannelNoi.Audible = true;
            nes.Apu.ChannelDpm.Audible = true;

            if (nes.Apu.External != null)
            {
                if (nes.Apu.External is Vrc6ExternalComponent)
                {
                    ((Vrc6ExternalComponent)nes.Apu.External).ChannelSq1.Audible = true;
                    ((Vrc6ExternalComponent)nes.Apu.External).ChannelSq2.Audible = true;
                    ((Vrc6ExternalComponent)nes.Apu.External).ChannelSaw.Audible = true;
                }
                else if (nes.Apu.External is Mmc5ExternalComponent)
                {
                    ((Mmc5ExternalComponent)nes.Apu.External).ChannelSq1.Audible = true;
                    ((Mmc5ExternalComponent)nes.Apu.External).ChannelSq2.Audible = true;
                    ((Mmc5ExternalComponent)nes.Apu.External).ChannelPcm.Audible = true;
                }
                else if (nes.Apu.External is Ss5BExternalComponent)
                {
                    ((Ss5BExternalComponent)nes.Apu.External).ChannelVW1.Audible = true;
                    ((Ss5BExternalComponent)nes.Apu.External).ChannelVW2.Audible = true;
                    ((Ss5BExternalComponent)nes.Apu.External).ChannelVW3.Audible = true;
                }
            }

            nes.soundDevice.SetVolume(VOLUME);

            if (!nes.SoundEnabled)
                nes.Apu.Stop();

            nes.PAUSE = false;
        }
        
        private void ApplyKeyboardInputSettings()
        {
            if (nes == null)
                return;

            nes.PAUSE = true;

            nes.SetupInput(new Keyboard(input), new Keyboard(input), new Keyboard(input), new Keyboard(input));
            //nes.CpuMemory.IsFourPlayers = false;
            nes.CpuMemory.Zapper = null;// zapper;

            nes.PAUSE = false;
        }
        
        private void ApplyGamepadInputSettings()
        {
            if (nes == null)
                return;

            nes.PAUSE = true;

            Gamepad Joy1 = new Gamepad();
            Gamepad Joy2 = new Gamepad();
            Gamepad Joy3 = new Gamepad();
            Gamepad Joy4 = new Gamepad();
            nes.SetupInput(Joy1, Joy2, Joy3, Joy4);
            nes.CpuMemory.Zapper = null;// zapper;

            nes.PAUSE = false;
        }
    }
}
