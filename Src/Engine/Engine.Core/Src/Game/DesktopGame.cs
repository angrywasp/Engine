using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Engine.Game
{
    public class DesktopGame : GameBase
    {
        private int targetFPS = 0;
        private TimeSpan _targetElapsedTime = TimeSpan.FromTicks(0);

        private bool _shouldExit;

        public override string WindowTitle => "Editor";

        public bool IsMouseVisible
        {
            get => platform.IsMouseVisible;
            set => platform.IsMouseVisible = value;
        }

        public SdlGameWindow Window => platform.Window;

        public int TargetFPS
        {
            get => targetFPS;
            set
            {
                targetFPS = value;
                var ticks = (long)((1.0f / (float)value) * 10000000);
                _targetElapsedTime = TimeSpan.FromTicks(ticks);
            }
        }

        public override void ExitGame()
        {
            _shouldExit = true;
        }

        public void ResetElapsedTime()
        {
            _gameTimer.Reset();
            _gameTimer.Start();
            _accumulatedElapsedTime = TimeSpan.Zero;
            _gameTime.ElapsedGameTime = TimeSpan.Zero;
            _previousTicks = 0L;
        }
        
        public void Run()
        {
            AssertNotDisposed();

            _gameTimer = Stopwatch.StartNew();
            DoUpdate(new GameTime());
            platform.RunLoop();
            Dispose();
        }

        private TimeSpan _accumulatedElapsedTime;
        private readonly GameTime _gameTime = new GameTime();
        private Stopwatch _gameTimer;
        private long _previousTicks = 0;

        public override void Tick()
        {
            // NOTE: This code is very sensitive and can break very badly
            // with even what looks like a safe change.  Be sure to test 
            // any change fully in both the fixed and variable timestep 
            // modes across multiple devices and platforms.

        RetryTick:

            if (!platform.IsActive)
                System.Threading.Thread.Sleep(500);
            
            // Advance the accumulated elapsed time.
            var currentTicks = _gameTimer.Elapsed.Ticks;
            _accumulatedElapsedTime += TimeSpan.FromTicks(currentTicks - _previousTicks);
            _previousTicks = currentTicks;

            // If we're in the fixed timestep mode and not enough time has elapsed
            // to perform an update we sleep off the the remaining time to save battery
            // life and/or release CPU time to other threads and processes.
            if (_accumulatedElapsedTime < _targetElapsedTime)
                goto RetryTick;

            // Perform a single variable length update.
            _gameTime.ElapsedGameTime = _accumulatedElapsedTime;
            _gameTime.TotalGameTime += _accumulatedElapsedTime;
            _accumulatedElapsedTime = TimeSpan.Zero;

            DoUpdate(_gameTime);
            DoDraw(_gameTime);

            if (_shouldExit)
                platform.Exit();
        }

        protected virtual void Draw(GameTime gameTime) { }

        protected virtual void Update(GameTime gameTime) { }

        internal void DoUpdate(GameTime gameTime)
        {
            AssertNotDisposed();
            FrameworkDispatcher.Update();
            Update(gameTime);
        }

        internal void DoDraw(GameTime gameTime)
        {
            AssertNotDisposed();
            Draw(gameTime);
            platform.Present();
        }
    }
}

