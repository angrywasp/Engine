using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Engine.Content;
using Engine.Editor.Components;
using Engine.Editor.Components.Gizmo;
using Engine.Editor.Forms;
using Engine.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Engine.Editor.Views
{
    public class PhysicsView : View3D
    {
        private PhysicsModel model;
        private PhysicsForm form;

        private GizmoComponent<PhysicsBodyGizmoSelection, IConvexShape> gizmo;

        public override void InitializeView(string path)
		{
            gizmo = new GizmoComponent<PhysicsBodyGizmoSelection, IConvexShape>(engine);
            gizmo.Translate += PhysicsBodyGizmoSelection.Translate;
            gizmo.Rotate += PhysicsBodyGizmoSelection.Rotate;

            engine.Scene.Physics.SimulationUpdate = false;
			base.InitializeView(path);
            CreateDefaultScene();

            model = ContentLoader.LoadJson<PhysicsModel>(path);

            model.Load(engine);

            form = new PhysicsForm(engine, model);
            ShowForm(form);
		}

        public override bool ShouldUpdateCameraController()
        {
            if (!base.ShouldUpdateCameraController())
                return false;

            if (gizmo.ActiveAxis != GizmoAxis.None)
                return false;

            return true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (engine.Interface.Terminal.Visible)
                return;

            if (engine.Input.Keyboard.KeyJustPressed(Keys.P))
                engine.Scene.Physics.SimulationUpdate = !engine.Scene.Physics.SimulationUpdate;

            if (model != null)
                model.Update(gameTime);

            gizmo.Update(engine.Camera, gameTime);

            if (engine.Input.Mouse.LeftJustPressed)
            {
                Ray r = engine.Input.Mouse.ToRay(engine.Camera.View, engine.Camera.Projection);
                var rayHits = engine.Scene.Physics.RayCast(r);

                if (rayHits.Count == 0)
                    return;

                var body = model.GetBodyForReference(rayHits[0].Collidable);

                if (body == null)
                    return;

                if (!engine.Input.Keyboard.KeyDown(Keys.LeftShift))
                    gizmo.Clear();

                gizmo.Select(new PhysicsBodyGizmoSelection
                {
                    Tag = body
                });

                form.AddSelection(body);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (!engine.Interface.Terminal.Visible)
            {
                gizmo.Draw();
                gizmo.Draw2D();
            }
        }
    }
}
