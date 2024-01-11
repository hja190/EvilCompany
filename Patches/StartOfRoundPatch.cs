using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace EvilCompany.Patches
{
    internal class StartOfRoundPatch
    {
        [HarmonyPatch(typeof(StartOfRound), "EndOfGame")]
        [HarmonyPostfix]
        private static void ResetCommands()
        {
            Plugin.Instance.ResetVars();
            Plugin.Instance.IncrementEvilPoints();
        }

        [HarmonyPatch(typeof(StartOfRound), "Update")]
        [HarmonyPostfix]
        private static void Broadcast(StartOfRound __instance)
        {
            if (Plugin.inputActionClass.KillKey.WasPressedThisFrame() && Plugin.evilPoints >= Plugin.killCost)
            {
                Plugin.Log.LogInfo("Broadcasting Kill message!"); // DEBUG

                string targetID = GetTargetedPlayer(__instance);
                if (targetID.Equals(string.Empty))
                {
                    Plugin.Log.LogInfo("Player is not dead. Ignoring broadcast request...");
                    return;
                }

                string[] data = new string[] {
                    "Kill",
                    targetID
                };
                LC_API.Networking.Network.Broadcast(Plugin.signature, Plugin.Instance.PackBroadcastData(data));
                Plugin.evilPoints -= Plugin.killCost;
            }
            if (Plugin.inputActionClass.DamageKey.WasPressedThisFrame() && Plugin.evilPoints >= Plugin.damageCost)
            {
                Plugin.Log.LogInfo("Broadcasting Damage message!"); // DEBUG

                string targetID = GetTargetedPlayer(__instance);
                if (targetID.Equals(string.Empty))
                {
                    Plugin.Log.LogInfo("Player is not dead. Ignoring broadcast request...");
                    return;
                }

                string[] data = new string[] {
                    "Damage",
                    targetID
                };
                LC_API.Networking.Network.Broadcast(Plugin.signature, Plugin.Instance.PackBroadcastData(data));
                Plugin.evilPoints -= Plugin.damageCost;
            }
            if (Plugin.inputActionClass.CrouchKey.WasPressedThisFrame() && Plugin.evilPoints >= Plugin.crouchCost)
            {
                Plugin.Log.LogInfo("Broadcasting Crouch message!"); // DEBUG

                string targetID = GetTargetedPlayer(__instance);
                if (targetID.Equals(string.Empty))
                {
                    Plugin.Log.LogInfo("Player is not dead. Ignoring broadcast request...");
                    return;
                }

                string[] data = new string[] {
                    "Crouch",
                    targetID
                };
                LC_API.Networking.Network.Broadcast(Plugin.signature, Plugin.Instance.PackBroadcastData(data));
                Plugin.evilPoints -= Plugin.crouchCost;
            }
            if (Plugin.inputActionClass.DeleteKey.WasPressedThisFrame() && Plugin.evilPoints >= Plugin.deleteItemCost)
            {
                Plugin.Log.LogInfo("Broadcasting Delete message!"); // DEBUG

                string targetID = GetTargetedPlayer(__instance);
                if (targetID.Equals(string.Empty))
                {
                    Plugin.Log.LogInfo("Player is not dead. Ignoring broadcast request...");
                    return;
                }

                string[] data = new string[] {
                    "Delete",
                    targetID
                };
                LC_API.Networking.Network.Broadcast(Plugin.signature, Plugin.Instance.PackBroadcastData(data));
                Plugin.evilPoints -= Plugin.deleteItemCost;
            }
            if (Plugin.inputActionClass.NoJumpKey.WasPressedThisFrame() && Plugin.evilPoints >= Plugin.noJumpCost)
            {
                Plugin.Log.LogInfo("Broadcasting NoJump message!"); // DEBUG

                string targetID = GetTargetedPlayer(__instance);
                if (targetID.Equals(string.Empty))
                {
                    Plugin.Log.LogInfo("Player is not dead. Ignoring broadcast request...");
                    return;
                }

                string[] data = new string[] {
                    "NoJump",
                    targetID
                };
                LC_API.Networking.Network.Broadcast(Plugin.signature, Plugin.Instance.PackBroadcastData(data));
                Plugin.evilPoints -= Plugin.noJumpCost;
            }
            if (Plugin.inputActionClass.DebugKey.WasPressedThisFrame()) // DEBUG
            {
                Plugin.Log.LogInfo("Current Evil Points: " + Plugin.evilPoints);
            }
        }

        [HarmonyPatch(typeof(StartOfRound), "Update")]
        [HarmonyPostfix]
        private static void ActOnBroadcastOrder(StartOfRound __instance)
        {
            if (Plugin.isExecutingSomeone)
            {
                Plugin.Log.LogInfo("Attempting to execute target: " + Plugin.targetID); // DEBUG

                foreach (PlayerControllerB playerController in __instance.allPlayerScripts)
                {
                    if (!playerController.disconnectedMidGame && !playerController.isPlayerDead && !playerController.isPlayerControlled)
                    {
                        Plugin.Log.LogInfo("Player " + playerController.actualClientId + " is not controlled by a player. Skipping...");
                        continue;
                    }
                    if (playerController.disconnectedMidGame || playerController.isPlayerDead)
                    {
                        Plugin.Log.LogInfo("Player " + playerController.actualClientId + " is not in control of a body. Skipping...");
                        continue;
                    }
                    if (!playerController.isPlayerDead && playerController.actualClientId != Plugin.targetID)
                    {
                        Plugin.Log.LogInfo("Player " + playerController.actualClientId + " is not targetted player. Skipping...");
                        continue;
                    }

                    playerController.KillPlayer(Vector3.zero);
                }
                Plugin.isExecutingSomeone = false;
                Plugin.targetID = 999;
            }
            if (Plugin.isDamagingSomeone)
            {
                Plugin.Log.LogInfo("Attempting to damage target: " + Plugin.targetID); // DEBUG

                foreach (PlayerControllerB playerController in __instance.allPlayerScripts)
                {
                    if (!playerController.disconnectedMidGame && !playerController.isPlayerDead && !playerController.isPlayerControlled)
                    {
                        Plugin.Log.LogInfo("Player " + playerController.actualClientId + " is not controlled by a player. Skipping...");
                        continue;
                    }
                    if (playerController.disconnectedMidGame || playerController.isPlayerDead)
                    {
                        Plugin.Log.LogInfo("Player " + playerController.actualClientId + " is not in control of a body. Skipping...");
                        continue;
                    }
                    if (!playerController.isPlayerDead && playerController.actualClientId != Plugin.targetID)
                    {
                        Plugin.Log.LogInfo("Player " + playerController.actualClientId + " is not targetted player. Skipping...");
                        continue;
                    }

                    playerController.DamagePlayer(10);
                }
                Plugin.isDamagingSomeone = false;
                Plugin.targetID = 999;
            }
            if (Plugin.isForcingSomeoneToCrouch)
            {
                Plugin.Log.LogInfo("Attempting to force crouch target: " + Plugin.targetID); // DEBUG

                foreach (PlayerControllerB playerController in __instance.allPlayerScripts)
                {
                    if (!playerController.disconnectedMidGame && !playerController.isPlayerDead && !playerController.isPlayerControlled)
                    {
                        Plugin.Log.LogInfo("Player " + playerController.actualClientId + " is not controlled by a player. Skipping...");
                        continue;
                    }
                    if (playerController.disconnectedMidGame || playerController.isPlayerDead)
                    {
                        Plugin.Log.LogInfo("Player " + playerController.actualClientId + " is not in control of a body. Skipping...");
                        continue;
                    }
                    if (!playerController.isPlayerDead && playerController.actualClientId != Plugin.targetID)
                    {
                        Plugin.Log.LogInfo("Player " + playerController.actualClientId + " is not targetted player. Skipping...");
                        continue;
                    }

                    playerController.Crouch(!__instance.localPlayerController.isCrouching);
                }
                Plugin.isForcingSomeoneToCrouch = false;
                Plugin.targetID = 999;
            }
            if (Plugin.isDeletingSomeonesHeldItem)
            {
                Plugin.Log.LogInfo("Attempting to delete held item of target: " + Plugin.targetID); // DEBUG

                foreach (PlayerControllerB playerController in __instance.allPlayerScripts)
                {
                    if (!playerController.disconnectedMidGame && !playerController.isPlayerDead && !playerController.isPlayerControlled)
                    {
                        Plugin.Log.LogInfo("Player " + playerController.actualClientId + " is not controlled by a player. Skipping...");
                        continue;
                    }
                    if (playerController.disconnectedMidGame || playerController.isPlayerDead)
                    {
                        Plugin.Log.LogInfo("Player " + playerController.actualClientId + " is not in control of a body. Skipping...");
                        continue;
                    }
                    if (!playerController.isPlayerDead && playerController.actualClientId != Plugin.targetID)
                    {
                        Plugin.Log.LogInfo("Player " + playerController.actualClientId + " is not targetted player. Skipping...");
                        continue;
                    }

                    playerController.DespawnHeldObject();
                }
                Plugin.isDeletingSomeonesHeldItem = false;
                Plugin.targetID = 999;
            }
        }

        private static string GetTargetedPlayer(StartOfRound __instance)
        {
            if (!PlayerControllerBPatch.isDead)
                return string.Empty;

            return __instance.localPlayerController.spectatedPlayerScript.actualClientId.ToString();
        }
    }
}
