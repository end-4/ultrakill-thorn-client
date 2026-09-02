using System;
using System.Reflection;
using HarmonyLib;
using NukeLib.UI;
using NukeLib.Utils;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.HUD;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ThornClient.Modules.HUD;

/// <summary>
/// Module that shows alerts, similar to BetterPvP (the Minecraft mod)'s Notifications
/// </summary>
public class Alerts : FramedHudModule {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "warning");

    /// <inheritdoc />
    public override string[] Tags => ["health", "hp", "low", "rocket", "whiplash", "FUP", "slam"];

    public Setting<bool> HealthEnable;
    public Setting<int> HealthThreshold;
    public Setting<bool> RocketPullEnable;
    public Setting<bool> RocketPullWhenNotJumpingOnly;
    public Setting<bool> DangerousSlamEnable;
    public Setting<bool> DangerousSlamWhenSlammingOnly;

    /// <inheritdoc />
    public Alerts() : base("thorn.alerts", "Alerts", "Alerts for low health, rocket whipping, and dangerous slamming") {
        CreateHeader("health", "<color=#ff3737>Health</color>");
        HealthEnable = CreateSetting("healthEnable", "Enable", "Whether to warn on low health", true);
        HealthThreshold = CreateSetting("healthThreshold", "Low threshold", "At what HP do we consider low health", 30);
        HealthThreshold.Hints = InterfaceHints.RangeHint(0, 100);
        CreateHeader("rocketPull", "<color=#ff7f39>Rocket pull</color>");
        RocketPullEnable = CreateSetting("rocketPullEnable", "Enable",
            "Whether to warn when pulling a rocket without holding jump", true);
        RocketPullWhenNotJumpingOnly = CreateSetting("rocketPullWhenNotJumpingOnly", "Warn when not holding Jump only",
            "Because you can safely ride the rocket by holding Jump", true);
        CreateHeader("dangerousSlam", "<color=#ff59ff>Dangerous stuff below</color>");
        DangerousSlamEnable = CreateSetting("dangerousSlamEnable", "Enable",
            "Whether to warn when something dangerous is below", true);
        DangerousSlamWhenSlammingOnly = CreateSetting("dangerousSlamWhenSlammingOnly", "Warn when slamming only",
            "Only show indicator when slamming", true);
    }

    protected override GameObject CreateContentObject() {
        var obj = Object.Instantiate(AssetManager.Get<GameObject>(HudManager.BundleKey, "Alerts"));
        if (obj == null) return null!;
        obj.AddComponent<AlertController>().ModuleInstance = this;
        return obj;
    }

    /// <summary>
    /// Controller for alerts
    /// </summary>
    protected class AlertController : MonoBehaviour {
        public Alerts? ModuleInstance;
        private GameObject? _lowHealth;
        private GameObject? _rocketPull;
        private GameObject? _dangerSlam;
        private GameObject? _preview;
        private bool _showAll;

        private static readonly FieldInfo? CaughtGrenadeField = typeof(HookArm).GetField(
            "caughtGrenade", BindingFlags.NonPublic | BindingFlags.Instance);

        private void OnEnable() {
            if (!SceneUtils.IsSafe()) return;
            foreach (Transform childTrans in transform) {
                childTrans.gameObject.SetActive(false);
            }

            _lowHealth = gameObject.FindRecursive("LowHealth");
            _rocketPull = gameObject.FindRecursive("RocketWhip");
            _preview = gameObject.FindRecursive("Preview");
            _dangerSlam = gameObject.FindRecursive("DangerousSlam");
            if (ClickGUI.Instance == null) return;
            ClickGUI.Instance.OnToggleStateChanged += TogglePreview;
            if (SceneUtils.IsSafe()) TogglePreview(ClickGUI.Instance.IsEnabled);
        }

        private void TogglePreview(bool showAll) {
            _showAll = showAll;
            UpdateAllVisibilities();
            ModuleInstance?.UpdateOverlay();
        }

        private void UpdateAllVisibilities() {
            UpdateLowHealthVisibility();
            UpdateRocketPullVisibility();
            UpdateDangerSlamVisibility();
            UpdatePreviewVisibility();
        }

        private void OnDisable() {
            if (ClickGUI.Instance != null) {
                ClickGUI.Instance.OnToggleStateChanged -= TogglePreview;
            }
        }

        private void Update() {
            UpdateLowHealthVisibility();
        }

        private void FixedUpdate() {
            UpdateRocketPullVisibility();
            UpdateDangerSlamVisibility();
        }

        private void UpdateLowHealthVisibility() {
            var nm = NewMovement.Instance;
            if (nm == null || ModuleInstance == null || _lowHealth == null) return;
            var toShow = ModuleInstance.HealthEnable.Value && nm.hp <= ModuleInstance.HealthThreshold.Value;
            if (_showAll) toShow = true;
            if (toShow != _lowHealth.activeSelf) {
                _lowHealth.SetActive(toShow);
                ModuleInstance.UIElement?.UnfuckLayoutHack();
            }
        }

        private void UpdateRocketPullVisibility() {
            var ha = HookArm.Instance;
            var inputSource = InputManager.Instance?.InputSource;
            if (ha == null || ModuleInstance == null || inputSource == null || _rocketPull == null) return;
            var caught = CaughtGrenadeField?.GetValue(ha) as Grenade;
            var toShow = ModuleInstance.RocketPullEnable.Value && caught != null && caught.rocket &&
                         (!inputSource.Jump.IsPressed || !ModuleInstance.RocketPullWhenNotJumpingOnly.Value);
            if (_showAll) toShow = true;
            if (toShow != _rocketPull.activeSelf) {
                _rocketPull.SetActive(toShow);
                ModuleInstance.UIElement?.UnfuckLayoutHack();
            }
        }

        private void UpdateDangerSlamVisibility() {
            var nm = NewMovement.Instance;
            if (nm == null || ModuleInstance == null || _dangerSlam == null) return;
            if (!ModuleInstance.DangerousSlamEnable.Value && _dangerSlam.activeSelf) {
                _dangerSlam.SetActive(false);
                ModuleInstance.UIElement?.UnfuckLayoutHack();
                return;
            }

            bool toShow = false;
            var somethingDownThere = IsStuffBelow(out var raycastHit);
            // Plugin.Log.LogInfo($"hit {raycastHit}");
            GameObject? objectBelow = raycastHit.collider?.gameObject;
            // Plugin.Log.LogInfo($"object below {objectBelow}");

            if (!somethingDownThere ||
                (objectBelow != null &&
                 (objectBelow.GetComponent<HurtZone>() != null ||
                  objectBelow.GetComponent<DeathZone>() != null ||
                  objectBelow.GetComponent<OutOfBounds>() != null ||
                  objectBelow.name == "ExtraFanSoundBounds") // 1-R floor-less fight
                )
               ) {
                // Void or has hurt/death zone -> it's dangerous
                toShow = true;
            }

            if (toShow && ModuleInstance.DangerousSlamWhenSlammingOnly.Value) {
                toShow = nm.gc.heavyFall;
            }

            if (_showAll) toShow = true;
            if (toShow != _dangerSlam.activeSelf) {
                _dangerSlam.SetActive(toShow);
                ModuleInstance.UIElement?.UnfuckLayoutHack();
            }
        }

        private void UpdatePreviewVisibility() {
            if (_preview == null) return;
            if (_preview.activeSelf != _showAll) {
                _preview.SetActive(_showAll);
            }
            ModuleInstance?.UIElement?.UnfuckLayoutHack();
        }

        protected bool IsStuffBelow(out RaycastHit hit) {
            Vector3 gravityDir = NewMovement.Instance?.rb.GetGravityDirection() ?? Vector3.down;
            Vector3 origin = NewMovement.Instance != null
                ? NewMovement.Instance.transform.position
                : Vector3.zero;

            int layerMask = LayerMask.GetMask("EnvironmentBaked", "OutdoorsBaked", "Environment", "Outdoors",
                "Outdoors Non-solid", "Default");

            return Physics.Raycast(
                origin,
                gravityDir,
                out hit,
                float.PositiveInfinity,
                layerMask,
                QueryTriggerInteraction.Collide
            );
        }
    }
}
