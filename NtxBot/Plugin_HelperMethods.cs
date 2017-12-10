using Lib_K_Relay.GameData;

namespace NtxBot
{
    public partial class Plugin
    {
        //private DateTime? dtLastTick = null;
        //private Location moveTarget = null;
        //private bool blockNextGotoAck = false;

        //private void MovePlayerTowardsTarget(Client client, int sourceTickId, TimeSpan deltaTime)
        //{
        //    const double MIN_MOVE_SPEED = 4;
        //    const double MAX_MOVE_SPEED = 9.6;

        //    // Character speed in tiles per second
        //    double playerSpeed = MIN_MOVE_SPEED + ((client.PlayerData.Speed / 75.0) * (MAX_MOVE_SPEED - MIN_MOVE_SPEED));
        //    // Number of tiles the character can move in this tick
        //    double maxTickDistance = playerSpeed * deltaTime.TotalSeconds;

        //    // Get the current position of the player
        //    Location currentPos = client.PlayerData.Pos;

        //    // Calculate the distance to target
        //    // Using double-precision methods instead of the built-in float distance method for maximum accuracy
        //    double distanceToTarget = Math.Sqrt(Math.Pow(moveTarget.X - currentPos.X, 2) + Math.Pow(moveTarget.Y - currentPos.Y, 2));

        //    // Create a new GOTO packet
        //    GotoPacket gp = Packet.Create<GotoPacket>(PacketType.GOTO);

        //    // Target is within the maximum move distance
        //    if (distanceToTarget <= maxTickDistance)
        //    {
        //        // Move to the target
        //        gp.Location = moveTarget;

        //        // Reset the move target because it is no longer needed
        //        moveTarget = null;
        //    }
        //    // Target is too far to be reached in a single tick
        //    else
        //    {
        //        // Calculate the partial target
        //        double moveScale = maxTickDistance / distanceToTarget;

        //        double partialX = currentPos.X + ((moveTarget.X - currentPos.X) * moveScale);
        //        double partialY = currentPos.Y + ((moveTarget.Y - currentPos.Y) * moveScale);

        //        // Move to the partial target
        //        gp.Location = new Location((float)partialX, (float)partialY);
        //    }

        //    // Set the object ID to the ID of the player's character
        //    gp.ObjectId = client.ObjectId;

        //    // Bock the next GOTOACK packet
        //    blockNextGotoAck = true;

        //    // Send the GOTO packet to the client
        //    client.SendToClient(gp);
        //}

        public static void Log(string text) => ui?.AppendLog(text);

        private static void ShowUI()
        {
            if (ui == null)
            {
                ui = new FormUI();
            }

            ui.Show();
            //PluginUtils.ShowGUI(ui);
        }
    }
}