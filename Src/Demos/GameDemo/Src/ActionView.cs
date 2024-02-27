using Engine;
using Engine.World;
using Microsoft.Xna.Framework;

namespace GameDemo
{
    public class ActionView : IView
    {
        private EngineCore engine;
        private MapObject character;
        private MapObject controller;

        public ActionView(MapObject character, MapObject controller)
        {
            this.engine = EngineCore.Instance;
            this.character = character;
            this.controller = controller;
        }
        
        public void Draw(GameTime gameTime)
        {
            
        }

        public void Initialize()
        {
            engine.Interface.CreateDefaultForm();
        }

        public void Resize(int w, int h) { }

        public void UnInitialize()
        {
            
        }

        public bool ShouldUpdate()
        {
            return !engine.Interface.Terminal.Visible;
        }

        public void Update(GameTime gameTime)
        {
            engine.Camera.Controller.Update(gameTime);
        }
    }
}