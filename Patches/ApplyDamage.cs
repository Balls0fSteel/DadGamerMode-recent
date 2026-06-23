using System;
using System.Reflection;
using SPT.Reflection.Patching;
using dvize.GodModeTest;
using EFT.HealthSystem;
using HarmonyLib;

namespace dvize.DadGamerMode.Patches
{
    internal class ApplyDamage : ModulePatch
    {
        private static ActiveHealthController healthController;
        private static EFT.HealthSystem.ValueStruct currentHealth;
        private static bool potentialHealthLowerThanMinimum;
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ActiveHealthController), "ApplyDamage");
        }

        [PatchPrefix]
        private static bool Prefix(ActiveHealthController __instance, ref float damage, EBodyPart bodyPart, DamageInfoStruct damageInfo)
        {
            try
            {
                if (__instance.Player != null &&
                __instance.Player.IsYourPlayer)
                {
                    healthController = __instance.Player.ActiveHealthController;
                    currentHealth = healthController.GetBodyPartHealth(bodyPart, false);

                    //just set damage to 0 and not run apply damage for GodMode
                    if (dadGamerPlugin.Godmode.Value)
                    {
                        damage = 0f;
                        return false;
                    }

                    //if headshot damage ignore it
                    if (bodyPart == EBodyPart.Head && dadGamerPlugin.IgnoreHeadShotDamage.Value)
                    {
                        damage = 0f;
                        return false;
                    }

                    //if there's a custom damage value use that (skip for head hits when the headshot
                    //percentage is active so that setting OVERRIDES the all-body value instead of stacking)
                    if (dadGamerPlugin.CustomDamageModeVal.Value != 100 &&
                        !(bodyPart == EBodyPart.Head && dadGamerPlugin.PercentageHeadShotDamageOnly.Value))
                    {
                        //set damage early so we can use it in the keep1health check
                        damage = damage * ((float)dadGamerPlugin.CustomDamageModeVal.Value / 100);
                    }

                    //if there's a custom headshot damage value use that (overrides the all-body percentage)
                    if (bodyPart == EBodyPart.Head && dadGamerPlugin.PercentageHeadShotDamageOnly.Value)
                    {
                        //set damage early so we can use it in the keep1health check
                        damage = damage * ((float)dadGamerPlugin.CustomHeadDamageModeVal.Value / 100);
                    }

                    //if keep 1 health enabled, ensure health does not drop to the part minimum
                    if (dadGamerPlugin.Keep1Health.Value)
                    {
                        potentialHealthLowerThanMinimum = (currentHealth.Current - damage) <= currentHealth.Minimum;

                        // If this damage would bring the part to (or below) its minimum, block the killing blow.
                        // (Zeroing the damage and skipping the original is what actually preserves the part; we
                        // do NOT write currentHealth.Current here because GetBodyPartHealth returns a struct copy.)
                        if (potentialHealthLowerThanMinimum)
                        {
                            if ((dadGamerPlugin.Keep1HealthSelection.Value == "Head And Thorax" && (bodyPart == EBodyPart.Head || bodyPart == EBodyPart.Chest)))
                            {
                                damage = 0f;
                                return false;
                            }

                            else if (dadGamerPlugin.Keep1HealthSelection.Value == "All")
                            {
                                damage = 0f;
                                return false;
                            }
                        }
                    }

                    //add check if bodypart at minimum for non-critical parts as it can still kill you and CODMode is not enabled.
                    //respect the Keep1Health toggle and the body-part selection so "Head And Thorax" only protects Head/Chest.
                    if (currentHealth.AtMinimum &&
                        dadGamerPlugin.Keep1Health.Value &&
                        !dadGamerPlugin.CODModeToggle.Value &&
                        (dadGamerPlugin.Keep1HealthSelection.Value == "All" ||
                            (dadGamerPlugin.Keep1HealthSelection.Value == "Head And Thorax" &&
                                (bodyPart == EBodyPart.Head || bodyPart == EBodyPart.Chest))))
                    {
                        return false;
                    }
                }
                else
                {
                    //multiply damage by multiplier if a type of player
                    if (__instance.Player != null)
                    {
                        damage = damage * dadGamerPlugin.enemyDamageMultiplier.Value;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }

            return true;
        }

    }
}
