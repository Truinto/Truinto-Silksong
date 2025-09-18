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

        /// <summary>
        /// Reduce some damage to 1. Also reduces the damage effect sound.
        /// </summary>
        [HarmonyPatch(typeof(HeroController), nameof(HeroController.TakeDamage))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspilerLessDamage(IEnumerable<CodeInstruction> code, ILGenerator generator, MethodBase original)
        {
            var tool = new TranspilerTool(code, generator, original);
            tool.Seek(typeof(DeliveryQuestItem), nameof(DeliveryQuestItem.TakeHit), [typeof(int)]);
            tool.InsertAfter(patch);
            return tool;

            static void patch(ref int damageAmount)
            {
                if (damageAmount > 1)
                    damageAmount = 1;
            }
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
