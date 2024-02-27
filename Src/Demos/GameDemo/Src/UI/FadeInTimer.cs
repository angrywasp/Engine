using Microsoft.Xna.Framework;

namespace GameDemo.Helpers
{
    public class FadeInTimer
    {
        public delegate void TimerEventHandler(FadeInTimer sender);
        public event TimerEventHandler Finished;

        float elapsedTime = 0.0f;
        float fadeTime = 3;
        bool isFinished = false;
        float fade = 1.0f;
        byte fadeByte = 255;

        private bool running = false;

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
            if (!running)
                return;

            elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (elapsedTime < fadeTime)
                fade = fadeTime - elapsedTime;
            else
                fade = 0;

            fadeByte = (byte)(fade * (byte)(255 / fadeTime));

            if (elapsedTime >= fadeTime)
            {
                if (Finished != null)
                    Finished(this);

                running = false;
                isFinished = true;
            }
        }
    }
}
