using System;
using System.Linq;
using BubbleBuffs.Multiplayer.Networking.Messages;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Persistence;
using Newtonsoft.Json;

namespace BubbleBuffs.Multiplayer.HarmonyPatches
{
    [HarmonyPatch]
    public class AreaLoadedPatches
    {
        [HarmonyPatch(typeof(Game), nameof(Game.LoadArea), [typeof(BlueprintArea), typeof(BlueprintAreaEnterPoint), typeof(AutoSaveMode), typeof(bool), typeof(SaveInfo), typeof(Action)])]
        [HarmonyPostfix]
        public static void Game_LoadArea_Postfix()
        {
            if (!WOTRMultiplayer.Main.Multiplayer.IsActive)
            {
                return;
            }

            LoadingProcess.Instance.StartLoadingProcess(() =>
            {
                var bubble = GlobalBubbleBuffer.Instance;
                var buttons = bubble.Buttons.Where(b => b != null).ToList();
                // buttons are recreated after each area load, so no need to care about duplicate subscriptions
                for (int i = 0; i < bubble.Buttons.Count; i++)
                {
                    var button = bubble.Buttons[i];
                    var buttonType = (BuffGroup)i;
                    button.m_OnLeftClick.AddListener(() => OnBuffUsed(buttonType));
                }
            }, LoadingProcessTag.Load);
        }

        private static void OnBuffUsed(BuffGroup buffGroup)
        {
            var savedState = GlobalBubbleBuffer.Instance.SpellbookController.state.SavedState;
            var state = JsonConvert.SerializeObject(savedState);

            var message = new NotifyBubbleBuffsUsed
            {
                Group = buffGroup,
                RawBufferState = state,
            };

            Main.Send(message);
        }
    }
}
