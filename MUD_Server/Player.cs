using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace MUD_Server
{
    public class Player
    {
        private bool[] keyInputs;
        private enum InputIds
        {
            None,
            WalkNorth,
            WalkEast,
            WalkSouth,
            WalkWest,
            Length   //This has to be last!
        }

        public Guid id;
        public Vector3 position;
        public Quaternion rotation;
        public int worldId;

        private float moveSpeed;

        public Player(Guid newId)
        {
            id = newId;
            moveSpeed = 10f;
            keyInputs = new bool[(int)InputIds.Length];

            // For testing only
            Random rnd = new Random();
            position.X = rnd.Next(10);
            position.Z = rnd.Next(10);

        }

        public void Update(float deltaTime)
        {

            CheckMovementInput();

            if (Game.testMovement)
            {
                //Just for testing
                Vector2 direction = Vector2.Zero;
                direction.Y = 1;
                Move(direction);
            }
        }

        public void SetKey(int inputId)
        {
            keyInputs[inputId] = true;
        }

        private void CheckMovementInput()
        {
            Vector2 direction = Vector2.Zero;
            if (keyInputs[(int)InputIds.WalkNorth])
            {
                direction.Y = 1;
                keyInputs[(int)InputIds.WalkNorth] = false;
            }
            else if (keyInputs[(int)InputIds.WalkSouth])
            {
                direction.Y = -1;
                keyInputs[(int)InputIds.WalkSouth] = false;
            }

            if (keyInputs[(int)InputIds.WalkEast])
            {
                direction.X = -1;
                keyInputs[(int)InputIds.WalkEast] = false;
            }
            else if (keyInputs[(int)InputIds.WalkWest])
            {
                direction.X = 1;
                keyInputs[(int)InputIds.WalkWest] = false;
            }

            if (direction != Vector2.Zero)
            {
                Move(direction);
            }
        }

        private void Move(Vector2 inputDirection)
        {
            Vector3 forward = Vector3.Transform(new Vector3(0f, 0f, 1f), rotation);
            Vector3 right = Vector3.Normalize(Vector3.Cross(forward, new Vector3(0f, 1f, 0f)));
            Vector3 moveDirection = right * inputDirection.X + forward * inputDirection.Y;
            moveDirection.Y = 0;
            position += moveDirection * moveSpeed * Game.DeltaTime;

            if (position.X < 0)
                position.X = 0;
            if (position.Z < 0)
                position.Z = 0;

            foreach (Client client in Server.GetClients())
            {
                Server.SendPlayerPosition(in client.id, this);
            }
        }
    }
}
