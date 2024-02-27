using Engine.Cameras;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using MonoGame.OpenGL;
using Engine.UI;
using System.Numerics;
using Engine.Content;
using System.Linq;
using AngryWasp.Helpers;
using Engine.Graphics.Effects;
using Engine.Graphics.Vertices;
using Engine.Configuration;

namespace Engine.Editor.Components.Gizmo
{
    public class GizmoComponent<T, U> where T : IGizmoSelection<U>
    {
        public event EngineEventHandler<T, Vector3> Scale;
        public event EngineEventHandler<T, Quaternion> Rotate;
        public event EngineEventHandler<T,  Vector3> Translate;

        private bool isActive = true;
        private ColorEffect lineEffect;
        private ColorEffect gizmoEffect;
        private ColorEffect quadEffect;
        private EngineCore engine;
        private Color highlightColor = Color.Gold;
        private GizmoAxis activeAxis = GizmoAxis.None;
        private Matrix4x4 view = Matrix4x4.Identity;
        private Matrix4x4 projection = Matrix4x4.Identity;
        private Matrix4x4 scaledWorld = Matrix4x4.Identity;

        private Font font;
        private Vector3 _lastIntersectionPosition;
        private Vector3 _translationDelta = Vector3.Zero;
        private Quaternion _rotationDelta = Quaternion.Identity;
        private Vector3 _scaleDelta = Vector3.Zero;
        private Matrix4x4 _screenScaleMatrix;

        private float _screenScale;
        private Vector3 _translationScaleSnapDelta;
        private float _rotationSnapDelta;
        private Vector3 cameraPosition;
        private Vector3 position = Vector3.Zero;
        private Matrix4x4 _rotationMatrix = Matrix4x4.Identity;

        private Vertices verts;

        public GizmoAxis ActiveAxis => activeAxis;

        public GizmoMode ActiveMode { get; set; } = GizmoMode.None;

        public TransformSpace ActiveSpace { get; set; } = TransformSpace.Local;

        public PivotType ActivePivot { get; set; } = PivotType.SelectionCenter;

        private List<T> selectedItems = new List<T>();

        public List<T> SelectedItems => selectedItems;

        public BoundingBox XAxisBox => new BoundingBox(new Vector3(Constants.LINE_OFFSET, 0, 0), new Vector3(Constants.LINE_OFFSET + Constants.LINE_LENGTH, Constants.SINGLE_AXIS_THICKNESS, Constants.SINGLE_AXIS_THICKNESS));
        public BoundingBox YAxisBox => new BoundingBox(new Vector3(0, Constants.LINE_OFFSET, 0), new Vector3(Constants.SINGLE_AXIS_THICKNESS, Constants.LINE_OFFSET + Constants.LINE_LENGTH, Constants.SINGLE_AXIS_THICKNESS));
        public BoundingBox ZAxisBox => new BoundingBox(new Vector3(0, 0, Constants.LINE_OFFSET), new Vector3(Constants.SINGLE_AXIS_THICKNESS, Constants.SINGLE_AXIS_THICKNESS, Constants.LINE_OFFSET + Constants.LINE_LENGTH));
        public BoundingBox XZAxisBox => new BoundingBox(Vector3.Zero, new Vector3(Constants.LINE_OFFSET, Constants.MULTI_AXIS_THICKNESS, Constants.LINE_OFFSET));
        public BoundingBox XYBox => new BoundingBox(Vector3.Zero, new Vector3(Constants.LINE_OFFSET, Constants.LINE_OFFSET, Constants.MULTI_AXIS_THICKNESS));
        public BoundingBox YZBox => new BoundingBox(Vector3.Zero, new Vector3(Constants.MULTI_AXIS_THICKNESS, Constants.LINE_OFFSET, Constants.LINE_OFFSET));

