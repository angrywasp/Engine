using System;
using Microsoft.Xna.Framework;
using Engine.Interfaces;
using System.Numerics;

namespace Engine.Content
{
    public class ParticleSettings : IJsonSerialized
    {
        public string TextureName { get; set; } = null;

        public int MaxParticles { get; set; } = 100;

        public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(1);

        public float DurationRandomness { get; set; } = 0;

        public float EmissionRate { get; set; } = 10;

        public float EmitterVelocitySensitivity { get; set; } = 1;

        public float MinHorizontalVelocity { get; set; } = 0;
        public float MaxHorizontalVelocity { get; set; } = 0;

        public float MinVerticalVelocity { get; set; } = 0;
        public float MaxVerticalVelocity { get; set; } = 0;

        public Vector3 Gravity = Vector3.Zero;

        public float EndVelocity { get; set; } = 1;

        public Color MinColor { get; set; } = Color.White;
        public Color MaxColor { get; set; } = Color.White;

        public float MinRotateSpeed { get; set; } = 0;
        public float MaxRotateSpeed { get; set; } = 0;

        public float MinStartSize { get; set; } = 100;
        public float MaxStartSize { get; set; } = 100;

        public float MinEndSize { get; set; } = 100;
        public float MaxEndSize { get; set; } = 100;
    }
}
