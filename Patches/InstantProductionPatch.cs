using System;
using System.Collections.Generic;
using System.Reflection;
using dvize.GodModeTest;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace dvize.DadGamerMode.Patches
{

    // Patch for the production controller Update method.
    // SPT 4.0 (EFT 40087): production controller GClass1931 -> GClass2431,
    // producing item GClass1937 -> GClass2438 (progress holder Class1666 -> Class1951).
    internal class InstantUpdatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass2431), nameof(GClass2431.Update));
        }

        [PatchPrefix]
        private static bool Prefix(GClass2431 __instance)
        {
            if (dadGamerPlugin.InstantProductionEnabled.Value)
            {
                if (__instance == null || __instance.ProducingItems == null)
                {
                    return false;
                }

                // Filter itemsToComplete by removing bitcoin farm and fuel
                List<KeyValuePair<string, GClass2438>> itemsToComplete = new List<KeyValuePair<string, GClass2438>>(__instance.ProducingItems);
                itemsToComplete.RemoveAll(x => x.Key == "5d5589c1f934db045e6c5492" || x.Key == "5d5c205bd582a50d042a3c0e"); //bitcoin and fuel?

                foreach (var kvp in itemsToComplete)
                {
                    if (__instance.Schemes != null && __instance.Schemes.TryGetValue(kvp.Key, out ProductionBuildAbstractClass scheme))
                    {
                        __instance.CompleteProduction(kvp.Value, scheme);
                    }
                }

                // Allow normal update processing for Bitcoin items
                return true;
            }

            return true;
        }
    }

    // Extension method to handle CompleteProduction
    internal static class GClass2431Extensions
    {
        private static readonly FieldInfo ProgressHolderField;
        private static readonly FieldInfo ProgressField;

        static GClass2431Extensions()
        {
            // GClass2438.Class1951_0 holds the per-item progress object (Class1951);
            // Class1951.Double_1 is the progress value (exposed publicly as Class1951.Progress).
            ProgressHolderField = AccessTools.Field(typeof(GClass2438), "Class1951_0");
            ProgressField = AccessTools.Field(typeof(GClass2438.Class1951), "Double_1");

            // AccessTools.Field returns null (it does not throw) when an obfuscated name no longer
            // exists. Surface that clearly here instead of letting it become an opaque NRE later.
            if (ProgressHolderField == null || ProgressField == null)
            {
                dadGamerPlugin.Logger?.LogError(
                    "InstantProduction: could not resolve progress fields (GClass2438.Class1951_0 / Class1951.Double_1). " +
                    "The obfuscated names likely changed for this game build; instant production will not work.");
            }
        }

        public static void CompleteProduction(this GClass2431 __instance, GClass2438 producingItem, ProductionBuildAbstractClass scheme)
        {
            if (__instance == null || producingItem == null || scheme == null)
            {
                dadGamerPlugin.Logger.LogError("CompleteProduction: __instance, producingItem, or scheme is null.");
                return;
            }

            if (ProgressHolderField == null || ProgressField == null)
            {
                dadGamerPlugin.Logger.LogError("CompleteProduction: progress field reflection unavailable; skipping.");
                return;
            }

            try
            {
                var progressHolder = ProgressHolderField.GetValue(producingItem);
                if (progressHolder == null)
                {
                    dadGamerPlugin.Logger.LogError("CompleteProduction: progressHolder is null.");
                    return;
                }

                // Set the Progress field to 1.0 (complete)
                ProgressField.SetValue(progressHolder, 1.0);

                Item item = __instance.CreateCompleteItem(scheme);
                if (item == null)
                {
                    dadGamerPlugin.Logger.LogError("CompleteProduction: item is null.");
                    return;
                }

                // Check if the SchemeId exists in the dictionary before calling BeforeProductionComplete
                if (__instance.ProducingItems != null && __instance.ProducingItems.ContainsKey(producingItem.SchemeId))
                {
                    __instance.BeforeProductionComplete(producingItem.SchemeId);
                    __instance.CompleteItemsStorage.AddItem(scheme._id, item);
                    __instance.ProducingItems.Remove(producingItem.SchemeId);
                    __instance.SetDetailsData();
                }
                else
                {
                    dadGamerPlugin.Logger.LogError($"SchemeId {producingItem.SchemeId} not found in ProducingItems.");
                }
            }
            catch (Exception ex)
            {
                dadGamerPlugin.Logger.LogError($"Unexpected error during CompleteProduction: {ex.Message}");
            }
        }
    }
}
