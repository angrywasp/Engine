using Microsoft.Xna.Framework;
using Engine.Content;
using Microsoft.Xna.Framework.Audio;
using System.Numerics;
using System;
using AngryWasp.Math;
using Engine.World.Objects;
using Engine.World.Components;

namespace Engine.Editor.Views
{
    public class SoundEffectView : View3D
    {
        private SoundEffect soundEffect;
        private SoundEffectInstance soundInstance;

        private AudioListener listener = new AudioListener();
        private AudioEmitter emitter = new AudioEmitter();

        public override void InitializeView(string path)
        {
            base.InitializeView(path);

            Vector3 cp = new Vector3(0f, 2.5f, 2.5f);
            cameraController.Position = cp;
            cameraController.Pitch = -(MathF.Atan2(cp.Y, cp.Z) * Radian.ToDegCoefficient);

            engine.Scene.MapLoaded += (_, _) => {
                var mo = engine.Scene.CreateMapObject<RuntimeMapObject>("Source");
                mo.Loaded += async (mapObject, gameObject) =>
                {
                    var meshComponent = await mo.AddMeshComponent(new MeshComponentType
                    {
                        Mesh = "Engine/Renderer/Meshes/Sphere.mesh",
                        LocalTransform = WorldTransform3.Create(Quaternion.Identity, Vector3.UnitY * 2)
                    }).ConfigureAwait(false);

                    await meshComponent.Mesh.SetMaterialAsync(engine, "Engine/Materials/Colors/DeepSkyBlue.material", 0).ConfigureAwait(false);

                    emitter.Position = meshComponent.GlobalTransform.Translation;
                    soundEffect = await ContentLoader.LoadSoundEffectAsync(path).ConfigureAwait(false);
                    soundInstance = soundEffect.CreateInstance();

                    soundInstance.Volume = 0.5f;
                    soundInstance.IsLooped = true;
                    soundInstance.Play();
                };
            };

            CreateDefaultScene();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            listener.Position = cameraController.Position;
            listener.Forward = -cameraController.Transform.Forward();
            listener.Up = -cameraController.Transform.Up();

            soundInstance.Apply3D(listener, emitter);
        }
    }
}
