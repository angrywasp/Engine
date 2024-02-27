using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Engine.Game
{
    public class ConsoleGame : GameBase
    {
        public override string WindowTitle => "Console";
        
        public ConsoleGame()
        {
            if (platform == null)
                Debugger.Break();
            Tick();
        }

        protected override void Initialize()
        {
            graphicsDeviceManager.PreferredBackBufferWidth = 1;
            graphicsDeviceManager.PreferredBackBufferHeight = 1;

            platform.IsMouseVisible = false;

            base.Initialize();
        }

        public void Run(Action<ConsoleGame> action)
        {
            Task.Run(() => { action(this); });

            while(true)
            {
                Tick();
                Thread.Sleep(100);
            }
        }

        public override void Tick() => Threading.Run();

        public override void ExitGame() => Dispose();

        public async Task ExitGameAsync()
        {
            await new AsyncUiTask().Run(() => {
                ExitGame();
            }).ConfigureAwait(false);
        }
    }
}