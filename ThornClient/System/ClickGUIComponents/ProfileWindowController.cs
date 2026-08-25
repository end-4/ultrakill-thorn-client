using System;
using NukeLib.UI;
using NukeLib.Utils;
using ThornClient.Managers;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ThornClient.System.ClickGUIComponents;

/// <summary>
/// Controller for a profile selection window
/// </summary>
public class ProfileWindowController : MonoBehaviour {
    private Transform? _profileList;
    private Button? _newBtn;
    private GameObject? _profileButtonPrefab;

    bool _repopulating = false;

    private void Start() {
        // Find targets
        _profileList = gameObject.FindRecursive("Scroll View/Viewport/Content/Modules/Profiles")?.transform;
        _newBtn = gameObject.FindRecursive("Scroll View/Viewport/Content/Modules/NewActions/NewEmpty")?.GetComponent<Button>();
        var titlebar = gameObject.FindRecursive("Header");

        // Get assets
        _profileButtonPrefab = AssetManager.Get<GameObject>(
            ClickGUI.BundleKey, "ProfileButton"
        );

        // Set up stuff
        titlebar?.GetOrAddComponent<TitlebarDragHandler>();
        if (_newBtn != null) _newBtn.onClick.AddListener(ProfileManager.CreateProfile);
        ProfileManager.ProfilesChanged += RepopulateNextFrame; // Somehow delaying a frame prevents crashes
        Repopulate();
    }

    private void OnDestroy() {
        ProfileManager.ProfilesChanged -= RepopulateNextFrame;
    }

    private void RepopulateNextFrame() {
        ExecutionUtils.RunNextFrame(Repopulate);
    }

    private void Repopulate() {
        if (_repopulating) return;
        _repopulating = true;
        if (_profileList == null) return;
        foreach (Transform child in _profileList) {
            Object.Destroy(child.gameObject);
        }

        AddProfileButton(ProfileManager.DefaultProfileName);
        foreach (var profileName in ProfileManager.GetProfiles()) {
            if (profileName == ProfileManager.DefaultProfileName) continue;
            AddProfileButton(profileName);
        }
        gameObject.UnfuckLayoutHack();
        ExecutionUtils.RunNextFrame(gameObject.UnfuckLayoutHack);
        _repopulating = false;
    }

    private void AddProfileButton(string profileName) {
        if (_profileButtonPrefab == null || _profileList == null) return;
        var button = Object.Instantiate(_profileButtonPrefab, _profileList);
        var comp = button.AddComponent<ProfileButtonController>();
        comp.ProfileName = profileName;
    }
}
