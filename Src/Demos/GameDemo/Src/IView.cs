using Microsoft.Xna.Framework;

namespace GameDemo
{
    public interface IView
    {
        void Initialize();
        void UnInitialize();
        void Resize(int w, int h);
        bool ShouldUpdate();
        void Update(GameTime gameTime);
        void Draw(GameTime gameTime);
    }

    public class ViewImpl : IView
    {
        public void Initialize() { }

        public void UnInitialize() { }

        public void Resize(int w, int h) { }

        public bool ShouldUpdate() { return true; }

        public void Update(GameTime gameTime) { }

        public void Draw(GameTime gameTime) { }
    }
}
