using System.Collections.Generic;
using System.Linq;
using NukeLib.UI;
using ThornClient.Core;
using ThornClient.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

internal class ModuleCategoryController : MonoBehaviour {
    private bool _doneSetup = false;
    public ModuleCategory Category;
    // icon name map
    private static readonly Dictionary<ModuleCategory, string> _iconNameMap = new() {
        { ModuleCategory.Enemy, "enemy" },
        { ModuleCategory.Player, "v1" },
        { ModuleCategory.Render, "eye" },
        { ModuleCategory.World, "compass" },
        { ModuleCategory.Misc, "shapes" },
    };

    private void Start() {
        SetupModules();
    }

    public void SetupModules() {
        if (_doneSetup) return;
        _doneSetup = true;

        var moduleCol = gameObject.FindRecursive("Modules");

        // Header: icon, text, dragging behavior
        var categoryIcon = gameObject.FindRecursive("Header/Icon").GetComponent<Image>();
        var categoryText = gameObject.FindRecursive("Header/CategoryName").GetComponent<TextMeshProUGUI>();
        categoryIcon.sprite = ClickGUI.Bundle.LoadAsset<Sprite>(_iconNameMap[Category]);
        categoryText.text = Category.ToString().ToUpper();
        gameObject.FindRecursive("Header").AddComponent<TitlebarDragHandler>();

        // Populate with modules
        if (ClickGUI.ModuleButtonPrefab == null) {
            Plugin.Log.LogError("ModuleButtonPrefab is null, cannot populate modules");
            return;
        }
        var moduleList = ModuleManager.GetByCategory(Category).Where(m => (!(m is SystemModule)));
        foreach (var module in moduleList) {
            var moduleButtonObj = Object.Instantiate(ClickGUI.ModuleButtonPrefab, moduleCol.transform);
            moduleButtonObj.SetActive(true);
            var buttonController = moduleButtonObj.GetOrAddComponent<ModuleButtonController>();
            buttonController.TargetModule = module;
        }
    }
}
