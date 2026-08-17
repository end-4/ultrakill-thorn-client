using System;
using System.Collections.Generic;
using System.Linq;
using NukeLib.Game;
using NukeLib.Utils;
using UnityEngine;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;

namespace ThornClient.Modules.Render;

/// <summary>
/// Module that makes enemies edges.
/// </summary>
public class Edges : Module {
    /// <summary>
    /// The instance of this module
    /// </summary>
    public static Edges? Instance;

    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "icosahedron");

    public Setting<float> EdgeThickness;
    public Setting<Color> FillColor;
    public Setting<bool> SwapEdgeAndFill;
    public Dictionary<EnemyType, Setting<Color>> EnemyEdgeColors = [];

    private static readonly Dictionary<EnemyType, Color> _defaultEnemyColors = new() {
        { EnemyType.Cerberus, 0xFF9402.ToColor32() },
        { EnemyType.Drone, 0x9A00FF.ToColor32() },
        { EnemyType.HideousMass, 0xFF6D73.ToColor32() },
        { EnemyType.Filth, 0xADD571.ToColor32() },
        { EnemyType.MaliciousFace, 0xFFD19B.ToColor32() },
        { EnemyType.Mindflayer, 0x00FFCB.ToColor32() },
        { EnemyType.Streetcleaner, 0xFC710F.ToColor32() },
        { EnemyType.Swordsmachine, 0xFFC609.ToColor32() },
        { EnemyType.V2, 0xFF0000.ToColor32() },
        { EnemyType.Virtue, 0x00A8FF.ToColor32() },
        { EnemyType.Wicked, 0x7D7D7D.ToColor32() },
        { EnemyType.Minos, 0xF100FF.ToColor32() },
        { EnemyType.Stalker, 0xFFFF2A.ToColor32() },
        { EnemyType.Stray, 0xFF564A.ToColor32() },
        { EnemyType.Schism, 0xB8782C.ToColor32() },
        { EnemyType.Soldier, 0x5DA9FF.ToColor32() },
        { EnemyType.Gabriel, 0xF3AA36.ToColor32() },
        { EnemyType.FleshPrison, 0xA66157.ToColor32() },
        { EnemyType.MinosPrime, 0x86B9FF.ToColor32() },
        { EnemyType.Sisyphus, 0xFF71B5.ToColor32() },
        { EnemyType.Turret, 0xCFFF00.ToColor32() },
        { EnemyType.Idol, 0x8F80FF.ToColor32() },
        { EnemyType.V2Second, 0xFF0000.ToColor32() },
        { EnemyType.CancerousRodent, 0x40FF40.ToColor32() },
        { EnemyType.VeryCancerousRodent, 0x40FF40.ToColor32() },
        { EnemyType.Mandalore, 0xFFAACB.ToColor32() },
        { EnemyType.Ferryman, 0x00C3B8.ToColor32() },
        { EnemyType.Leviathan, 0x00C3B8.ToColor32() },
        { EnemyType.GabrielSecond, 0xFF3900.ToColor32() },
        { EnemyType.SisyphusPrime, 0xFFB322.ToColor32() },
        { EnemyType.FleshPanopticon, 0xFF9362.ToColor32() },
        { EnemyType.Mannequin, 0xF0C4D6.ToColor32() },
        { EnemyType.Minotaur, 0xFFFFFF.ToColor32() },
        { EnemyType.Gutterman, 0xFF8D39.ToColor32() },
        { EnemyType.Guttertank, 0xFF2C2C.ToColor32() },
        { EnemyType.Centaur, 0xDDE8F3.ToColor32() },
        { EnemyType.Puppet, 0xCB0000.ToColor32() },
        { EnemyType.BigJohnator, 0xFF1445.ToColor32() },
        { EnemyType.Providence, 0xFFFFDA.ToColor32() },
        { EnemyType.Deathcatcher, 0xFF0B00.ToColor32() },
        { EnemyType.Power, 0xF8CA32.ToColor32() },
        { EnemyType.MirrorReaper, 0xC061A9.ToColor32() },
        { EnemyType.Geryon, 0xFF564A.ToColor32() },
    };

    /// <summary>
    /// Constructor
    /// </summary>
    public Edges() : base("thorn.edges", "Edges", "Applies wireframe shader to stuff", ModuleCategory.Render) {
        if (Instance != null) return;
        Instance = this;
        CreateHeader("enemies", "Enemies", "Changes applied to newly spawned enemies");
        EdgeThickness = CreateSetting("edgeThickness", "Edge thickness", "How thick the edge lines are", 2f);
        FillColor = CreateSetting("fillColor", "Fill color", "The color that fills the faces", new Color(0, 0, 0, 1f));
        SwapEdgeAndFill = CreateSetting("swapEdgeAndFill", "Swap edge and fill colors",
            "Makes edge colors apply to fill and fill color apply to edges", false);

        var enemyColorGroup = CreateGroup("enemyColors", "Enemy Colors", "Colors for each enemy type");
        var enemyTypes = Enum.GetValues(typeof(EnemyType)).Cast<EnemyType>().OrderBy(e => e.ToString());

        foreach (var enemyType in enemyTypes) {
            var defaultColor = _defaultEnemyColors.TryGetValue(enemyType, out var color) ? color : Color.red;
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
    private readonly Dictionary<EnemyType, Action<Color>> _onColorChangedActions = [];

    private Color GetFillColor(Color edgeColor) {
        return edgeColor;
    }

    private void UpdateAllMats() {
        foreach (var pair in _enemyMatCache) {
            var eType = pair.Key;
            var mat = pair.Value;
            mat.SetColor(SwapEdgeAndFill.Value ? "_WireframeColor" : "_FillColor", GetFillColor(FillColor.Value));
            mat.SetColor(SwapEdgeAndFill.Value ? "_FillColor" : "_WireframeColor", EnemyEdgeColors[eType].Value);
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

        var enemyColor = Color.red;
        if (Instance.EnemyEdgeColors.TryGetValue(enemyType, out var colorSetting)) {
            enemyColor = colorSetting.Value;
        }

        var mat = new Material(baseMat) {
            color = enemyColor
        };

        var edgeColor = enemyColor;
        var fillColor = Instance?.FillColor.Value ?? Color.black;
        mat.SetColor("_WireframeColor", Instance.SwapEdgeAndFill.Value ? fillColor : edgeColor);
        mat.SetColor("_FillColor", Instance.SwapEdgeAndFill.Value ? edgeColor : fillColor);
        mat.SetFloat("_WireframeThickness", Instance.EdgeThickness.Value);
        // Plugin.Log.LogInfo($"Enemy color {enemyColor}");
        _enemyMatCache[enemyType] = mat;
        return mat;
    }

    private void OnEnemyColorChanged(EnemyType enemyType, Color newColor) {
        if (_enemyMatCache.TryGetValue(enemyType, out var mat) && mat != null) {
            mat.color = newColor;
            mat.SetColor(SwapEdgeAndFill.Value ? "_FillColor" : "_WireframeColor", newColor);
            mat.SetFloat("_WireframeThickness", EdgeThickness.Value);
        }
    }

    /// <inheritdoc />
    protected override void OnEnable() {
        EnemyEvents.OnSpawn += AddWireframizer;
        FillColor.OnChanged += UpdateAllMats;
        EdgeThickness.OnChanged += UpdateAllMats;
        UpdateAllMats();

        foreach (var pair in EnemyEdgeColors) {
            var enemyType = pair.Key;
            var setting = pair.Value;
            Action<Color> action = (newColor) => OnEnemyColorChanged(enemyType, newColor);
            _onColorChangedActions[enemyType] = action;
            setting.OnValueChanged += action;
            OnEnemyColorChanged(enemyType, setting.Value);
        }
    }

    /// <inheritdoc />
    protected override void OnDisable() {
        EnemyEvents.OnSpawn -= AddWireframizer;
        FillColor.OnChanged -= UpdateAllMats;
        EdgeThickness.OnChanged += UpdateAllMats;

        foreach (var pair in EnemyEdgeColors) {
            if (_onColorChangedActions.TryGetValue(pair.Key, out var action)) {
                pair.Value.OnValueChanged -= action;
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
