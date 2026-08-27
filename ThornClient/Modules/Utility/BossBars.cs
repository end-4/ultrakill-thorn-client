using HarmonyLib;
using NukeLib.Game;
using NukeLib.UI;
using NukeLib.Utils;
using UnityEngine;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;
using ThornClient.Managers;
using ThornClient.System;
using TMPro;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ThornClient.Modules.Utility;

/// <summary>
/// Module that forces boss bars
/// </summary>
public class BossBars : Module {
    /// <summary>
    /// The single instance of this module
    /// </summary>
    public static BossBars? Instance;

    /// <summary>
    /// Icon of the module
    /// </summary>
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "health_bar");

    /// <summary>
    /// Tags for searching
    /// </summary>
    public override string[] Tags => ["blood", "hp", "health", "enemy", "damage"];

    public Setting<EnemyList> TargetEnemyTypes;
    public Setting<float> TargetHealthAmount;
    public Setting<EnemyList> ExcludedEnemyTypes;

    // public Setting<bool> ShowEnemyIcons;

    public BossBars() : base("thorn.bossBars", "Boss Bars",
        "Forces boss bar to show for selected enemies. (Configuration needed!)", ModuleCategory.Utility) {
        if (Instance != null) return;
        Instance = this;
        CreateHeader("conditions", "Conditions [OR-combined]",
            "Enemies satisfying one or more conditions below get a boss bar");
        TargetHealthAmount = CreateSetting("targetHealthAmount", "Has health >=",
            "Enemies having at least this much health will get a boss bar on them",
            9999f
        );
        TargetEnemyTypes = CreateSetting("targetEnemyTypes", "Enemy type whitelist",
            "Enemies of this type will have boss bars shown",
            new EnemyList());

        // CreateHeader("enhancements", "Enhancements", "Tweaks to make the bars look nicer");
        // ShowEnemyIcons = CreateSetting("showEnemyIcons", "Show enemy icons",
        //     "Shows an icon to the left of the health bars", true);

        CreateHeader("exclusions", "Exclusions",
            "Enemies satisfying one or more conditions below don't get a boss bar");
        ExcludedEnemyTypes = CreateSetting("excludedEnemyTypes", "Enemy type blacklist",
            "Tip: Idols are instakilled, so having a health bar might not help much",
            new EnemyList());
    }

    /// <inheritdoc />
    protected override void OnEnable() {
        UpdateAll(true);
        // UpdateAllBars();
        EnemyEvents.OnSpawn += UpdateEnemy;
        TargetEnemyTypes.OnChanged += UpdateAll;
        TargetHealthAmount.OnChanged += UpdateAll;
        // ShowEnemyIcons.OnChanged += UpdateAllBars;
        // Plugin.HarmonyInstance.PatchAll(typeof(BossHealthBarPatches));
    }

    /// <inheritdoc />
    protected override void OnDisable() {
        EnemyEvents.OnSpawn -= UpdateEnemy;
        TargetEnemyTypes.OnChanged -= UpdateAll;
        TargetHealthAmount.OnChanged -= UpdateAll;
        // ShowEnemyIcons.OnChanged -= UpdateAllBars;
        UpdateAll(false);

        // var original = AccessTools.Method(typeof(BossHealthBarTemplate), "Initialize");
        // var patchMethod = AccessTools.Method(typeof(BossHealthBarPatches), nameof(BossHealthBarPatches.Initialize_Postfix));
        // Plugin.HarmonyInstance.Unpatch(original, patchMethod);
    }

    private void UpdateAll() {
        UpdateAll(IsEnabled);
    }

    private void UpdateAll(bool enabled) {
        foreach (EnemyIdentifier enemy in Object.FindObjectsOfType<EnemyIdentifier>()) {
            UpdateEnemy(enemy, enabled);
        }
    }

    private void UpdateEnemy(EnemyIdentifier eid) {
        UpdateEnemy(eid, IsEnabled);
    }

    private void UpdateEnemy(EnemyIdentifier eid, bool enabled) {
        var toShow = enabled && (
            TargetEnemyTypes.Value.Includes(eid.enemyType)
            || eid.health >= TargetHealthAmount.Value
        ) && (
            !ExcludedEnemyTypes.Value.Includes(eid.enemyType)
        );
        eid.BossBar(toShow);
    }

    // /// <summary>
    // /// Updates the appearance on all health bars
    // /// </summary>
    // public void UpdateAllBars() {
    //     foreach (BossHealthBarTemplate bar in Object.FindObjectsOfType<BossHealthBarTemplate>()) {
    //         EnsureBossBarEnhancements(bar);
    //     }
    // }

    internal const string HealthBarObjectName = "HealthBarEnemyIcon";

    // /// <summary>
    // /// Ensures a certain boss bar has the enhancements as defined in BossBars module
    // /// </summary>
    // /// <param name="healthbarTemplateComponent"></param>
    // public static void EnsureBossBarEnhancements(BossHealthBarTemplate healthbarTemplateComponent) {
    //     if (Instance == null) return;
    //     bool showIcon = Instance.ShowEnemyIcons.Value;
    //     var bar = healthbarTemplateComponent.gameObject;
    //     var ico = bar.FindRecursive(HealthBarObjectName);
    //     if (!showIcon && ico != null && ico.activeSelf) ico.SetActive(false);
    //     if (showIcon) {
    //         if (ico == null) {
    //             ico = Object.Instantiate(AssetManager.Get<GameObject>(HudManager.BundleKey, HealthBarObjectName),
    //                 bar.transform);
    //             var icoComp = ico.GetOrAddComponent<EnemyIconController>();
    //             var nameObj = bar.FindRecursive("Panel/Filler")?.transform.GetChild(2)?.gameObject
    //                 .FindRecursive("HP Text");
    //             var text = nameObj?.GetComponent<TextMeshProUGUI>().text ?? "";
    //             icoComp.enemyName = text;
    //         }

    //         ico?.SetActive(true);
    //     }
    // }
}

// [HarmonyPatch(typeof(BossHealthBarTemplate))]
// internal static class BossHealthBarPatches {
//     [HarmonyPostfix]
//     [HarmonyPatch("Initialize")]
//     internal static void Initialize_Postfix(BossHealthBarTemplate __instance) {
//         try {
//             Plugin.Log.LogInfo("post init bar");
//             if (BossBars.Instance == null || !BossBars.Instance.IsEnabled) return;
//             BossBars.EnsureBossBarEnhancements(__instance);
//         } catch {
//             // Never let exceptions happen in patches
//         }
//     }
// }
