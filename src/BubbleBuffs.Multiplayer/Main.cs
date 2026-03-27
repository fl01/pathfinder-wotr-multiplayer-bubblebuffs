using System;
using System.Reflection;
using BubbleBuffs.Multiplayer.Networking.Messages;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnityModManagerNet;
using WOTRMultiplayer.Abstractions.IO;
using WOTRMultiplayer.Abstractions.Unity;
using WOTRMultiplayer.Extensions;
using WOTRMultiplayer.Networking.Abstractions;
using WOTRMultiplayer.Networking.Messages;

namespace BubbleBuffs.Multiplayer
{
    public class Main
    {
        public static ILogger<Main> Logger { get; private set; }

        private static string _modId;

        public static bool Load(UnityModManager.ModEntry entry)
        {
            _modId = entry.Info.Id;

            Logger = WOTRMultiplayer.Main.GetLogger<Main>();
            Logger.LogDebug("Mod is ready to use WOTRMultiplayer types. ModId={ModId}", _modId);

            InitializeNetworking();
            SubscribeToNetworkMessages();

            entry.OnToggle += OnToggle;

            var harmony = new Harmony(entry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }

        /// <summary>
        /// Either server or client is active at the same time
        /// </summary>
        /// <param name="message"></param>
        public static void Send(object message)
        {
            var server = WOTRMultiplayer.Main.ServiceProvider.GetService<INetworkServer>();
            if (server.IsActive)
            {
                server.SendAll(message);
                return;
            }

            var client = WOTRMultiplayer.Main.ServiceProvider.GetService<INetworkClient>();
            if (client.IsActive)
            {
                client.Send(message);
            }
        }

        private static void SubscribeToNetworkMessages()
        {
            var server = WOTRMultiplayer.Main.ServiceProvider.GetService<INetworkServer>();
            server.On<NotifyBubbleBuffsUsed>(OnNotifyBubbleBuffsUsed);

            var client = WOTRMultiplayer.Main.ServiceProvider.GetService<INetworkClient>();
            client.On<NotifyBubbleBuffsUsed>(OnNotifyBubbleBuffsUsed);
        }

        /// <summary>
        /// 1. Override .json config
        /// 2. Reload BubbleBuffer state
        /// 3. Remove obsolete UI elements
        /// </summary>
        /// <param name="receivedFrom"></param>
        /// <param name="message"></param>
        private static void OnNotifyBubbleBuffsUsed(long receivedFrom, NotifyBubbleBuffsUsed message)
        {
            var fileSystem = WOTRMultiplayer.Main.ServiceProvider.GetService<IFileSystemService>();
            fileSystem.WriteFile(BubbleBuffSpellbookController.SettingsPath, message.RawBufferState);
            GlobalBubbleBuffer.Instance.SpellbookController.CreateBuffstate();

            // every message handler runs on a background thread, but you need to be in the main thread to access Unity (UI) stuff
            WOTRMultiplayer.Main.ServiceProvider.GetService<IMainThreadAccessor>().Post(() =>
            {
                try
                {
                    var spellbook = GlobalBubbleBuffer.Instance.SpellbookController;
                    if (spellbook.Root != null)
                    {
                        spellbook.Root.CleanupAllChildren();
                        spellbook.WindowCreated = false;
                    }

                    GlobalBubbleBuffer.Instance.SpellbookController.Execute(message.Group);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error while applying buff group. Group={Group}", message.Group);
                    throw;
                }
            });
        }

        private static void InitializeNetworking()
        {
            var assembly = Assembly.GetExecutingAssembly();
            NetworkMessages.Register(assembly);
            Logger.LogInformation("Networking has been initialized. ModId={ModId}", _modId);
        }

        private static bool OnToggle(UnityModManager.ModEntry entry, bool isOn)
        {
            // you can't unsubscribe from network messages as of now
            if (!isOn)
            {
                entry.Logger.Error("Disabling on the fly is not supported yet. Please restart the game");
            }

            return true;
        }
    }
}
