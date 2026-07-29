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

internal class SearchWindowController : MonoBehaviour {
    public int maxResults = 10;
    private bool _populated = false;
    private TMP_InputField? _input;

    private GameObject? _results;

    private Dictionary<string, GameObject> _allModules = new();
    private Searchable[]? _searchables;

    private void OnEnable() {
        var inputObj = gameObject.FindRecursive("Modules/Input");
        if (inputObj != null) _input = inputObj.GetComponent<TMP_InputField>();
        _results = gameObject.FindRecursive("Modules/Results");
        PopulateIfNeeded();
        if (_input != null) _input.onValueChanged.AddListener(Query);
        gameObject.FindRecursive("Header")?.GetOrAddComponent<TitlebarDragHandler>();
        Query("");
    }

    private void OnDisable() {
        if (_input != null) _input.onValueChanged.RemoveListener(Query);
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

        foreach (var module in ModuleManager.Items) {
            if (module is SystemModule or HudModule) continue;
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

        _searchables = ModuleManager.Items
            .Where(item => !(item is SystemModule or HudModule))
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
        // Plugin.Log.LogInfo($"Query: {query} -> ({results.Length} results)");

        foreach (var button in _allModules.Values) {
            button.UnfuckLayoutHack();
            button.SetActive(false);
        }

        for (int i = 0; i < results.Length; i++) {
            if (i >= maxResults) break;
            var result = results[i];
            var item = _allModules[result.Primary];
            item.SetActive(true);
            item.transform.SetAsLastSibling();
        }

        if (_results != null) _results.UnfuckLayoutHack();
        gameObject.UnfuckLayoutHack();
    }
}
