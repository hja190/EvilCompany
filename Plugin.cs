using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using EvilCompany.Patches;
using UnityEngine.InputSystem;
using LethalCompanyInputUtils.Api;
using LC_API.Networking;

namespace EvilCompany
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        public static Plugin Instance;

        public static readonly InputActionClass inputActionClass = new InputActionClass();

        public const string signature = PluginInfo.PLUGIN_GUID;
        public const int killCost = 5;
        public const int damageCost = 2;
        public const int crouchCost = 1;
        public const int deleteItemCost = 3;
        public const int noJumpCost = 4;

        public static int evilPoints;
        public static ulong targetID;
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

            Log = Logger;
            evilPoints = 0;
            canJump = true;
            Network.RegisterAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        [NetworkMessage(signature)]
        public static void EvilCompanyHandler(ulong sender, string data)
        {
            if (!BroadcastDataSanityCheck(data))
                return;

            string[] unpackedData = data.Split();
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
            if (unpackedData.Length < 2)
            {
                Log.LogError("Missing data!");
                return false;
            }

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

            return true;
        }

        public void ResetVars()
        {
            canJump = true;
        }

        public void IncrementEvilPoints()
        {
            evilPoints += 1;
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
}
