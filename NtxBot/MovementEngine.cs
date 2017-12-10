using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets.DataObjects;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Lib_K_Relay.GameData;

namespace NtxBot
{
    public class MovementEngine
    {
        private const double MOVEMENT_THRESHOLD_DISTANCE = 0.5;
        private static readonly TimeSpan TIMESPAN_BETWEEN_ITERATIONS = TimeSpan.FromSeconds(0.05);
        private static readonly TimeSpan TIMESPAN_RESTART_MOVEMENT = TimeSpan.FromSeconds(0.5);

        private Client client;
        private FlashClient flash;
        private GameMap map;

        private Task moveTask;
        private CancellationTokenSource moveCTS;

        public bool Moving { get => moveTask != null && moveTask.Status == TaskStatus.Running; }

        public MovementEngine(Client client, FlashClient flash, GameMap map)
        {
            this.client = client;
            this.flash = flash;
            this.map = map;

            moveTask = null;
        }

        public void BeginMove(Point target)
        {
            // Cancel the previous movement task if there was any
            if (Moving)
            {
                moveCTS.Cancel();
                moveTask.Wait();
            }

            // Create a new movement task
            moveCTS = new CancellationTokenSource();
            moveTask = Task.Factory.StartNew(() =>
            {
                IEnumerable<Location> shortestPath = FindShortestPath(target);

                // Move along the path
                shortestPath.ForEach(x =>
                {
                    if (moveCTS.IsCancellationRequested)
                    {
                        return;
                    }

                    MoveDirectlyToTarget(x);
                });
            }, moveCTS.Token);
        }

        public void CancelMove() => moveCTS.Cancel();

        private async void MoveDirectlyToTarget(Location target)
        {
            // Get the current time and player position
            Location lastPlayerPos = client.PlayerData.Pos;
            DateTime dtLastMove = DateTime.Now;

            // Move towards the target location
            // Loop breaks when the target location is reached, when a task
            // cancellation is requested or when the client connection drops
            while (client.Connected && !moveCTS.IsCancellationRequested &&
                flash.SetMovementDirection(target.X - client.PlayerData.Pos.X, target.Y - client.PlayerData.Pos.Y, MOVEMENT_THRESHOLD_DISTANCE))
            {
                // Add some delay between the iterations
                await Task.Delay(TIMESPAN_BETWEEN_ITERATIONS, moveCTS.Token);

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
        }

        private IEnumerable<Location> FindShortestPath(Point target)
        {
            return new SpatialAStar<GameMapTile>(map.Tiles).Search((Point)client.PlayerData.Pos, target)
                // Construct a location from the tile coordinates
                // Make it point to the center of the tile rather than the top-left corner
                .Select(x => new Location(x.X + 0.5f, x.Y + 0.5f));
        }
    }
}