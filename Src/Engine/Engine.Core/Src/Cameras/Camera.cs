using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.Interfaces;
using Engine.Configuration;
using System.Numerics;
using System;
using AngryWasp.Math;

namespace Engine.Cameras
{
    public class Camera
    {
        #region Constants

        private const float DEFAULT_ASPECT_RATIO = 1.0f;

        #endregion

        #region Fields

        private EngineCore engine;
		private Vector3[] cornersWs = new Vector3[8]; //Frustum cornders in world space;
		private Vector3[] cornersVs = new Vector3[8]; //Frustum corners in View space
		private Vector3[] currentCorners = new Vector3[4]; //4 farthest points on view frustum

        private ICameraController controller = new NullCameraController();
		private float nearClip;
		private float farClip;
		private Degree fovYDegrees;
		private float tanFovY;

		private Matrix4x4 transform = Matrix4x4.Identity;
		private Matrix4x4 view = Matrix4x4.Identity;
		private Matrix4x4 projection = Matrix4x4.Identity;
		private BoundingFrustum frustum = new BoundingFrustum(Matrix4x4.Identity);

        #endregion

        #region Properties

        public ICameraController Controller
		{
			get { return controller; }
			set
			{
				controller = value;
				controller.Transform = transform;
				controller.Engine = engine;
			}
		}

		public float NearClip => nearClip;

		public float FarClip => farClip;

		public Degree FovYDegrees => fovYDegrees;

		public float TanFovY => tanFovY;

		public Matrix4x4 Transform => transform;

		public Matrix4x4 View => view;

		public Matrix4x4 Projection => projection;

		public Viewport Viewport => engine.GraphicsDevice.Viewport;

		public BoundingFrustum Frustum => frustum;

		public Vector3 Position => transform.Translation;

		public float AspectRatio => Viewport.AspectRatio == 0 ? DEFAULT_ASPECT_RATIO : Viewport.AspectRatio;

        public Vector3[] FarCorners => currentCorners;

		#endregion

		public Camera(EngineCore engine)
		{
			this.engine = engine;
            Resize();
		}

        public void Resize()
        {
            nearClip = Settings.Engine == null ? 0.1f : Settings.Engine.Camera.NearClip;
            farClip = Settings.Engine == null ? 100f : Settings.Engine.Camera.FarClip;
            fovYDegrees = Settings.Engine == null ? 75f : Settings.Engine.Camera.Fov;
            tanFovY = MathF.Tan((fovYDegrees * 0.5f).ToRadians());
            projection = Matrix4x4.CreatePerspectiveFieldOfView(fovYDegrees.ToRadians(), AspectRatio, nearClip, farClip);
            Update();
        }

        public void Update()
        {
            transform = controller.Transform;
            Matrix4x4.Invert(transform, out view);
			frustum.Matrix = view * projection;
            UpdateFrustumCorners();
        }

        public void Update(Matrix4x4 transform)
        {
            Matrix4x4.Invert(transform, out view);
			frustum.Matrix = view * projection;
            UpdateFrustumCorners();
        }

        private void UpdateFrustumCorners()
        {
			frustum.GetCorners(cornersWs);
			Matrix4x4 matView = view; //this is the inverse of our camera transform
			cornersWs.Transform(ref matView, cornersVs);

			for (int i = 0; i < 4; i++)
				currentCorners[i] = cornersVs[i + 4];

			Vector3 temp = currentCorners[3];
			currentCorners[3] = currentCorners[2];
			currentCorners[2] = temp;
        }

		public Ray ToViewportRay(Vector2 pos)
        {
            Vector3 nearPoint = new Vector3(pos.X, pos.Y, 0);
            Vector3 farPoint = new Vector3(pos.X, pos.Y, 1);

            nearPoint = engine.GraphicsDevice.Viewport.Unproject(nearPoint,
                                                     projection,
                                                     view,
                                                     Matrix4x4.Identity);
            farPoint = engine.GraphicsDevice.Viewport.Unproject(farPoint,
                                                    projection,
                                                    view,
                                                    Matrix4x4.Identity);

            Vector3 direction = Vector3.Normalize(farPoint - nearPoint);

            return new Ray(nearPoint, direction, Vector3.Distance(nearPoint, farPoint));
        }
    }

    public class NullCameraController : ICameraController
    {
        public Matrix4x4 Transform { get; set; }
        public EngineCore Engine { get; set; }
        public void Update(GameTime gameTime) { }
    }
}
