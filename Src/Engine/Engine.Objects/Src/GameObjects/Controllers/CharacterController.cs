using System;
using System.Collections.Generic;
using CameraController = Engine.Objects.Controllers.CameraControllers.Base;
using InputController = Engine.Objects.Controllers.InputControllers.Base;
using Engine.Cameras;
using Engine.Objects.Controllers;
using Engine.Objects.Controllers.CameraControllers;
using Engine.Objects.Controllers.InputControllers;
using AngryWasp.Logger;
using System.Numerics;
using AngryWasp.Helpers;
using Engine.World;
using Microsoft.Xna.Framework;

namespace Engine.Objects.GameObjects.Controllers
{
    public class CharacterControllerType : UnitControllerType
    {
    }

    public class CharacterController : UnitController
    {
        #region Required in every script class

        private CharacterControllerType _type = null;

        public new CharacterControllerType Type => _type;

        #endregion

        private Character controlledCharacter;
        public List<InputController> inputControllers = new List<InputController>();
        private PlayerFps fpsCameraController;
        private PlayerTps tpsCameraController;

        private Camera_Type cameraType = Camera_Type.Tps;

        /// <summary>
        /// First step is update the InputController
        /// In doing so the controlled object consumes the commands raised by the InputController
        /// Then we update the ControlledObject, then we update the CameraController to calculate 
        /// our final camera position for the frame
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="gameTime"></param>

        public override void Update(Camera camera, GameTime gameTime)
        {
            base.Update(camera, gameTime);
            foreach (var ic in inputControllers)
                ic.Update(gameTime);

            fpsCameraController.Pitch = tpsCameraController.Pitch = pitch;
        }

        float yaw;
        float pitch;

        public override void AssignControlledObject(GameObject gameObject)
        {
            base.AssignControlledObject(gameObject);
            controlledCharacter = gameObject as Character;

            if (controlledCharacter == null)
            {
                Log.Instance.WriteFatal("CharacterController can only be applied to a Character");
                return;
            }

            fpsCameraController = new PlayerFps(engine.Camera, gameObject);
            tpsCameraController = new PlayerTps(engine.Camera, gameObject);

            engine.Camera.Controller = tpsCameraController;
            inputControllers.Add(new KeyboardMouse(engine, gameObject));
        }

        private void CycleCameraType()
        {
            cameraType++;

            if (cameraType >= Camera_Type.Invalid)
                cameraType = (Camera_Type)0;

            switch (cameraType)
            {
                case Camera_Type.Fps:
                    engine.Camera.Controller = fpsCameraController;
                    break;
                case Camera_Type.Tps:
                    engine.Camera.Controller = tpsCameraController;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public override void DoInputControllerCommand(float[] keys)
        {
            if (keys[ControlKeys.ChangeCamera] > 0)
                CycleCameraType();

            if (keys[ControlKeys.LookUp] != 0)
            {
                float mult = keys[ControlKeys.LookUp];
                pitch = MathHelper.Clamp(pitch - mult, -MathHelper.PiOver2, MathHelper.PiOver2);
            }
            else if (keys[ControlKeys.LookDown] != 0)
            {
                float mult = keys[ControlKeys.LookDown];
                pitch = MathHelper.Clamp(pitch - mult, -MathHelper.PiOver2, MathHelper.PiOver2);
            }
            
            if (keys[ControlKeys.LookLeft] != 0)
            {
                yaw -= keys[ControlKeys.LookLeft];
                controlledCharacter.TryTurn(yaw);
            }
            else if (keys[ControlKeys.LookRight] != 0)
            {
                yaw -= keys[ControlKeys.LookRight];
                controlledCharacter.TryTurn(yaw);
            } 

            if (keys[ControlKeys.Jump] > 0)
                controlledCharacter.TryJump();

            Vector2 movementDirection = default;

            if (keys[ControlKeys.Forward] > 0)
                movementDirection = Vector2.UnitY;

            if (keys[ControlKeys.Backward] > 0)
                movementDirection += -Vector2.UnitY;

            if (keys[ControlKeys.Left] > 0)
                movementDirection += -Vector2.UnitX;

            if (keys[ControlKeys.Right] > 0)
                movementDirection += Vector2.UnitX;

            var movementDirectionLengthSquared = movementDirection.LengthSquared();
            if (movementDirectionLengthSquared > 0)
                movementDirection /= MathF.Sqrt(movementDirectionLengthSquared);

            controlledCharacter.TryMove(movementDirection, keys[ControlKeys.Run] > 0);
        }
    }
}
