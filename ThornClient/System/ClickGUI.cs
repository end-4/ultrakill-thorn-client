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
using ThornClient.Managers;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ThornClient.System;

internal class ClickGUI : SystemModule {
    internal static ClickGUI? Instance;
    private const string PauseGameStateKey = "Thorn_ClickGUI";
    private static readonly string BundlePath = Path.Combine(Plugin.workingDir, "assets", "thorn_clickgui.bundle");
    public static readonly string BundleKey = "clickGui";

    private static GameObject? _layoutedPagePrefab;

    public ClickGUI() : base("thorn.clickGui", "ClickGUI", "The main interaction panel") {
        AssetManager.LoadBundle(BundleKey, BundlePath);
        // Note Keybind is registered in ThornModule
        Plugin.SafeSceneLoaded += (_, __) => _isInitialized = InitializeIfNeeded();
        Plugin.SafeSceneLoaded += (_, __) => {
            // Hide menu every toggle
            // Also ensures the initial show state is always hidden, so the user wouldn't have to press the bind twice
            if (IsEnabled) Toggle();
        };
    }

    private bool _isInitialized = false;
    private GameObject? _canvas;
    private GameObject? _tabBar;
    private GameObject? _modulePage;
    private GameObject? _hudPage;
    private GameObject? _settingsPage;
    private GameObject? _configPopupPage;
    private readonly Stack<GameObject> _panelStack = new();
    private GameObject? _tooltip;
    private GameObject? _tabBarButtonRow;
    private ModuleSearchWindowController? _moduleSearchController;
    private ModuleSearchWindowController? _hudModuleSearchController;
    private List<Tuple<string, GameObject>> _tabPages = new();
    private const string ModuleTabName = "Modules";
    private const string HudTabName = "HUD";
    private static readonly Vector2 PageHiddenOffset = Vector2.down * 15;

    private static OptionsManager? opts => OptionsManager.Instance;

    private bool InitializeIfNeeded() {
        if (Instance != null || (_isInitialized && _canvas != null)) return true;
        Instance = this;
        // Plugin.Log.LogInfo("Loading ClickGUI");

        var basePrefab = AssetManager.Get<GameObject>(BundleKey, "ThornClickGUICanvas");
        var tabBarPrefab = AssetManager.Get<GameObject>(BundleKey, "TabBar");
        _layoutedPagePrefab = AssetManager.Get<GameObject>(BundleKey, "LayoutedPage");
        var tooltipPrefab = AssetManager.Get<GameObject>(BundleKey, "Tooltip");
        var tabButtonPrefab = AssetManager.Get<GameObject>(BundleKey, "TabButton");
        var moduleCategoryPrefab = AssetManager.Get<GameObject>(BundleKey, "ModuleCategory");
        var searchPrefab = AssetManager.Get<GameObject>(BundleKey, "SearchWindow");

        // Make the canvas
        _canvas = Object.Instantiate(basePrefab);
        _canvas.hideFlags = HideFlags.DontSave;
        Object.DontDestroyOnLoad(_canvas);
        _canvas.SetActive(true); // For size updates to happen... will disable later down below, at least I think this should work

        // Tab bar
        _tabBar = Object.Instantiate(tabBarPrefab, _canvas.transform);
        _tabBar.SetActive(true);
        _tabBar.GetOrAddComponent<TabBarController>();

        // Make pages
        _modulePage = Object.Instantiate(_layoutedPagePrefab, _canvas.transform);
        _hudPage = Object.Instantiate(_layoutedPagePrefab, _canvas.transform);
        _settingsPage = Object.Instantiate(_layoutedPagePrefab, _canvas.transform);

        _tabPages.Add(Tuple.Create(ModuleTabName, _modulePage));
        _tabPages.Add(Tuple.Create(HudTabName, _hudPage));
        _tabPages.Add(Tuple.Create("Settings", _settingsPage));
        _lastTabName = "Modules";

        // Populate page: Module
        for (int i = 0; i < Enum.GetValues(typeof(ModuleCategory)).Length; i++) {
            var category = (ModuleCategory)i;
            if (category == ModuleCategory.Hud) continue;
            var categoryObj = Object.Instantiate(moduleCategoryPrefab);
            AddToLayoutedPage(_modulePage, categoryObj);
            var catController = categoryObj.GetOrAddComponent<ModuleCategoryController>();
            catController.Category = category;
            catController.SetupModules();
            categoryObj.UnfuckLayoutHack();
        }

        var searchWindowObj = Object.Instantiate(searchPrefab);
        AddToLayoutedPage(_modulePage, searchWindowObj);
        _moduleSearchController = searchWindowObj.AddComponent<ModuleSearchWindowController>();
        _moduleSearchController.ModuleFilter = module => (module is not SystemModule && module is not HudModule);

        _modulePage.UnfuckLayoutHack();

        // Populate page: HUD
        var hudCatObj = Object.Instantiate(moduleCategoryPrefab);
        AddToLayoutedPage(_hudPage, hudCatObj);
        var hudCatCtl = hudCatObj.GetOrAddComponent<ModuleCategoryController>();
        hudCatCtl.Category = ModuleCategory.Hud;
        hudCatCtl.SetupModules();
        hudCatObj.UnfuckLayoutHack();
        var hudCatRect = (RectTransform)hudCatObj.transform;
        hudCatObj.UnfuckLayoutHack();
        hudCatRect.pivot = new Vector2(0.5f, 0.5f);
        hudCatRect.localPosition = new Vector3(0f, 0f, 0f);

        var hudSearchWindowObj = Object.Instantiate(searchPrefab);
        AddToLayoutedPage(_hudPage, hudSearchWindowObj);
        _hudModuleSearchController = hudSearchWindowObj.AddComponent<ModuleSearchWindowController>();
        _hudModuleSearchController.ModuleFilter = module => (module is HudModule);

        _hudPage.UnfuckLayoutHack();

        // Populate page: Settings
        var settingsObj = Object.Instantiate(moduleCategoryPrefab);
        AddToLayoutedPage(_settingsPage, settingsObj);
        var settingRect = (RectTransform)settingsObj.transform;
        settingRect.pivot = new Vector2(0.5f, 0.5f);
        settingRect.localPosition = new Vector3(0f, 0f, 0f);
        var configController = settingsObj.GetOrAddComponent<ConfigurableWindowController>();
        configController.TargetConfigurable = ThornModule.Instance;
        // settingsObj.UnfuckLayoutHack();
        _settingsPage.UnfuckLayoutHack();

        // Add buttons to tab bar
        _tabBarButtonRow = _tabBar.FindRecursive("Tabs");
        foreach (var tup in _tabPages) {
            var key = tup.Item1;
            var tabButton = Object.Instantiate(tabButtonPrefab, _tabBarButtonRow.transform);
            tabButton.FindRecursive("Text").GetComponent<TextMeshProUGUI>().text = key;
            tabButton.GetComponent<Button>().onClick.AddListener(() => { SetTab(key); });

            tup.Item2.SetActiveAnimated(false, PageHiddenOffset); // Add animation component
        }

        _tabBarButtonRow.UnfuckLayoutHack();

        // Tooltip
        _tooltip = Object.Instantiate(tooltipPrefab, _canvas.transform);
        _tooltip.SetActive(false);

        // Hide canvas
        var cgroupComp = _canvas.GetComponent<CanvasGroup>();
        if (cgroupComp != null) cgroupComp.alpha = 1;
        SetTab(ModuleTabName);
        _canvas.SetActive(false);
        return true;
    }

