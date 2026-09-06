using System;
using System.Collections.Generic;
using System.Linq;
using NukeLib.Game;
using NukeLib.Utils;
using UnityEngine;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;
using ThornClient.Managers;
using ThornClient.System;

namespace ThornClient.Modules.Render;

/// <summary>
/// Module that applies an "edges" look to enemies
/// </summary>
public class Edges : Module {
    /// <summary>
    /// The instance of this module
    /// </summary>
    public static Edges? Instance;

    /// <inheritdoc />
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "icosahedron");

    /// <inheritdoc />
    public override string[] Tags => ["wireframe", "world"];

    public Setting<float> EdgeThickness;
    public Setting<EnhancedColor> FillColor;
    public Setting<bool> SwapEdgeAndFill;
    public Dictionary<EnemyType, Setting<EnhancedColor>> EnemyEdgeColors = [];

    private static readonly Dictionary<EnemyType, EnhancedColor> _defaultEnemyColors = new() {
        { EnemyType.Cerberus, 0xFF9402.ToEnhancedColor() },
        { EnemyType.Drone, 0x9A00FF.ToEnhancedColor() },
        { EnemyType.HideousMass, 0xFF6D73.ToEnhancedColor() },
        { EnemyType.Filth, 0xADD571.ToEnhancedColor() },
        { EnemyType.MaliciousFace, 0xFFD19B.ToEnhancedColor() },
        { EnemyType.Mindflayer, 0x00FFCB.ToEnhancedColor() },
        { EnemyType.Streetcleaner, 0xFC710F.ToEnhancedColor() },
        { EnemyType.Swordsmachine, 0xFFC609.ToEnhancedColor() },
        { EnemyType.V2, 0xFF0000.ToEnhancedColor() },
        { EnemyType.Virtue, 0x00A8FF.ToEnhancedColor() },
        { EnemyType.Wicked, 0x7D7D7D.ToEnhancedColor() },
        { EnemyType.Minos, 0xF100FF.ToEnhancedColor() },
        { EnemyType.Stalker, 0xFFFF2A.ToEnhancedColor() },
        { EnemyType.Stray, 0xFF564A.ToEnhancedColor() },
        { EnemyType.Schism, 0xB8782C.ToEnhancedColor() },
        { EnemyType.Soldier, 0x5DA9FF.ToEnhancedColor() },
        { EnemyType.Gabriel, 0xF3AA36.ToEnhancedColor() },
        { EnemyType.FleshPrison, 0xA66157.ToEnhancedColor() },
        { EnemyType.MinosPrime, 0x86B9FF.ToEnhancedColor() },
        { EnemyType.Sisyphus, 0xFF71B5.ToEnhancedColor() },
        { EnemyType.Turret, 0xCFFF00.ToEnhancedColor() },
        { EnemyType.Idol, 0x8F80FF.ToEnhancedColor() },
        { EnemyType.V2Second, 0xFF0000.ToEnhancedColor() },
        { EnemyType.CancerousRodent, 0x40FF40.ToEnhancedColor() },
        { EnemyType.VeryCancerousRodent, 0x40FF40.ToEnhancedColor() },
        { EnemyType.Mandalore, 0xFFAACB.ToEnhancedColor() },
        { EnemyType.Ferryman, 0x00C3B8.ToEnhancedColor() },
        { EnemyType.Leviathan, 0x00C3B8.ToEnhancedColor() },
        { EnemyType.GabrielSecond, 0xFF3900.ToEnhancedColor() },
        { EnemyType.SisyphusPrime, 0xFFB322.ToEnhancedColor() },
        { EnemyType.FleshPanopticon, 0xFF9362.ToEnhancedColor() },
        { EnemyType.Mannequin, 0xF0C4D6.ToEnhancedColor() },
        { EnemyType.Minotaur, 0xFFFFFF.ToEnhancedColor() },
        { EnemyType.Gutterman, 0xFF8D39.ToEnhancedColor() },
        { EnemyType.Guttertank, 0xFF2C2C.ToEnhancedColor() },
        { EnemyType.Centaur, 0xDDE8F3.ToEnhancedColor() },
        { EnemyType.Puppet, 0xCB0000.ToEnhancedColor() },
        { EnemyType.BigJohnator, 0xFF1445.ToEnhancedColor() },
        { EnemyType.Providence, 0xFFFFDA.ToEnhancedColor() },
        { EnemyType.Deathcatcher, 0xFF0B00.ToEnhancedColor() },
        { EnemyType.Power, 0xF8CA32.ToEnhancedColor() },
        { EnemyType.MirrorReaper, 0xC061A9.ToEnhancedColor() },
        { EnemyType.Geryon, 0xFF564A.ToEnhancedColor() },
    };

    /// <summary>
    /// Constructor
    /// </summary>
    public Edges() : base("thorn.edges", "Edges (Enemies)", "Applies wireframe shader to enemies", ModuleCategory.Render) {
        if (Instance != null) return;
        Instance = this;
        CreateHeader("enemies", "Enemies", "Changes applied to newly spawned enemies");
        EdgeThickness = CreateSetting("edgeThickness", "Edge thickness", "How thick the edge lines are", 2f);
        FillColor = CreateSetting("fillColor", "Fill color", "The color that fills the faces", new EnhancedColor(0, 0, 0));
        SwapEdgeAndFill = CreateSetting("swapEdgeAndFill", "Swap edge and fill colors",
            "Makes edge colors apply to fill and fill color apply to edges", false);

        var enemyColorGroup = CreateGroup("enemyColors", "Enemy Colors", "Colors for each enemy type");
        var enemyTypes = Enum.GetValues(typeof(EnemyType)).Cast<EnemyType>().OrderBy(e => e.ToString());

        foreach (var enemyType in enemyTypes) {
            var defaultColor = _defaultEnemyColors.TryGetValue(enemyType, out var color) ? color : new EnhancedColor(Color.red);
            var setting = CreateSetting(
                $"enemyColor_{enemyType}",
                $"{enemyType}",
                $"The edge color for {enemyType}",
                defaultColor,
                enemyColorGroup
            );
            EnemyEdgeColors[enemyType] = setting;
        }

        SwapEdgeAndFill.OnChanged += UpdateAllMats;
    }

    private static Dictionary<EnemyType, Material> _enemyMatCache = [];
    private readonly Dictionary<EnemyType, Action> _onColorChangedActions = [];

    private Color GetFillColor(Color edgeColor) {
        return edgeColor;
    }

    private void UpdateAllMats() {
        foreach (var pair in _enemyMatCache) {
            var eType = pair.Key;
            var mat = pair.Value;
            mat.SetColor(SwapEdgeAndFill.Value ? "_WireframeColor" : "_FillColor", GetFillColor(FillColor.Value.GetCurrentColor()));
            mat.SetColor(SwapEdgeAndFill.Value ? "_FillColor" : "_WireframeColor", EnemyEdgeColors[eType].Value.GetCurrentColor());
            mat.SetFloat("_WireframeThickness", EdgeThickness.Value);
        }
    }

    /// <summary>
    /// Gets (cached) wireframe enemy material
    /// </summary>
    /// <param name="enemyType">The enemy type</param>
    /// <returns>The material for the enemy</returns>
    public static Material? GetEnemyMat(EnemyType enemyType) {
        if (Instance == null) return null;
        if (_enemyMatCache.TryGetValue(enemyType, out var cached) && cached != null)
            return cached;

        var baseMat = EffectManager.GetMaterial("GeometryWireframeMaterial");
        if (baseMat == null) {
            Plugin.Log.LogError("[Edges] Failed to load GeometryWireframeMaterial asset!");
            return null;
        }

        var enemyColor = new EnhancedColor(Color.red);
        if (Instance.EnemyEdgeColors.TryGetValue(enemyType, out var colorSetting)) {
            enemyColor = colorSetting.Value;
        }

        var mat = new Material(baseMat) {
            color = enemyColor.GetCurrentColor()
        };

        var edgeColor = enemyColor.GetCurrentColor();
        var fillColor = Instance.FillColor.Value.GetCurrentColor();
        mat.SetColor("_WireframeColor", Instance.SwapEdgeAndFill.Value ? fillColor : edgeColor);
        mat.SetColor("_FillColor", Instance.SwapEdgeAndFill.Value ? edgeColor : fillColor);
        mat.SetFloat("_WireframeThickness", Instance.EdgeThickness.Value);
        // Plugin.Log.LogInfo($"Enemy color {enemyColor}");
        _enemyMatCache[enemyType] = mat;
        return mat;
    }

    private void OnEnemyColorChanged(EnemyType enemyType, EnhancedColor newColor) {
        if (_enemyMatCache.TryGetValue(enemyType, out var mat) && mat != null) {
            mat.color = newColor.GetCurrentColor();
            mat.SetColor(SwapEdgeAndFill.Value ? "_FillColor" : "_WireframeColor", newColor.GetCurrentColor());
            mat.SetFloat("_WireframeThickness", EdgeThickness.Value);
        }
    }

    /// <inheritdoc />
    protected override void OnEnable() {
        EnemyEvents.OnSpawn += AddWireframizer;
        FillColor.OnChanged += UpdateAllMats;
        FillColor.OnEffectiveValueChanged += UpdateAllMats;
        EdgeThickness.OnChanged += UpdateAllMats;
        UpdateAllMats();

        foreach (var pair in EnemyEdgeColors) {
            var enemyType = pair.Key;
            var setting = pair.Value;
            Action action = () => OnEnemyColorChanged(enemyType, setting.Value);
            _onColorChangedActions[enemyType] = action;
            setting.OnChanged += action;
            setting.OnEffectiveValueChanged += action;
            OnEnemyColorChanged(enemyType, setting.Value);
        }
    }

    /// <inheritdoc />
    protected override void OnDisable() {
        EnemyEvents.OnSpawn -= AddWireframizer;
        FillColor.OnChanged -= UpdateAllMats;
        FillColor.OnEffectiveValueChanged -= UpdateAllMats;
        EdgeThickness.OnChanged -= UpdateAllMats;

        foreach (var pair in EnemyEdgeColors) {
            if (_onColorChangedActions.TryGetValue(pair.Key, out var action)) {
                pair.Value.OnChanged -= action;
                pair.Value.OnEffectiveValueChanged -= action;
            }
        }

        _onColorChangedActions.Clear();
    }

    private void AddWireframizer(EnemyIdentifier eid) {
        var comp = eid.gameObject.AddComponent<EnemyWireframizer>();
        comp.TargetEnemyType = eid.enemyType;
    }

    private class EnemyWireframizer : MonoBehaviour {
        public EnemyType? TargetEnemyType;
        private Renderer[] _renderers = [];

        private void Start() {
            if (TargetEnemyType == null) return;
            ExecutionUtils.RunNextFrame(FindReplacementTargets);
        }

        private void FindReplacementTargets() {
            _renderers = gameObject?.GetComponentsInChildren<Renderer>(true)?
                .Where(r => r is (MeshRenderer or SkinnedMeshRenderer))?
                .ToArray() ?? [];
        }

        private void Update() {
            if (TargetEnemyType == null || _renderers.Length == 0) return;

            var mat = Edges.GetEnemyMat((EnemyType)TargetEnemyType);
            if (mat == null) return;

            foreach (var comp in _renderers) {
                if (comp == null) continue;

                List<Material> sharedMats = [];
                comp.GetSharedMaterials(sharedMats);
                int count = sharedMats.Count;
                if (count == 0) continue;

                // Check if we really have to update
                bool needsUpdate = false;
                for (int i = 0; i < count; i++) {
                    if (sharedMats[i] != mat) {
                        needsUpdate = true;
                        break;
                    }
                }

                // Apply change if needed
                if (needsUpdate) {
                    Material[] newMats = new Material[count];
                    for (int i = 0; i < count; i++) {
                        newMats[i] = mat;
                    }

                    comp.sharedMaterials = newMats;
                }
            }
        }
    }
}
