using UnityEngine;
using ThornClient.Core;
using ThornClient.Settings;
using UnityEngine.SceneManagement;

namespace ThornClient.Modules.Render;

public class FullBright : Module {
    public Setting<float> Brightness { get; }

    public FullBright() : base("FullBright", "Adjustable brightness for accessibility", ModuleCategory.Render,
        KeyCode.Semicolon) {
        Brightness = RegisterSetting("Brightness", "How bright the world should be.", 0.2f);
    }

    private bool lastFogEnabled;
    private Color lastAmbientColor = Color.black;
    private bool isStateSaved = false;

    protected override void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Brightness.OnValueChanged += UpdateAmbientLightColor;

        if (SceneManager.GetActiveScene().isLoaded) {
            ApplyFullBright();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        // isStateSaved = false;
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
        } catch (System.Exception e) {
            Plugin.Log.LogError($"[FullBright] RenderSettings not ready yet: {e}");
        }
    }

    private void UpdateAmbientLightColor(float brightness) {
        if (!isStateSaved) return; // Don't touch light matrices if baseline isn't cached
        RenderSettings.ambientLight = Color.white * brightness;
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