    private static GameObject AddToLayoutedPage(GameObject page, GameObject item) {
        var layout = page.FindRecursive("Layout")?.transform;
        if (layout == null || item == null) return item!;
        item.transform.SetParent(layout, false);
        return item;
    }

    protected override void OnEnable() {
        // Plugin.Log.LogInfo($"[ClickGUI] Enable");
        if (_canvas == null) return;
        _canvas.transform.SetAsLastSibling(); // Show on top of everything
        _canvas.SetActive(true);
        var canvasComp = _canvas.GetComponent<Canvas>();
        if (canvasComp != null) {
            canvasComp.sortingOrder = 69; // On top of most things
        }

        _canvas.UnfuckLayoutHack();

        if (ThornModule.Instance?.MenuPausesGame.Value ?? true) Pauser.Pause(true, PauseGameStateKey);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PerformInitialFocus();
    }

    protected override void OnDisable() {
        // Plugin.Log.LogInfo($"[ClickGUI] Disable");
        if (_canvas == null) return;
        _canvas.SetActive(false);
        Pauser.Pause(true, PauseGameStateKey); // Ensure consistent cursor appearance state
        Pauser.Pause(false, PauseGameStateKey);
        if (_tooltip != null) _tooltip.SetActive(false);
        // while (_panelStack.Count > 0) {
        //     var topPanel = Instance._panelStack.Pop();
        //     Object.Destroy(topPanel);
        // }
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

    public static string GetTooltipText() {
        if (Instance == null || Instance._tooltip == null) return "";
        var textObj = Instance._tooltip.FindRecursive("Text");
        if (textObj == null) return "";
        return textObj.GetComponent<TextMeshProUGUI>().text;
    }

    public static void SetTooltipText(string text) {
        var wrappedText = text.WrapText(30);
        if (Instance == null || Instance._tooltip == null) return;
        var textObj = Instance._tooltip.FindRecursive("Text");
        if (textObj == null) return;
        textObj.GetComponent<TextMeshProUGUI>().text = wrappedText;
        Instance._tooltip.SetActive(true);
        Instance._tooltip.UnfuckLayoutHack();
        Instance.UpdateTooltipPos();
    }

    public static void SurrenderTooltipText(string text, bool force = false) {
        var wrappedText = text.WrapText(30);
        if (Instance == null || Instance._tooltip == null) return;
        var textObj = Instance._tooltip.FindRecursive("Text");
        if (textObj == null) return;
        var comp = textObj.GetComponent<TextMeshProUGUI>();
        if (comp.text == wrappedText || force) {
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
            bool active = (tabName == key);
            // Plugin.Log.LogInfo($"Set {tabName} active: {active}");
            // val.SetActiveAnimated(active, PageHiddenOffset);
            val.SetActive(active);
            val.UnfuckLayoutHack();
            if (val != null && tabName == key) {
                _lastTabName = tabName;
                currIndex = i;
            }
        }

        if (tabButtonRow != null) {
            for (int i = 0; i < tabButtonRow.transform.childCount; i++) {
                bool active = (i == currIndex);
                bool atLeft = (i == 0);
                bool atRight = (i + 1 == pages.Count);
                var newSprite = ConnectedButtonGroupSettingController.GetSprite(atLeft, atRight, active);
                Color targetColor = active ? ThornModule.AccentColor : Color.white;
                Color targetTextColor = active ? Color.black : Color.white;

                var btnObj = tabButtonRow.transform.GetChild(i);
                var img = btnObj?.GetComponent<Image>();
                var txt = btnObj?.gameObject.FindRecursive("Text");
                if (btnObj == null || img == null || txt == null) continue;
                img.sprite = newSprite;
                img.color = targetColor;
                txt.GetComponent<TextMeshProUGUI>().color = targetTextColor;
            }
        }

        Instance.PerformInitialFocus();
    }

    private void PerformInitialFocus() {
        if (_lastTabName == ModuleTabName) {
            if (_moduleSearchController != null) _moduleSearchController.FocusSearch();
        } else if (_lastTabName == HudTabName) {
            if (_hudModuleSearchController != null) _hudModuleSearchController.FocusSearch();
        }
    }

    /// <summary>
    /// Cycles through the tabs by an offset (e.g. +1 for next, -1 for previous).
    /// </summary>
    /// <param name="diff">The number of steps</param>
    public static void CycleTab(int diff) {
        if (Instance == null || Instance._tabPages.Count == 0 || !Instance.IsEnabled) return;

        int currentIndex = Instance._tabPages.FindIndex(p => p.Item1 == _lastTabName);
        if (currentIndex == -1) currentIndex = 0;

        int totalCount = Instance._tabPages.Count;
        int newIndex = ((currentIndex + diff) % totalCount + totalCount) % totalCount;

        SetTab(Instance._tabPages[newIndex].Item1);
    }

    private static string _lastTabName = "";

    /// <summary>
    /// Puts the given content onto a new nested panel on top of the current view, hiding what's underneath.
    /// </summary>
    /// <param name="panelContent">The content to display inside the new panel.</param>
    public static void NestPanel(GameObject panelContent) {
        if (Instance == null || Instance._canvas == null || _layoutedPagePrefab == null) return;

        // Create the page container
        var page = Object.Instantiate(_layoutedPagePrefab, Instance._canvas.transform);

        // Parent and center the content
        AddToLayoutedPage(page, panelContent);
        var settingRect = (RectTransform)panelContent.transform;
        settingRect.pivot = new Vector2(0.5f, 0.5f);
        settingRect.localPosition = Vector3.zero;

        // Hide tab bar and current top-level view
        if (Instance._tabBarButtonRow != null) Instance._tabBarButtonRow.SetActive(false);
        if (Instance._panelStack.Count > 0) {
            Instance._panelStack.Peek().SetActive(false);
        } else {
            Instance._tabPages.ForEach(p => p.Item2.SetActive(false));
        }

        // Show and push new panel
        page.SetActive(true);
        Instance._panelStack.Push(page);

        SurrenderTooltipText("", force: true);
    }

    /// <summary>
    /// Spawns content on the currently active panel
    /// </summary>
    /// <param name="content"></param>
    public static void SpawnContent(GameObject content) {
        if (Instance == null || content == null) return;

        GameObject? activePanel = null;
        if (Instance._panelStack.Count > 0) {
            activePanel = Instance._panelStack.Peek();
        } else {
            activePanel = Instance._tabPages.FirstOrDefault(p => p.Item1 == _lastTabName)?.Item2;
        }

        if (activePanel == null) return;
        AddToLayoutedPage(activePanel, content);
    }

    public static void NavigateBack() {
        if (Instance == null || !Instance.IsEnabled) return;

        if (Instance._panelStack.Count > 0) {
            var topPanel = Instance._panelStack.Pop();
            Object.Destroy(topPanel);

            if (Instance._panelStack.Count > 0) {
                Instance._panelStack.Peek().SetActive(true);
            } else {
                SetTab(_lastTabName);
                if (Instance._tabBarButtonRow != null) Instance._tabBarButtonRow.SetActive(true);
            }
        } else if (Instance.IsEnabled) {
            Instance.Toggle();
        }

        SurrenderTooltipText("", force: true);
    }
}
