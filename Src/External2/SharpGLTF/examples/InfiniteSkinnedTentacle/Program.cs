﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using SharpGLTF.Schema2;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;

namespace InfiniteSkinnedTentacle
{
    using VERTEX = VertexBuilder<VertexPosition, VertexColor1, VertexJoints4>;
    using MESH = MeshBuilder<VertexPosition, VertexColor1, VertexJoints4>;

    class Program
    {
        // Skinning use cases and examples: https://github.com/KhronosGroup/glTF/issues/1403

        // hierarchy created:

        // Mesh1
        // Skin1─> Armature1
        // Skin2─> Armature2
        // Skin3─> Armature3
        // Scene
        // ├── Armature1
        // │   ├── Bone1
        // │   ├── Bone2
        // │   └── Bone3        
        // ├── SkinnedMesh1─> Mesh1, Skin1
        // ├── Armature2
        // │   ├── Bone1
        // │   ├── Bone2
        // │   └── Bone3
        // ├── SkinnedMesh2─> Mesh1, Skin2
        // ├── Armature3
        // │   ├── Bone1
        // │   ├── Bone2
        // │   └── Bone3
        // └── SkinnedMesh3─> Mesh1, Skin3
        // ...

        static void Main(string[] args)
        {
            var model = ModelRoot.CreateModel();
            var scene = model.UseScene("default");

            var mesh = model
                .CreateMeshes(CreateMesh(10))
                .First();

            RecusiveTentacle(scene, scene, Matrix4x4.CreateTranslation(+25, 0, +25), mesh, Quaternion.CreateFromYawPitchRoll(0f, 0.2f, 0f), 2);
            RecusiveTentacle(scene, scene, Matrix4x4.CreateTranslation(-25, 0, +25), mesh, Quaternion.CreateFromYawPitchRoll(0.2f, 0f, 0f), 2);
            RecusiveTentacle(scene, scene, Matrix4x4.CreateTranslation(-25, 0, -25), mesh, Quaternion.CreateFromYawPitchRoll(0f, 0f, 0.2f), 2);
            RecusiveTentacle(scene, scene, Matrix4x4.CreateTranslation(+25, 0, -25), mesh, Quaternion.CreateFromYawPitchRoll(0.2f, 0f, 0f), 2);            

            model.SaveGLB("recursive tentacles.glb");
            model.SaveGLTF("recursive tentacles.gltf");

            model.SaveAsWavefront("recursive_tentacles_at_000.obj", model.LogicalAnimations[0], 0);
            model.SaveAsWavefront("recursive_tentacles_at_025.obj", model.LogicalAnimations[0], 0.25f);
            model.SaveAsWavefront("recursive_tentacles_at_050.obj", model.LogicalAnimations[0], 0.50f);
            model.SaveAsWavefront("recursive_tentacles_at_075.obj", model.LogicalAnimations[0], 0.75f);
            model.SaveAsWavefront("recursive_tentacles_at_100.obj", model.LogicalAnimations[0], 1);
        }
        
        static void RecusiveTentacle(Scene scene, IVisualNodeContainer parent, Matrix4x4 offset, Mesh mesh, Quaternion anim, int repeat)
        {
            parent = parent
                .CreateNode()
                .WithLocalTransform(offset);

            parent = AddTentacleSkeleton(scene, parent as Node, mesh, anim);

            if (repeat == 0) return;

            var scale = Matrix4x4.CreateScale(0.2f);

            RecusiveTentacle(scene, parent, Matrix4x4.CreateTranslation(+15, 0, +15) * scale, mesh, Quaternion.CreateFromYawPitchRoll(0f, 0.2f, 0f), repeat - 1);
            RecusiveTentacle(scene, parent, Matrix4x4.CreateTranslation(-15, 0, +15) * scale, mesh, Quaternion.CreateFromYawPitchRoll(0.2f, 0f, 0f), repeat - 1);
            RecusiveTentacle(scene, parent, Matrix4x4.CreateTranslation(-15, 0, -15) * scale, mesh, Quaternion.CreateFromYawPitchRoll(0f, 0f, 0.2f), repeat - 1);
            RecusiveTentacle(scene, parent, Matrix4x4.CreateTranslation(+15, 0, -15) * scale, mesh, Quaternion.CreateFromYawPitchRoll(0.2f, 0f, 0f), repeat - 1);
        }

        static Node AddTentacleSkeleton(Scene scene, Node skeleton, Mesh mesh, Quaternion anim)
        {
            var bindings = new List<Node>();

            Node bone = null;

            for (int i = 0; i < 10; ++i)
            {
                if (bone == null)
                {
                    bone = skeleton;
                }
                else
                {
                    bone = bone.CreateNode();
                    bone.LocalTransform = Matrix4x4.CreateTranslation(0, 10, 0);
                }

                bone.WithRotationAnimation("Track0", (0, Quaternion.Identity), (1, anim), (2, Quaternion.Identity));

                bindings.Add(bone);                
            }

            scene
                .CreateNode()
                .WithSkinnedMesh(mesh, skeleton.WorldMatrix, bindings.ToArray());

            return bindings.Last();
        }

        static MESH CreateMesh(int boneCount)
        {
            var mesh = new MESH("skinned mesh");
            mesh.VertexPreprocessor.SetValidationPreprocessors();

            var prim = mesh.UsePrimitive(new SharpGLTF.Materials.MaterialBuilder("Default"));

            var a0 = default(VERTEX);
            var a1 = default(VERTEX);
            var a2 = default(VERTEX);
            var a3 = default(VERTEX);            

            for (int i = 0; i < boneCount; ++i)
            {
                var b0 = new VERTEX(new Vector3(-5, i * 10, -5), Vector4.One, (i, 1));
                var b1 = new VERTEX(new Vector3(+5, i * 10, -5), Vector4.One, (i, 1));
                var b2 = new VERTEX(new Vector3(+5, i * 10, +5), Vector4.One, (i, 1));
                var b3 = new VERTEX(new Vector3(-5, i * 10, +5), Vector4.One, (i, 1));

                if (i == 0)
                {
                    prim.AddQuadrangle(b0, b1, b2, b3);
                }
                else
                {
                    prim.AddQuadrangle(b0, b1, a1, a0);
                    prim.AddQuadrangle(b1, b2, a2, a1);
                    prim.AddQuadrangle(b2, b3, a3, a2);
                    prim.AddQuadrangle(b3, b0, a0, a3);
                }

                a0 = b0;
                a1 = b1;
                a2 = b2;
                a3 = b3;
            }

            prim.AddQuadrangle(a3, a2, a1, a0);

            return mesh;
        }
    }    
}
