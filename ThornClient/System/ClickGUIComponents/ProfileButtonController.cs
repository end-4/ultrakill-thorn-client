using System;
using NukeLib.UI;
using NukeLib.Utils;
using ThornClient.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

public class ProfileButtonController : MonoBehaviour {
    /// <summary>
    /// Name of the profile
    /// </summary>
    public string ProfileName = string.Empty;

    private Button? _button;
    private Button? _dupe;
    private Button? _delete;
    private Image? _icon;
    private TextMeshProUGUI? _nameText;
    private TMP_InputField? _name;

    private void Start() {
        if (ProfileName == string.Empty) return;

        // Find targets
        _name = gameObject.FindRecursive("Input")?.GetComponent<TMP_InputField>();
        _nameText = gameObject.FindRecursive("Input/Text Area/Text")?.GetComponent<TextMeshProUGUI>();
        _icon = gameObject.FindRecursive("Icon")?.GetComponent<Image>();
        _button = gameObject.GetComponent<Button>();
        _dupe = gameObject.FindRecursive("Actions/Duplicate")?.GetComponent<Button>();
        _delete = gameObject.FindRecursive("Actions/Delete")?.GetComponent<Button>();

        // Update stuff
        if (_name != null) _name.text = ProfileName;
        if (_icon != null) _icon.sprite = GetIconForName(ProfileName);
        if (_button != null) _button.onClick.AddListener(() => ProfileManager.SwitchProfile(ProfileName));
        if (_dupe != null) _dupe.onClick.AddListener(() => ProfileManager.CloneProfile(ProfileName));
        if (_delete != null) {
            if (ProfileName != ProfileManager.DefaultProfileName)
                _delete.onClick.AddListener(() => { ProfileManager.DeleteProfile(ProfileName); });
            else
                _delete.interactable = false;
        }

        ProfileManager.ProfileSwitched += UpdateActive;
        UpdateActive();
    }

    private void OnDestroy() {
        ProfileManager.ProfileSwitched -= UpdateActive;
    }

    private void UpdateActive(string _, string newProfileName) {
        if (newProfileName != ProfileName) return;
        UpdateActive();
    }

    private void UpdateActive() {
        bool active = ProfileName == ProfileManager.ActiveProfile;
        if (_icon != null) _icon.color = active ? ThornModule.AccentColor : Color.white;
        if (_nameText != null) _nameText.color = active ? ThornModule.AccentColor : Color.white;
    }

    // private static string[] _iconNamePool = ["square", "circle", "triangle"];
    private static string[] _iconNamePool = ["cube", "icosahedron"];

    private Sprite? GetIconForName(string name) {
        string iconName = "";
        if (name == ProfileManager.DefaultProfileName) iconName = "default";
        else {
            int index = TextUtils.GetHashIndex(name, _iconNamePool.Length);
            iconName = _iconNamePool[index];
        }

        return AssetManager.Get<Sprite>(ClickGUI.BundleKey, iconName) ?? null;
    }
}
