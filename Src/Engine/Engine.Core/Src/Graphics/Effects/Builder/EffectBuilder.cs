using System;
using System.Collections.Generic;
using System.Linq;
using AngryWasp.Logger;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Graphics.Effects.Builder
{
    public class EffectBuilder
    {
        private GraphicsDevice device;
        private Effect effect;

        private List<EffectProgram> programs = new List<EffectProgram>();

        public EffectBuilder Start()
        {
            this.device = null;
            this.effect = null;
            return this;
        }

        public EffectBuilder Start(GraphicsDevice device)
        {
            this.device = device;
            this.effect = new Effect(device);
            return this;
        }

        public EffectBuilder CreateProgram(VertexShader vertexShader, PixelShader pixelShader, string name = "Default")
        {
            EffectProgram ep = new EffectProgram(effect, name, vertexShader, null, null, pixelShader);
            programs.Add(ep);
            return this;
        }

        public EffectBuilder CreateProgram(VertexShader vertexShader, TessellationControlShader tessCtrlShader, TessellationEvaluationShader tessEvalShader, PixelShader pixelShader, string name = "Default")
        {
            EffectProgram ep = new EffectProgram(effect, name, vertexShader, tessCtrlShader, tessEvalShader, pixelShader);
            programs.Add(ep);
            return this;
        }

        public Effect Finish()
        {
            Dictionary<string, EffectParameter> parameters = null;
            Dictionary<string, int> locations = null;
            Dictionary<string, EffectParameter> ep = new Dictionary<string, EffectParameter>();

            try
            {
                foreach (var pass in programs)
                {
                    ShaderProgram sp = ShaderProgramBuilder.Build(device, pass.VertexShader, pass.TessellationControlShader, pass.TessellationEvaluationShader, pass.PixelShader);
                    sp.Query(ref parameters, ref locations, pass.Name);

                    foreach (var p in parameters)
                        if (!ep.ContainsKey(p.Key))
                            ep.Add(p.Key, p.Value);

                    pass.SetData(sp, locations);
                }

                effect.SetData(new EffectParameterCollection(ep.Values.ToArray()), new EffectProgramCollection(programs.ToArray()));

                return effect;
            }
            catch (Exception e)
            {
                throw Log.Instance.WriteFatalException(e, "EffectBuilder.Finish failed to create effect");
            }
        }
    }
}