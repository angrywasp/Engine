using Microsoft.Xna.Framework;

namespace GameDemo.Helpers
{
    public class Timer
    {
        public delegate void TimerEventHandler(Timer sender);
        public event TimerEventHandler Finished;

        float elapsedTime = 0.0f;
        float fadeTime = 3;
        float time = 8;
        bool isFinished = false;
        float fade = 1.0f;
        byte fadeByte = 255;

        private bool running = false;

        public bool Running => running;

        public float ElapsedTime => elapsedTime;

        public byte Fade => fadeByte;

        public bool IsFinished => isFinished;


        public void Start()
        {
            elapsedTime = 0;
            running = true;
            isFinished = false;
        }

        private void Stop()
        {
            elapsedTime = 0;
            running = false;
        }

        public void Update(GameTime gameTime)
        {
            if (!Running)
                return;

            elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (elapsedTime < fadeTime)
                fade = fadeTime - elapsedTime;
            else if (elapsedTime > (time - fadeTime))
                fade = fadeTime - (time - elapsedTime);
            else
                fade = 0;

            fadeByte = (byte)(fade * (byte)(255 / fadeTime));

            if (elapsedTime >= time)
            {
                if (Finished != null)
                    Finished(this);

                running = false;
                isFinished = true;
            }
        }
    }
}