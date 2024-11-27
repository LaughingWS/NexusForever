﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NexusForever.Shared;
using NexusForever.Shared.Configuration;
using NexusForever.Shared.Database;
using NexusForever.Shared.Game;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Command;
using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Game.RBAC;
using NexusForever.WorldServer.Game;
using NexusForever.WorldServer.Game.Achievement;
using NexusForever.WorldServer.Game.CharacterCache;
using NexusForever.WorldServer.Game.Cinematic;
using NexusForever.WorldServer.Game.Combat;
using NexusForever.WorldServer.Game.Contact;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Movement;
using NexusForever.WorldServer.Game.Entity.Network;
using NexusForever.WorldServer.Game.Guild;
using NexusForever.WorldServer.Game.Housing;
using NexusForever.WorldServer.Game.Loot;
using NexusForever.WorldServer.Game.Map;
using NexusForever.WorldServer.Game.PathContent;
using NexusForever.WorldServer.Game.Prerequisite;
using NexusForever.WorldServer.Game.Quest;
using NexusForever.WorldServer.Game.Reputation;
using NexusForever.WorldServer.Game.Social;
using NexusForever.WorldServer.Game.Spell;
using NexusForever.WorldServer.Game.Storefront;
using NexusForever.WorldServer.Game.TextFilter;
using NexusForever.WorldServer.Network;
using NexusForever.WorldServer.Script;
using NLog;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Hosting.Systemd;
using System;

namespace NexusForever.WorldServer
{
    public class HostedService : IHostedService
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

#if DEBUG
        private const string Title = "NexusForever: World Server (DEBUG)";
#else
        private const string Title = "NexusForever: World Server (RELEASE)";
#endif

        private UpdateTimer titleUpdate = new(TimeSpan.FromSeconds(1));
        private double tickCount = 0u;
        private bool isService = false;

        public HostedService()
        {
            isService = WindowsServiceHelpers.IsWindowsService() || SystemdHelpers.IsSystemdService();
        }

        /// <summary>
        /// Start <see cref="WorldServer"/> and any related resources.
        /// </summary>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            log.Info("Starting...");

            DatabaseManager.Instance.Initialise(ConfigurationManager<WorldServerConfiguration>.Instance.Config.Database);
            DatabaseManager.Instance.Migrate();

            // RBACManager must be initialised before CommandManager
            RBACManager.Instance.Initialise();
            DisableManager.Instance.Initialise();

            GameTableManager.Instance.Initialise();
            BaseMapManager.Instance.Initialise();
            SearchManager.Instance.Initialise();
            EntityManager.Instance.Initialise();
            EntityCommandManager.Instance.Initialise();
            EntityCacheManager.Instance.Initialise();
            FactionManager.Instance.Initialise();
            GlobalMovementManager.Instance.Initialise();

            CharacterManager.Instance.Initialise(); // must be initialised before residences
            GlobalChatManager.Instance.Initialise(); // must be initialised before guilds
            GlobalAchievementManager.Instance.Initialise(); // must be initialised before guilds
            GlobalPathContentManager.Instance.Initialise();
            GlobalGuildManager.Instance.Initialise(); // must be initialised before residences
            GlobalContactManager.Instance.Initialise();
            GlobalResidenceManager.Instance.Initialise();
            GlobalGuildManager.Instance.ValidateCommunityResidences();

            AssetManager.Instance.Initialise();
            ItemManager.Instance.Initialise();
            PrerequisiteManager.Instance.Initialise();
            GlobalSpellManager.Instance.Initialise();
            GlobalQuestManager.Instance.Initialise();
            GlobalLootManager.Instance.Initialise();

            GlobalStorefrontManager.Instance.Initialise();
            GlobalCinematicManager.Instance.Initialise();

            ScriptManager.Instance.Initialise();
            DamageCalculator.Instance.Initialise();
            
            ServerManager.Instance.Initialise(ConfigurationManager<WorldServerConfiguration>.Instance.Config.RealmId);

            TextFilterManager.Instance.Initialise();

            // initialise world after all assets have loaded but before any network or command handlers might be invoked
            WorldManager.Instance.Initialise(lastTick =>
            {
                tickCount += lastTick;

                // NetworkManager must be first and MapManager must come before everything else
                NetworkManager<WorldSession>.Instance.Update(lastTick);
                MapManager.Instance.Update(lastTick);

                BuybackManager.Instance.Update(lastTick);
                GlobalQuestManager.Instance.Update(lastTick);
                GlobalPathContentManager.Instance.Update(lastTick);
                GlobalLootManager.Instance.Update(lastTick);
                GlobalGuildManager.Instance.Update(lastTick);
                GlobalResidenceManager.Instance.Update(lastTick); // must be after guild update
                GlobalChatManager.Instance.Update(lastTick);
                GlobalContactManager.Instance.Update(lastTick);
                LoginQueueManager.Instance.Update(lastTick);
                ShutdownManager.Instance.Update(lastTick);

                // process commands after everything else in the tick has processed
                CommandManager.Instance.Update(lastTick);

                if (isService)
                    return;

                titleUpdate.Update(lastTick);
                if (titleUpdate.HasElapsed)
                {
                    Console.Title = $"{Title} | Users: {NetworkManager<WorldSession>.Instance.GetSessionsCount(x => x.Account != null)} (Queued: {NetworkManager<WorldSession>.Instance.GetSessionsCount(x => x.IsQueued != false)}) | Uptime {TimeSpan.FromSeconds(tickCount).ToString(@"dd\:hh\:mm\:ss")}";
                    titleUpdate.Reset();
                }
            });

            // initialise network and command managers last to make sure the rest of the server is ready for invoked handlers
            MessageManager.Instance.Initialise();
            NetworkManager<WorldSession>.Instance.Initialise(ConfigurationManager<WorldServerConfiguration>.Instance.Config.Network);

            CommandManager.Instance.Initialise();

            log.Info("Started!");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stop <see cref="WorldServer"/> and any related resources.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            log.Info("Stopping...");

            // stop network manager listening for incoming connections
            // it is still possible for incoming packets to be parsed though won't be handled once the world thread is stopped
            NetworkManager<WorldSession>.Instance.Shutdown();

            // stop command manager listening for commands
            CommandManager.Instance.Shutdown();

            // stop server manager pinging other servers
            ServerManager.Instance.Shutdown();

            // stop world manager processing the world thread
            // at this point no incoming packets will be handled
            WorldManager.Instance.Shutdown();

            // save residences, guilds and players to the database
            GlobalResidenceManager.Instance.Shutdown();
            GlobalGuildManager.Instance.Shutdown();

            foreach (WorldSession worldSession in NetworkManager<WorldSession>.Instance)
                worldSession.Player?.SaveDirect();

            log.Info("Stopped!");
            return Task.CompletedTask;
        }
    }
}
