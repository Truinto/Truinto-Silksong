using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace EasyFleaFestival
{
    [BepInPlugin("Truinto." + ModInfo.MOD_NAME, ModInfo.MOD_NAME, ModInfo.MOD_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Plugin));
            Logger.LogMessage($"Plugin loaded");
        }

        private static bool FleafestivalCheat;
        [HarmonyPatch(typeof(EventRegister), nameof(EventRegister.SendEvent), typeof(int), typeof(GameObject))]
        [HarmonyPrefix]
        public static bool FleaFestivalSkipFailureEvent(int eventNameHash)
        {
            if (eventNameHash == 1915973471) //DID TAUNT
                FleafestivalCheat = !FleafestivalCheat;
            else if (eventNameHash == 1679357604) //ENTERING SCENE
                FleafestivalCheat = false;

            else if (eventNameHash == -1320927282) //FLEA FAIL (bounce)
                return false;
            else if (eventNameHash == -690034751 && FleafestivalCheat) //GAME END (all)
                return false;
            // - triggers if jumping into water during jump game: SendEvent GAME END MANUAL (hash) 906724835
            // - triggers if scoring a point (any game): SendEvent SCORE (hash) -400758112
            return true;
        }
    }
}
