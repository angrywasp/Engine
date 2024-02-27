using Engine.Multiplayer;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Lidgren.Network;
using Engine.Cameras;
using System.Numerics;
using BepuPhysics.Collidables;
using BepuPhysics;
using Engine.Physics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using AngryWasp.Helpers;
using Engine.World.Components;
using Engine.Content.Model.Instance;
using Engine.Debug.Shapes.Physics;

namespace Engine.Objects.GameObjects
{
    public class CharacterType : UnitType
    {
        [JsonProperty] public float Height { get; set; } = 1.8f;

        [JsonProperty] public float Radius { get; set; } = 0.5f;

        [JsonProperty] public float Mass { get; set; } = 1.0f;

        [JsonProperty] public float WalkSpeed { get; set; } = 1.0f;

        [JsonProperty] public float SprintCoefficient { get; set; } = 2.0f;

        [JsonProperty] public float WalkForce { get; set; } = 10.0f;

        [JsonProperty] public float JumpSpeed { get; set; } = 10.0f;
    }

    public class Character : Unit
    {
        public new enum Network_Message : ushort
        {
            Move,
            Turn,
            Jump
        }

        #region Required in every script class

        private CharacterType _type = null;

        public new CharacterType Type => _type;

        #endregion

        float onGroundTime;
        bool onGround;
        bool movementUpdated = false;

        Capsule shape;
        BodyHandle bodyHandle;
        BodyReference bodyReference;
        

        Quaternion orientation;
        DebugPhysicsCharacterCapsule debugShape;

        private List<MeshInstance> attachedMeshes = new List<MeshInstance>();

        public override void OnAddedToMap()
        {
            base.OnAddedToMap();
        
            //todo: Need type properties
            var speculativeMargin = 0.1f;
            var maximumHorizontalForce = _type.WalkForce;
            var maximumVerticalGlueForce = 100f;

            if (_type.Height < _type.Radius * 2)
                throw new ArgumentException("Chatacter height must be at least 2x the radius");

            var shape = new Capsule(_type.Radius, _type.Height - (_type.Radius * 2));
            var shapeIndex = engine.Scene.Physics.Simulation.Shapes.Add(shape);

            var description = BodyDescription.CreateDynamic(transform.Translation,
                new BodyVelocity(default, default),
                new BodyInertia { InverseMass = 1f / _type.Mass },
                new CollidableDescription(shapeIndex, speculativeMargin),
                //make negative to prevent character sleeping
                new BodyActivityDescription(-(shape.Radius * 0.02f)));

            bodyHandle = engine.Scene.Physics.AddDynamicBody(description, new PhysicsMaterial
            {
                FrictionCoefficient = float.MaxValue,
                MaximumRecoveryVelocity = 2,
                SpringSettings = new BepuPhysics.Constraints.SpringSettings(30, 1)
            });
            ref var characterController = ref engine.Scene.Physics.AddCharacter(bodyHandle);
            bodyReference = new BodyReference(bodyHandle, engine.Scene.Physics.Simulation.Bodies);

            debugShape = new DebugPhysicsCharacterCapsule(ref shape, ref bodyReference);
            engine.Scene.Graphics.DebugRenderer.QueueShapeAdd(debugShape);

            characterController.LocalUp = new Vector3(0, 1, 0);
            characterController.CosMaximumSlope = MathF.Cos(MathHelper.PiOver4);
            characterController.JumpVelocity = _type.JumpSpeed;
            characterController.MaximumVerticalForce = maximumVerticalGlueForce;
            characterController.MaximumHorizontalForce = maximumHorizontalForce;
            characterController.MinimumSupportDepth = shape.Radius * -0.01f;
            characterController.MinimumSupportContinuationDepth = -speculativeMargin;
            this.shape = shape;

            foreach (var c in Components)
            {
                if (c.Value is not MeshComponent mc)
                    continue;

                attachedMeshes.Add(mc.Mesh);
            }
        }

        public override void Update(Camera camera, GameTime gameTime)
        {
            base.Update(camera, gameTime);
            ref var characterController = ref engine.Scene.Physics.GetCharacter(bodyHandle);
            onGround = characterController.Supported;

            if (onGround)
                onGroundTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            else
                onGroundTime = 0;

            switch (engine.NetworkType)
            {
                case Network_User_Type.Offline:
                case Network_User_Type.Server:
                    {
                        if (onGround && !movementUpdated)
                            characterController.TargetVelocity = Vector2.Zero;

                        foreach (var c in Components)
                            c.Value.Update(camera, gameTime);

                        transform.Update(orientation, bodyReference.Pose.Position);
                    }
                    break;
                case Network_User_Type.Client:
                    {
                        Debugger.Break();

                        /*
                        if (onGround && !movementUpdated)
                            physicsController.HorizontalMotionConstraint.MovementDirection = Vector2.Zero;

                        Vector2 xx = new Vector2(physicsController.Body.Position.X, physicsController.Body.Position.Z);
                        Vector2 horizontalPositionChange = xx - lastSentHorizontalPosition;

                        if (characterController.Body.Position != lastSentPosition)
                        {
                            #region send it to the server

                            var msg = engine.NetworkClient.NetworkObject.CreateMessage();
                            msg.Write((ushort)Custom_Message_Type.MapObjectUpdate);
                            msg.Write(networkIdentifier);
                            msg.Write((ushort)Network_Message.Move);

                            

                            msg.Write(physicsController.Body.Position);
                            msg.Write(physicsController.HorizontalMotionConstraint.MovementDirection);

                            engine.NetworkClient.NetworkObject.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);

                            #endregion

                            lastSentHorizontalPosition = xx;
                            lastSentMovementDirection = physicsController.HorizontalMotionConstraint.MovementDirection;
                            lastSentPosition = physicsController.Body.Position;
                        }

                        if (physicsController.Body.Orientation != lastSentRotation)
                        {
                            #region send to server

                            var msg = engine.NetworkClient.NetworkObject.CreateMessage();
                            msg.Write((ushort)Custom_Message_Type.MapObjectUpdate);
                            msg.Write(networkIdentifier);
                            msg.Write((ushort)Network_Message.Turn);

                            msg.Write(physicsController.Body.Orientation);

                            engine.NetworkClient.NetworkObject.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);

                            #endregion

                            lastSentRotation = physicsController.Body.Orientation;
                        }

                        SetTransform(physicsController.Body.Position, physicsController.Body.Orientation, Vector3.One);
                        */
                    }
                    break;
            }

            movementUpdated = false;
            debugShape.Update();
        }

