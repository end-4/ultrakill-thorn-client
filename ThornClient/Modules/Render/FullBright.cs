using System;
using System.Collections.Generic;
using NukeLib.Utils;
using UnityEngine;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using UnityEngine.SceneManagement;
using ThornClient.Managers;
using ThornClient.System;

namespace ThornClient.Modules.Render;

/// <summary>
/// Module that adjusts brightness of the world
/// </summary>
public class FullBright : Module {
    /// <summary>
    /// How bright should it be. 0 is normal
    /// </summary>
    public Setting<float> Brightness { get; }

    /// <summary>
    /// Icon of this module
    /// </summary>
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "light_bulb");
    /// <summary>
    /// Tags for search
    /// </summary>
    public override string[] Tags => ["light", "vision", "sight", "clear", "see", "illuminate"];

    /// <summary>
    /// Constructor
    /// </summary>
    public FullBright() : base("thorn.fullBright", "FullBright", "Adjust brightness for accessibility", ModuleCategory.Render) {
        Brightness = CreateSetting("brightness", "Brightness", "How bright the world should be", 0.2f);
    }

    private bool _lastFogEnabled;
    private Color _lastAmbientColor = Color.black;
    private bool _isStateSaved = false;

    /// <summary>
    /// Why leaderboards might be disabled
    /// </summary>
    public override string? CheatReason {
        get {
            List<string> darkLevels = ["Level 0-S", "Level 4-3"];
            bool isCheaty = IsEnabled && darkLevels.Contains(SceneHelper.CurrentScene);
            return isCheaty ? "Darkness is a core challenge of this level" : "";
        }
    }

    /// <summary>
    /// Stuff that run when enabled
    /// </summary>
    protected override void OnEnable() {
        SceneUtils.SafeSceneLoaded += OnSceneLoaded;
        Brightness.OnValueChanged += UpdateAmbientLightColor;

        if (SceneManager.GetActiveScene().isLoaded) {
            ApplyFullBright();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        _isStateSaved = false;
        ApplyFullBright();
    }

    private void ApplyFullBright() {
        try {
            if (!_isStateSaved) {
                _lastFogEnabled = RenderSettings.fog;
                // Normal ambient color to be black most of the time. Setting it dynamically tends to be quite weird,
                //      and it doesn't reset across scene loads so it's quite difficult to manage

                // lastAmbientColor = RenderSettings.ambientLight;
                // Plugin.Log.LogInfo($"Last ambient color {lastAmbientColor}");
                _isStateSaved = true;
            }

            RenderSettings.fog = false;
            UpdateAmbientLightColor(Brightness.Value);
        } catch (Exception e) {
            Plugin.Log.LogError($"[FullBright] RenderSettings not ready yet: {e}");
        }
    }

    private void UpdateAmbientLightColor(float brightness) {
        if (!_isStateSaved) return; // Don't touch light matrices if baseline isn't cached
        RenderSettings.ambientLight = Color.white * brightness;
        CheatManager.UpdateCheatiness();
    }

    /// <summary>
    /// Stuff that run when disabled
    /// </summary>
    protected override void OnDisable() {
        SceneUtils.SafeSceneLoaded -= OnSceneLoaded;
        Brightness.OnValueChanged -= UpdateAmbientLightColor;

        if (_isStateSaved) {
            RenderSettings.fog = _lastFogEnabled;
            RenderSettings.ambientLight = _lastAmbientColor;
            _isStateSaved = false;
        }
    }
}
