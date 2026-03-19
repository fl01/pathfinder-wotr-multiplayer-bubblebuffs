using System.Linq;
using System.Reflection;
using BubbleBuffs.Multiplayer.Networking.Messages;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnityModManagerNet;
using WOTRMultiplayer.Networking.Abstractions;

namespace BubbleBuffs.Multiplayer
{
    public class Main
    {
        public static ILogger<Main> Logger { get; private set; }

        public static bool Load(UnityModManager.ModEntry entry)
        {
            Logger = WOTRMultiplayer.Main.GetLogger<Main>();

            Logger.LogInformation("Mod is ready to use WOTRMultiplayer types. ModName={ModName}", entry.Info.Id);
            InitializeNetworkMessagesMetadata();

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

        private static void OnNotifyBubbleBuffsUsed(long receivedFrom, NotifyBubbleBuffsUsed message)
        {
            var bufferState = JsonConvert.DeserializeObject<SavedBufferState>(message.RawBufferState);
            GlobalBubbleBuffer.Instance.SpellbookController.state = new BufferState(bufferState);
            GlobalBubbleBuffer.Execute(message.Group);
        }

        /// <summary>
        /// Adds dynamic logging for custom network messages
        /// </summary>
        private static void InitializeNetworkMessagesMetadata()
        {
            var loggableObjects = typeof(NotifyBubbleBuffsUsed).Assembly.GetTypes()
                .Where(x => x.GetCustomAttribute<BeetleX.Packets.MessageTypeAttribute>() != null)
                .ToList();

            WOTRMultiplayer.Logging.Object.ObjectLoggingMetadata.Initialize(loggableObjects);
        }

        private static bool OnToggle(UnityModManager.ModEntry entry, bool isOn)
        {
            if (!isOn)
            {
                entry.Logger.Error("Disabling on the fly is not supported yet. Please restart the game");
            }

            return true;
        }
    }
}