        public BoundingSphere XSphere => new BoundingSphere(Vector3.Transform(verts.TranslationLineVertices[1].Position, scaledWorld), Constants.SPHERE_RADIUS * _screenScale);
        public BoundingSphere YSphere => new BoundingSphere(Vector3.Transform(verts.TranslationLineVertices[7].Position, scaledWorld), Constants.SPHERE_RADIUS * _screenScale);
        public BoundingSphere ZSphere => new BoundingSphere(Vector3.Transform(verts.TranslationLineVertices[13].Position, scaledWorld), Constants.SPHERE_RADIUS * _screenScale);

        public GizmoComponent(EngineCore engine)
        {
            this.engine = engine;
            lineEffect = new ColorEffect();
            gizmoEffect = new ColorEffect();
            quadEffect = new ColorEffect();
            verts = new Vertices();

#pragma warning disable CS4014
            lineEffect.LoadAsync(engine.GraphicsDevice);
            gizmoEffect.LoadAsync(engine.GraphicsDevice);
            quadEffect.LoadAsync(engine.GraphicsDevice);
#pragma warning restore CS4014

            font = ContentLoader.LoadFontPackage(engine.GraphicsDevice, "Engine/Fonts/Default.fontpkg").GetByFontSize(12);
        }

        public void Update(Camera camera, GameTime gameTime)
        {
            if (ActiveMode == GizmoMode.None)
                return;

            view = camera.View;
            projection = camera.Projection;
            cameraPosition = camera.Position;

            if (isActive)
            {
                if (engine.Input.Mouse.ButtonDown(engine.Input.Mouse.LeftButton) && activeAxis != GizmoAxis.None)
                {
                    switch (ActiveMode)
                    {
                        case GizmoMode.UniformScale:
                        case GizmoMode.NonUniformScale:
                        case GizmoMode.Translate:
                            UpdateTranslateScale(gameTime);
                            break;
                        case GizmoMode.Rotate:
                            UpdateRotate(gameTime);
                            break;
                    }
                }
                else
                {
                    _lastIntersectionPosition = Vector3.Zero;
                    if (engine.Input.Mouse.ButtonUp(engine.Input.Mouse.LeftButton) && engine.Input.Mouse.ButtonUp(engine.Input.Mouse.RightButton))
                        SelectAxis(engine.Input.Mouse.Position);
                }

                SetGizmoPosition();

                if (engine.Input.Mouse.ButtonDown(engine.Input.Mouse.LeftButton))
                {
                    if (_translationDelta != Vector3.Zero)
                    {
                        foreach (var entity in selectedItems)
                            Translate?.Invoke(entity, _translationDelta);

                        position += _translationDelta;
                        _translationDelta = Vector3.Zero;
                    }

                    if (_rotationDelta != Quaternion.Identity)
                    {
                        foreach (var entity in selectedItems)
                            Rotate?.Invoke(entity, _rotationDelta);

                        _rotationDelta = Quaternion.Identity;
                    }

                    if (_scaleDelta != Vector3.Zero)
                    {
                        foreach (var entity in selectedItems)
                            Scale?.Invoke(entity, _scaleDelta);

                        _scaleDelta = Vector3.Zero;
                    }
                }
            }

            if (selectedItems.Count < 1)
            {
                isActive = false;
                activeAxis = GizmoAxis.None;
                return;
            }
            if (!isActive)
                SetGizmoPosition();

            isActive = true;

            Vector3 vLength = cameraPosition - position;
            const float scaleFactor = 25;

            _screenScale = vLength.Length() / scaleFactor;
            _screenScaleMatrix = Matrix4x4.CreateScale(_screenScale);

            Quaternion r = Quaternion.CreateFromRotationMatrix(selectedItems[0].Transform);

            if (selectedItems.Count > 1)
                for (int i = 1; i < selectedItems.Count; i++)
                    r = Quaternion.Slerp(r, Quaternion.CreateFromRotationMatrix(selectedItems[i].Transform), 0.5f);

            Matrix4x4 loc = Matrix4x4.Identity * Matrix4x4.CreateFromQuaternion(r) * Matrix4x4.CreateTranslation(position);

            Vector3 localForward = loc.Forward();
            Vector3 localUp = loc.Up();
            Vector3 localRight = loc.Right();

            if (ActiveSpace == TransformSpace.World)
            {
                scaledWorld = _screenScaleMatrix * Matrix4x4.CreateWorld(position, -Vector3.UnitZ, Vector3.UnitY);
                _rotationMatrix = _rotationMatrix.Forward(-Vector3.UnitZ).Up(Vector3.UnitY).Right(Vector3.UnitX);
            }
            else
            {
                scaledWorld = _screenScaleMatrix * Matrix4x4.CreateWorld(position, localForward, localUp);
                _rotationMatrix = _rotationMatrix.Forward(localForward).Up(localUp).Right(localRight);
            }

            ApplyColor(GizmoAxis.X, Constants.AxisColors[0]);
            ApplyColor(GizmoAxis.Y, Constants.AxisColors[1]);
            ApplyColor(GizmoAxis.Z, Constants.AxisColors[2]);

            ApplyColor(activeAxis, highlightColor);
        }

