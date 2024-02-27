using System;
using System.Collections.Generic;
using System.Numerics;
using AngryWasp.Helpers;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using Engine.Graphics.Vertices;
using Microsoft.Xna.Framework;

namespace Engine.Debug
{
    public static class ShapeGenerator
    {
        public static (VertexPositionColor[] vertices, short[] indices) BoundingBox(BoundingBox boundingBox, Color color) =>
            Box(boundingBox.GetCorners(), color);

        public static (VertexPositionColor[] vertices, short[] indices) BoundingFrustum(BoundingFrustum frustum, Color color) =>
            Box(frustum.GetCorners(), color);

        public static (VertexPositionColor[] vertices, short[] indices) BoundingSphere(BoundingSphere boundingSphere, Color color) =>
            Sphere(boundingSphere.Radius, 32, color);

        public static (VertexPositionColor[] vertices, short[] indices) Box(float width, float height, float length, Color color)
        {
            var h = new Vector3(width / 2, height / 2, length / 2);
            var corners = new BoundingBox(-h, h).GetCorners();
            return Box(corners, color);
        }

        public static (VertexPositionColor[] vertices, short[] indices) Square(int sz, Color color)
        {
            var v = new VertexPositionColor[4];
            var i = new short[v.Length * 2];
            float rad = sz / 2.0f;

            for (int x = 0; x < v.Length; x++)
            {
                v[x] = new VertexPositionColor(Vector3.Zero, color);
                i[x] = (short)x;

                short n = (short)(x + 1);
                if (n >= v.Length)
                    n = 0;

                i[x + 1] = n;
            }

            v[0].Position.X = -rad;
            v[0].Position.Z = -rad;

            v[1].Position.X = -rad;
            v[1].Position.Z = rad;

            v[2].Position.X = rad;
            v[2].Position.Z = rad;

            v[3].Position.X = rad;
            v[3].Position.Z = -rad;

            return (v, i);
        }

        public static void UpdateSquare(float sz, ref VertexPositionColor[] v)
        {
            float rad = sz / 2.0f;

            v[0].Position.X = -rad;
            v[0].Position.Z = -rad;

            v[1].Position.X = -rad;
            v[1].Position.Z = rad;

            v[2].Position.X = rad;
            v[2].Position.Z = rad;

            v[3].Position.X = rad;
            v[3].Position.Z = -rad;
        }

        public static (VertexPositionColor[] vertices, short[] indices) Circle(float radius, int resolution, Color color)
        {
            var ver = new List<VertexPositionColor>();
            List<short> ind = new List<short>();
            float step = MathHelper.TwoPi / (float)resolution;

            for (float a = 0f; a <= MathHelper.TwoPi; a += step)
            {
                Vector3 position = new Vector3(MathF.Cos(a), 0f, MathF.Sin(a));
                position *= radius;
                ver.Add(new VertexPositionColor(position, color));
            }

            for (short i = 0; i < ver.Count - 1; i++)
            {
                ind.Add(i);
                ind.Add((short)(i + 1));
            }

            ind.Add((short)(ver.Count - 1));
            ind.Add(0);

            return (ver.ToArray(), ind.ToArray());
        }

        public static void UpdateCircle(float radius, int resolution, ref VertexPositionColor[] v)
        {
            float step = MathHelper.TwoPi / (float)resolution;
            int index = 0;
            for (float a = 0f; a <= MathHelper.TwoPi; a += step)
            {
                var vec = new Vector2(MathF.Cos(a), MathF.Sin(a));
                vec *= radius;
                v[index].Position.X = vec.X;
                v[index].Position.Z = vec.Y;
                index++;
            }
        }

        public static (VertexPositionColor[] vertices, short[] indices) Box(Vector3[] corners, Color color)
        {
            var vertices = new VertexPositionColor[corners.Length];
            var indices = new short[]
            {
                0, 1,
                1, 2,
                2, 3,
                3, 0,

                0, 4,
                1, 5,
                2, 6,
                3, 7,

                4, 5,
                5, 6,
                6, 7,
                7, 4,
            };

            for (int i = 0; i < corners.Length; i++)
                vertices[i] = new VertexPositionColor(corners[i], color);

            return (vertices, indices);
        }

