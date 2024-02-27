using BepuPhysics;
using Engine.Content.Model.Template;
using Engine.Physics;
using Engine.Physics.Collidables;
using Engine.World;
using Engine.World.Components.Lights;
using System;
using System.Diagnostics;
using System.Numerics;

namespace Engine.Editor.Components
{
    public interface IGizmoSelection<U>
    {
        string Id { get; }

        Matrix4x4 Transform { get; }

        U Tag { get; set; }
    }

    public class GizmoTransformHelper
    {
        public static bool ScaleLightComponent(Component component, Vector3 scaleDelta)
        {
            if (component is SpotLightComponent spotLight)
            {
                spotLight.Type.SpotAngle += scaleDelta.X;
                spotLight.Type.SpotAngle += scaleDelta.Y;

                spotLight.Type.Radius += scaleDelta.Z;

                spotLight.SpotAngle = spotLight.Type.SpotAngle;
                spotLight.Radius = spotLight.Type.Radius;
                return true;
            }
            else if (component is PointLightComponent pointLight)
            {
                pointLight.Type.Radius += scaleDelta.X;
                pointLight.Type.Radius += scaleDelta.Y;
                pointLight.Type.Radius += scaleDelta.Z;

                pointLight.Radius = pointLight.Type.Radius;
                return true;
            }
            else if (component is DirectionalLightComponent directionalLight)
            {
                throw new NotImplementedException();
                return true;
            }

            return false;
        }
    }

    public class MeshViewGizmoSelection : IGizmoSelection<MeshTemplate>
    {
        private WorldTransform3 transform = new WorldTransform3();

        public string Id => string.Empty;

        public Matrix4x4 Transform => transform.Matrix;

        public MeshTemplate Tag { get; set; }

        public static void Translate(MeshViewGizmoSelection transformable, Vector3 translationDelta)
        {
            var it = transformable.transform;

            var rot = it.Rotation;
            var tra = it.Translation;

            tra += translationDelta;

            transformable.transform.Update(rot, tra);
            transformable.transform.Update(rot, tra);
        }

        public static void Rotate(MeshViewGizmoSelection transformable, Quaternion rotationDelta)
        {
            var it = transformable.transform;

            var rot = it.Rotation;
            var tra = it.Translation;

            rot *= rotationDelta;

            transformable.transform.Update(rot, tra);
            transformable.transform.Update(rot, tra);
        }

        public static void Scale(MeshViewGizmoSelection transformable, Vector3 scaleDelta)
        {
            var it = transformable.transform;

            var scl = it.Scale;
            var rot = it.Rotation;
            var tra = it.Translation;

            scl += scaleDelta;

            transformable.transform.Update(scl, rot, tra);
            transformable.transform.Update(scl, rot, tra);
        }
    }

    public class MapViewGizmoSelection : IGizmoSelection<MapObject>
    {
        public string Id => Tag.Name;

        public Matrix4x4 Transform => Tag.Transform.Matrix;

        public MapObject Tag { get; set; }

        public static void Translate(MapViewGizmoSelection transformable, Vector3 translationDelta)
        {
            var it = transformable.Tag.InitialTransform;

            var rot = it.Rotation;
            var tra = it.Translation;

            tra += translationDelta;

            transformable.Tag.InitialTransform.Update(rot, tra);
            transformable.Tag.Transform.Update(rot, tra);
        }

        public static void Rotate(MapViewGizmoSelection transformable, Quaternion rotationDelta)
        {
            var it = transformable.Tag.InitialTransform;

            var rot = it.Rotation;
            var tra = it.Translation;

            rot *= rotationDelta;

            transformable.Tag.InitialTransform.Update(rot, tra);
            transformable.Tag.Transform.Update(rot, tra);
        }

        public static void Scale(MapViewGizmoSelection transformable, Vector3 scaleDelta)
        {
            var scl = transformable.Tag.InitialTransform.Scale;
            var rot = transformable.Tag.InitialTransform.Rotation;
            var tra = transformable.Tag.InitialTransform.Translation;

            scl += scaleDelta;

            transformable.Tag.InitialTransform.Update(scl, rot, tra);
            transformable.Tag.Transform.Update(scl, rot, tra);
        }
    }

