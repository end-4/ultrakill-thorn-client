using System.Collections.Generic;
using System.IO;
using NukeLib.Game.Controls;
using UnityEngine;
using ThornClient.Core;
using ThornClient.System.ClickGUIComponents;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using NukeLib.Text;
using NukeLib.UI;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ThornClient.System;

internal class ClickGUI : SystemModule {
    internal static ClickGUI? Instance;
    internal static readonly string BundlePath = Path.Combine(Plugin.workingDir, "assets", "thorn_clickgui.bundle");
    internal static GameObject? ModuleCategoryPrefab { get; private set; }
    internal static GameObject ModuleButtonPrefab { get; private set; }
    internal static GameObject ModuleDescriptionPrefab { get; private set; }
    internal static GameObject EnabledButtonPrefab { get; private set; }
    internal static GameObject SettingRowWrapperPrefab { get; private set; }
    internal static GameObject BoolSettingPrefab { get; private set; }
    internal static GameObject NumberSettingPrefab { get; private set; }
    internal static GameObject KeybindSettingPrefab { get; private set; }
    internal static GameObject TextSettingPrefab { get; private set; }
    internal static GameObject ColorSettingPrefab { get; private set; }
    private static GameObject? _pagePrefab;

    private static AssetBundle? _assetBundle = null;
    internal static AssetBundle Bundle {
        get {
            if (_assetBundle == null) _assetBundle = AssetBundle.LoadFromFile(BundlePath);
            return _assetBundle;
        }
    }

    public ClickGUI() : base("thorn.clickGui", "ClickGUI", "The main interaction panel") {
        // Keybind is registered in ThornModule
        SceneManager.sceneLoaded += (_, __) => _isInitialized = InitializeIfNeeded();
    }

    private bool _isInitialized = false;
    private GameObject? _canvas;
    private GameObject? _tabBar;
    private GameObject? _modulePage;
    private GameObject? _hudPage;
    private GameObject? _settingsPage;
    private GameObject? _configPopupPage;
    private GameObject? _tooltip;
    private GameObject? _tabBarButtonRow;
    private List<Tuple<string, GameObject>> _tabPages = new();

    private static OptionsManager? opts => OptionsManager.Instance;

