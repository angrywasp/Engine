namespace Microsoft.Xna.Framework.Graphics
{
    public class Effect : GraphicsResource
    {
        private EffectParameterCollection parameters;
        private EffectProgramCollection programs;

        public EffectParameterCollection Parameters => parameters;
        public EffectProgramCollection Programs => programs;

        public EffectProgram DefaultProgram { get; set; }

        public Effect(GraphicsDevice device)
        {
            GraphicsDevice = device;
        }

        public void SetData(EffectParameterCollection parameters, EffectProgramCollection programs)
        {
            this.parameters = parameters;
            this.programs = programs;

            DefaultProgram = this.programs[0];
        }

        public void Apply()
        {
            foreach (var p in parameters)
                p.Apply();
        }
    }
}