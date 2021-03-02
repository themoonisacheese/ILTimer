using System;
using HarmonyLib;
using UnityModManagerNet;
using System.Reflection;

namespace ILTimer
{
    public class Mod
    {
        public static UnityModManager.ModEntry mod; // despite usual coding best practises, leaving your mod fields as public is best, enabling other modders to interact with your mod.
        public static bool enabled;
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = new Harmony(modEntry.Info.Id); //register a new harmony patch
            harmony.PatchAll(Assembly.GetExecutingAssembly()); //apply all of our patches

            mod = modEntry;
            modEntry.OnToggle = onToggleEnabled; //function to call when we get enabled/diasbled in modmanager

            return true;
        }

        static bool onToggleEnabled(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        public static string TimeToReadable(float time) //function adapted from EventChallengeManager to display ms
        {
            int seconds = (int)time;
            int ms = (int)((time - seconds) * 1000);
            int num = seconds / 60;
            int num2 = seconds % 60;
            return string.Format("{0:D2}:{1:D2}.{2:D3}", num, num2, ms);
        }




        [HarmonyPatch(typeof(LevelManager))]
        [HarmonyPatch("GameOver")]
        static class Mod_Patch
        {

            static void Postfix()
            {
                if (!Mod.enabled)
                    return;

                try
                {


                    if (GameStates.currentState == GameStates.CURRENT_STATE.GAME_OVER) //not quite sure why but the game method does a similar check.
                    {
                        AccessTools.FieldRef<EventChallengeManager, EventChallengeManager.EventInstanceData> eventDataRef = AccessTools.FieldRefAccess<EventChallengeManager, EventChallengeManager.EventInstanceData>("currentEventData");
                        string readableTime = Mod.TimeToReadable(eventDataRef(EventChallengeManager.Instance()).trackTime);
                        EventNotification.AddMessage(readableTime);
                        Mod.mod.Logger.Log(readableTime);
                    }
                }
                catch (Exception e)
                {
                    Mod.mod.Logger.Error(e.ToString());
                }
            }
        }
    }
}
