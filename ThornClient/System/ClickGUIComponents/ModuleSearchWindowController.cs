using System;
using System.Collections.Generic;
using System.Linq;
using NukeLib.UI;
using NukeLib.Utils;
using ThornClient.Core;
using ThornClient.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ThornClient.System.ClickGUIComponents;

internal class ModuleSearchWindowController : MonoBehaviour {
    public int maxResults = 10;
    private bool _populated = false;
    private TMP_InputField? _input;

    private GameObject? _results;

    private Dictionary<string, GameObject> _allModules = new();
    private Searchable[]? _searchables;

    /// <summary>
    /// Filter predicate. Returns true if the module should be included in search results.
    /// Defaults to true (includes everything)
    /// </summary>
    public Predicate<Module> ModuleFilter {
        get;
        set {
            field = value;
            Repopulate();
        }
    } = _ => true;

    private void Awake() {
        _results = gameObject.FindRecursive("Modules/Results");
    }

    private void Start() {
        var inputObj = gameObject.FindRecursive("Modules/Input");
        if (inputObj != null) _input = inputObj.GetComponent<TMP_InputField>();
        PopulateIfNeeded();
        if (_input != null) _input.onValueChanged.AddListener(Query);
        gameObject.FindRecursive("Header")?.GetOrAddComponent<TitlebarDragHandler>();
        Query("");
    }

    private void OnDestroy() {
        if (_input != null) _input.onValueChanged.RemoveListener(Query);
    }

    /// <summary>
    /// Call this if you change <see cref="ModuleFilter"/> dynamically while the UI is already active.
    /// </summary>
    private void Repopulate() {
        _populated = false;

        // Clear existing generated buttons
        foreach (var button in _allModules.Values) {
            if (button != null) Destroy(button);
        }
        _allModules.Clear();
        _searchables = null;

        PopulateIfNeeded();
        Query(_input != null ? _input.text : "");
    }

    public void FocusSearch() {
        if (_input != null) {
            EventSystem.current.SetSelectedGameObject(_input.gameObject, null);
            _input.ActivateInputField();
        }
    }

    private void PopulateIfNeeded() {
        if (_populated) return;
        _populated = true;

        // Filter items based on the dynamic ModuleFilter predicate
        var filteredModules = ModuleManager.Items.Where(m => ModuleFilter(m)).ToList();

        foreach (var module in filteredModules) {
            var button = Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "ModuleButton"),
                _results?.transform);
            button.UnfuckLayoutHack();
            button.SetActive(false);
            var buttonController = button.GetOrAddComponent<ModuleButtonController>();
            buttonController.TargetModule = module;
            try {
                _allModules.TryAdd(module.Name, button);
            } catch (Exception e) {
                Plugin.Log.LogWarning(
                    $"Failed to add module {module.Name} to search, are there two with the same name?\n{e}");
            }
        }

        _searchables = filteredModules
            .Select(module => new Searchable {
                Primary = module.Name,
                Secondaries = new Dictionary<string, string> {
                    { "description", module.Description },
                    { "tags", string.Join(" ", module.Tags ?? []) },
                }
            }).ToArray();
    }

    private void Query(string query) {
        if (_searchables == null) return;
        Searchable[] results = query.Length > 0 ? Search.Invoke(query, _searchables) : [];

        foreach (var button in _allModules.Values) {
            if (button == null) continue;
            button.UnfuckLayoutHack();
            button.SetActive(false);
        }

        for (int i = 0; i < results.Length; i++) {
            if (i >= maxResults) break;
            var result = results[i];
            if (_allModules.TryGetValue(result.Primary, out var item)) {
                if (item == null) continue;
                item.SetActive(true);
                item.transform.SetAsLastSibling();
            }
        }

        if (_results != null) _results.UnfuckLayoutHack();
        gameObject.UnfuckLayoutHack();
    }
}