    private bool InitializeIfNeeded() {
        if (Instance != null || (_isInitialized && _canvas != null)) return true;
        Instance = this;
        // Plugin.Log.LogInfo("Loading ClickGUI");

        var basePrefab = Bundle.LoadAsset<GameObject>("ThornClickGUICanvas");
        var tabBarPrefab = Bundle.LoadAsset<GameObject>("TabBar");
        _pagePrefab = Bundle.LoadAsset<GameObject>("Page");
        var tooltipPrefab = Bundle.LoadAsset<GameObject>("Tooltip");
        var tabButtonPrefab = Bundle.LoadAsset<GameObject>("TabButton");
        ModuleCategoryPrefab = Bundle.LoadAsset<GameObject>("ModuleCategory");
        ModuleButtonPrefab = Bundle.LoadAsset<GameObject>("ModuleButton");
        ModuleDescriptionPrefab = Bundle.LoadAsset<GameObject>("ModuleDescription");
        EnabledButtonPrefab = Bundle.LoadAsset<GameObject>("EnabledButton");
        SettingRowWrapperPrefab = Bundle.LoadAsset<GameObject>("SettingRowWrapper");
        BoolSettingPrefab = Bundle.LoadAsset<GameObject>("BoolSetting");
        NumberSettingPrefab = Bundle.LoadAsset<GameObject>("NumberSetting");
        KeybindSettingPrefab = Bundle.LoadAsset<GameObject>("KeybindSetting");
        TextSettingPrefab = Bundle.LoadAsset<GameObject>("TextSetting");
        ColorSettingPrefab = Bundle.LoadAsset<GameObject>("ColorSetting");


        // Make the canvas
        _canvas = Object.Instantiate(basePrefab);
        _canvas.hideFlags = HideFlags.DontSave;
        Object.DontDestroyOnLoad(_canvas);
        _canvas.SetActive(false);

        // Tab bar
        _tabBar = Object.Instantiate(tabBarPrefab, _canvas.transform);
        _tabBar.SetActive(true);
        _tabBar.GetOrAddComponent<TabBarController>();

        // Make pages
        _modulePage = Object.Instantiate(_pagePrefab, _canvas.transform);
        _modulePage.SetActive(true);
        // _hudPage = Object.Instantiate(_pagePrefab, _canvas.transform);
        // _hudPage.SetActive(false);
        _settingsPage = Object.Instantiate(_pagePrefab, _canvas.transform);
        _settingsPage.SetActive(false);

        _tabPages.Add(Tuple.Create("Modules", _modulePage));
        // _tabPages.Add(Tuple.Create("HUD", _hudPage));
        _tabPages.Add(Tuple.Create("Settings", _settingsPage));
        lastTabName = "Modules";

        // Populate page: Module
        float canvasWidth = ((RectTransform)_canvas.transform).sizeDelta.x;
        float canvasHeight = ((RectTransform)_canvas.transform).sizeDelta.y;
        float tabBarWidth = ((RectTransform)_tabBar.transform).sizeDelta.x;
        float tabBarHeight = ((RectTransform)_tabBar.transform).sizeDelta.y;
        float screenPadding = canvasHeight / 2 - _tabBar.transform.localPosition.y;

        float baseYPos =
            _tabBar.transform.localPosition.y - tabBarHeight -
            screenPadding * 2; // * 2 shouldn't be necessary but uhh idk
        float baseXPos = -canvasWidth / 2 + screenPadding;
        float currXPos = baseXPos;
        float spacing = 8;

        _canvas.SetActive(true); // For size updates to happen
        for (int i = 0; i < Enum.GetValues(typeof(ModuleCategory)).Length; i++) {
            var category = (ModuleCategory)i;
            var categoryObj = Object.Instantiate(ModuleCategoryPrefab, _modulePage.transform);
            var catController = categoryObj.GetOrAddComponent<ModuleCategoryController>();
            catController.Category = category;
            catController.SetupModules();
            categoryObj.UnfuckLayoutHack();
            categoryObj.transform.localPosition = new Vector3(currXPos, baseYPos, 0f);
            currXPos += ((RectTransform)categoryObj.transform).sizeDelta.x + spacing;
        }

        _modulePage.UnfuckLayoutHack();
        _canvas.SetActive(false);

        // Populate page: Settings
        var settingsObj = Object.Instantiate(ModuleCategoryPrefab, _settingsPage.transform);
        var settingRect = (RectTransform)settingsObj.transform;
        settingRect.pivot = new Vector2(0.5f, 0.5f);
        settingRect.localPosition = new Vector3(0f, 0f, 0f);
        var configController = settingsObj.GetOrAddComponent<ConfigurableWindowController>();
        configController.TargetConfigurable = ThornModule.Instance;

        // Add buttons to tab bar
        _tabBarButtonRow = _tabBar.FindRecursive("Tabs");
        _canvas.SetActive(true);
        foreach (var tup in _tabPages) {
            var key = tup.Item1;
            var tabButton = Object.Instantiate(tabButtonPrefab, _tabBarButtonRow.transform);
            tabButton.FindRecursive("Text").GetComponent<TextMeshProUGUI>().text = key;
            tabButton.GetComponent<Button>().onClick.AddListener(() => { SetTab(key); });
        }

        _tabBarButtonRow.UnfuckLayoutHack();
        _canvas.SetActive(false);

        // Tooltip
        _tooltip = Object.Instantiate(tooltipPrefab, _canvas.transform);
        _tooltip.SetActive(false);
        return true;
    }

