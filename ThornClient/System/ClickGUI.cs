using System.Collections.Generic;
using System.IO;
using NukeLib.Game.Controls;
using UnityEngine;
using ThornClient.Core;
using ThornClient.System.ClickGUIComponents;
using UnityEngine.SceneManagement;
using System;
using NukeLib.UI;
using Object = UnityEngine.Object;

namespace ThornClient.System;

internal class ClickGUI : SystemModule {
    internal static readonly string BundlePath = Path.Combine(Plugin.workingDir, "assets", "thorn_clickgui.bundle");
    internal static GameObject? ModuleCategoryPrefab { get; private set; }
    internal static GameObject? ModuleButtonPrefab { get; private set; }

    private static AssetBundle? _assetBundle = null;
    internal static AssetBundle Bundle {
        get {
            if (_assetBundle == null) _assetBundle = AssetBundle.LoadFromFile(BundlePath);
            return _assetBundle;
        }
    }

    public ClickGUI() : base("ClickGUI", "The main interaction panel", KeyCode.RightShift) {
        SceneManager.sceneLoaded += (_, __) => _isInitialized = InitializeIfNeeded();
    }

    private bool _isInitialized = false;
    private GameObject? _canvas;
    private GameObject? _tabBar;
    private GameObject? _modulePage;
    private List<GameObject?> _tabPages = [];

    private bool InitializeIfNeeded() {
        if (_isInitialized && _canvas != null) return true;
        // Plugin.Log.LogInfo("Loading ClickGUI");

        var basePrefab = Bundle.LoadAsset<GameObject>("ThornClickGUICanvas");
        var tabBarPrefab = Bundle.LoadAsset<GameObject>("TabBar");
        var pagePrefab = Bundle.LoadAsset<GameObject>("Page");
        ModuleCategoryPrefab = Bundle.LoadAsset<GameObject>("ModuleCategory");
        ModuleButtonPrefab = Bundle.LoadAsset<GameObject>("ModuleButton");

        // Make the canvas
        _canvas = Object.Instantiate(basePrefab);
        _canvas.hideFlags = HideFlags.DontSave;
        Object.DontDestroyOnLoad(_canvas);
        _canvas.SetActive(false);

        // Make the tab bar and pages
        _modulePage = Object.Instantiate(pagePrefab, _canvas.transform);
        _modulePage.SetActive(true);

        _tabBar = Object.Instantiate(tabBarPrefab, _canvas.transform);
        _tabBar.SetActive(true);
        _tabBar.GetOrAddComponent<TabBarController>();

        // Populate page: Module
        var moduleGroupRow = Object.Instantiate(Bundle.LoadAsset<GameObject>("ModuleGroupRow"), _modulePage.transform);
        for (int i = 0; i < Enum.GetValues(typeof(ModuleCategory)).Length; i++) {
            var category = (ModuleCategory)i;
            var categoryObj = Object.Instantiate(ModuleCategoryPrefab, moduleGroupRow.transform);
            var catController = categoryObj.GetOrAddComponent<ModuleCategoryController>();
            catController.Category = category;
        }
        moduleGroupRow.UnfuckLayoutHack();


        _tabPages.Add(_modulePage);
        // Plugin.Log.LogInfo("Loaded ClickGUI successfully");
        return true;
    }

    protected override void OnEnable() {
        // Plugin.Log.LogInfo($"Show ClickGUI, null={_canvas==null}");
        if (_canvas == null) return;
        _canvas.transform.SetAsLastSibling(); // Show on top of everything
        _canvas.SetActive(true);
        _canvas.UnfuckLayoutHack();
        var canvasComp = _canvas.GetComponent<Canvas>();
        if (canvasComp != null) {
            canvasComp.sortingOrder = 69;
        }
        Pauser.Pause(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    protected override void OnDisable() {
        // Plugin.Log.LogInfo($"Hide ClickGUI, null={_canvas==null}");
        if (_canvas == null) return;
        _canvas.SetActive(false);
        Pauser.Pause(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