        public static (VertexPositionColor[] vertices, short[] indices) Sphere(float radius, int resolution, Color color)
        {
            VertexPositionColor[] ver = new VertexPositionColor[(resolution + 1) * 3];

            short index = 0;

            float step = MathHelper.TwoPi / (float)resolution;

            List<short> ind = new List<short>();

            //create the loop on the XY plane first
            for (float a = 0f; a <= MathHelper.TwoPi; a += step)
            {
                Vector3 position = new Vector3(MathF.Cos(a), MathF.Sin(a), 0f);
                position *= radius;
                ver[index++] = new VertexPositionColor(position, color);
            }

            short x = 0;

            for (short i = x; i < index - 1; i++)
            {
                ind.Add(i);
                ind.Add((short)(i + 1));
            }

            ind.Add((short)(index - 1));
            ind.Add(x);

            x = index;

            //next on the XZ plane
            for (float a = 0f; a <= MathHelper.TwoPi; a += step)
            {
                Vector3 position = new Vector3(MathF.Cos(a), 0f, MathF.Sin(a));
                position *= radius;
                ver[index++] = new VertexPositionColor(position, color);
            }

            for (short i = x; i < index - 1; i++)
            {
                ind.Add(i);
                ind.Add((short)(i + 1));
            }

            ind.Add((short)(index - 1));
            ind.Add(x);

            x = index;

            //finally on the YZ plane
            for (float a = 0f; a <= MathHelper.TwoPi; a += step)
            {
                Vector3 position = new Vector3(0f, MathF.Cos(a), MathF.Sin(a));
                position *= radius;
                ver[index++] = new VertexPositionColor(position, color);
            }

            for (short i = x; i < index - 1; i++)
            {
                ind.Add(i);
                ind.Add((short)(i + 1));
            }

            ind.Add((short)(index - 1));
            ind.Add(x);

            return (ver, ind.ToArray());
        }

