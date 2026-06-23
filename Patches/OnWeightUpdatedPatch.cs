using System.Reflection;
using SPT.Reflection.Patching;
using dvize.GodModeTest;
using EFT.InventoryLogic;
using HarmonyLib;

namespace dvize.DadGamerMode.Patches
{

    // SPT 4.0 (EFT 40087): the inventory "EquipmentClass" is now EFT.InventoryLogic.InventoryEquipment,
    // and the total-weight helper method_10(IEnumerable<Slot>) is now the static smethod_1(IEnumerable<Slot>).
    // Rather than reimplementing the weight calculation (which relied on obfuscated helper delegates that
    // change every patch), we let the game compute the weight and simply scale the result in a postfix.
    internal class OnWeightUpdatedPatch : ModulePatch
    {

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(InventoryEquipment), "smethod_1");
        }

        [PatchPostfix]
        internal static void Postfix(ref float __result)
        {
            // Get the total weight reduction setting (100 = normal weight)
            int totalWeightReduction = dadGamerPlugin.totalWeightReductionPercentage.Value;

            if (totalWeightReduction == 100)
            {
                return;
            }

            // Convert it into a reduction factor: 0% -> full reduction (factor = 0), 100% -> no reduction (factor = 1)
            __result *= totalWeightReduction / 100f;
        }
    }


}
