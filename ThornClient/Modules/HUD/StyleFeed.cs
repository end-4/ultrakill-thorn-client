using System;
using System.Reflection;
using NukeLib.Game;
using NukeLib.UI;
using NukeLib.Utils;
using ThornClient.Core.ConfigurableElements;
using ThornClient.HUD;
using ThornClient.Managers;
using ThornClient.System;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ThornClient.Modules.HUD;

/// <summary>
/// HUD module that shows style multiplier
/// </summary>
public class StyleFeed : FramedHudModule {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "style_bonuses");

    /// <inheritdoc />
    public override string[] Tags => ["points", "bonus"];

    public Setting<bool> ShowOnlyNamedPointAdditions;
    public Setting<bool> ShowPoints;

    /// <inheritdoc />
    public StyleFeed() : base("thorn.styleFeed", "Style Feed", "Shows + Style bonuses") {
        ShowOnlyNamedPointAdditions = CreateSetting("showOnlyNamedPointAdditions", "Show only named point additions",
            "Ignores style gains from simply damaging enemies", true);
        ShowPoints = CreateSetting("showPoints", "Show points", "Show number of style points gained next to each bonus",
            true);
    }

    protected override GameObject CreateContentObject() {
        var content = Object.Instantiate(AssetManager.Get<GameObject>(HudManager.BundleKey, "StyleFeedLayout"));
        var comp = content.GetOrAddComponent<StyleFeedController>();
        comp.ModuleInstance = this;
        return content;
    }

    /// <summary>
    /// Component that controls the style feed
    /// </summary>
    protected class StyleFeedController : MonoBehaviour {
        public StyleFeed? ModuleInstance;

        private static GameObject? StyleItemPrefab =>
            AssetManager.Get<GameObject>(HudManager.BundleKey, "StylePointItem");

        private GameObject? _itemColumn;

        private void Start() {
            _itemColumn = gameObject.FindRecursive("StyleFeedColumn");
        }

        private void OnEnable() {
            StyleHelper.StylePointAdded += OnStylePointAdded;
        }

        private void OnDisable() {
            StyleHelper.StylePointAdded -= OnStylePointAdded;
        }

        private void OnStylePointAdded(StylePointEvent pointEvent) {
            if (ModuleInstance == null || _itemColumn == null) return;
            if (ModuleInstance.ShowOnlyNamedPointAdditions.Value &&
                StyleHUD.Instance?.GetLocalizedName(pointEvent.pointID) == "") return;
            var newItem = Instantiate(StyleItemPrefab, _itemColumn.transform);
            if (newItem == null) return;
            var comp = newItem.AddComponent<StyleItemController>();
            comp.AddedStylePointEvent = pointEvent;
            comp.ModuleInstance = ModuleInstance;
            _itemColumn.UnfuckLayoutHack();
            ExecutionUtils.RunNextFrame(() => {
                if (_itemColumn != null) _itemColumn.UnfuckLayoutHack();
            });
        }
    }

    /// <summary>
    /// Component that controls a style item
    /// </summary>
    protected class StyleItemController : MonoBehaviour {
        public StyleFeed? ModuleInstance;
        /// <summary>
        /// The style point event this item handles
        /// </summary>
        public StylePointEvent AddedStylePointEvent;

        /// <summary>
        /// Destroy text item after how long (in seconds)
        /// </summary>
        public float DestroyDelay = 3f;

        private void Start() {
            if (ModuleInstance == null) return;
            var txPts = gameObject.FindRecursive("Points")?.GetComponent<TextMeshProUGUI>();
            var txName = gameObject.FindRecursive("Name/Content")?.GetComponent<TextMeshProUGUI>();
            var pts = AddedStylePointEvent.points;
            var formattedName = StyleHelper.GetFormattedString(AddedStylePointEvent);
            var content = formattedName.StartsWith("+ ") ? formattedName[2..] : formattedName;

            if (ModuleInstance.ShowPoints.Value) txPts?.SetText($"{pts}");
            else txPts?.gameObject.SetActive(false);
            txName?.SetText(content);
            gameObject.UnfuckLayoutHack();
            Destroy(gameObject, DestroyDelay);
        }
    }
}