        public static (VertexPositionColor[] vertices, short[] indices) Capsule(float radius, float length, int resolution, Color color)
        {
            short x = 0;
            float y = length / 2;

            List<VertexPositionColor> ver = new List<VertexPositionColor>();
            List<short> ind = new List<short>();

            short index = 0;

            List<short> topEnd = new List<short>();
            List<short> bottomEnd = new List<short>();

            float step = MathHelper.TwoPi / (float)resolution;

            { //create ring
                for (float a = 0f; a <= MathHelper.TwoPi; a += step)
                {
                    Vector3 p = new Vector3(MathF.Cos(a), 0, MathF.Sin(a));
                    p *= radius;
                    p.Y = y;
                    ver.Add(new VertexPositionColor(p, color));
                    ++index;
                }

                for (short i = x; i < x + index - 1; i++)
                {
                    ind.Add(i);
                    ind.Add((short)(i + 1));
                }

                //close ring
                ind.Add((short)(x + index - 1));
                ind.Add(x);

                index = 0;
                x = (short)ver.Count;
            }

            { //create first loop
                for (float a = (MathHelper.Pi + MathHelper.PiOver2); a <= (MathHelper.TwoPi + MathHelper.PiOver2 + step); a += step)
                {
                    Vector3 p = new Vector3(0, MathF.Cos(a), MathF.Sin(a));
                    p *= radius;
                    p.Y += y;
                    ver.Add(new VertexPositionColor(p, color));
                    ++index;
                }

                topEnd.Add(x);

                for (short i = x; i < x + index - 1; i++)
                {
                    ind.Add(i);
                    ind.Add((short)(i + 1));
                }

                topEnd.Add((short)(x + index - 1));

                index = 0;
                x = (short)(ver.Count);
            }

            { //create second loop
                for (float a = 0; a <= (MathHelper.Pi + step); a += step)
                {
                    Vector3 p = new Vector3(MathF.Cos(a), MathF.Sin(a), 0f);
                    p *= radius;
                    p.Y += y;
                    ver.Add(new VertexPositionColor(p, color));
                    ++index;
                }

                topEnd.Add(x);

                for (short i = x; i < x + index - 1; i++)
                {
                    ind.Add(i);
                    ind.Add((short)(i + 1));
                }

                topEnd.Add((short)(x + index - 1));

                index = 0;
                x = (short)(ver.Count);
            }

            { //create ring
                for (float a = 0f; a <= MathHelper.TwoPi; a += step)
                {
                    Vector3 p = new Vector3(MathF.Cos(a), 0, MathF.Sin(a));
                    p *= radius;
                    p.Y = -y;
                    ver.Add(new VertexPositionColor(p, color));
                    ++index;
                }

                for (short i = x; i < x + index - 1; i++)
                {
                    ind.Add(i);
                    ind.Add((short)(i + 1));
                }

                //close ring
                ind.Add((short)(x + index - 1));
                ind.Add(x);

                index = 0;
                x = (short)ver.Count;
            }

            { //create first loop
                for (float a = MathHelper.PiOver2; a <= (MathHelper.Pi + MathHelper.PiOver2 + step); a += step)
                {
                    Vector3 p = new Vector3(0, MathF.Cos(a), MathF.Sin(a));
                    p *= radius;
                    p.Y -= y;
                    ver.Add(new VertexPositionColor(p, color));
                    ++index;
                }

                bottomEnd.Add(x);

                for (short i = x; i < x + index - 1; i++)
                {
                    ind.Add(i);
                    ind.Add((short)(i + 1));
                }

                bottomEnd.Add((short)(x + index - 1));

                index = 0;
                x = (short)ver.Count;
            }

            { //create second loop
                for (float a = MathHelper.Pi; a <= (MathHelper.TwoPi + step); a += step)
                {
                    Vector3 p = new Vector3(MathF.Cos(a), MathF.Sin(a), 0f);
                    p *= radius;
                    p.Y -= y;
                    ver.Add(new VertexPositionColor(p, color));
                    ++index;
                }

                bottomEnd.Add(x);

                for (short i = x; i < x + index - 1; i++)
                {
                    ind.Add(i);
                    ind.Add((short)(i + 1));
                }

                bottomEnd.Add((short)(x + index - 1));
            }

            ind.Add(topEnd[0]);
            ind.Add(bottomEnd[1]);

            ind.Add(topEnd[1]);
            ind.Add(bottomEnd[0]);

            ind.Add(topEnd[2]);
            ind.Add(bottomEnd[3]);

            ind.Add(topEnd[3]);
            ind.Add(bottomEnd[2]);

            return (ver.ToArray(), ind.ToArray());
        }

        public static (VertexPositionColor[] vertices, short[] indices) Cylinder(float radius, float length, int resolution, Color color)
        {
            short x = 0;
            float y = length / 2;

            List<VertexPositionColor> ver = new List<VertexPositionColor>();
            List<short> ind = new List<short>();

            List<VertexPositionColor> v1;
            List<short> i1;

            CreateEnd(x, y, radius, resolution, color, out v1, out i1);
            ver.AddRange(v1);
            ind.AddRange(i1);

            x = (short)v1.Count;

            List<VertexPositionColor> v2;
            List<short> i2;

            CreateEnd(x, -y, radius, resolution, color, out v2, out i2);
            ver.AddRange(v2);
            ind.AddRange(i2);

            for (int j = 0; j < i1.Count; j += 8)
            {
                ind.Add(i1[j]);
                ind.Add(i2[j]);
            }

            return (ver.ToArray(), ind.ToArray());
        }

