﻿/*
using Lib_K_Relay;
using Lib_K_Relay.Utilities;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Client;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Networking.Packets.DataObjects;
*/

using System.Threading.Tasks;
using Lib_K_Relay;
using Lib_K_Relay.GameData;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Networking.Packets.Client;
using Lib_K_Relay.Networking.Packets.DataObjects;

namespace NtxBot
{
    public partial class Plugin : IPlugin
    {
        private static FormUI ui;

        private FlashClient flash;

        private FlashClient Flash
        {
            get
            {
                if (flash == null)
                {
                    flash = new FlashClient();
                }

                return flash;
            }
        }

        public string GetAuthor() => "natiiix";

        public string[] GetCommands() => new string[0];

        public string GetDescription() => "RotMG bot by natiiix";

        public string GetName() => "NTX Bot";

        public void Initialize(Proxy proxy)
        {
            ShowUI();

            // Information about a specified tile
            proxy.HookCommand("tileinfo", (client, cmd, args) =>
            {
                int x = 0;
                int y = 0;

                if (args.Length == 0)
                {
                    x = (int)client.PlayerData.Pos.X;
                    y = (int)client.PlayerData.Pos.Y;
                }
                else if (args.Length == 2 && (!int.TryParse(args[0], out x) || !int.TryParse(args[1], out y)))
                {
                    Log("Invalid coordinates!");
                    return;
                }
                else if (args.Length != 2)
                {
                    Log("Unexpected number of arguments!");
                    return;
                }

                Log(map.Tiles[x, y].ToString());
            });

            // Information about player's location
            proxy.HookCommand("where", (client, cmd, args) =>
            {
                Log("Player's location: " + client.PlayerData.Pos.ToString());
            });

            // Information about living entities on the current map
            proxy.HookCommand("living", (client, cmd, args) =>
            {
                map.LivingEntities.ForEach(x => Log("Id=" + x.Status.ObjectId.ToString() + " Type=" + x.ObjectType.ToString() + " (" + GameData.Objects.ByID(x.ObjectType).Name + ")"));
            });

            proxy.HookCommand("goto", (client, cmd, args) =>
            {
                new MovementEngine(client, Flash, map).BeginMove(new Point(int.Parse(args[0]), int.Parse(args[1])));
            });

            proxy.HookCommand("uncover", (client, cmd, args) =>
            {
                Task.Factory.StartNew(new AbyssBot(client, Flash, map).UncoverPath);
            });

            proxy.HookCommand("abyss", (client, cmd, args) =>
            {
                Task.Factory.StartNew(new AbyssBot(client, Flash, map).Run);
            });

            proxy.HookCommand("quest", (client, cmd, args) =>
            {
                Entity questObject = map?.QuestObject;
                Log("Quest: " + (questObject == null ? "NULL" : GameData.Objects.ByID(questObject.ObjectType).Name));
            });

            proxy.HookCommand("flash", (client, cmd, args) =>
            {
                flash = new FlashClient();
                Log("Flash Player binding is now ready");
            });

            proxy.HookPacket<UpdatePacket>(OnUpdate);
            proxy.HookPacket<NewTickPacket>(OnNewTick);
            proxy.HookPacket<MapInfoPacket>(OnMapInfo);
            proxy.HookPacket<GotoAckPacket>(OnGotoAck);
            proxy.HookPacket<QuestObjIdPacket>(OnQuestObjId);

            Log("Packets hooked");
        }
    }
}