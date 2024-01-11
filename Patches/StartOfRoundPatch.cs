using BepInEx.Configuration;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
            if (Plugin.inputActionClass.KillKey.WasPressedThisFrame() && Plugin.evilPoints >= Config.killCost.Value)
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
                Plugin.evilPoints -= Config.killCost.Value;
            }
            if (Plugin.inputActionClass.DamageKey.WasPressedThisFrame() && Plugin.evilPoints >= Config.damageCost.Value)
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
                Plugin.evilPoints -= Config.damageCost.Value;
            }
            if (Plugin.inputActionClass.CrouchKey.WasPressedThisFrame() && Plugin.evilPoints >= Config.crouchCost.Value)
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
                Plugin.evilPoints -= Config.crouchCost.Value;
            }
            if (Plugin.inputActionClass.DeleteKey.WasPressedThisFrame() && Plugin.evilPoints >= Config.deleteCost.Value)
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
                Plugin.evilPoints -= Config.deleteCost.Value;
            }
            if (Plugin.inputActionClass.NoJumpKey.WasPressedThisFrame() && Plugin.evilPoints >= Config.jumpCost.Value)
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
                Plugin.evilPoints -= Config.jumpCost.Value;
            }
            if (Plugin.inputActionClass.DebugKey.WasPressedThisFrame()) // DEBUG
            {
                Plugin.Log.LogInfo("Current Evil Points: " + Plugin.evilPoints);
                Plugin.Log.LogInfo("Points needed to kill: " + Config.killCost.Value);
                Plugin.Log.LogInfo("Points needed to damage: " + Config.damageCost.Value);
                Plugin.Log.LogInfo("Points needed to force crouch/uncrouch: " + Config.crouchCost.Value);
                Plugin.Log.LogInfo("Points needed to delete held item: " + Config.deleteCost.Value);
                Plugin.Log.LogInfo("Points needed to prevent jumping: " + Config.jumpCost.Value);
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

                    playerController.DamagePlayer(Config.damageAmount.Value);
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

        [HarmonyPatch(typeof(StartOfRound), "OnEnable")]
        [HarmonyPostfix]
        private static void SetEvilPoints()
        {
            Plugin.evilPoints = Config.evilPointsStart.Value;
        }

        [HarmonyPatch(typeof(StartOfRound), "StartGame")]
        [HarmonyPostfix]
        private static void SyncConfig() // https://lethal.wiki/dev/intermediate/custom-config-syncing did not work...
        {
            if (!PlayerControllerBPatch.isHost)
                return;

            string[] data = new string[] {
                "Config",
                Config.evilPointsStart.Value.ToString(),
                Config.evilPointsIncrement.Value.ToString(),
                Config.killCost.Value.ToString(),
                Config.damageCost.Value.ToString(),
                Config.damageAmount.Value.ToString(),
                Config.crouchCost.Value.ToString(),
                Config.deleteCost.Value.ToString(),
                Config.jumpCost.Value.ToString()
            };

            Plugin.Log.LogInfo("Syncing config with clients!");
            LC_API.Networking.Network.Broadcast(Plugin.signature, Plugin.Instance.PackBroadcastData(data));
        }

        [HarmonyPatch(typeof(StartOfRound), "OnDisable")]
        [HarmonyPostfix]
        private static void ResetVars()
        {
            Plugin.isSyncedWithHost = false;
            Plugin.ResetConfigVars();
        }

        private static string GetTargetedPlayer(StartOfRound __instance)
        {
            if (!PlayerControllerBPatch.isDead)
                return string.Empty;

            return __instance.localPlayerController.spectatedPlayerScript.actualClientId.ToString();
        }
    }
}
