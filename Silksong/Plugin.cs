using BepInEx;
using HarmonyLib;
using Shared;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace TruintoSilksong
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
                //result.NailDamageMultiplier = (result.NailDamageMultiplier + 1f) / 2;
                return result;
            }
        }

        [HarmonyPatch(typeof(HeroController), nameof(HeroController.SilkGain), typeof(HitInstance))]
        [HarmonyPostfix]
        public static void MultiHitSilk(HitInstance hitInstance, HeroController __instance)
        {
            //Debug.Log($"HIT: silk={hitInstance.SilkGeneration} attack={hitInstance.AttackType} hit={hitInstance.HitEffectsType}");
            if (hitInstance.SilkGeneration is not HitSilkGeneration.Full
                && hitInstance.AttackType is AttackTypes.Nail
                && hitInstance.HitEffectsType is EnemyHitEffectsProfile.EffectsTypes.Full)
            {
                if (hitInstance.IsFirstHit)
                    __instance.AddSilk(1, false);
                else
                    __instance.AddSilkParts(1);
            }
        }

        private static ToolItem? Magnet;
        [HarmonyPatch(typeof(CurrencyObjectBase), nameof(CurrencyObjectBase.MagnetToolIsEquipped))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Low)]
        public static bool MagnetShards(CurrencyObjectBase __instance, ref bool __result)
        {
            if (__instance is ShellShard && (Magnet ??= ToolItemManager.GetToolByName("Rosary Magnet")) && Magnet.IsEquipped)
            {
                __result = true;
                return false;
            }
            return true;
        }

        //private static ToolItem? MagnetTool;
        //private static ToolItem? MagnetBuff;
        //private static GameObject? MagnetEffect;
        //[HarmonyPatch(typeof(CurrencyObjectBase), nameof(CurrencyObjectBase.Awake))]
        //[HarmonyPrefix]
        //[HarmonyPriority(Priority.Low)]
        //public static void MagnetShards(CurrencyObjectBase __instance)
        //{
        //    if (__instance is GeoControl)
        //    {
        //        MagnetTool = __instance.magnetTool;
        //        MagnetBuff = __instance.magnetBuffTool;
        //        MagnetEffect = __instance.magnetEffect;
        //    }
        //    else if (__instance is ShellShard)
        //    {
        //        __instance.magnetTool = MagnetTool;
        //        __instance.magnetBuffTool = MagnetBuff;
        //        __instance.magnetEffect = MagnetEffect;
        //    }
        //}

        //[HarmonyPatch(typeof(tk2dSpriteAnimation), nameof(tk2dSpriteAnimation.ValidateLookup))]
        //[HarmonyPrefix]
        public static void ModifyCrestAttacks(tk2dSpriteAnimation __instance)
        {
            //HeroController.instance.ResetAllCrestState();
            //var crest = ToolItemManager.GetCrestByName(PlayerData.instance.CurrentCrestID);
            //var anim = crest.HeroConfig.heroAnimOverrideLib;
            // NailAttackBase?
            // DamageEnemies.DoDamage?

            if (__instance.isValid || __instance.clips == null)
                return;

            Debug.Log($"ANIM: {__instance.name}" +
                $"\n\t{__instance.clips.Join(f => $"{f.name}")}");
        }
    }
}

/*
 * All tools: Silk Spear (Seidenspeer), Thread Sphere (Fadensturm), Parry (Kreuzstich), Silk Charge (Scharfpfeil), Silk Bomb (Runenzorn), Silk Boss Needle (Bleiche Nägel), Straight Pin (Gerade Nadel), Tri Pin (Dreifach-Nadel), Sting Shard (Stachelsplitter), Tack (Reißzwecken), Harpoon (Langnadel), Curve Claws (Kurvenkralle), Curve Claws Upgraded (Kurvensichel), Shakra Ring (Wurfring), Pimpilo (Pimpillo), Conch Drill (Muschelschneider), WebShot Forge (Seidenschuss), WebShot Architect (Seidenschuss), WebShot Weaver (Seidenschuss), Screw Attack (Forschungsbohrer), Cogwork Saw (Räderwerkrad), Cogwork Flier (Räderfliege), Rosary Cannon (Rosenkranzperlen-Kanone), Lightning Rod (Voltbehälter), Flintstone (Feuersteintafel), Silk Snare (Schlingensetzer), Flea Brew (Flohgebräu), Lifeblood Syringe (Plasmium-Fläschchen), Extractor (Nähnadel-Ampulle), Mosscreep Tool 1 (Druidenauge), Mosscreep Tool 2 (Druidenaugen), Lava Charm (Magmaglocke), Bell Bind (Abwehrglocke), Poison Pouch (Pollip-Tasche), Fractured Mask (Gebrochene Maske), Multibind (Multibinder), White Ring (Weblicht), Brolly Spike (Sägezahn-Stirnreif), Quickbind (Injektorenband), Spool Extender (Rollen-Verlängerung), Reserve Bind (Reservebinde), Dazzle Bind (Krallenspiegel), Dazzle Bind Upgraded (Krallenspiegel), Revenge Crystal (Erinnerungskristall), Thief Claw (Spitzelhacke), Zap Imbuement (Voltfaser), Quick Sling (Schnellschleuder), Maggot Charm (Kranz der Reinheit), Longneedle (Langkralle), Wisp Lantern (Büschelfeuer-Laterne), Flea Charm (Flohlia-Ei), Pinstress Tool (Nadelabzeichen), Compass (Kompass), Bone Necklace (Splitteranhänger), Rosary Magnet (Magnetit-Brosche), Weighted Anklet (Beschwerter Gürtel), Barbed Wire (Stachelarmreif), Dead Mans Purse (Handtasche eines toten Käfers), Shell Satchel (Schalenbeutel), Magnetite Dice (Magnetit-Würfel), Scuttlebrace (Krabbelklammer), Wallcling (Aufsteigergriff), Musician Charm (Spinnenfäden), Sprintmaster (Seidentempo-Fußketten), Thief Charm (Diebeszeichen)
 */
