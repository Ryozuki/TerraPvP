﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Mono.Data.Sqlite;
using System.IO;
using System.Data;

namespace TerraPvP
{
    [ApiVersion(1, 25)]
    public class TerraPvP : TerrariaPlugin
    {
        public static IDbConnection Db { get; private set; }
        public static PRankManager RankManager { get; private set; }
        public List<PRank> usersinqeue = new List<PRank>();

        #region Info
        public override string Name { get { return "TerraPvP"; } }
        public override string Author { get { return "Ryozuki"; } }
        public override string Description { get { return "A PvP plugin with MMR, ranks and stats"; } }
        public override Version Version { get { return new Version(1, 0, 0); } }
        #endregion

        public TerraPvP(Main game) : base(game)
        {

        }

        #region Initialize
        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
            TShockAPI.Hooks.PlayerHooks.PlayerPostLogin += OnPlayerLogin;

        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnPostInitialize);
                TShockAPI.Hooks.PlayerHooks.PlayerPostLogin -= OnPlayerLogin;
            }
        }

        

        void OnInitialize(EventArgs args)
        {
            Commands.ChatCommands.Add(new Command("terrapvp.qeue", pvpqeue, "pvpqeue")
            {
                HelpText = "Usage: /pvpqeue"
            });
            Commands.ChatCommands.Add(new Command("terrapvp.stats", getstats, "pvpstats")
            {
                HelpText = "Usage: /pvpstats <name> or /pvpstats"
            });

            Db = new SqliteConnection("uri=file://" + Path.Combine(TShock.SavePath, "terrapvp.sqlite") + ",Version=3");
        }

        private void OnPostInitialize(EventArgs args)
        {
            RankManager = new PRankManager(Db);
        }

        void OnPlayerLogin(TShockAPI.Hooks.PlayerPostLoginEventArgs args)
        {
            PRank playerrank = new PRank(args.Player.User.ID, args.Player.Name, 1500, "wood");
            RankManager.addPlayer(playerrank);
        }

        void pvpqeue(CommandArgs e)
        {
            int mmr = 0;
            string rank = "";
            for (int i = 0; i < RankManager.pranks.Count; i++)
            {
                if (RankManager.pranks[i].UserID == e.Player.User.ID)
                {
                    mmr = RankManager.pranks[i].MMR;
                    rank = RankManager.pranks[i].Rank;
                }

            }
            PRank player = new PRank(e.Player.User.ID, e.Player.Name, mmr, rank);
            usersinqeue.Add(player);
            e.Player.SendSuccessMessage("You entered the qeue succesfully");
        }

        void getstats(CommandArgs e)
        {
            int mmr = 0;
            string rank = "";

            if (e.Parameters.Count == 0)
            {
                for (int i = 0; i < RankManager.pranks.Count; i++)
                {
                    if (RankManager.pranks[i].UserID == e.Player.User.ID)
                    {
                        mmr = RankManager.pranks[i].MMR;
                        rank = RankManager.pranks[i].Rank;
                    }

                }
                e.Player.SendSuccessMessage("Stats for " + e.Player.Name);
                e.Player.SendSuccessMessage("Rank: " + rank);
                e.Player.SendSuccessMessage("MMR: " + mmr);
            }
            else
            {
                string args = String.Join(" ", e.Parameters.ToArray());
                int playerid;
                try
                {
                    playerid = TShock.Users.GetUserByName(args).ID;
                }
                catch
                {
                    e.Player.SendErrorMessage("Player not found");
                    return;
                }

                for (int i = 0; i < RankManager.pranks.Count; i++)
                {
                    if (RankManager.pranks[i].UserID == playerid)
                    {
                        mmr = RankManager.pranks[i].MMR;
                        rank = RankManager.pranks[i].Rank;
                        e.Player.SendSuccessMessage("Stats for " + RankManager.pranks[i].Name);
                        e.Player.SendSuccessMessage("Rank: " + rank);
                        e.Player.SendSuccessMessage("MMR: " + mmr);
                    }
                }
            }
        }
    }
}