        public static (VertexPositionColor[] vertices, short[] indices) Cone(float radius, float length, int resolution, Color color)
        {
            short x = 0;
            float y = 0;

            List<VertexPositionColor> ver = new List<VertexPositionColor>();
            List<short> ind = new List<short>();

            List<VertexPositionColor> v1;
            List<short> i1;

            y = -length / 4;
            CreateEnd(x, y, radius, resolution, color, out v1, out i1);
            ver.AddRange(v1);
            ind.AddRange(i1);

            Vector3 p = new Vector3(0, length + y, 0);
            ver.Add(new VertexPositionColor(p, color));

            for (int j = 0; j < i1.Count; j += 8)
            {
                ind.Add(i1[j]);
                ind.Add((short)(ver.Count - 1));
            }

            return (ver.ToArray(), ind.ToArray());
        }

        public static (VertexPositionColor[] vertices, short[] indices) LightCone(float radius, float length, int resolution, Color color)
        {
            short x = 0;

            List<VertexPositionColor> ver = new List<VertexPositionColor>();
            List<short> ind = new List<short>();

            List<VertexPositionColor> v1;
            List<short> i1;

            CreateEnd(x, -length, radius, resolution, color, out v1, out i1);
            ver.AddRange(v1);
            ind.AddRange(i1);

            Vector3 p = new Vector3(0, 0, 0);
            ver.Add(new VertexPositionColor(p, color));

            for (int j = 0; j < i1.Count; j += 8)
            {
                ind.Add(i1[j]);
                ind.Add((short)(ver.Count - 1));
            }

            return (ver.ToArray(), ind.ToArray());
        }

        public static (VertexPositionColor[] vertices, short[] indices) TriangleBuffer(Buffer<Triangle> shape, Color color)
        {
            var vertices = new List<VertexPositionColor>();
            //var indices = new List<short>();

            //short offset = 0;

            for (int i = 0; i < shape.Length; i++)
            {
                vertices.Add(new VertexPositionColor(shape[i].A, color));
                vertices.Add(new VertexPositionColor(shape[i].B, color));
                vertices.Add(new VertexPositionColor(shape[i].C, color));

                /*short[] ind = {
                    offset, (short)(offset + 1),
                    (short)(offset + 1), (short)(offset + 2),
                    (short)(offset + 2), offset
                };

                indices.AddRange(ind);
                offset += 3;*/
            }

            return (vertices.ToArray(), null);
        }

        private static void CreateEnd(short x, float y, float radius, int resolution, Color color, out List<VertexPositionColor> vertices, out List<short> indices)
        {
            vertices = new List<VertexPositionColor>();
            indices = new List<short>();

            float step = MathHelper.TwoPi / (float)resolution;
            for (float a = 0f; a <= MathHelper.TwoPi; a += step)
            {
                Vector3 position = new Vector3(MathF.Cos(a), 0, MathF.Sin(a));
                position *= radius;
                position.Y += y;
                vertices.Add(new VertexPositionColor(position, color));
            }

            for (short i = x; i < x + vertices.Count - 1; i++)
            {
                indices.Add(i);
                indices.Add((short)(i + 1));
            }

            indices.Add((short)(x + vertices.Count - 1));
            indices.Add(x);
        }

        public static void UpdateSphere(float radius, int resolution, ref VertexPositionColor[] vertices)
        {
            short index = 0;

            float step = MathHelper.TwoPi / (float)resolution;

            //create the loop on the XY plane first
            for (float a = 0f; a <= MathHelper.TwoPi; a += step)
            {
                Vector3 position = new Vector3(MathF.Cos(a), MathF.Sin(a), 0f);
                position *= radius;
                vertices[index++].Position = position;
            }

            //next on the XZ plane
            for (float a = 0f; a <= MathHelper.TwoPi; a += step)
            {
                Vector3 position = new Vector3(MathF.Cos(a), 0f, MathF.Sin(a));
                position *= radius;
                vertices[index++].Position = position;
            }

            //finally on the YZ plane
            for (float a = 0f; a <= MathHelper.TwoPi; a += step)
            {
                Vector3 position = new Vector3(0f, MathF.Cos(a), MathF.Sin(a));
                position *= radius;
                vertices[index++].Position = position;
            }
        }
    }
}