using System.IO;
using EmulatorCore;
using Engine;
using Engine.Configuration;
using Engine.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace C64Emulator
{
    public class Emulator : EmulatorGame
    {
        string BASIC_ROM_FILE = "Emulator/Roms/C64/basic.bin";
        string KERNAL_ROM_FILE = "Emulator/Roms/C64/kernal.v2.bin";
        string CHAR_ROM_FILE = "Emulator/Roms/C64/chargen.bin";
        string FLOPPY_ROM_FILE = "Emulator/Roms/C64/1541.bin";

        File basic, kernal, chargen, rom1541;

        private Board.Board board;
        private DiskDrive.CBM1541 drive;
        private Input.Keyboard keyboard;
        private VideoOutput video;

        private const int OFFSET_X = 0;
        private const int OFFSET_W = 140;

        private const int OFFSET_Y = 16;
        private const int OFFSET_H = 43;

        private Vector2i renderSize;
        private Rectangle renderRegion;

        private byte currentJoystick;

        public override string WindowTitle => "C64 Emulator";

        protected override void Initialize()
        {
            rt = new RenderTarget2D(graphicsDevice, Video.VIC.X_RESOLUTION, Video.VIC.Y_RESOLUTION);
            video = new VideoOutput(rt);
            renderSize = new Vector2i(rt.Width, rt.Height);
            renderRegion = new Rectangle(OFFSET_X, OFFSET_Y, rt.Width - OFFSET_W, rt.Height - OFFSET_H);

            basic = LoadRom(BASIC_ROM_FILE);
            kernal = LoadRom(KERNAL_ROM_FILE);
            chargen = LoadRom(CHAR_ROM_FILE);
            rom1541 = LoadRom(FLOPPY_ROM_FILE);

            CreateEmulator();
            base.Initialize();

            AttachImage("Emulator/Roms/C64/1942.d64");
            board.Start();
        }

        int rowIndex = 0;
        int colIndex = 0;
        bool isMacro = false;
        string[] macro;

        private void StartMacro(string[] m)
        {
            rowIndex = 0;
            colIndex = 0;
            isMacro = true;
            macro = m;
        }

        private void IterateMacro()
        {
            if (colIndex >= macro.Length)
            {
                rowIndex = 0;
                colIndex = 0;
                isMacro = false;
                macro = null;
                return;
            }

            if (rowIndex >= macro[colIndex].Length)
            {
                rowIndex = 0;
                keyboard.KeyDown(Input.Keyboard.Keys.KEY_RET, false);
                ++colIndex;
                return;
            }

            var thisKey = KeyMap.GetValue(macro[colIndex][rowIndex]);
            keyboard.KeyDown(thisKey.Key, thisKey.Shifted);

            ++rowIndex;
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (running)
            {
                board.Start();
                Keys[] k = input.Keyboard.State.GetPressedKeys();

                keyboard.AllKeysUp();

                if (isMacro)
                    IterateMacro();
                else if (input.Keyboard.KeyJustPressed(Keys.Tab))
                {
                    if (!isMacro)
                    {
                        string[] testMacro = 
                        {
                            //"10 PRINT \"{CLR/HOME}\"",
                            "20 PRINT CHR$(205.5 + RND(1));",
                            "40 GOTO 20",
                            //"RUN"
                        };
                        //todo: macro to run basic scripts from file
                        StartMacro(testMacro);
                    }
                }
                else
                {
                    foreach (Keys kk in k)
                    {
                        byte val = (byte)kk;

                        //numbers D0 - D9
                        if (val >= 48 && val <= 57)
                        {
                            if (input.Keyboard.ModifierKeyDown(Sdl.Keyboard.Keymod.LeftShift) ||
                                input.Keyboard.ModifierKeyDown(Sdl.Keyboard.Keymod.RightShift))
                            {
                                switch (val)
                                {
                                    case 48: //map shift+0 to ), shift+9 on c64 kb
                                        keyboard.KeyDown(Input.Keyboard.Keys.KEY_9, true);
                                        break;
                                    case 50: //map shift+2 to @
                                        keyboard.KeyDown(Input.Keyboard.Keys.KEY_AT, false);
                                        break;
                                    case 54: //do nothing, no key for ^
                                        break;
                                    case 55: //map shift+7 to &, shift+6 on c64 kb
                                        keyboard.KeyDown(Input.Keyboard.Keys.KEY_6, true);
                                        break;
                                    case 56: //map shift+8 to *
                                        keyboard.KeyDown(Input.Keyboard.Keys.KEY_STAR, false);
                                        break;
                                    case 57: //map shift+9 to (, shift+8 on c64 kb
                                        keyboard.KeyDown(Input.Keyboard.Keys.KEY_8, true);
                                        break;
                                    default:
                                        keyboard.KeyDown((Input.Keyboard.Keys)(val - 48), true);
                                        break;
                                }
                            }
                            else
                                keyboard.KeyDown((Input.Keyboard.Keys)(val - 48), false);
                        }
                        else if (val >= 65 && val <= 90)
                            keyboard.KeyDown((Input.Keyboard.Keys)(val - 65 + 10), false);
                        else if (kk == Keys.OemQuotes)
                        {
                            //" maps to shift+2 on the commodore keyboard
                            //' maps to shift+7
                            if (input.Keyboard.ModifierKeyDown(Sdl.Keyboard.Keymod.LeftShift) ||
                                input.Keyboard.ModifierKeyDown(Sdl.Keyboard.Keymod.RightShift))
                                keyboard.KeyDown(Input.Keyboard.Keys.KEY_2, true);
                            else
                                keyboard.KeyDown(Input.Keyboard.Keys.KEY_7, true);
                        }
                        else if (kk == Keys.OemComma)
                        {
                            keyboard.KeyDown(Input.Keyboard.Keys.KEY_COM,
                                input.Keyboard.ModifierKeyDown(Sdl.Keyboard.Keymod.LeftShift) ||
                                input.Keyboard.ModifierKeyDown(Sdl.Keyboard.Keymod.RightShift));
                        }
                        else if (kk == Keys.OemPeriod)
                        {
                            keyboard.KeyDown(Input.Keyboard.Keys.KEY_DOT,
                                input.Keyboard.ModifierKeyDown(Sdl.Keyboard.Keymod.LeftShift) ||
                                input.Keyboard.ModifierKeyDown(Sdl.Keyboard.Keymod.RightShift));
                        }
                        else if (kk == Keys.OemMinus)
                            keyboard.KeyDown(Input.Keyboard.Keys.KEY_MI, false);
                        else if (kk == Keys.OemPlus)
                        {
                            if (input.Keyboard.ModifierKeyDown(Sdl.Keyboard.Keymod.LeftShift) ||
                                input.Keyboard.ModifierKeyDown(Sdl.Keyboard.Keymod.RightShift))
                                keyboard.KeyDown(Input.Keyboard.Keys.KEY_PL, false);
                            else
                                keyboard.KeyDown(Input.Keyboard.Keys.KEY_EQ, false);
                        }
                        else if (kk == Keys.OemQuestion)
                        {
                            keyboard.KeyDown(Input.Keyboard.Keys.KEY_SLASH,
                                input.Keyboard.ModifierKeyDown(Sdl.Keyboard.Keymod.LeftShift) ||
                                input.Keyboard.ModifierKeyDown(Sdl.Keyboard.Keymod.RightShift));
                        }
                        else if (kk == Keys.OemSemicolon)
                        {
                            if (input.Keyboard.ModifierKeyDown(Sdl.Keyboard.Keymod.LeftShift) ||
                                input.Keyboard.ModifierKeyDown(Sdl.Keyboard.Keymod.RightShift))
                                keyboard.KeyDown(Input.Keyboard.Keys.KEY_COL, false);
                            else
                                keyboard.KeyDown(Input.Keyboard.Keys.KEY_SCOL, false);
                        }
                        else if (kk == Keys.Delete || kk == Keys.Back)
                            keyboard.KeyDown(Input.Keyboard.Keys.KEY_DEL, false);
                        else if (kk == Keys.Space)
                            keyboard.KeyDown(Input.Keyboard.Keys.KEY_SP, false);
                        else if (kk == Keys.Enter)
                            keyboard.KeyDown(Input.Keyboard.Keys.KEY_RET, false);
                    }
                }
            }
            else
            {
                //todo: pause system
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            graphicsDevice.Clear(GraphicsDevice.DiscardDefault);
            uiRenderer.DrawRectangle(video.RenderTarget, Color.White, Vector2i.Zero, ui.Size, renderRegion);
            ui.Draw();
        }

        public File LoadRom(string fileName)
        {
            fileName = EngineFolders.ContentPathVirtualToReal(fileName);
            return new File(new FileInfo(fileName));
        }

        private void CreateEmulator()
        {
            DestoryEmulator();
            board = new Board.Board(video, kernal, basic, chargen);
            drive = new DiskDrive.CBM1541(rom1541, board.Serial);
            board.SystemClock.OnPhaseEnd += drive.DriveClock.Run;
            board.OnLoadState += drive.ReadDeviceState;
            board.OnSaveState += drive.WriteDeviceState;
            keyboard = new Input.Keyboard(board.SystemCias[0].PortA, board.SystemCias[0].PortB, null);
            KeyMap.Initialize(keyboard);
        }

        private void DestoryEmulator()
        {
            if (board != null)
            {
                board.SystemClock.OnPhaseEnd -= drive.DriveClock.Run;
                board.OnLoadState -= drive.ReadDeviceState;
                board.OnSaveState -= drive.WriteDeviceState;

                board = null;
                drive = null;
                keyboard = null;
            }
        }

        public void LoadState(string fileName)
        {
            fileName = EngineFolders.ContentPathVirtualToReal(fileName);
            board.LoadState(new File(new FileInfo(fileName)));
        }

        public void SaveState(string fileName)
        {
            fileName = EngineFolders.ContentPathVirtualToReal(fileName);
            board.SaveState(new File(new FileInfo(fileName)));
        }

        public void AttachImage(string fileName)
        {
            fileName = EngineFolders.ContentPathVirtualToReal(fileName);
            drive.Drive.Attach(new File(new FileInfo(fileName)));
        }

        public void SwapJoystick()
        {
            currentJoystick = currentJoystick == 0 ? (byte)5 : (byte)0;
        }

        private readonly RasterizerState DefaultRasterizerState = new RasterizerState()
        {
            CullMode = CullMode.CullCounterClockwiseFace,
            ScissorTestEnable = true
        };
    }
}
