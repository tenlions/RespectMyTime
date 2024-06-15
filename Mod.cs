using Kitchen;
using KitchenLib;
using KitchenLib.Event;
using KitchenMods;
using System.Reflection;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using HarmonyLib;

// Namespace should have "Kitchen" in the beginning
namespace KitchenRespectMyTime
{
    public class Mod : BaseMod, IModSystem
    {
        // GUID must be unique and is recommended to be in reverse domain name notation
        // Mod Name is displayed to the player and listed in the mods menu
        // Mod Version must follow semver notation e.g. "1.2.3"
        public const string MOD_GUID = "de.glpste.respect-my-time";
        public const string MOD_NAME = "Respect My Time";
        public const string MOD_VERSION = "0.0.1";
        public const string MOD_AUTHOR = "geilepaste";
        public const string MOD_GAMEVERSION = ">=1.1.4";
        // Game version this mod is designed for in semver
        // e.g. ">=1.1.3" current and all future
        // e.g. ">=1.1.3 <=1.2.3" for all from/until

        // Boolean constant whose value depends on whether you built with DEBUG or RELEASE mode, useful for testing
#if DEBUG
        public const bool DEBUG_MODE = true;
#else
        public const bool DEBUG_MODE = false;
#endif

        public const float XP_MODIFIER_MIN = .1f;
        public float XP_MODIFIER = 5f;
        public bool USE_PATCHER = false;

        private EntityQuery GrantsQuery;

        public static AssetBundle Bundle;

        public Mod() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        protected override void OnInitialise()
        {
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");
            LogInfo($"Multiplying all XP gains by {XP_MODIFIER}!!!");
            GrantsQuery = GetEntityQuery(typeof(CExpGrant));
        }

        private void AddGameData()
        {
            LogInfo("Attempting to register game data...");

            LogInfo("Done loading game data.");
        }

        protected override void OnUpdate()
        {
            if (!USE_PATCHER)
            {
                MultiplyExperienceGrants();
            }
        }

        protected void MultiplyExperienceGrants()
        {
            if (GrantsQuery.IsEmpty) return;

            // get all 
            CExpGrant[] grants = GrantsQuery.ToEntityArray(Allocator.TempJob);
            foreach (CExpGrant grant in grants)
            {
                if (!grant.isGranted)
                {
                    grant.Amount = grant.Amount * XP_MODIFIER;
                    LogInfo($"Modified XP of grant with ID {grant.ExpIdentifier}");
                }
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(ProgressionHelpers))]
        public static class ExperiencePatcher
        {

            [HarmonyLib.HarmonyPatch(typeof(ProgressionHelpers.AdvanceByExp))]
            [HarmonyLib.HarmonyPrefix]
            public static void AdvanceByExp(this ref SPlayerLevel current, ref int exp)
            {
                if (Mod.USE_PATCHER)
                {
                    exp = exp * Mod.XP_MODIFIER;
                }
      
            }
        }

        protected override void OnPostActivate(KitchenMods.Mod mod)
        {
            // TODO: Uncomment the following if you have an asset bundle.
            // TODO: Also, make sure to set EnableAssetBundleDeploy to 'true' in your ModName.csproj

            // LogInfo("Attempting to load asset bundle...");
            // Bundle = mod.GetPacks<AssetBundleModPack>().SelectMany(e => e.AssetBundles).First();
            // LogInfo("Done loading asset bundle.");

            // Register custom GDOs
            AddGameData();

            // Perform actions when game data is built
            Events.BuildGameDataEvent += delegate (object s, BuildGameDataEventArgs args)
            {
            };
        }
        #region Logging
        public static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }
        #endregion
    }
}
