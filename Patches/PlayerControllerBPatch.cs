using GameNetcodeStuff;
using HarmonyLib;
using System.Runtime.CompilerServices;

namespace EvilCompany.Patches
{
    internal class PlayerControllerBPatch
    {
        public static ulong actualClientID { get; private set; }
        public static bool isDead { get; private set; }
        public static bool isHost { get; private set; }

        [HarmonyPatch(typeof(PlayerControllerB), "ConnectClientToPlayerObject")]
        [HarmonyPostfix]
        private static void SetVars(PlayerControllerB __instance)
        {
            actualClientID = __instance.actualClientId;
            isDead = __instance.isPlayerDead;

            isHost = __instance.NetworkManager.IsHost;
            if (isHost)
                Plugin.isSyncedWithHost = true;
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
