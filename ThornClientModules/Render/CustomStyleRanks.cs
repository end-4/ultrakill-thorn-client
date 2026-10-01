using System.Collections.Generic;
using System.IO;
using NukeLib.Utils;
using UnityEngine;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ThornClient.Managers;
using ThornClient.System;
using System.Linq;
using NukeLib.UI;
using ThornClient;
using ThornClient.Core.DataTypes;

namespace ThornClientModules.Render;

/// <summary>
/// Module that changes the scale of the user interface
/// </summary>
public class CustomStyleRanks : Module {
    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "style_rank");

    /// <summary>
    /// The directory containing the default style rank images of this module
    /// </summary>
    public static readonly string DefaultStyleDir = Path.Combine(
        ModulesBundleInfo.workingDir, "assets", "skins", "style_ranks", "glow"
    );

    /// <inheritdoc />
    public override string[] Tags => [
        "rank title swapper", "tier", "ultrakill", "ssshitstorm", "ssadistic", "supreme", "anarchic", "brutal",
        "chaotic", "destructive"
    ];

    /// <summary>
    /// Rank image settings
    /// </summary>
    public readonly Dictionary<string, Setting<FilePath>> Ranks = [];

    public static readonly string[] RankKeys = [
        "RankD", "RankC", "RankB", "RankA", "RankS", "RankSS", "RankSSS", "RankU"
    ];

    /// <summary>
    /// Constructor
    /// </summary>
    public CustomStyleRanks() : base("thorn.customStyleRank", "Custom Style Ranks", "Changes the style rank image",
        ModuleCategory.Render) {
        Ranks["RankD"] = CreateSetting(
            "rankDImage",
            "<color=#0094ff>D</color>ESTRUCTIVE",
            "Image for the DESTRUCTIVE style rank", Path.Combine(DefaultStyleDir, "RankD.png").ToFilePath());
        Ranks["RankC"] = CreateSetting(
            "rankCImage",
            "<color=#4cff00>C</color>HAOTIC",
            "Image for the CHAOTIC style rank",
            Path.Combine(DefaultStyleDir, "RankC.png").ToFilePath());
        Ranks["RankB"] = CreateSetting(
            "rankBImage",
            "<color=#ffd800>B</color>RUTAL", "Image for the BRUTAL style rank",
            Path.Combine(DefaultStyleDir, "RankB.png").ToFilePath());
        Ranks["RankA"] = CreateSetting(
            "rankAImage",
            "<color=#ff6a00>A</color>NARCHIC",
            "Image for the ANARCHIC style rank",
            Path.Combine(DefaultStyleDir, "RankA.png").ToFilePath());
        Ranks["RankS"] = CreateSetting(
            "rankSImage",
            "<color=#ff0000>S</color>UPREME",
            "Image for the SUPREME style rank",
            Path.Combine(DefaultStyleDir, "RankS.png").ToFilePath());
        Ranks["RankSS"] = CreateSetting(
            "rankSSImage",
            "<color=#d10000>S</color><color=#ff0000>S</color>ADISTIC",
            "Image for the SSADISTIC style rank", Path.Combine(DefaultStyleDir, "RankSS.png").ToFilePath());
        Ranks["RankSSS"] = CreateSetting(
            "rankSSSImage",
            "<color=#d10000>SS</color><color=#ff0000>S</color>HITSTORM",
            "Image for the SSSHITSTORM style rank", Path.Combine(DefaultStyleDir, "RankSSS.png").ToFilePath());
        Ranks["RankU"] = CreateSetting(
            "rankUImage",
            "<color=#ffd800>ULTRAKILL</color>",
            "Image for the ULTRAKILL style rank",
            Path.Combine(DefaultStyleDir, "RankU.png").ToFilePath());
    }

    private readonly Dictionary<string, Sprite> DefaultSprites = [];

    /// <inheritdoc />
    protected override void OnEnable() {
        SaveDefaultRanks();
        foreach (var setting in Ranks.Values) {
            setting.OnChanged += DoReplacement;
        }
        SceneUtils.SafeSceneLoadedNoParam += OnSceneLoaded;

        DoReplacement();
    }

    /// <inheritdoc />
    protected override void OnDisable() {
        foreach (var setting in Ranks.Values) {
            setting.OnChanged -= DoReplacement;
        }
        SceneUtils.SafeSceneLoadedNoParam -= OnSceneLoaded;

        LoadDefaultRanks();
    }

    private void OnSceneLoaded() {
        SaveDefaultRanks();
        DoReplacement();
    }

    private void SaveDefaultRanks() {
        var shud = StyleHUD.Instance;
        if (shud == null) return;
        foreach (var rank in shud.ranks) {
            var name = rank.sprite.name;
            if (!RankKeys.Contains(name)) continue;
            DefaultSprites[name] = rank.sprite;
        }
    }

    private void LoadDefaultRanks() {
        var shud = StyleHUD.Instance;
        if (shud == null) return;
        foreach (var rank in shud.ranks) {
            var name = rank.sprite.name;
            if (!DefaultSprites.ContainsKey(name)) continue;
            rank.sprite = DefaultSprites[name];
        }
    }

    private void DoReplacement() {
        var shud = StyleHUD.Instance;
        if (shud == null) return;
        foreach (var rank in shud.ranks) {
            // Lookup
            var name = rank.sprite.name;
            if (!Ranks.TryGetValue(name, out var targetSetting)) continue;

            // Destroy old sprite if not default
            if (!DefaultSprites.ContainsValue(rank.sprite)) Object.Destroy(rank.sprite);

            // Load new sprite
            var targetPath = targetSetting.Value.ToString();
            if (!File.Exists(targetPath)) continue;
            var newSprite = FileAssetUtils.LoadNewSprite(targetPath);
            newSprite.name = name;
            rank.sprite = newSprite;
        }
    }
}
