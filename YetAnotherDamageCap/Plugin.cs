using BepInEx;
using HarmonyLib;
using Shared;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace AnotherDamageCap
{
    [BepInPlugin("Truinto." + ModInfo.MOD_NAME, ModInfo.MOD_NAME, ModInfo.MOD_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Plugin));
            Logger.LogMessage($"Plugin loaded");
        }

        /// <summary>
        /// Reduces the damage effect sound.
        /// </summary>
        [HarmonyPatch(typeof(HeroController), nameof(HeroController.TakeDamage))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> LessDamage1(IEnumerable<CodeInstruction> code, ILGenerator generator, MethodBase original)
        {
            var tool = new TranspilerTool(code, generator, original);
            tool.Seek(typeof(PlayerData), nameof(PlayerData.TakeHealth));
            tool.InsertBefore(patch);
            return tool;

            static void patch(ref int damageAmount)
            {
                if (damageAmount is > 1 and < 999)
                    damageAmount = 1;
            }
        }

        public static float TimeLastDamage;
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.TakeHealth))]
        [HarmonyPrefix]
        public static void LessDamage2(ref int amount)
        {
            if (amount is > 0 and < 999)
            {
                float now;
                if ((now = Time.time) - TimeLastDamage < 0.75f)
                {
                    amount = 0;
                }
                else
                {
                    amount = 1;
                    TimeLastDamage = now;
                }
            }
        }
    }
}
