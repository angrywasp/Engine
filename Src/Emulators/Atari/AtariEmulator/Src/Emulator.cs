using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using AngryWasp.Helpers;
using AtariEmulator.Renderers;
using EMU7800.Core;
using EmulatorCore;
using Engine.Configuration;
using Engine.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AtariEmulator
{
    public class Emulator : EmulatorGame
	{
		private const string ATARI_ROM_LIST_XML = "EmulatorRoms/Atari/RomList.xml";
		
		private const int OFFSET_X = 18;
		private const int OFFSET_W = 18;

		private const int OFFSET_Y = 22;
		private const int OFFSET_H = 33;

		private MachineBase atari;
		private HSC7800 hsc;
		Dictionary<string, RomListEntry> romList = new Dictionary<string, RomListEntry>();
		private FrameRendererBase frameRenderer;
		private DynamicSoundEffectInstance snd;
		private DateTime prev;
		private byte[] buff = new byte[0];
		
		private Vector2i renderSize;
		private Rectangle renderRegion;
		
		public override string WindowTitle => "Atari Emulator";

		protected override void Initialize()
		{
            base.Initialize();

			LoadRomList();

			MachineType mt;
			Controller c;
			Cart cart = CreateCart("2600/River Raid.bin", out mt, out c);
			if (cart != null)
			{
				Bios7800 bios = CreateBios(mt);
				hsc = CreateHsc();
				atari = MachineBase.Create(mt, cart, bios, hsc, c, c, null);

				switch (mt)
				{
					case MachineType.A2600NTSC:
					case MachineType.A2600PAL:
						frameRenderer = new FrameRenderer160();
						break;
					case MachineType.A7800NTSC:
					case MachineType.A7800PAL:
						frameRenderer = new FrameRenderer320();
						break;
				}

				frameRenderer.Create(mt, atari);
				rt = new RenderTarget2D(graphicsDeviceManager.GraphicsDevice, frameRenderer.Width, frameRenderer.Height, false, SurfaceFormat.Rgba64, DepthFormat.None);
				snd = new DynamicSoundEffectInstance(atari.SoundSampleFrequency / 2, AudioChannels.Mono);
	            snd.BufferNeeded += new EventHandler<EventArgs>(snd_BufferNeeded);
	            prev = DateTime.Now;
	            snd.Play();

				running = true;
			}
			
			renderSize = new Vector2i(Settings.Engine.Resolution.X, Settings.Engine.Resolution.Y);
			renderRegion = new Rectangle(OFFSET_X, OFFSET_Y, rt.Width - OFFSET_W, rt.Height - OFFSET_H);
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (input.Mouse.ButtonDown(input.Mouse.LeftButton))
				atari.InputState.RaiseInput(0, MachineInput.Fire, true);
			else
				atari.InputState.RaiseInput(0, MachineInput.Fire, false);

			if (input.Mouse.ButtonDown(input.Mouse.RightButton))
				atari.InputState.RaiseInput(0, MachineInput.Fire2, true);
			else
				atari.InputState.RaiseInput(0, MachineInput.Fire2, false);

			if (input.Keyboard.KeyDown(Keys.A))
				atari.InputState.RaiseInput(0, MachineInput.Left, true);
			else
				atari.InputState.RaiseInput(0, MachineInput.Left, false);

			if (input.Keyboard.KeyDown(Keys.D))
				atari.InputState.RaiseInput(0, MachineInput.Right, true);
			else
				atari.InputState.RaiseInput(0, MachineInput.Right, false);

			if (input.Keyboard.KeyDown(Keys.Space))
				atari.InputState.RaiseInput(0, MachineInput.Select, true);
			else
				atari.InputState.RaiseInput(0, MachineInput.Select, false);

			if (input.Keyboard.KeyDown(Keys.Enter))
				atari.InputState.RaiseInput(0, MachineInput.Reset, true);
			else
				atari.InputState.RaiseInput(0, MachineInput.Reset, false);

			if (running)
			{
				var fb = frameRenderer.FrameBuffer;
				atari.ComputeNextFrame(fb);
				
				buff = new byte[fb.SoundBufferByteLength];
            
	            int x = 0;
	            for (int i = 0; i < fb.SoundBufferElementLength; i++)
				{
					buff[x++] = fb.SoundBuffer[i][0];
					buff[x++] = fb.SoundBuffer[i][1];
					buff[x++] = fb.SoundBuffer[i][2];
					buff[x++] = fb.SoundBuffer[i][3];
				}
				
				snd.SubmitBuffer(buff);
			}
		}

		protected override void Draw(GameTime gameTime)
		{
			frameRenderer.UpdateRenderTarget();
			rt.SetData<byte>(frameRenderer.FrameData);
			uiRenderer.DrawRectangle(rt, Color.White, Vector2i.Zero, renderSize, renderRegion);
			ui.Draw();
		}

		void snd_BufferNeeded(object sender, EventArgs e)
        {
        	if (!running)
        		return;
        		
            if (buff.Length != 0)
                snd.SubmitBuffer(buff);
        }

		private void LoadRomList()
		{
			string file = EngineFolders.ContentPathVirtualToReal("Emulator/Roms/Atari/RomList.xml");
			XDocument doc = XHelper.LoadDocument(file);
			XElement a2600 = XHelper.GetNodeByName(doc.Root, "a2600");
			XElement a7800 = XHelper.GetNodeByName(doc.Root, "a7800");

			LoadRomList2("2600", a2600);
			LoadRomList2("7800", a7800);
		}

		private void LoadRomList2(string platform, XElement el)
		{
			foreach (XElement e in el.Elements())
			{
				string n = string.Format("{0}/{1}", platform, e.Attribute("n").Value);
				string fn = EngineFolders.ContentPathVirtualToReal(string.Format("Emulator/Roms/Atari/{0}", n));

				RomListEntry rle = new RomListEntry()
				{
					c = (Controller)int.Parse(e.Attribute("c").Value),
					ct = (CartType)int.Parse(e.Attribute("ct").Value),
					mt = (MachineType)int.Parse(e.Attribute("mt").Value),
					n = fn
				};

				romList.Add(n, rle);
			}
		}

		private Cart CreateCart(string filename, out MachineType mt, out Controller c)
		{
			RomListEntry rle;

			if (romList.TryGetValue(filename, out rle))
			{

				mt = rle.mt;
				c = rle.c;
				Cart cart = Cart.Create(File.ReadAllBytes(rle.n), rle.ct);
				return cart;
			}
			else
			{
				mt = MachineType.None;
				c = Controller.None;
				return null;
			}
		}

		private Bios7800 CreateBios(MachineType mt)
		{
			bool use7800Bios = true;
			Bios7800 bios = null;

			if (use7800Bios)
			{
				string biosFileName = null;

				switch (mt)
				{
					case MachineType.A7800NTSC:
						biosFileName = EngineFolders.ContentPathVirtualToReal("Emulator/Roms/Atari/_NtscBios.bin");
						break;
					case MachineType.A7800PAL:
						biosFileName = EngineFolders.ContentPathVirtualToReal("Emulator/Roms/Atari/_PalBios.bin");
						break;
				}

				if (biosFileName != null)
				{
					byte[] biosBytes = File.ReadAllBytes(biosFileName);
					bios = new Bios7800(biosBytes);
                    AngryWasp.Logger.Log.Instance.Write("BIOS Created");
				}
				else
                    AngryWasp.Logger.Log.Instance.Write("Skipped creating bios. No file path specified in settings");
			}

			return bios;
		}

		private HSC7800 CreateHsc()
		{
			bool useHsc = true;

			//set up a high score cart if we are using that
			if (useHsc)
			{
				byte[] hscCartBytes = File.ReadAllBytes(EngineFolders.ContentPathVirtualToReal("Emulator/Roms/Atari/_HighScoreCart.bin"));
				string hscRamFileName = EngineFolders.ContentPathVirtualToReal("Emulator/Roms/Atari/_HighScoreCart_Ram.bin");
				byte[] hscRamBytes = null;
				if (File.Exists(hscRamFileName))
					hscRamBytes = File.ReadAllBytes(hscRamFileName);
				else
					hscRamBytes = new byte[2048];

				AngryWasp.Logger.Log.Instance.Write("HSC Created");
				return new HSC7800(hscCartBytes, hscRamBytes);
			}
			return null;
		}
	}

	public class RomListEntry
	{
		public Controller c;
		public CartType ct;
		public MachineType mt;
		public string n;
	}
}