        public void Draw()
        {
            if (ActiveMode == GizmoMode.None)
                return;

            if (!isActive)
                return;

            if (view == Matrix4x4.Identity || projection == Matrix4x4.Identity)
                return;

            lineEffect.World = scaledWorld;
            lineEffect.View = view;
            lineEffect.Projection = projection;

            lineEffect.Apply();
            engine.GraphicsDevice.DrawUserPrimitives(GLPrimitiveType.Lines, verts.TranslationLineVertices, 0, verts.TranslationLineVertices.Length, VertexPositionColor.VertexDeclaration);

            switch (ActiveMode)
            {
                case GizmoMode.NonUniformScale:
                case GizmoMode.Translate:
                    switch (ActiveAxis)
                    {
                        case GizmoAxis.ZX:
                        case GizmoAxis.YZ:
                        case GizmoAxis.XY:
                            {
                                engine.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
                                engine.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

                                quadEffect.World = scaledWorld;
                                quadEffect.View = view;
                                quadEffect.Projection = projection;

                                quadEffect.Apply();

                                Quad activeQuad = new Quad();
                                switch (ActiveAxis)
                                {
                                    case GizmoAxis.XY:
                                        activeQuad = Constants.Quads[0];
                                        break;

                                    case GizmoAxis.ZX:
                                        activeQuad = Constants.Quads[1];
                                        break;

                                    case GizmoAxis.YZ:
                                        activeQuad = Constants.Quads[2];
                                        break;
                                }

                                engine.GraphicsDevice.DrawUserIndexedPrimitives(GLPrimitiveType.Triangles, activeQuad.Vertices, 0, 4, activeQuad.Indexes, 0, 6, VertexPositionNormalTexture.VertexDeclaration);

                                engine.GraphicsDevice.BlendState = BlendState.Opaque;
                                engine.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                            }
                            break;
                    }
                    break;
                case GizmoMode.UniformScale:
                    if (ActiveAxis != GizmoAxis.None)
                    {
                        engine.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                        engine.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

                        quadEffect.World = scaledWorld;
                        quadEffect.View = view;
                        quadEffect.Projection = projection;
                        quadEffect.Apply();

                        for (int i = 0; i < Constants.Quads.Length; i++)
                            engine.GraphicsDevice.DrawUserIndexedPrimitives(GLPrimitiveType.Triangles, Constants.Quads[i].Vertices, 0, 4,
                                    Constants.Quads[i].Indexes, 0, 6, VertexPositionNormalTexture.VertexDeclaration);

                        engine.GraphicsDevice.BlendState = BlendState.Opaque;
                        engine.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                    }
                    break;
            }

            GizmoModel activeModel;

            switch (ActiveMode)
            {
                case GizmoMode.Translate:
                    activeModel = GizmoModel.Translate;
                    break;

                case GizmoMode.Rotate:
                    activeModel = GizmoModel.Rotate;
                    break;

                default:
                    activeModel = GizmoModel.Scale;
                    break;
            }

            for (int i = 0; i < 3; i++)
            {
                Vector4 color;
                switch (ActiveMode)
                {
                    case GizmoMode.UniformScale:
                        color = Constants.AxisColors[0].ToVector4();
                        break;

                    default:
                        color = Constants.AxisColors[i].ToVector4();
                        break;
                }

                gizmoEffect.World = Constants.ModelLocalSpace[i] * scaledWorld;
                gizmoEffect.ColorMultiplier = color;
                gizmoEffect.View = view;
                gizmoEffect.Projection = projection;

                gizmoEffect.Apply();
                engine.GraphicsDevice.DrawUserIndexedPrimitives(GLPrimitiveType.Triangles, activeModel.Vertices, 0, activeModel.Vertices.Length,
                    activeModel.Indices, 0, activeModel.Indices.Length, VertexPositionColor.VertexDeclaration);
            }
        }

