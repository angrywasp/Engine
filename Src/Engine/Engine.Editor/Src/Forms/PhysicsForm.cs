using System.Collections.Generic;
using System.Numerics;
using Engine.Physics;
using Engine.Physics.Collidables;
using Engine.Physics.Constraints;
using Engine.Serializers;
using Engine.UI;
using Engine.UI.Controls;
using Microsoft.Xna.Framework;

namespace Engine.Editor.Forms
{
    public class PhysicsForm : EditorForm
    {
        private List<IConvexShape> selections = new List<IConvexShape>(); 

        private PhysicsModel model;

        private ListBox lbxShapeList;
        private ListBox lbxConstraintList;

        private PosePropertiesControl posePropertiesControl;

        public PhysicsForm(EngineCore engine, PhysicsModel model) : base(engine)
        {
            this.model = model;
            ConstructLayout();
        }

        private void ConstructLayout()
        {
            int w = ui.Size.X;
            Vector2i btnSize = new Vector2i(150, 25);

            var btnShapes = this.RadioButton(new Vector2(0.8f, 0), new Vector2(0.1f, 0.05f), text: "Shapes");
            var btnJoints = this.RadioButton(new Vector2(0.9f, 0), new Vector2(0.1f, 0.05f), text: "Constraints");
            var btnAdd = this.Button(new Vector2(0.8f, 0.95f), new Vector2(0.2f, 0.05f), text: "Add");

            btnShapes.Checked = true;

            lbxShapeList = this.ListBox(new Vector2(0.8f, 0.05f), new Vector2(0.3f, 0.9f));
            lbxConstraintList = this.ListBox(new Vector2(0.8f, 0.05f), new Vector2(0.3f, 0.9f));
            lbxConstraintList.Visible = false;

            var shapeDataSource = new SortedDictionary<string, IConvexShape>();
            shapeDataSource.Add("Box", new Box(true));
            shapeDataSource.Add("Capsule", new Capsule(true));
            shapeDataSource.Add("Cylinder", new Cylinder(true));
            shapeDataSource.Add("Sphere", new Sphere(true));

            lbxShapeList.DataBind(shapeDataSource);

            var constraintDataSource = new SortedDictionary<string, IConstraint>();
            constraintDataSource.Add("1 Angular Motor", new OneBodyAngularMotor());
            constraintDataSource.Add("1 Angular Servo", new OneBodyAngularServo());
            constraintDataSource.Add("1 Linear Motor", new OneBodyLinearMotor());
            constraintDataSource.Add("1 Linear Servo", new OneBodyLinearServo());
            constraintDataSource.Add("Angular Axis Motor", new AngularAxisMotor());
            constraintDataSource.Add("Angular Hinge", new AngularHinge());
            constraintDataSource.Add("Angular Motor", new AngularMotor());
            constraintDataSource.Add("Angular Servo", new AngularServo());
            constraintDataSource.Add("Angular Swivel Hinge", new AngularSwivelHinge());
            constraintDataSource.Add("Ball Socket Limit", new BallSocketLimit());
            constraintDataSource.Add("Ball Socket Motor", new BallSocketMotor());
            constraintDataSource.Add("Ball Socket Servo", new BallSocketServo());
            constraintDataSource.Add("Linear Axis Motor", new LinearAxisMotor());
            constraintDataSource.Add("Twist Limit", new TwistLimit());
            constraintDataSource.Add("Twist Motor", new TwistMotor());
            constraintDataSource.Add("Twist Servo", new TwistServo());

            lbxConstraintList.DataBind(constraintDataSource);

            btnShapes.MouseClick += (s, e) =>
            {
                lbxConstraintList.Visible = false;
                lbxShapeList.Visible = true;
            };

            btnJoints.MouseClick += (s, e) =>
            {
                lbxShapeList.Visible = false;
                lbxConstraintList.Visible = true;
            };

            btnAdd.MouseClick += (s, e) => {
                
                /*if (lbxShapeList.Visible)
                {
                    if (lbxShapeList.SelectedItem == null)
                        return;

                    var shape = (IConvexShape)Activator.CreateInstance(lbxShapeList.SelectedItem.Tag.GetType());
                    model.AddShape(shape);
                    shape.AddDynamicToScene(engine);
                }*/
                
            };

            DataBind();

            posePropertiesControl = this.Add<PosePropertiesControl>(Vector2i.Zero, new Vector2i(512, 128));
            posePropertiesControl.ConstructLayout();
            posePropertiesControl.Load();
        }

        public void AddSelection(IConvexShape shape)
        {
            posePropertiesControl?.Assign(shape.Pose);
        }

        private void DataBind()
        {
            var ds = new SortedDictionary<string, IConvexShape>();
            
            for (int i = 0; i < model.Shapes.Count; i++)
                ds.Add($"{i}: {model.Shapes[i].GetType().Name}", model.Shapes[i]);

            //lbx.DataBind(ds);
        }

        public override bool ShouldUpdateCameraController()
        {
            if (engine.Input.Mouse.InsideRect(lbxShapeList.Bounds))
                return false;

            if (engine.Input.Mouse.InsideRect(lbxConstraintList.Bounds))
                return false;

            return true;
        }

        public override void ViewUpdate(GameTime gameTime) { }

        public override void ViewDraw() { }
    }

    public class PosePropertiesControl : UiControl
    {
        private Vector3 position;
        private Quaternion rotation;

        private (TextBox PropertyName, TextInput PropertyValue) PositionField;
        private (TextBox PropertyName, TextInput PropertyValue) RotationField;

        int h = 32;

        public void ConstructLayout()
        {
            var y = 0;
            var txtTile = this.TextBox(new Vector2i(0, y), new Vector2i(this.PixelSize.X, h));
            txtTile.Text = "Pose";

            PositionField = ConstructField("Position", ref y);
            RotationField = ConstructField("Rotation", ref y);

            var btnApply = this.Button(new Vector2i(0, y += h), new Vector2i(this.PixelSize.X, h), text: "OK");
        }

        private (TextBox PropertyNameControl, TextInput PropertyValueControl) ConstructField(string propertyName, ref int y)
        {
            var root = this.Control(new Vector2i(0, y += h), new Vector2i(this.PixelSize.X, h), skinElement: "BlankControl");
            var txt = root.TextBox(new Vector2(0, 0), new Vector2(0.3f, 1), text: propertyName);
            var val = root.TextInput(new Vector2(0.3f, 0), new Vector2(0.7f, 1));

            return (txt, val);
        }

        public void Assign(Pose pose)
        {
            this.position = pose.Position;
            this.rotation = pose.Rotation;

            PositionField.PropertyValue.Text = new Vector3Serializer().Serialize(position);
            RotationField.PropertyValue.Text = new QuaternionSerializer().Serialize(rotation);
        }
    }

    public class MaterialPropertiesControl : UiControl
    {
        private float frequency;
        private float dampingRatio;
        private float frictionCoefficient;
        private float maximumRecoveryVelocity;

        public MaterialPropertiesControl(Material material)
        {
            this.frequency = material.Frequency;
            this.dampingRatio = material.DampingRatio;
            this.frictionCoefficient = material.FrictionCoefficient;
            this.maximumRecoveryVelocity = material.MaximumRecoveryVelocity;
        }
    }

    public class BoxPropertiesControl : UiControl
    {
        private float length;
        private float height;
        private float width;

        public BoxPropertiesControl(Box box)
        {
            this.length = box.Length;
            this.height = box.Height;
            this.width = box.Width;
        }
    }
}