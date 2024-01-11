using GameNetcodeStuff;
using HarmonyLib;

namespace EvilCompany.Patches
{
    internal class PlayerControllerBPatch
    {
        public static ulong actualClientID { get; private set; }
        public static bool isDead { get; private set; }

        [HarmonyPatch(typeof(PlayerControllerB), "ConnectClientToPlayerObject")]
        [HarmonyPostfix]
        private static void SetActualClientID(PlayerControllerB __instance)
        {
            actualClientID = __instance.actualClientId;
            isDead = __instance.isPlayerDead;
        }

        [HarmonyPatch(typeof(PlayerControllerB), "KillPlayer")]
        [HarmonyPostfix]
        private static void SetDeathBool(PlayerControllerB __instance)
        {
            isDead = __instance.isPlayerDead;
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Jump_performed")]
        [HarmonyPrefix]
        private static bool AffectJump()
        {
            return Plugin.canJump;
        }
    }
}