        public void Draw2D()
        {
            if (ActiveMode == GizmoMode.None)
                return;

            if (!isActive)
                return;

            for (int i = 0; i < 3; i++)
            {
                Vector3 screenPos = engine.GraphicsDevice.Viewport.Project(
                    Constants.ModelLocalSpace[i].Translation + Constants.ModelLocalSpace[i].Backward() + Constants.AxisTextOffset,
                    projection, view, scaledWorld);

                if (screenPos.Z < 0f || screenPos.Z > 1.0f)
                    continue;

                Color color = Constants.AxisColors[i];
                switch (i)
                {
                    case 0:
                        if (ActiveAxis == GizmoAxis.X || ActiveAxis == GizmoAxis.XY || ActiveAxis == GizmoAxis.ZX)
                            color = highlightColor;
                        break;

                    case 1:
                        if (ActiveAxis == GizmoAxis.Y || ActiveAxis == GizmoAxis.XY || ActiveAxis == GizmoAxis.YZ)
                            color = highlightColor;
                        break;

                    case 2:
                        if (ActiveAxis == GizmoAxis.Z || ActiveAxis == GizmoAxis.YZ || ActiveAxis == GizmoAxis.ZX)
                            color = highlightColor;
                        break;
                }

                engine.Interface.DrawString(Constants.AxisText[i], new Vector2i((int)screenPos.X, (int)screenPos.Y), font, color);
            }
        }

