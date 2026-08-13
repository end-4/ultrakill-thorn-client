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
        { ModuleCategory.Utility, "v1" },
        { ModuleCategory.Render, "eye" },
        { ModuleCategory.Gameplay, "compass" },
        { ModuleCategory.Misc, "shapes" },
        { ModuleCategory.Hud, "hud" },
    };

    private void Start() {
        SetupModules();
    }

    public void SetupModules() {
        if (_doneSetup) return;
        _doneSetup = true;

        var moduleCol = gameObject.FindRecursive("Scroll View/Viewport/Content/Modules");

        // Header: icon, text, dragging behavior
        gameObject.FindRecursive("Header/TitleButton").GetComponent<Button>().interactable = false;
        var categoryIcon = gameObject.FindRecursive("Header/TitleButton/TitleIcon").GetComponent<Image>();
        var categoryText = gameObject.FindRecursive("Header/TitleName").GetComponent<TextMeshProUGUI>();
        categoryIcon.sprite = AssetManager.Get<Sprite>(ClickGUI.BundleKey, _iconNameMap[Category]);
        categoryText.text = Category.ToString().ToUpper();
        gameObject.FindRecursive("Header")?.GetOrAddComponent<TitlebarDragHandler>();

        // Populate with modules
        if (AssetManager.Get<GameObject>(ClickGUI.BundleKey, "ModuleButton") == null) {
            Plugin.Log.LogError("ModuleButtonPrefab is null, cannot populate modules");
            return;
        }
        var moduleList = ModuleManager.GetByCategory(Category).Where(m => (!(m is SystemModule)));
        foreach (var module in moduleList) {
            var module1 = module;
            var moduleButtonObj = Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "ModuleButton"), moduleCol.transform);
            var buttonController = moduleButtonObj.GetOrAddComponent<ModuleButtonController>();
            buttonController.TargetModule = module1;
        }
        gameObject.UnfuckLayoutHack();
    }
}
