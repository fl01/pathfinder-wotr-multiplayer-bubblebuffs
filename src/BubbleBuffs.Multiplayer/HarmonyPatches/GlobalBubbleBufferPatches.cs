using System;
using BubbleBuffs.Multiplayer.Networking.Messages;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BubbleBuffs.Multiplayer.HarmonyPatches
{
    [HarmonyPatch]
    public class GlobalBubbleBufferPatches
    {
        [HarmonyPatch(typeof(GlobalBubbleBuffer), nameof(GlobalBubbleBuffer.Execute))]
        [HarmonyPrefix]
        public static void GlobalBubbleBuffer_Execute_Prefix(BuffGroup group)
        {
            if (!WOTRMultiplayer.Main.Multiplayer.IsActive)
            {
                return;
            }

            try
            {
                var savedState = GlobalBubbleBuffer.Instance.SpellbookController.state.SavedState;
                var state = JsonConvert.SerializeObject(savedState);

                var message = new NotifyBubbleBuffsUsed
                {
                    Group = group,
                    RawBufferState = state,
                };

                Main.Send(message);
            }
            catch (Exception ex)
            {
                Main.Logger.LogError(ex, "Error while sending buff group execution. Group={Group}", group);
                throw;
            }
        }
    }
}