        private void UpdateTranslateScale(GameTime gameTime)
        {

            Vector3 delta = Vector3.Zero;
            Ray ray = engine.Input.Mouse.ToRay(view, projection);

            Matrix4x4 transform;
            Matrix4x4.Invert(_rotationMatrix, out transform);
            ray.Position = Vector3.Transform(ray.Position, transform);
            ray.Direction = Vector3.TransformNormal(ray.Direction, transform);

            switch (activeAxis)
            {
                case GizmoAxis.XY:
                case GizmoAxis.X:
                    {
                        Matrix4x4 inverseRotation;
                        Matrix4x4.Invert(_rotationMatrix, out inverseRotation);
                        Plane plane = new Plane(Vector3Orientation.Forward, Vector3.Transform(position, inverseRotation).Z);
                        float intersection;
                        if (ray.Intersects(ref plane, out intersection))
                        {
                            Vector3 _intersectPosition = (ray.Position + (ray.Direction * intersection));
                            if (MathHelper.IsValid(_intersectPosition) && MathHelper.IsValid(_lastIntersectionPosition))
                            {
                                if (_lastIntersectionPosition != Vector3.Zero)
                                {
                                    Vector3 _tDelta = _intersectPosition - _lastIntersectionPosition;
                                    delta = activeAxis == GizmoAxis.X
                                              ? new Vector3(_tDelta.X, 0, 0)
                                              : new Vector3(_tDelta.X, _tDelta.Y, 0);
                                }
                                _lastIntersectionPosition = _intersectPosition;
                            }
                        }
                    }
                    break;

                case GizmoAxis.Z:
                case GizmoAxis.YZ:
                case GizmoAxis.Y:
                    {
                        Matrix4x4 inverseRotation;
                        Matrix4x4.Invert(_rotationMatrix, out inverseRotation);
                        Plane plane = new Plane(Vector3Orientation.Left, Vector3.Transform(position, inverseRotation).X);
                        float intersection;
                        if (ray.Intersects(ref plane, out intersection))
                        {
                            Vector3 _intersectPosition = (ray.Position + (ray.Direction * intersection));
                            if (MathHelper.IsValid(_intersectPosition) && MathHelper.IsValid(_lastIntersectionPosition))
                            {
                                if (_lastIntersectionPosition != Vector3.Zero)
                                {
                                    Vector3 _tDelta = _intersectPosition - _lastIntersectionPosition;

                                    switch (activeAxis)
                                    {
                                        case GizmoAxis.Y:
                                            delta = new Vector3(0, _tDelta.Y, 0);
                                            break;

                                        case GizmoAxis.Z:
                                            delta = new Vector3(0, 0, _tDelta.Z);
                                            break;

                                        default:
                                            delta = new Vector3(0, _tDelta.Y, _tDelta.Z);
                                            break;
                                    }
                                }
                                _lastIntersectionPosition = _intersectPosition;
                            }
                        }
                    }
                    break;

                case GizmoAxis.ZX:
                    {
                        Matrix4x4 inverseRotation;
                        Matrix4x4.Invert(_rotationMatrix, out inverseRotation);
                        Plane plane = new Plane(Vector3Orientation.Down, Vector3.Transform(position, inverseRotation).Y);
                        float intersection;
                        if (ray.Intersects(ref plane, out intersection))
                        {
                            Vector3 _intersectPosition = (ray.Position + (ray.Direction * intersection));
                            if (MathHelper.IsValid(_intersectPosition) && MathHelper.IsValid(_lastIntersectionPosition))
                            {
                                if (_lastIntersectionPosition != Vector3.Zero)
                                {
                                    Vector3 _tDelta = _intersectPosition - _lastIntersectionPosition;
                                    delta = new Vector3(_tDelta.X, 0, _tDelta.Z);
                                }
                                _lastIntersectionPosition = _intersectPosition;
                            }
                        }
                    }
                    break;
            }

            if (ActiveMode == GizmoMode.Translate)
            {
                if (Settings.Engine.EditorGizmo.TranslationSnapEnabled)
                    Snap(ref delta, Settings.Engine.EditorGizmo.TranslationSnapValue);

                Vector3 newDelta = Vector3.Transform(delta, _rotationMatrix);
                delta = newDelta;

                _translationDelta = delta;
            }
            else if (ActiveMode == GizmoMode.NonUniformScale || ActiveMode == GizmoMode.UniformScale)
            {
                if (Settings.Engine.EditorGizmo.ScaleSnapEnabled)
                    Snap(ref delta, Settings.Engine.EditorGizmo.ScaleSnapValue);

                _scaleDelta += delta;
            }
            else
                delta *= Constants.PRECISION_MODE_SCALE;
        }

