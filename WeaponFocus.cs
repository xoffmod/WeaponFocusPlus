using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace WeaponFocusPlus
{
    public static class WeaponFocus
    {
        public static void Init(string path)
        {
            WeaponGroupsMap = new Dictionary<string, string>();

            var jDoc = JObject.Parse(System.IO.File.ReadAllText(path + "groups.json"));
            foreach (var jGroup in jDoc.Children<JProperty>())
            {
                string groupName = jGroup.Name;
                foreach (var weaponName in jGroup.Value.Values<string>())
                {
                    if (weaponName != null && weaponName != "")
                    {
                        WeaponGroupsMap.Add(weaponName, groupName);
                    }
                }
            }
        }

        public static readonly string[] WeaponGroupFormatSettings = { "Off", "Name - Group", "Group - Name" };
        private static readonly string[] WeaponGroupFormats = { null, "{0} - {1}", "{1} - {0}" };

        private static Dictionary<string, string> WeaponGroupsMap { get; set; }

        private static string GetWeaponGroup(Kingmaker.Enums.WeaponCategory weaponCategory)
        {
            return WeaponGroupsMap.TryGetValue(weaponCategory.ToString(), out var ret) ? ret : null;
        }

        private static string FormatWeaponGroupStr(Kingmaker.Enums.WeaponCategory weaponCategory, string format, string str)
        {
            if (str != null)
            {
                var wg = GetWeaponGroup(weaponCategory);
                return wg != null ? String.Format(format, str, wg) : str;
            }
            else
            {
                return null;
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(Kingmaker.Blueprints.Root.Strings.StatsStrings), "GetText")]
        [HarmonyLib.HarmonyPatch(new Type[] { typeof(Kingmaker.Enums.WeaponCategory) })]
        internal static class StatsStrings_GetText_Patch
        {
            internal static void Postfix(Kingmaker.Enums.WeaponCategory stat, ref string __result)
            {
                if (Main.Enabled && Main.Settings.FeatFormatIndex != 0)
                {
                    __result = FormatWeaponGroupStr(stat, WeaponGroupFormats[Main.Settings.FeatFormatIndex], __result);
                }
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon), "SubtypeName", HarmonyLib.MethodType.Getter)]
        internal static class BlueprintItemWeapon_SubtypeName_Getter_Patch
        {
            internal static void Postfix(Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon __instance, ref string __result)
            {
                if (Main.Enabled && Main.Settings.WeaponCategoryFormatIndex != 0)
                {
                    __result = FormatWeaponGroupStr(__instance.Category, WeaponGroupFormats[Main.Settings.WeaponCategoryFormatIndex], __result);
                }
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(Kingmaker.UI.Common.UIUtilityItem), "GetHandUse")]
        internal static class UIUtilityItem_GetHandUse_Patch
        {
            [HarmonyLib.HarmonyPriority(HarmonyLib.Priority.Last)]
            internal static void Postfix(Kingmaker.Items.ItemEntity item, ref string __result)
            {
                if (Main.Enabled && Main.Settings.WeaponCategoryFormatIndex == 0 && (item is Kingmaker.Items.ItemEntityWeapon itemEntityWeapon))
                {
                    __result = FormatWeaponGroupStr(itemEntityWeapon.Blueprint.Category, "{0} ({1})", __result);
                }
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(Kingmaker.Blueprints.Classes.Selection.FeatureParam), "Equals")]
        [HarmonyLib.HarmonyPatch(new Type[] { typeof(Kingmaker.Blueprints.Classes.Selection.FeatureParam) })]
        internal static class FeatureParam_Equals_Patch
        {
            internal static void Postfix(Kingmaker.Blueprints.Classes.Selection.FeatureParam __instance, Kingmaker.Blueprints.Classes.Selection.FeatureParam other, ref bool __result)
            {
                if (Main.Enabled && !__result && object.Equals(__instance.Blueprint, other?.Blueprint))
                {
                    if (__instance.SpellSchool.Equals(other?.SpellSchool) && __instance.StatType.Equals(other?.StatType))
                    {
                        if (__instance.WeaponCategory != null && other?.WeaponCategory != null)
                        {
                            var weaponGroup = GetWeaponGroup(__instance.WeaponCategory.Value);
                            var weaponGroup2 = GetWeaponGroup(other.WeaponCategory.Value);

                            if (weaponGroup != null && weaponGroup == weaponGroup2)
                            {
                                __result = true;
                            }
                        }
                    }
                }
            }
        }
    }
}
