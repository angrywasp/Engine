namespace Engine.Content.Model
{
    public class AnimationTrackInfo
    {
        private string name;
        private float duration;

        public string Name => name;
        public float Duration => duration;

        public AnimationTrackInfo(string name, float duration)
        {           
            this.name = name; 
            this.duration = duration;
        }

        public override string ToString() => $"{name}: {duration} seconds";
    }
}