        private void UpdateRotate(GameTime gameTime)
        {
            float delta = engine.Input.Mouse.MovementDelta.X;
            delta *= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (Settings.Engine.EditorGizmo.RotationSnapEnabled)
            {
                float snapValue = Settings.Engine.EditorGizmo.RotationSnapValue.ToRadians();
                if (Settings.Engine.EditorGizmo.PrecisionModeEnabled)
                {
                    delta *= Constants.PRECISION_MODE_SCALE;
                    snapValue *= Constants.PRECISION_MODE_SCALE;
                }

                _rotationSnapDelta += delta;

                float snapped = (int)(_rotationSnapDelta / snapValue) * snapValue;
                _rotationSnapDelta -= snapped;

                delta = snapped;
            }
            else if (Settings.Engine.EditorGizmo.PrecisionModeEnabled)
                delta *= Constants.PRECISION_MODE_SCALE;

            Quaternion quat = Quaternion.Identity;

            switch (activeAxis)
            {
                case GizmoAxis.X:
                    quat *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, delta);
                    break;

                case GizmoAxis.Y:
                    quat *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, delta);
                    break;

                case GizmoAxis.Z:
                    quat *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, delta);
                    break;
            }

            _rotationDelta = quat;
        }

        private void Snap(ref Vector3 delta, float snapValue)
        {
            if (Settings.Engine.EditorGizmo.PrecisionModeEnabled)
            {
                delta *= Constants.PRECISION_MODE_SCALE;
                snapValue *= Constants.PRECISION_MODE_SCALE;
            }

            _translationScaleSnapDelta += delta;

            delta = new Vector3(
              (int)(_translationScaleSnapDelta.X / snapValue) * snapValue,
              (int)(_translationScaleSnapDelta.Y / snapValue) * snapValue,
              (int)(_translationScaleSnapDelta.Z / snapValue) * snapValue);

            _translationScaleSnapDelta -= delta;
        }

        public void SetGizmoPosition()
        {
            switch (ActivePivot)
            {
                case PivotType.ObjectCenter:
                    if (selectedItems.Count > 0)
                        position = selectedItems[0].Transform.Translation;
                    break;

                case PivotType.SelectionCenter:
                    position = GetSelectionCenter();
                    break;

                case PivotType.WorldOrigin:
                    position = Vector3.Zero;
                    break;
            }
            position += _translationDelta;
        }

        private Vector3 GetSelectionCenter()
        {
            int count = selectedItems.Count;

            if (count == 0)
                return Vector3.Zero;

            Vector3 center = Vector3.Zero;
            foreach (var sel in selectedItems)
                center += sel.Transform.Translation;
            return center / count;
        }

        private void SelectAxis(Vector2i mousePosition)
        {
            float closestintersection = float.MaxValue;
            Ray ray = engine.Input.Mouse.ToRay(view, projection);

            float intersection;
            BoundingBox xb = BoundingBox.Transform(XAxisBox, scaledWorld);
            BoundingBox yb = BoundingBox.Transform(YAxisBox, scaledWorld);
            BoundingBox zb = BoundingBox.Transform(ZAxisBox, scaledWorld);

            if (ray.Intersects(ref xb, out intersection))
                if (intersection < closestintersection)
                {
                    activeAxis = GizmoAxis.X;
                    closestintersection = intersection;
                }

            if (ray.Intersects(ref yb, out intersection))
                if (intersection < closestintersection)
                {
                    activeAxis = GizmoAxis.Y;
                    closestintersection = intersection;
                }

            if (ray.Intersects(ref zb, out intersection))
                if (intersection < closestintersection)
                {
                    activeAxis = GizmoAxis.Z;
                    closestintersection = intersection;
                }

            if (ActiveMode == GizmoMode.Rotate || ActiveMode == GizmoMode.UniformScale || ActiveMode == GizmoMode.NonUniformScale)
            {
                BoundingSphere xs = XSphere;
                BoundingSphere ys = YSphere;
                BoundingSphere zs = ZSphere;

                if (ray.Intersects(ref xs, out intersection))
                    if (intersection < closestintersection)
                    {
                        activeAxis = GizmoAxis.X;
                        closestintersection = intersection;
                    }

                if (ray.Intersects(ref ys, out intersection))
                    if (intersection < closestintersection)
                    {
                        activeAxis = GizmoAxis.Y;
                        closestintersection = intersection;
                    }

                if (ray.Intersects(ref zs, out intersection))
                    if (intersection < closestintersection)
                    {
                        activeAxis = GizmoAxis.Z;
                        closestintersection = intersection;
                    }
            }
            if (ActiveMode == GizmoMode.Translate || ActiveMode == GizmoMode.NonUniformScale || ActiveMode == GizmoMode.UniformScale)
            {
                if (closestintersection >= float.MaxValue)
                    closestintersection = float.MinValue;

                BoundingBox xyb = BoundingBox.Transform(XYBox, scaledWorld);
                BoundingBox xzb = BoundingBox.Transform(XZAxisBox, scaledWorld);
                BoundingBox yzb = BoundingBox.Transform(YZBox, scaledWorld);

                if (ray.Intersects(ref xyb, out intersection))
                    if (intersection > closestintersection)
                    {
                        activeAxis = GizmoAxis.XY;
                        closestintersection = intersection;
                    }

                if (ray.Intersects(ref xzb, out intersection))
                    if (intersection > closestintersection)
                    {
                        activeAxis = GizmoAxis.ZX;
                        closestintersection = intersection;
                    }

                if (ray.Intersects(ref yzb, out intersection))
                    if (intersection > closestintersection)
                    {
                        activeAxis = GizmoAxis.YZ;
                        closestintersection = intersection;
                    }
            }
            if (closestintersection >= float.MaxValue || closestintersection <= float.MinValue)
                activeAxis = GizmoAxis.None;
        }

        private void ApplyColor(GizmoAxis axis, Color color)
        {
            switch (ActiveMode)
            {
                case GizmoMode.NonUniformScale:
                case GizmoMode.Translate:
                    switch (axis)
                    {
                        case GizmoAxis.X:
                            ApplyLineColor(0, 6, color);
                            break;
                        case GizmoAxis.Y:
                            ApplyLineColor(6, 6, color);
                            break;
                        case GizmoAxis.Z:
                            ApplyLineColor(12, 6, color);
                            break;
                        case GizmoAxis.XY:
                            ApplyLineColor(0, 4, color);
                            ApplyLineColor(6, 4, color);
                            break;
                        case GizmoAxis.YZ:
                            ApplyLineColor(6, 2, color);
                            ApplyLineColor(12, 2, color);
                            ApplyLineColor(10, 2, color);
                            ApplyLineColor(16, 2, color);
                            break;
                        case GizmoAxis.ZX:
                            ApplyLineColor(0, 2, color);
                            ApplyLineColor(4, 2, color);
                            ApplyLineColor(12, 4, color);
                            break;
                    }
                    break;
                case GizmoMode.Rotate:
                    switch (axis)
                    {
                        case GizmoAxis.X:
                            ApplyLineColor(0, 6, color);
                            break;
                        case GizmoAxis.Y:
                            ApplyLineColor(6, 6, color);
                            break;
                        case GizmoAxis.Z:
                            ApplyLineColor(12, 6, color);
                            break;
                    }
                    break;

                case GizmoMode.UniformScale:
                    ApplyLineColor(0, verts.TranslationLineVertices.Length, activeAxis == GizmoAxis.None ? Constants.AxisColors[0] : highlightColor);
                    break;
            }
        }

        private void ApplyLineColor(int startindex, int count, Color color)
        {
            for (int i = startindex; i < (startindex + count); i++)
                verts.TranslationLineVertices[i].Color = color;
        }

        public void Select(T gs)
        {
            if (selectedItems.Where(x => x.Id == gs.Id).Count() > 0)
                return;

            selectedItems.Add(gs);
        }

        public void Deselect(T gs)
        {
            int index = -1;
            for (int i = 0; i < selectedItems.Count; i++)
                if (gs.Id == selectedItems[i].Id)
                {
                    index = i;
                    break;
                }

            if (index == -1)
                return;

            selectedItems.RemoveAt(index);
        }

        public void Clear() => selectedItems.Clear();
    }
}