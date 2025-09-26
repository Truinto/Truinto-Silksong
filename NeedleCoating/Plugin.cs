using BepInEx;
using GlobalSettings;
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

        public static NailImbuementConfig? CoatingFire;
        public static NailImbuementConfig? CoatingPoison;

        [HarmonyPatch(typeof(HeroNailImbuement), nameof(HeroNailImbuement.Update))]
        [HarmonyPostfix]
        public static void HeroNailImbuements(HeroNailImbuement __instance)
        {
            if (CoatingFire == null)
            {
                ToolItemManager.GetToolByName("Flintstone").type = ToolItemType.Blue;
                CoatingFire = cloneCoating(NailElements.Fire);
                CoatingPoison = cloneCoating(NailElements.Poison);
            }

            if (__instance.CurrentElement != NailElements.None)
            { }
            else if (ToolItemManager.IsToolEquipped("Flintstone"))
                __instance.currentImbuement = CoatingFire;
            else if (GlobalSettings.Gameplay.PoisonPouchTool.IsEquipped)
                __instance.currentImbuement = CoatingPoison;
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
                result.Duration = 600f;
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

        #region color

        // idea: patch tools to inflict burn (like poison does)
        // PoisonTintBase
        // GlobalSettings.Gameplay.PoisonPouchTool
        // DamageEnemies.PoisonDamageTicks
        // ParticleSystem.startColor
        // corpseRegular.SpawnElementalEffects(ElementalEffectType.Fire); DoBurnEffect

        // a bunch of components make a local copy of the color like ToolRing.poisonTint
        // does not affect most sprites (with partial color changes) nor weapon effects
        //[HarmonyPatch(typeof(PoisonTintBase), nameof(PoisonTintBase.UpdatePoison), typeof(bool))]
        //[HarmonyPrefix]
        public static void ChangeColor(bool isPoison, PoisonTintBase __instance)
        {
            Gameplay.Get().poisonPouchHeroTintColour = Color.red;
            Gameplay.Get().poisonPouchTintColour = Color.red;
        }

        //[HarmonyPatch(typeof(Gameplay), nameof(Gameplay.PoisonPouchTintColour), MethodType.Getter)]
        //[HarmonyPostfix]
        public static void ChangeColor2(ref Color __result)
        {
            __result = Color.red;
        }

        #endregion
    }
}
