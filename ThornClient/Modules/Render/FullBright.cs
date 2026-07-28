using System;
using System.Collections.Generic;
using UnityEngine;
using ThornClient.Core;
using UnityEngine.SceneManagement;
using ThornClient.Managers;
using ThornClient.System;

namespace ThornClient.Modules.Render;

public class FullBright : Module {
    public Setting<float> Brightness { get; }

    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "light_bulb");
    public override string[] Tags => ["light", "vision", "sight", "clear", "see", "illuminate"];

    public FullBright() : base("thorn.fullBright", "FullBright", "Adjust brightness for accessibility", ModuleCategory.Render) {
        Brightness = RegisterSetting("brightness", "Brightness", "How bright the world should be", 0.2f);
    }

    private bool lastFogEnabled;
    private Color lastAmbientColor = Color.black;
    private bool isStateSaved = false;

    public override string? CheatReason {
        get {
            List<string> darkLevels = ["Level 0-S", "Level 4-3"];
            bool isCheaty = IsEnabled && darkLevels.Contains(SceneHelper.CurrentScene);
            return isCheaty ? "Darkness is a core challenge of this level" : "";
        }
    }

    protected override void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Brightness.OnValueChanged += UpdateAmbientLightColor;

        if (SceneManager.GetActiveScene().isLoaded) {
            ApplyFullBright();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        isStateSaved = false;
        ApplyFullBright();
    }

    private void ApplyFullBright() {
        try {
            if (!isStateSaved) {
                lastFogEnabled = RenderSettings.fog;
                // Normal ambient color to be black most of the time. Setting it dynamically tends to be quite weird,
                //      and it doesn't reset across scene loads so it's quite difficult to manage

                // lastAmbientColor = RenderSettings.ambientLight;
                // Plugin.Log.LogInfo($"Last ambient color {lastAmbientColor}");
                isStateSaved = true;
            }

            RenderSettings.fog = false;
            UpdateAmbientLightColor(Brightness.Value);
        } catch (Exception e) {
            Plugin.Log.LogError($"[FullBright] RenderSettings not ready yet: {e}");
        }
    }

    private void UpdateAmbientLightColor(float brightness) {
        if (!isStateSaved) return; // Don't touch light matrices if baseline isn't cached
        RenderSettings.ambientLight = Color.white * brightness;
        CheatManager.UpdateCheatiness();
    }

    protected override void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Brightness.OnValueChanged -= UpdateAmbientLightColor;

        if (isStateSaved) {
            RenderSettings.fog = lastFogEnabled;
            RenderSettings.ambientLight = lastAmbientColor;
            isStateSaved = false;
        }
    }
}
