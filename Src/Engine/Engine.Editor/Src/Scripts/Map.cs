using System.Collections.Generic;
using System.IO;
using System.Numerics;
using AngryWasp.Logger;
using AngryWasp.Math;
using AngryWasp.Random;
using Engine.Helpers;
using Engine.World;

namespace Engine.Editor.Scripts
{
    public static class MapScripts
    {
        private static XoShiRo128PlusPlus rng = new XoShiRo128PlusPlus();
        private static Map map;

        private static bool isPainting = false;
        private static bool isRandomMesh = false;
        private static string paintedMesh;

        public static bool IsPainting => isPainting;

        public static string PaintedMesh => paintedMesh;

        public static bool IsRandomMesh => isRandomMesh;

        private static Range randomScaleRange = new Range(0.5f, 1.5f);

        private static Range randomRotationRangeX = new Range(-10.0f, 10.0f);
        private static Range randomRotationRangeY = new Range(0.0f, 360.0f);

        private static Range randomRotationRangeZ = new Range(-10.0f, 10.0f);
        private static List<string> randomMeshes = new List<string>();

        public static void Load(Map m)
        {
            map = m;
        }

        public static void Paint(string mesh, int radius)
        {
            isPainting = true;
            isRandomMesh = false;
            paintedMesh = mesh;
        }

        public static void EndPaint()
        {
            isPainting = false;
            isRandomMesh = false;
            paintedMesh = null;
            randomMeshes.Clear();
        }

        public static void SetRandomScaleRange(float min, float max) => randomScaleRange = new Range(min, max);

        public static float GetRandomScale() => rng.NextFloat(randomScaleRange.Minimum, randomScaleRange.Maximum);

        public static Vector3 GetRandomRotation() => new Vector3(
            rng.NextFloat(randomRotationRangeX.Minimum, randomRotationRangeX.Maximum) * Degree.ToRadCoefficient,
            rng.NextFloat(randomRotationRangeY.Minimum, randomRotationRangeY.Maximum) * Degree.ToRadCoefficient,
            rng.NextFloat(randomRotationRangeZ.Minimum, randomRotationRangeZ.Maximum) * Degree.ToRadCoefficient);

        public static void SetRandomMeshFolder(string path)
        {
            isPainting = true;
            isRandomMesh = true;
            paintedMesh = null;

            string fullPath = EngineFolders.ContentPathVirtualToReal(path);
            if (!Directory.Exists(fullPath))
            {
                Log.Instance.WriteError($"Directory {path} does not exist");
                return;
            }

            foreach (var f in Directory.GetFiles(EngineFolders.ContentPathVirtualToReal(path), "*.mesh"))
                randomMeshes.Add(EngineFolders.ContentPathRealToVirtual(f));
        }

        public static string GetRandomMesh()
        {
            if (randomMeshes.Count == 0)
            {
                Log.Instance.WriteError($"No meshes present in ");
                return null;
            }

            return randomMeshes[rng.Next(0, randomMeshes.Count)];
        }
    }
}