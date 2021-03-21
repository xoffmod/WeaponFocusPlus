using System;
using UnityEngine;
using UnityModManagerNet;

namespace WeaponFocusPlus
{
    public class Settings : UnityModManager.ModSettings
    {
        public int FeatFormatIndex = 0;
        public int WeaponCategoryFormatIndex = 0;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }

#if DEBUG
    [EnableReloading]
#endif
    public class Main
    {
        public static bool Enabled = true;
        public static Settings Settings = new Settings();
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static HarmonyLib.Harmony HarmonyInstance;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;

            Settings = Settings.Load<Settings>(modEntry);
            modEntry.OnGUI = new Action<UnityModManager.ModEntry>(Main.OnGUI);
            modEntry.OnSaveGUI = new Action<UnityModManager.ModEntry>(Main.OnSaveGUI);
            modEntry.OnToggle = new Func<UnityModManager.ModEntry, bool, bool>(Main.OnToggle);
            modEntry.OnUnload = new Func<UnityModManager.ModEntry, bool>(Main.OnUnload);

            WeaponFocus.Init(modEntry.Path);

            HarmonyInstance = new HarmonyLib.Harmony(modEntry.Info.Id);
            HarmonyInstance.PatchAll(typeof(Main).Assembly);

            return true;
        }

        static bool OnUnload(UnityModManager.ModEntry modEntry)
        {
#if DEBUG
            HarmonyInstance.UnpatchAll(null);
#endif
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Show group in feat name: ", GUILayout.ExpandWidth(false));
            Settings.FeatFormatIndex = GUILayout.SelectionGrid(Settings.FeatFormatIndex, WeaponFocus.WeaponGroupFormatSettings, WeaponFocus.WeaponGroupFormatSettings.Length, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Show group in weapon category name: ", GUILayout.ExpandWidth(false));
            Settings.WeaponCategoryFormatIndex = GUILayout.SelectionGrid(Settings.WeaponCategoryFormatIndex, WeaponFocus.WeaponGroupFormatSettings, WeaponFocus.WeaponGroupFormatSettings.Length, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Settings.Save(modEntry);
        }
    }
}
