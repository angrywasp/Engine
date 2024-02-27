using System.Numerics;
using Newtonsoft.Json;

namespace Engine
{
    [JsonObject(MemberSerialization.OptIn)]
    public class WorldTransform1
    {
        public EngineEventHandler<WorldTransform1> TransformChanged;

        private Vector3 translation = Vector3.Zero;
        private Matrix4x4 matrix = Matrix4x4.Identity;

        public Vector3 Translation => translation;
        public Matrix4x4 Matrix => matrix;

        public static WorldTransform1 Create(Vector3 translation) =>
            new WorldTransform1()
            {
                translation = translation,
                matrix = Matrix4x4.CreateTranslation(translation)
            };

        public static WorldTransform1 Create(Matrix4x4 matrix)
        {
            var t = new WorldTransform1();
            t.Update(matrix);
            return t;
        }

        public void Update(Quaternion rotation, Vector3 translation)
        {
            if (this.translation != translation)
            {
                this.translation = translation;
                this.matrix = Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(translation);
                TransformChanged?.Invoke(this);
            }
        }

        public void Update(Matrix4x4 matrix)
        {
            if (this.matrix.Translation != matrix.Translation)
            {
                this.matrix = matrix;
                this.translation = matrix.Translation;
                TransformChanged?.Invoke(this);
            }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class WorldTransform2
    {
        public EngineEventHandler<WorldTransform2> TransformChanged;

        private Quaternion rotation = Quaternion.Identity;
        private Vector3 translation = Vector3.Zero;
        private Matrix4x4 matrix = Matrix4x4.Identity;

        public Quaternion Rotation => rotation;
        public Vector3 Translation => translation;
        public Matrix4x4 Matrix => matrix;

        public static WorldTransform2 Create(Quaternion rotation, Vector3 translation) =>
            new WorldTransform2()
            {
                rotation = rotation,
                translation = translation,
                matrix = Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(translation)
            };

        public static WorldTransform2 Create(Matrix4x4 matrix)
        {
            var t = new WorldTransform2();
            t.Update(matrix);
            return t;
        }

        public void Update(Quaternion rotation, Vector3 translation)
        {
            if (this.rotation != rotation || this.translation != translation)
            {
                this.translation = translation;
                this.rotation = rotation;
                this.matrix = Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(translation);
                TransformChanged?.Invoke(this);
            }
        }

        public void Update(Matrix4x4 matrix)
        {
            if (this.matrix != matrix)
            {
                this.matrix = matrix;
                Matrix4x4.Decompose(matrix, out _, out rotation, out translation);
                TransformChanged?.Invoke(this);
            }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class WorldTransform3
    {
        public EngineEventHandler<WorldTransform3> TransformChanged;

        private Vector3 scale = Vector3.One;
        private Quaternion rotation = Quaternion.Identity;
        private Vector3 translation = Vector3.Zero;
        private Matrix4x4 matrix = Matrix4x4.Identity;

        public Vector3 Scale => scale;
        public Quaternion Rotation => rotation;
        public Vector3 Translation => translation;
        public Matrix4x4 Matrix => matrix;

        public static WorldTransform3 Create(Vector3 translation) => 
            new WorldTransform3()
            {
                translation = translation,
                matrix = Matrix4x4.CreateTranslation(translation)
            };

        public static WorldTransform3 Create(Quaternion rotation, Vector3 translation) =>
            new WorldTransform3()
            {
                rotation = rotation,
                translation = translation,
                matrix = Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(translation)
            };

        public static WorldTransform3 Create(Vector3 scale, Quaternion rotation, Vector3 translation) =>
            new WorldTransform3()
            {
                scale = scale,
                rotation = rotation,
                translation = translation,
                matrix = Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(translation)
            };

        public static WorldTransform3 Create(Matrix4x4 matrix)
        {
            var t = new WorldTransform3();
            t.Update(matrix);
            return t;
        }

        public void Update(Quaternion rotation, Vector3 translation)
        {
            if (this.rotation != rotation || this.translation != translation)
            {
                this.translation = translation;
                this.rotation = rotation;
                this.matrix = Matrix4x4.CreateScale(this.scale) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(translation);
                TransformChanged?.Invoke(this);
            }
        }

        public void Update(Vector3 scale, Quaternion rotation, Vector3 translation)
        {
            if (this.scale != scale || this.rotation != rotation || this.translation != translation)
            {
                this.translation = translation;
                this.rotation = rotation;
                this.scale = scale;
                this.matrix = Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(translation);
                TransformChanged?.Invoke(this);
            }
        }

        public void Update(Matrix4x4 matrix)
        {
            if (this.matrix != matrix)
            {
                this.matrix = matrix;
                Matrix4x4.Decompose(matrix, out scale, out rotation, out translation);
                TransformChanged?.Invoke(this);
            }
        }
    }
}