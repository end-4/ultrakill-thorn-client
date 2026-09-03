using NukeLib.Utils;
using ThornClient.HUD;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;
using UnityEngine.UI;
using NotImplementedException = System.NotImplementedException;

namespace ThornClient.Modules.HUD;

/// <summary>
/// HUD module that shows style multiplier
/// </summary>
public class StyleRank : FramedHudModule {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "style_rank");

    /// <inheritdoc />
    public override string[] Tags => [""];

    /// <inheritdoc />
    public StyleRank() : base("thorn.styleRank", "Style Rank", "Shows the multiplier for air/slide time") {

    }

    protected override GameObject CreateContentObject() {
        var obj = Object.Instantiate(AssetManager.Get<GameObject>(HudManager.BundleKey, "StyleRankContent"));
        var comp = obj?.GetOrAddComponent<StyleRankSyncer>();
        return obj;
    }

    protected class StyleRankSyncer : MonoBehaviour {
        private Image? _styleImg;

        private void Awake() {
            _styleImg = gameObject.GetComponent<Image>();
        }

        private void Update() {
            var shud = StyleHUD.Instance;
            if (shud == null || _styleImg == null) return;
            var curr = shud.currentRank.sprite;
            if (curr != _styleImg.sprite) _styleImg.sprite = curr;
        }
    }
}
