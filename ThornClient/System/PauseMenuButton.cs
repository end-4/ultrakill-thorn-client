using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using NukeLib.UI;
using NukeLib.Utils;
using ThornClient.Managers;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ThornClient.System;

internal static class PauseMenuButton {
    private static string PauseNotifToggleButtonName = "ThornToggleClickGUI";
    private static GameObject? _clickGuiToggleButton;

    public static void Initialize() {
    }

    static PauseMenuButton() {
        SceneUtils.SafeSceneLoadedNoParam += PatchPauseMenu;
    }

    private static void PatchPauseMenu() {
        // Find og
        var canvas = SceneManager.GetActiveScene().GetRootGameObjects()
            .Where(obj => obj.name == "Canvas").FirstOrDefault();
        if (canvas == null) return;

        if (_clickGuiToggleButton != null) return;

        var pauseMenu = canvas.FindRecursive("PauseMenu");
        var resumeButton = pauseMenu?.FindRecursive("Resume");
        if (pauseMenu == null || resumeButton == null) return;

        // Clone
        _clickGuiToggleButton = Object.Instantiate(resumeButton, pauseMenu.transform);
        _clickGuiToggleButton.name = PauseNotifToggleButtonName;

        // Set click behavior
        var btnComp = _clickGuiToggleButton.GetComponent<Button>();
        btnComp.onClick = new Button.ButtonClickedEvent(); // nuke old behavior
        if (ClickGUI.Instance != null) btnComp.onClick.AddListener(ClickGUI.Instance.Toggle);

        // Add icon
        var childText = _clickGuiToggleButton.FindRecursive("Text");
        if (childText != null) Object.Destroy(childText);
        var thornIconPrefab = AssetManager.Get<GameObject>(ClickGUI.BundleKey, "ThornClickGUIToggleIcon");
        var btnIcon = Object.Instantiate(thornIconPrefab, _clickGuiToggleButton.transform);

        // Ensure the icon is centered and scaled correctly within the button
        RectTransform iconRt = btnIcon.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.3f, 0.3f);
        iconRt.anchorMax = new Vector2(0.7f, 0.7f);


        UpdateAppearance();
    }

    public static void UpdateAppearance() {
        if (_clickGuiToggleButton == null) return;
        var rt = _clickGuiToggleButton.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.sizeDelta = new Vector2(rt.sizeDelta.y, rt.sizeDelta.y); // Square
        bool forceNiceConfiggy = ThornModule.Instance?.MenuButtonPositionForceConfiggyNicePosition.Value ?? false;
        float x = forceNiceConfiggy ? -115 : -107;
        rt.localPosition = new Vector3(x, -90, 0);

        if (forceNiceConfiggy) {
            var configgyButton =
                _clickGuiToggleButton.transform.parent.gameObject.FindRecursive("UI_Button_Image(Clone)");
            if (configgyButton != null) {
                var crt = configgyButton.GetComponent<RectTransform>();
                crt.localPosition = new Vector3(115, crt.localPosition.y, crt.localPosition.z);
            }
        }
    }
}
