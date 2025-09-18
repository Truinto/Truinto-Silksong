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

                foreach (var setting in Settings.State.Slots)
                    if (setting.CrestID == "ALL" || setting.CrestID == item.name)
                        addSlots(ref item.slots, setting);

                fixMapping(ref item.slots);

                Debug.Log($"\t{item.name}");
                foreach (var slot in item.Slots)
                    Debug.Log($"\t\tType={slot.Type} attack={slot.AttackBinding} IsLocked={slot.IsLocked} Position={slot.Position}" +
                        $" l={slot.NavLeftIndex} d={slot.NavDownIndex} r={slot.NavRightIndex} u={slot.NavUpIndex}");
            }
            return;

            void addSlots(ref ToolCrest.SlotInfo[] slots, SettingCrest setting)
            {
                if (setting.SlotCount is <= 0 or > 20)
                    setting.SlotCount = 1;

                bool nextRedDown = false;
                int length_old = slots.Length;
                Array.Resize(ref slots, slots.Length + setting.SlotCount);

                if (setting.SlotType is SlotType.Auto)
                {
                    nextRedDown = true;
                    for (int i = 0; i < length_old; i++)
                    {
                        if (slots[i].Type is ToolItemType.Red or ToolItemType.Skill
                            && slots[i].AttackBinding is AttackToolBinding.Down)
                            nextRedDown = false;
                    }
                }

                float x = setting.PositionX;
                for (int i = length_old; i < slots.Length; i++)
                {
                    slots[i] = new()
                    {
                        IsLocked = false,
                        Position = new Vector2(x, setting.PositionY),
                        NavUpFallbackIndex = -1,
                        NavDownFallbackIndex = -1,
                        NavLeftFallbackIndex = -1,
                        NavRightFallbackIndex = -1,
                    };
                    slots[i].Type = setting.SlotType switch
                    {
                        SlotType.Auto => nextRedDown ? ToolItemType.Red : ToolItemType.Blue,
                        SlotType.Blue => ToolItemType.Blue,
                        SlotType.Yellow => ToolItemType.Yellow,
                        SlotType.RedUp => ToolItemType.Red,
                        SlotType.RedNeutral => ToolItemType.Red,
                        SlotType.RedDown => ToolItemType.Red,
                        SlotType.WhiteUp => ToolItemType.Skill,
                        SlotType.WhiteNeutral => ToolItemType.Skill,
                        SlotType.WhiteDown => ToolItemType.Skill,
                        _ => ToolItemType.Blue
                    };
                    slots[i].AttackBinding = setting.SlotType switch
                    {
                        SlotType.RedUp => AttackToolBinding.Up,
                        SlotType.RedNeutral => AttackToolBinding.Neutral,
                        SlotType.WhiteUp => AttackToolBinding.Up,
                        SlotType.WhiteNeutral => AttackToolBinding.Neutral,
                        _ => AttackToolBinding.Down
                    };
                    nextRedDown = false;
                    x += 1.5f;
                }
            }

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

            void fixMapping(ref ToolCrest.SlotInfo[] slots)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    Vector2 pos1 = slots[i].Position;

                    for (int dir1 = 1; dir1 <= 4; dir1++)
                    {
                        float minDistance = float.MaxValue;
                        int minIndex = -1;
                        for (int j = 0; j < slots.Length; j++)
                        {
                            if (i == j)
                                continue;
                            Vector2 pos2 = slots[j].Position;
                            var dir2 = pos1.GetDirectionTo(pos2);
                            if (dir1 != (int)dir2)
                                continue;
                            float dist = Vector2.Distance(pos1, pos2);
                            if (dist < minDistance)
                            {
                                minDistance = dist;
                                minIndex = j;
                            }
                        }

                        switch ((Direction)dir1)
                        {
                            case Direction.Left:
                                slots[i].NavLeftIndex = minIndex;
                                break;
                            case Direction.Right:
                                slots[i].NavRightIndex = minIndex;
                                break;
                            case Direction.Up:
                                slots[i].NavUpIndex = minIndex;
                                break;
                            case Direction.Down:
                                slots[i].NavDownIndex = minIndex;
                                break;
                        }
                    }
                }
            }
        }
    }

    public static class UtilDir
    {
        public static Direction GetDirectionTo(this Vector2 origin, Vector2 destination)
        {
            Vector2 rel = origin - destination;
            if (rel.x > 0 && rel.x >= Math.Abs(rel.y))
                return Direction.Left;

            if (rel.x < 0 && -rel.x >= Math.Abs(rel.y))
                return Direction.Right;

            if (rel.y > 0 && rel.y >= Math.Abs(rel.x))
                return Direction.Down;

            if (rel.y < 0 && -rel.y >= Math.Abs(rel.x))
                return Direction.Up;

            return Direction.None;
        }
    }

    public enum Direction
    {
        None, Left, Right, Up, Down,
    }
}
