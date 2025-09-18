using BepInEx;
using HarmonyLib;
using Shared;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace ExtraCrestSlots
{
    [BepInPlugin("Truinto." + ModInfo.MOD_NAME, ModInfo.MOD_NAME, ModInfo.MOD_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Plugin));
            Logger.LogMessage($"Plugin loaded");
        }

        [HarmonyPatch(typeof(ToolItemManager), nameof(ToolItemManager.Awake))]
        [HarmonyPostfix]
        public static void AddToolSlots(ToolItemManager __instance)
        {
            Debug.Log($"CRESTS:");
            foreach (var item in __instance.crestList)
            {
                if (item == null || item.slots == null)
                    continue;
                if (item.slots.Length is 0 or > 9)
                    continue;

                Debug.Log($"\t{item.name}");
                foreach (var slot in item.Slots)
                    Debug.Log($"\t\tType={slot.Type} attack={slot.AttackBinding} IsLocked={slot.IsLocked} Position={slot.Position}");

                addBottomSlots(ref item.slots);
            }
            return;

            void addBottomSlots(ref ToolCrest.SlotInfo[] slots)
            {
                bool toolDown = false;
                int length_old = slots.Length;
                Array.Resize(ref slots, slots.Length + 6);

                // NavIndex points to the slot index you get when pressing that button
                // change all down -1 to first custom slot
                for (int i = 0; i < length_old; i++)
                {
                    if (slots[i].NavDownIndex < 0)
                        slots[i].NavDownIndex = length_old;
                    if (slots[i].Type is ToolItemType.Red or ToolItemType.Skill
                        && slots[i].AttackBinding is AttackToolBinding.Down)
                        toolDown = true;
                }

                // use left and right to scroll through custom slots
                float x = -4.5f;
                for (int i = length_old; i < slots.Length; i++)
                {
                    slots[i] = new()
                    {
                        Type = !toolDown ? ToolItemType.Red : ToolItemType.Blue,
                        AttackBinding = AttackToolBinding.Down,
                        IsLocked = false,
                        Position = new Vector2(x, -3.5f),
                        NavUpFallbackIndex = -1,
                        NavDownFallbackIndex = -1,
                        NavLeftFallbackIndex = -1,
                        NavRightFallbackIndex = -1,
                        NavUpIndex = -1,
                        NavDownIndex = -1,
                        NavLeftIndex = i == length_old ? slots.Length - 1 : (i - 1),
                        NavRightIndex = i >= slots.Length ? -1 : (i + 1),
                    };
                    toolDown = true;
                    x += 1.5f;
                }
            }
        }
    }
}
