using BepInEx;
using HarmonyLib;
using Shared;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace NeedleCoating
{
    [BepInPlugin("Truinto." + ModInfo.MOD_NAME, ModInfo.MOD_NAME, ModInfo.MOD_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Plugin));
            Logger.LogMessage($"Plugin loaded");
        }

        public static NailImbuementConfig? CoatingPoison;
        public static NailImbuementConfig? CoatingFire;

        [HarmonyPatch(typeof(HeroNailImbuement), nameof(HeroNailImbuement.Update))]
        [HarmonyPostfix]
        public static void HeroNailImbuements(HeroNailImbuement __instance)
        {
            if (__instance.CurrentElement != NailElements.None)
            { }
            else if (GlobalSettings.Gameplay.PoisonPouchTool.IsEquipped)
                __instance.currentImbuement = CoatingPoison ??= cloneCoating(NailElements.Poison);
            else if (ToolItemManager.IsToolEquipped("Flintstone"))
                __instance.currentImbuement = CoatingFire ??= cloneCoating(NailElements.Fire);
            return;

            NailImbuementConfig cloneCoating(NailElements element)
            {
                var result = Instantiate(__instance.nailConfigs[(int)element]);
                //Debug.Log($"COATING: {element}" +
                //    $"\n\tTag = {result.DamageTag}" +
                //    $"\n\tDamageTagTicksOverride = {result.DamageTagTicksOverride}" +
                //    $"\n\tInertHitEffect = {result.InertHitEffect}" +
                //    $"\n\tLagHits = {result.LagHits}" +
                //    $"\n\tNailDamageMultiplier = {result.NailDamageMultiplier}" +
                //    $"\n\tSlashEffect = {result.SlashEffect}" +
                //    $"\n\tStartHitEffect = {result.StartHitEffect}");
                result.Duration = float.PositiveInfinity;
                result.ExtraSlashAudio = default;
                result.HeroFlashing = default;
                result.HeroParticles = default;
                if (element is NailElements.Poison)
                    result.NailDamageMultiplier = Settings.State.PoisonDmgMult;
                else if (element is NailElements.Fire)
                    result.NailDamageMultiplier = Settings.State.FireDmgMult;
                return result;
            }
        }
    }
}