        Vector2 lastSentHorizontalPosition = Vector2.Zero;
        Vector3 lastSentPosition = Vector3.Zero;
        Vector2 lastSentMovementDirection = Vector2.Zero;
        Quaternion lastSentRotation = Quaternion.Identity;

        public void TryJump()
        {
            ref var characterController = ref engine.Scene.Physics.GetCharacter(bodyHandle);

            characterController.TryJump = true;

            if (engine.NetworkType == Network_User_Type.Client)
            {
                #region send to server

                var msg = engine.NetworkClient.NetworkObject.CreateMessage();
                msg.Write((ushort)Custom_Message_Type.MapObjectUpdate);
                msg.Write(UID);
                msg.Write((ushort)Network_Message.Jump);

                engine.NetworkClient.NetworkObject.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);

                #endregion
            }
        }

        public void TryMove(Vector2 movementDirection, bool run)
        {
            //this should only be on the client because the controller is client side
            if (!onGround)
                return;

            ref var characterController = ref engine.Scene.Physics.GetCharacter(bodyHandle);
            float speed = run ? _type.WalkSpeed * _type.SprintCoefficient : _type.WalkSpeed;

            characterController.TargetVelocity = movementDirection * speed;
            movementUpdated = true;

            foreach (var m in attachedMeshes)
                m.UpdateSelectedAnimation(movementDirection == Vector2.Zero ? "Idle" : run ? "Run" : "Walk");
        }

        public void TryTurn(float yaw)
        {
            ref var characterController = ref engine.Scene.Physics.GetCharacter(bodyHandle);
            orientation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw);
            characterController.ViewDirection = Matrix4x4.CreateFromQuaternion(orientation).Forward();
        }

        public override void ProcessMessage(ushort uid, ushort mapObjectCommand, NetIncomingMessage message)
        {
            Debugger.Break();
            /*
            Network_Message msg = (Network_Message)mapObjectCommand;

            switch (engine.NetworkType)
            {
                case Network_User_Type.Server:
                    {
                        switch (msg)
                        {
                            case Network_Message.Move:
                                {
                                    //when we get a command from the client
                                    //we update the server side character and send the message out to the other clients to do the same
                                    Vector3 position = message.ReadVector3();
                                    Vector2 moveDirection = message.ReadVector2();

                                    physicsController.TeleportToPosition(position, 0);
                                    physicsController.HorizontalMotionConstraint.MovementDirection = moveDirection;

                                    #region Send to clients

                                    var msgOut = engine.NetworkServer.CreateMessageHeader(Custom_Message_Type.MapObjectUpdate);
                                    msgOut.Write(uid);
                                    msgOut.Write((ushort)Network_Message.Move);

                                    msgOut.Write(position);
                                    msgOut.Write(moveDirection);

                                    ServerSendToAllClientsExceptThis(uid, msgOut);

                                    #endregion
                                }
                                break;
                            case Network_Message.Turn:
                                {
                                    Quaternion rot = message.ReadQuaternion();

                                    physicsController.Body.Orientation = rot;
                                    physicsController.ViewDirection = Matrix4x4.CreateFromQuaternion(rot).Forward;

                                    #region Send to clients

                                    var msgOut = engine.NetworkServer.CreateMessageHeader(Custom_Message_Type.MapObjectUpdate);
                                    msgOut.Write(uid);
                                    msgOut.Write((ushort)Network_Message.Turn);

                                    msgOut.Write(rot);

                                    ServerSendToAllClientsExceptThis(uid, msgOut);

                                    #endregion
                                }
                                break;
                            case Network_Message.Jump:
                                {
                                    characterController.TryJump = true;
                                }
                                break;
                            default:
                                Debugger.Break();
                                break;
                        }
                    }
                    break;
                case Network_User_Type.Client:
                    {
                        switch (msg)
                        {
                            case Network_Message.Move:
                                {
                                    Vector3 position = message.ReadVector3();
                                    Vector2 moveDirection = message.ReadVector2();

                                    physicsController.TeleportToPosition(position, 0);
                                    physicsController.HorizontalMotionConstraint.MovementDirection = moveDirection;
                                }
                                break;
                            case Network_Message.Turn:
                                {
                                    Quaternion rot = message.ReadQuaternion();

                                    physicsController.Body.Orientation = rot;
                                    physicsController.ViewDirection = Matrix4x4.CreateFromQuaternion(rot).Forward;
                                }
                                break;
                            case Network_Message.Jump:
                                {
                                    characterController.TryJump = true;
                                }
                                break;
                            default:
                                Debugger.Break();
                                break;
                        }
                    }
                    break;
            }
            */
        }

        
    }
}