    public class PhysicsBodyGizmoSelection : IGizmoSelection<IConvexShape>
    {
        public string Id => Tag.BodyReference.CollidableReference.ToString();

        public Matrix4x4 Transform => Matrix4x4.CreateFromQuaternion(Tag.Pose.Rotation) * Matrix4x4.CreateTranslation(Tag.Pose.Position);

        public IConvexShape Tag { get; set; }

        //todo: need a scale handler to adjust the dimensions of each type of shape

        public static void Translate(PhysicsBodyGizmoSelection transformable, Vector3 translationDelta)
        {
            IConvexShape shape = transformable.Tag;
            var pos = shape.Pose.Position;
            var rot = shape.Pose.Rotation;
            pos += translationDelta;
            shape.Pose = new Pose(pos, rot);
            
            if (shape.IsDynamic)
            {
                shape.BodyReference.GetDescription(out BodyDescription description);
                description.Pose = shape.Pose.ToRigidPose();
                shape.BodyReference.ApplyDescription(description);
            }
            else
            {
                shape.StaticReference.GetDescription(out StaticDescription description);
                description.Pose = shape.Pose.ToRigidPose();
                shape.StaticReference.ApplyDescription(description);
            }
        }

        public static void Rotate(PhysicsBodyGizmoSelection transformable, Quaternion rotationDelta)
        {
            IConvexShape shape = transformable.Tag;
            var pos = shape.Pose.Position;
            var rot = shape.Pose.Rotation;
            rot *= rotationDelta;
            shape.Pose = new Pose(pos, rot);

            if (shape.IsDynamic)
            {
                shape.BodyReference.GetDescription(out BodyDescription description);
                description.Pose = shape.Pose.ToRigidPose();
                shape.BodyReference.ApplyDescription(description);
            }
            else
            {
                shape.StaticReference.GetDescription(out StaticDescription description);
                description.Pose = shape.Pose.ToRigidPose();
                shape.StaticReference.ApplyDescription(description);
            }
        }

        public static void Scale(PhysicsBodyGizmoSelection transformable, Vector3 scaleDelta)
        {
            if (transformable.Tag is Box box)
            {
                var sz = new Vector3(box.Width, box.Height, box.Length);
                sz += scaleDelta;
                box.Width = sz.X;
                box.Height = sz.Y;
                box.Length = sz.Z;
            }
            else
                Debugger.Break();
        }
    }

    public class ComponentGizmoSelection : IGizmoSelection<Component>
    {
        public string Id => Tag.Type.Name.ToString();

        public Matrix4x4 Transform => Tag.Type.LocalTransform.Matrix;

        public Component Tag { get; set; }

        public static void Translate(ComponentGizmoSelection transformable, Vector3 translationDelta)
        {
            var rot = transformable.Tag.Type.LocalTransform.Rotation;
            var tra = transformable.Tag.Type.LocalTransform.Translation;

            tra += translationDelta;

            transformable.Tag.Type.LocalTransform.Update(rot, tra);
            transformable.Tag.LocalTransform.Update(rot, tra);
        }

        public static void Rotate(ComponentGizmoSelection transformable, Quaternion rotationDelta)
        {
            var rot = transformable.Tag.Type.LocalTransform.Rotation;
            var tra = transformable.Tag.Type.LocalTransform.Translation;

            rot *= rotationDelta;

            transformable.Tag.Type.LocalTransform.Update(rot, tra);
            transformable.Tag.LocalTransform.Update(rot, tra);
        }

        public static void Scale(ComponentGizmoSelection transformable, Vector3 scaleDelta)
        {
            if (!GizmoTransformHelper.ScaleLightComponent(transformable.Tag, scaleDelta))
            {
                var scl = transformable.Tag.Type.LocalTransform.Scale;
                var rot = transformable.Tag.Type.LocalTransform.Rotation;
                var tra = transformable.Tag.Type.LocalTransform.Translation;

                scl += scaleDelta;

                transformable.Tag.Type.LocalTransform.Update(scl, rot, tra);
                transformable.Tag.LocalTransform.Update(scl, rot, tra);
            }
        }
    }
}