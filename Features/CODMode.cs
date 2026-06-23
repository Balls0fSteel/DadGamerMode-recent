using BepInEx.Logging;
using Comfort.Common;
using dvize.GodModeTest;
using EFT;
using EFT.HealthSystem;
using UnityEngine;

using AbstractIEffect = EFT.HealthSystem.ActiveHealthController.GClass3008;

namespace dvize.DadGamerMode.Features
{
    internal class CODModeComponent : MonoBehaviour
    {
        private static Player player;
        private static ActiveHealthController healthController;
        private static float timeSinceLastHit = 0f;
        private static bool isRegenerating = false;
        private static float newHealRate;
        private static HealthValue currentHealth;
        private static float timeSinceLastHeal = 0f;
        private const float HealInterval = 1f; // heal once per second of real time (framerate-independent)

        private static readonly EBodyPart[] bodyPartsDict = { EBodyPart.Stomach, EBodyPart.Chest, EBodyPart.Head, EBodyPart.RightLeg,
EBodyPart.LeftLeg, EBodyPart.LeftArm, EBodyPart.RightArm };

        protected static ManualLogSource Logger
        {
            get; private set;
        }
        private CODModeComponent()
        {
            if (Logger == null)
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(CODModeComponent));
            }
        }
        internal static void Enable()
        {
            if (Singleton<IBotGame>.Instantiated)
            {
                var gameWorld = Singleton<GameWorld>.Instance;
                gameWorld.GetOrAddComponent<CODModeComponent>();

                Logger.LogDebug("DadGamerMode: CODModeComponent enabled");
            }
        }
        private void Start()
        {
            player = Singleton<GameWorld>.Instance.MainPlayer;
            healthController = player.ActiveHealthController;
            isRegenerating = false;
            timeSinceLastHit = 0f;
            newHealRate = 0f;
            currentHealth = null;
            timeSinceLastHeal = 0f;

            player.OnPlayerDeadOrUnspawn += Player_OnPlayerDeadOrUnspawn;
            player.BeingHitAction += Player_BeingHitAction;
            healthController.EffectAddedEvent += HealthController_EffectAddedEvent;
        }

        private void HealthController_EffectAddedEvent(IEffect effect)
        {

#if DEBUG
            Logger.LogWarning("Effect added is of type: " + effect.Type);
            Logger.LogWarning("The Effect state is: " + effect.State);
            Logger.LogWarning("The BodyPart is: " + effect.BodyPart);
            Logger.LogWarning("The Effect Strength is: " + effect.Strength);
#endif

            //grabbed this from remove negative effects method
            if (dadGamerPlugin.CODModeToggle.Value && !dadGamerPlugin.CODBleedingDamageToggle.Value)
            {
                // SPT 4.0 (EFT 40087): the old GInterface252/253 effect markers can no longer be
                // referenced by name. Instead we explicitly remove the negative effects COD mode is
                // meant to nullify. LightBleeding/HeavyBleeding both derive from the public
                // ActiveHealthController.Bleeding base; Fracture/Pain/Tremor are matched by type name
                // (they are protected nested types and cannot be referenced directly).
                string effectName = effect.GetType().Name;
                bool isNegativeEffect = effect is ActiveHealthController.Bleeding
                    || effectName == "Fracture"
                    || effectName == "Pain"
                    || effectName == "Tremor";

                if (isNegativeEffect)
                {
                    healthController.RemoveEffectFromList(effect as AbstractIEffect);
#if DEBUG
                    Logger.LogDebug($"COD Mode removed negative effect: {effectName}");
#endif
                }
            }
        }

        private void Update()
        {
            if (dadGamerPlugin.CODModeToggle.Value)
            {
                timeSinceLastHit += Time.unscaledDeltaTime;

                if (timeSinceLastHit >= dadGamerPlugin.CODModeHealWait.Value)
                {
                    if (!isRegenerating)
                    {
                        isRegenerating = true;
                    }

                    // Heal on a fixed real-time interval so regen speed is the same regardless of FPS.
                    timeSinceLastHeal += Time.unscaledDeltaTime;
                    if (timeSinceLastHeal >= HealInterval)
                    {
                        timeSinceLastHeal = 0f;
                        StartHealing();
                    }
                }
            }
        }

        private void StartHealing()
        {
            if (isRegenerating && dadGamerPlugin.CODModeToggle.Value)
            {
                newHealRate = dadGamerPlugin.CODModeHealRate.Value;

                foreach (var limb in bodyPartsDict)
                {
                    // COD mode heals HP over time; clamp so a part never overshoots its maximum.
                    currentHealth = healthController.Dictionary_0_1[limb].Health;

                    if (!currentHealth.AtMaximum && !healthController.Dictionary_0_1[limb].IsDestroyed)
                    {
                        currentHealth.Current = Mathf.Min(currentHealth.Current + newHealRate, currentHealth.Maximum);
                    }
                }
            }
        }
        private void Disable()
        {
            if (player != null)
            {
                player.OnPlayerDeadOrUnspawn -= Player_OnPlayerDeadOrUnspawn;
                player.BeingHitAction -= Player_BeingHitAction;
                healthController.EffectAddedEvent -= HealthController_EffectAddedEvent;
            }
        }

        private void Player_BeingHitAction(DamageInfoStruct arg1, EBodyPart arg2, float arg3)
        {
            //Logger.LogDebug("DadGamerMode: Player_BeingHitAction called");
            timeSinceLastHit = 0f;
            timeSinceLastHeal = 0f;
            isRegenerating = false;
        }


        private void Player_OnPlayerDeadOrUnspawn(Player player)
        {
            Disable();
        }
    }
}
