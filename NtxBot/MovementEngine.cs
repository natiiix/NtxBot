﻿using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets.DataObjects;
using System;
using System.Threading.Tasks;

namespace NtxBot
{
    public class MovementEngine
    {
        private static readonly TimeSpan TIMESPAN_BETWEEN_ITERATIONS = TimeSpan.FromSeconds(0.05);
        private static readonly TimeSpan TIMESPAN_RESTART_MOVEMENT = TimeSpan.FromSeconds(0.5);

        private Client client;
        private FlashClient flash;
        private Task movementTask;
        private Location targetLocation;

        public bool Moving { get => movementTask != null && movementTask.Status != TaskStatus.RanToCompletion; }

        public MovementEngine(Client client, FlashClient flash)
        {
            this.client = client;
            this.flash = flash;

            movementTask = null;
            targetLocation = null;
        }

        public void Move(Location targetLocation)
        {
            this.targetLocation = targetLocation;

            if (Moving)
            {
                throw new Exception("Unable to start moving synchronously while moving asynchronously!");
            }

            MoveToTargetAsync();
        }

        public void BeginMove(Location targetLocation)
        {
            this.targetLocation = targetLocation;

            // Create a new task if there isn't one running now
            if (!Moving)
            {
                movementTask?.Dispose();
                movementTask = Task.Factory.StartNew(MoveToTargetAsync);
            }
        }

        public void StopMovement()
        {
            targetLocation = null;
        }

        private async void MoveToTargetAsync()
        {
            // Get the current time and player position
            Location lastPlayerPos = client.PlayerData.Pos;
            DateTime dtLastMove = DateTime.Now;

            // Move towards the target location
            // Loop breaks when the target location is reached or when the client connection drops
            while (targetLocation != null && client.Connected &&
                flash.SetMovementDirection(targetLocation.X - client.PlayerData.Pos.X, targetLocation.Y - client.PlayerData.Pos.Y, 0.5))
            {
                // Add some delay between the iterations
                await Task.Delay(TIMESPAN_BETWEEN_ITERATIONS);

                // Player has moved since the last iteration
                if (lastPlayerPos.X != client.PlayerData.Pos.X || lastPlayerPos.Y != client.PlayerData.Pos.Y)
                {
                    // Update the current time and player position
                    lastPlayerPos = client.PlayerData.Pos;
                    dtLastMove = DateTime.Now;
                }
                // Player hasn't moved for some time
                else if (DateTime.Now - dtLastMove >= TIMESPAN_RESTART_MOVEMENT)
                {
                    // Stop moving to reset the key states
                    flash.StopMovement();
                }
            }

            // Stop all movement
            flash.StopMovement();

            // Reset the target location
            targetLocation = null;
        }
    }
}