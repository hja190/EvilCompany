using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using EvilCompany.Patches;
using UnityEngine.InputSystem;
using LethalCompanyInputUtils.Api;
using LC_API.Networking;
using BepInEx.Configuration;
using Unity.Netcode;
using Unity.Collections;
using GameNetcodeStuff;
using System.Runtime.CompilerServices;

namespace EvilCompany
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        public static Plugin Instance;
        public static Config MyConfig { get; internal set; }

        public static readonly InputActionClass inputActionClass = new InputActionClass();

        public const string signature = PluginInfo.PLUGIN_GUID;

        public static int evilPoints;
        public static ulong targetID;
        public static bool isSyncedWithHost = false;
        public static int[] clientConfigVals = new int[8];

        public static bool canJump { get; private set; }
        public static bool isExecutingSomeone = false;
        public static bool isDamagingSomeone = false;
        public static bool isForcingSomeoneToCrouch = false;
        public static bool isDeletingSomeonesHeldItem = false;

        private static readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);

        private void Awake()
        {
            if (Instance != null)
                Logger.LogWarning($"More than one of the plugin {PluginInfo.PLUGIN_GUID} is detected!");
            Instance = this;

            harmony.PatchAll(typeof(Plugin));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));
            harmony.PatchAll(typeof(Config));

            Log = Logger;
            canJump = true;
            MyConfig = new Config(Config);
            Network.RegisterAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        [NetworkMessage(signature)]
        public static void EvilCompanyHandler(ulong sender, string data)
        {
            if (!BroadcastDataSanityCheck(data))
                return;

            string[] unpackedData = data.Split();
            if (unpackedData.Length < 3)
                targetID = ulong.Parse(unpackedData[1]);
            switch (unpackedData[0])
            {
                case "Kill":
                    Log.LogInfo("Received Kill broadcast!"); // DEBUG
                    isExecutingSomeone = true;
                    break;
                case "Damage":
                    Log.LogInfo("Received Damage broadcast!"); // DEBUG
                    isDamagingSomeone = true;
                    break;
                case "Crouch":
                    Log.LogInfo("Received Crouch broadcast!"); // DEBUG
                    isForcingSomeoneToCrouch = true;
                    break;
                case "Delete":
                    Log.LogInfo("Received Delete broadcast!"); // DEBUG
                    isDeletingSomeonesHeldItem = true;
                    break;
                case "NoJump":
                    Log.LogInfo("Received NoJump broadcast!"); // DEBUG
                    Log.LogInfo("Attempting to prevent jump on target: " + targetID); // DEBUG
                    if (PlayerControllerBPatch.actualClientID == targetID)
                    {
                        Log.LogInfo("Preventing this player from jumping!"); // DEBUG
                        canJump = false;
                    }
                    break;
                case "Config":
                    if (PlayerControllerBPatch.isHost || isSyncedWithHost)
                        break;

                    EvilCompany.Config.evilPointsStart.Value = int.Parse(unpackedData[1]);
                    EvilCompany.Config.evilPointsIncrement.Value = int.Parse(unpackedData[2]);
                    EvilCompany.Config.killCost.Value = int.Parse(unpackedData[3]);
                    EvilCompany.Config.damageCost.Value = int.Parse(unpackedData[4]);
                    EvilCompany.Config.damageAmount.Value = int.Parse(unpackedData[5]);
                    EvilCompany.Config.crouchCost.Value = int.Parse(unpackedData[6]);
                    EvilCompany.Config.deleteCost.Value = int.Parse(unpackedData[7]);
                    EvilCompany.Config.jumpCost.Value = int.Parse(unpackedData[8]);

                    evilPoints = EvilCompany.Config.evilPointsStart.Value;
                    Log.LogInfo("Synced config with host!"); // DEBUG
                    break;
                default:
                    Log.LogWarning("Received unknown order!");
                    break;
            }
        }

        public string PackBroadcastData(string[] dataStrings)
        {
            string packedData = string.Empty;
            foreach(string dataString in dataStrings)
            {
                packedData += (dataString + " ");
            }
            Log.LogInfo("Packed data: " + packedData.TrimEnd()); // DEBUG
            return packedData.TrimEnd();
        }

        private static bool BroadcastDataSanityCheck(string data)
        {
            string[] unpackedData = data.Split();
            if (unpackedData.Length < 2 || (unpackedData[0].Equals("Config") && unpackedData.Length < 9))
            {
                Log.LogError("Missing data!");
                return false;
            }

            if (unpackedData.Length < 3)
            {
                try
                {
                    ulong target = ulong.Parse(unpackedData[1]);
                }
                catch (Exception e)
                {
                    Log.LogError(e);
                    Log.LogError("Target string could not be parsed as ulong!");
                    return false;
                }
            }
            else
            {
                for (int i = 0; i < unpackedData.Length; i++)
                {
                    if (i == 0)
                        continue;

                    try
                    {
                        ulong value = ulong.Parse(unpackedData[i]);
                    }
                    catch (Exception e)
                    {
                        Log.LogError(e);
                        Log.LogError("Value string could not be parsed as ulong!");
                        return false;
                    }
                }
            }

            return true;
        }

        public void ResetVars()
        {
            canJump = true;
        }

        public void IncrementEvilPoints()
        {
            evilPoints += EvilCompany.Config.evilPointsIncrement.Value;
        }

        public static void ResetConfigVars()
        {
            EvilCompany.Config.evilPointsStart.Value = clientConfigVals[0];
            EvilCompany.Config.evilPointsIncrement.Value = clientConfigVals[1];
            EvilCompany.Config.killCost.Value = clientConfigVals[2];
            EvilCompany.Config.damageCost.Value = clientConfigVals[3];
            EvilCompany.Config.damageAmount.Value = clientConfigVals[4];
            EvilCompany.Config.crouchCost.Value = clientConfigVals[5];
            EvilCompany.Config.deleteCost.Value = clientConfigVals[6];
            EvilCompany.Config.jumpCost.Value = clientConfigVals[7];
        }
    }

    public class InputActionClass : LcInputActions
    {
        [InputAction("<Keyboard>/#(0)", Name = "Kill Key")]
        public InputAction KillKey { get; set; }

        [InputAction("<Keyboard>/#(9)", Name = "Damage Key")]
        public InputAction DamageKey { get; set; }

        [InputAction("<Keyboard>/#(8)", Name = "Force Crouch Key")]
        public InputAction CrouchKey { get; set; }

        [InputAction("<Keyboard>/#(7)", Name = "Delete Held Item Key")]
        public InputAction DeleteKey { get; set; }

        [InputAction("<Keyboard>/#(6)", Name = "No Jump Key")]
        public InputAction NoJumpKey { get; set; }

        [InputAction("<Keyboard>/#(5)", Name = "Debug Key")]
        public InputAction DebugKey { get; set; }
    }

    [Serializable]
    public class Config
    {
        public static ConfigEntry<int> evilPointsStart;
        public static ConfigEntry<int> evilPointsIncrement;
        public static ConfigEntry<int> killCost;
        public static ConfigEntry<int> damageCost;
        public static ConfigEntry<int> damageAmount;
        public static ConfigEntry<int> crouchCost;
        public static ConfigEntry<int> deleteCost;
        public static ConfigEntry<int> jumpCost;

        public Config(ConfigFile cfg)
        {
            evilPointsStart = cfg.Bind(
                "General",
                "Starting Evil Points",
                0,
                "How much evil points you start the game with"
            );

            evilPointsIncrement = cfg.Bind(
                "General",
                "Evil Points Increment",
                1,
                "How much evil points you gain at the end of each round"
            );

            killCost = cfg.Bind(
                "General",
                "Kill Cost",
                10,
                "How much evil points you need to kill someone on command"
            );

            damageCost = cfg.Bind(
                "General",
                "Damage Cost",
                3,
                "How much evil points you need to damage someone on command"
            );

            damageAmount = cfg.Bind(
                "General",
                "Damage Amount",
                10,
                "How much damage a person takes from the damage command"
            );

            crouchCost = cfg.Bind(
                "General",
                "Crouch Cost",
                1,
                "How much evil points you need to force someone to crouch on command"
            );

            deleteCost = cfg.Bind(
                "General",
                "Crouch Cost",
                5,
                "How much evil points you need to delete someone's held item on command"
            );

            jumpCost = cfg.Bind(
                "General",
                "No Jump Cost",
                5,
                "How much evil points you need to prevent someone from jumping on command"
            );

            Plugin.clientConfigVals[0] = evilPointsStart.Value;
            Plugin.clientConfigVals[1] = evilPointsIncrement.Value;
            Plugin.clientConfigVals[2] = killCost.Value;
            Plugin.clientConfigVals[3] = damageCost.Value;
            Plugin.clientConfigVals[4] = damageAmount.Value;
            Plugin.clientConfigVals[5] = crouchCost.Value;
            Plugin.clientConfigVals[6] = deleteCost.Value;
            Plugin.clientConfigVals[7] = jumpCost.Value;
        }
    }
}