    protected override void OnEnable() {
        // Plugin.Log.LogInfo($"Show ClickGUI, null={_canvas == null}");
        if (_canvas == null) return;
        _canvas.transform.SetAsLastSibling(); // Show on top of everything
        _canvas.SetActive(true);
        _canvas.UnfuckLayoutHack();
        var canvasComp = _canvas.GetComponent<Canvas>();
        if (canvasComp != null) {
            canvasComp.sortingOrder = 69;
        }

        Pauser.Pause(true);
        if (opts != null) opts.dontUnpause = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    protected override void OnDisable() {
        // Plugin.Log.LogInfo($"Hide ClickGUI, null={_canvas == null}");
        if (opts != null) opts.dontUnpause = false;
        if (_canvas == null) return;
        _canvas.SetActive(false);
        Pauser.Pause(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (_tooltip != null) _tooltip.SetActive(false);
    }

    public override void OnUpdate() {
        UpdateTooltipPos();
    }

    private void UpdateTooltipPos() {
        if (_tooltip == null || !_tooltip.activeInHierarchy) return;
        RectTransform rt = (RectTransform)_tooltip.transform;
        rt.position = Input.mousePosition + new Vector3(70f + rt.sizeDelta.x / 2, -40f - rt.sizeDelta.y / 2, 0f);
        _tooltip.transform.SetAsLastSibling();
    }

    public static void SetTooltipText(string text) {
        var wrappedText = text.WrapText(30);
        if (Instance == null || Instance._tooltip == null) return;
        Instance._tooltip.FindRecursive("Text").GetComponent<TextMeshProUGUI>().text = wrappedText;
        Instance._tooltip.SetActive(true);
        Instance._tooltip.UnfuckLayoutHack();
        Instance.UpdateTooltipPos();
    }

    public static void SurrenderTooltipText(string text) {
        var wrappedText = text.WrapText(30);
        if (Instance == null || Instance._tooltip == null) return;
        var comp = Instance._tooltip.FindRecursive("Text").GetComponent<TextMeshProUGUI>();
        if (comp.text == wrappedText) {
            Instance._tooltip.SetActive(false);
            comp.text = "";
        }
    }

    public static void SetTab(string tabName) {
        if (Instance == null) return;
        var pages = Instance._tabPages;
        var tabButtonRow = Instance._tabBarButtonRow;
        int currIndex = 0;
        for (int i = 0; i < pages.Count; i++) {
            var (key, val) = pages[i];
            val.SetActive(tabName == key);
            if (val != null && tabName == key) {
                lastTabName = tabName;
                currIndex = i;
            }
        }

        if (tabButtonRow != null) return;
        for (int i = 0; i < tabButtonRow.transform.childCount; i++) {
            tabButtonRow.transform.GetChild(i).GetComponent<Image>().sprite = Bundle
                .LoadAsset<Sprite>(i == currIndex ? "Round_FillLarge" : "Round_BorderLarge");
        }
    }

    private static string lastTabName = "";
    public static void OpenConfig(Configurable config) {
        if (Instance == null || Instance._canvas == null) return;
        if (Instance._configPopupPage == null) {
            Instance._configPopupPage = Object.Instantiate(_pagePrefab, Instance._canvas.transform);
        }
        if (Instance._configPopupPage == null) return;
        Instance._configPopupPage.SetActive(true);

        // Hide other pages
        var tabButtonRow = Instance._tabBarButtonRow;
        if (tabButtonRow != null) tabButtonRow.SetActive(false);
        var pages = Instance._tabPages;
        for (int i = 0; i < pages.Count; i++) {
            pages[i].Item2.SetActive(false);
        }

        // Clear previous stuff
        foreach (Transform g in Instance._configPopupPage.transform) {
            Object.Destroy(g.gameObject);
        }

        // Add new panel
        var configurableObj = Object.Instantiate(ModuleCategoryPrefab, Instance._configPopupPage.transform);
        if (configurableObj == null) return;
        // Center it
        var settingRect = (RectTransform)configurableObj.transform;
        settingRect.pivot = new Vector2(0.5f, 0.5f);
        settingRect.localPosition = new Vector3(0f, 0f, 0f);
        // Add controller
        var configController = configurableObj.GetOrAddComponent<ConfigurableWindowController>();
        configController.IsPopup = true;
        configController.TargetConfigurable = config;
    }

    public static void CloseConfig() {
        if (Instance == null) return;

        var tabButtonRow = Instance._tabBarButtonRow;
        if (tabButtonRow != null) tabButtonRow.SetActive(true);

        if (Instance._configPopupPage != null) Instance._configPopupPage.SetActive(false);
        SetTab(lastTabName);
    }
}
