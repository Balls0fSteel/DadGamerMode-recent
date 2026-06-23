using System;
using Comfort.Common;
using dvize.GodModeTest;
using EFT;
using UnityEngine;

namespace dvize.DadGamerMode.Features
{
    internal class MagReloadSpeed : MonoBehaviour
    {
        private Player player;

        // Snapshot of the engine's real reload/unload times, captured once before we first overwrite
        // them, so disabling the feature restores the genuine values instead of hardcoded guesses.
        private static bool originalsCaptured = false;
        private static float originalBaseLoadTime;
        private static float originalBaseUnloadTime;

        private static void ApplyConfiguredSpeeds()
        {
            var settings = Singleton<BackendConfigSettingsClass>.Instance;
            if (settings == null)
            {
                return;
            }

            if (!originalsCaptured)
            {
                originalBaseLoadTime = settings.BaseLoadTime;
                originalBaseUnloadTime = settings.BaseUnloadTime;
                originalsCaptured = true;
            }

            settings.BaseLoadTime = dadGamerPlugin.ReloadSpeed.Value;
            settings.BaseUnloadTime = dadGamerPlugin.UnloadSpeed.Value;
        }

        private static void RestoreOriginalSpeeds()
        {
            if (!originalsCaptured)
            {
                return;
            }

            var settings = Singleton<BackendConfigSettingsClass>.Instance;
            if (settings == null)
            {
                return;
            }

            settings.BaseLoadTime = originalBaseLoadTime;
            settings.BaseUnloadTime = originalBaseUnloadTime;
        }

        private void Start()
        {
            // Subscribe to configuration change events
            dadGamerPlugin.ReloadSpeed.SettingChanged += OnReloadSpeedChanged;
            dadGamerPlugin.UnloadSpeed.SettingChanged += OnUnloadSpeedChanged;
            dadGamerPlugin.ToggleReloadUnloadSpeed.SettingChanged += OnToggleReloadUnloadSpeedChanged;

            if (dadGamerPlugin.ToggleReloadUnloadSpeed.Value)
            {
                ApplyConfiguredSpeeds();
            }
        }
        private void OnReloadSpeedChanged(object sender, EventArgs e)
        {
            if (dadGamerPlugin.ToggleReloadUnloadSpeed.Value)
            {
                ApplyConfiguredSpeeds();
            }
        }

        private void OnUnloadSpeedChanged(object sender, EventArgs e)
        {
            if (dadGamerPlugin.ToggleReloadUnloadSpeed.Value)
            {
                ApplyConfiguredSpeeds();
            }
        }

        private void OnToggleReloadUnloadSpeedChanged(object sender, EventArgs e)
        {
            if (dadGamerPlugin.ToggleReloadUnloadSpeed.Value)
            {
                ApplyConfiguredSpeeds();
            }
            else
            {
                // Restore the engine's real reload/unload times. Leave the user's saved
                // ReloadSpeed/UnloadSpeed config values untouched so they persist for next enable.
                RestoreOriginalSpeeds();
            }
        }
        public static void Enable()
        {
            if (Singleton<IBotGame>.Instantiated)
            {
                var gameWorld = Singleton<GameWorld>.Instance;
                gameWorld.GetOrAddComponent<MagReloadSpeed>();
            }
        }
        private void OnDestroy()
        {
            // Restore the engine's real values so the modified times don't leak into the hideout/menu
            // and subsequent raids once this raid's component is torn down.
            if (dadGamerPlugin.ToggleReloadUnloadSpeed.Value)
            {
                RestoreOriginalSpeeds();
            }

            // Unsubscribe from configuration change events
            dadGamerPlugin.ReloadSpeed.SettingChanged -= OnReloadSpeedChanged;
            dadGamerPlugin.UnloadSpeed.SettingChanged -= OnUnloadSpeedChanged;
            dadGamerPlugin.ToggleReloadUnloadSpeed.SettingChanged -= OnToggleReloadUnloadSpeedChanged;
        }
    }
}

