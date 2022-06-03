﻿using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;

namespace SMBBMAssetBundleLoader
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(SMBBMLeaderboardDisabler.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BasePlugin
    {
        /// <summary>
        /// For logging convienence
        /// </summary>
        internal static new ManualLogSource Log;

        /// <summary>
        /// The directory that data for this Plugin is expected
        /// </summary>
        public static readonly string dataDir = $"{Paths.GameRootPath}{Path.DirectorySeparatorChar}UserData{Path.DirectorySeparatorChar}AssetBundles";

        /// <summary>
        /// A Key/Value map of AssetBundles to patch
        /// Key: Asset Bundle Name
        /// Value: Patch to Asset Bundle to patch it with
        /// </summary>
        internal static Dictionary<string, string> assetBundles;


        public override void Load()
        {
            Plugin.Log = base.Log;

            // Find and load all the AssetBundle configuration JSON files
            assetBundles = new Dictionary<string, string>();
            foreach (var file in Directory.EnumerateFiles(dataDir, "*.json", SearchOption.TopDirectoryOnly))
            {
                LoadJSONFile(file);
            }
            Log.LogDebug("Done loading json files");
            
            // Log the final AssetBundle key/value set for debugging use
            string dict = "";
            foreach (KeyValuePair<string, string> assetBundle in assetBundles)
            {
                dict += $"\"{assetBundle.Key}\", \"{assetBundle.Value}\"\n";
            }
            Log.LogDebug($"Final Asset Bundle List JSON is {{{dict}}}");

            // If we are patching something, make sure to disable the leaderboards
            // Will probably need something more complicated in the future (i.e. should UI patches disable it?)
            if(assetBundles.Count > 0)
            {
                SMBBMLeaderboardDisabler.Plugin.DisableLeaderboards(PluginInfo.PLUGIN_NAME);
            }

            // Harmony Patching
            var harmony = new Harmony("com.bobjrsenior.AssetBundlePatch");
            harmony.PatchAll();

            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        /// <summary>
        /// Loads the AssetBundle settings from a given JSON file
        /// </summary>
        /// <param name="filepath">filepath of the JSON file to load</param>
        private void LoadJSONFile(string filepath)
        {
            Log.LogDebug($"Loading file {filepath}");

            using (StreamReader file = File.OpenText(filepath))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                // Serializethe JSON file into a C# one
                JObject obj = JToken.ReadFrom(reader) as JObject;
                AssetBundleSettings assetBundleSettings = obj.ToObject<AssetBundleSettings>();
                if (assetBundleSettings != null)
                {
                    MergeMusic(assetBundleSettings);
                    Log.LogDebug($"Loaded: {assetBundleSettings}");
                }
                else
                {
                    Log.LogDebug($"Nothing to load");
                }
            }
        }

        /// <summary>
        /// Merges an AssetBundleSettings object with the current AssetBundle Key/Value Patch mapping
        /// </summary>
        /// <param name="assetBundleSettings">AssetBundleSettings to merge</param>
        internal void MergeMusic(AssetBundleSettings assetBundleSettings)
        {
            foreach (KeyValuePair<string, string> assetBundle in assetBundleSettings.asset_bundles)
            {
                assetBundles[assetBundle.Key] = $"{dataDir}{Path.DirectorySeparatorChar}{assetBundle.Value}";
            }
        }
    }
}
