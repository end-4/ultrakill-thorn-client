using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NukeLib.Game;
using NukeLib.UI;
using NukeLib.Utils;
using ThornClient.Core;
using ThornClient.HUD.HUDComponents;
using ThornClient.System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ThornClient.Managers;

/// <summary>
/// Contains some shared functionality for managing the HUD.
/// </summary>
public static class HudManager {
    private static readonly string BundlePath = Path.Combine(Plugin.workingDir, "assets", "thorn_hud.bundle");

    /// <summary>
    /// The key of the asset bundle used for HUD assets.
    /// </summary>
    public static readonly string BundleKey = "hud";

    private static Dictionary<HudSurface, GameObject?> _surfaces = new();

    /// <summary>
    /// Emitted when the HUD manager has finished preparing for a scene and is ready to spawn HUD modules.
    /// </summary>
    public static event Action? ReadyForScene;

    /// <summary>
    /// Scans the assembly via reflection and instantiates + registers HudModules
    /// </summary>
    public static void Initialize() {
        Plugin.Log.LogInfo($"[HUD Manager] Starting...");

        // Assets
        AssetManager.LoadBundle(BundleKey, BundlePath);

        // Hook
        SceneUtils.SafeSceneLoadedDelayed += OnSceneLoaded;
        FinalRankHelper.RankShown += HideHud;
        CheckpointEvents.CheckpointLoadedNoParam += ShowHud;
    }

    private const string GunCanvasName = "ThornGunCanvas";
    private const string StyleCanvasName = "ThornStyleCanvas";

    private static void ForceEnableHudPanel(GameObject panel) {
        // Plugin.Log.LogInfo($"Force enable hud panel {panel}");
        if (panel == null) return;
        var forceComp = panel.GetOrAddComponent<HudSidePanelUpdateForcer>();
        if (forceComp == null) return;
        forceComp.ForceActive = true;
        forceComp.ForceUpdate();
    }

    private static void HideHud() {
        foreach (GameObject? go in _surfaces.Values) {
            if (go == null) continue;
            go.SetActive(false);
        }
    }

    private static void ShowHud() {
        foreach (GameObject? go in _surfaces.Values) {
            if (go == null) continue;
            ForceEnableHudPanel(go);
        }
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        try {
            var rootGameObjects = scene.GetRootGameObjects();
            var player = rootGameObjects.Where(obj => obj.name == "Player").FirstOrDefault();
            var canvas = rootGameObjects.Where(obj => obj.name == "Canvas").FirstOrDefault();
            var hud = player?.FindRecursive("Main Camera/HUD Camera/HUD");
            if (hud != null) {
                var thornGunCanvas = hud.FindRecursive(GunCanvasName, false);
                var vanillaGunCanvas = hud.FindRecursive("GunCanvas");
                if (thornGunCanvas == null && vanillaGunCanvas != null) {
                    thornGunCanvas = Object.Instantiate(vanillaGunCanvas, vanillaGunCanvas.transform.parent);
                    thornGunCanvas.name = GunCanvasName;
                    foreach (Transform t in thornGunCanvas.transform) {
                        Object.Destroy(t.gameObject);
                    }

                    thornGunCanvas.transform.SetAsLastSibling();
                    var hudPos = thornGunCanvas.GetComponent<HUDPos>();
                }

                _surfaces[HudSurface.Left] = thornGunCanvas ?? vanillaGunCanvas;

                var thornStyleCanvas = hud.FindRecursive(StyleCanvasName, false);
                var vanillaStyleCanvas = hud.FindRecursive("StyleCanvas");
                if (thornStyleCanvas == null && vanillaStyleCanvas != null) {
                    thornStyleCanvas = Object.Instantiate(vanillaStyleCanvas, vanillaStyleCanvas.transform.parent);
                    thornStyleCanvas.name = StyleCanvasName;
                    foreach (Transform t in thornStyleCanvas.transform) {
                        Object.Destroy(t.gameObject);
                    }

                    thornStyleCanvas.transform.SetAsLastSibling();
                    Object.Destroy(thornStyleCanvas.GetComponent<StyleHUD>());
                    Object.Destroy(thornStyleCanvas.GetComponent<StyleCalculator>());
                }

                _surfaces[HudSurface.Right] = thornStyleCanvas ?? vanillaStyleCanvas;

                // _surfaces[HudSurface.Left] = hud.FindRecursive("GunCanvas");
                // _surfaces[HudSurface.Right] = hud.FindRecursive("StyleCanvas");
            }

            _surfaces[HudSurface.Overlay] = canvas?.FindRecursive("Crosshair Filler");
            if (_surfaces.Values.Any(g => g == null)) return;
            foreach (GameObject? go in _surfaces.Values) {
                if (go == null) continue;
                ForceEnableHudPanel(go);
                go.SetActive(SceneUtils.IsInGame());
            }

            // Plugin.Log.LogInfo("[HUD Manager] ReadyForScene...]");
            ReadyForScene?.Invoke();
        } catch (Exception e) {
            Plugin.Log.LogWarning($"[HUD Manager] Scene load hook failed: {e.Message}");
        }
    }

    /// <summary>
    /// Gets the GameObject surface for a given HudSurface enum value.
    /// </summary>
    /// <param name="hudSurface">The enum value</param>
    /// <param name="surface">The resulting GameObject</param>
    /// <returns>True if it's found, false otherwise</returns>
    public static bool GetSurface(HudSurface hudSurface, out GameObject? surface) {
        return _surfaces.TryGetValue(hudSurface, out surface);
    }

    /// <summary>
    /// Struct holding absolute snap target.
    /// This is relative to localPosition, and the width/height stuff should already be taken care of
    /// </summary>
    internal struct SnapCandidate {
        public RectTransform.Axis Axis;
        public float Value;
        public Tuple<float, float> OtherAxisRange;

        public override string ToString() {
            return $"[{(int)Axis}]={Value}, other=[{OtherAxisRange.Item1},{OtherAxisRange.Item2}]";
        }
    }

    private static bool CheckCandidate(float x, float y, float activationDistance, SnapCandidate candidate) {
        // Plugin.Log.LogInfo($"[?] Check ({x}, {y}) against target {candidate}");
        switch (candidate.Axis) {
            case RectTransform.Axis.Horizontal:
                if (Math.Abs(candidate.Value - x) > activationDistance) return false;
                if (y < candidate.OtherAxisRange.Item1 || y > candidate.OtherAxisRange.Item2) return false;
                return true;
            case RectTransform.Axis.Vertical:
                if (Math.Abs(candidate.Value - y) > activationDistance) return false;
                if (x < candidate.OtherAxisRange.Item1 || x > candidate.OtherAxisRange.Item2) return false;
                return true;
        }

        return false; // Should never be reached
    }

    /// <summary>
    /// Get offset from current pivot to the target pivot
    /// </summary>
    /// <param name="transform">The current RectTransform</param>
    /// <param name="pivotX">The target pivot's X value</param>
    /// <param name="pivotY">The target pivot's Y value</param>
    /// <returns></returns>
    internal static Vector2 GetPivotOffsetTo(this RectTransform transform, float pivotX, float pivotY) {
        var targetPivot = new Vector2(pivotX, pivotY);
        var pivotDelta = targetPivot - transform.pivot;
        return new Vector2(
            pivotDelta.x * transform.sizeDelta.x,
            pivotDelta.y * transform.sizeDelta.y
        );
    }

    /// <summary>
    /// Get local position of the pivot on the parent, if it was at a certain target values
    /// </summary>
    /// <param name="transform">The current RectTransform</param>
    /// <param name="pivotX">The target pivot X</param>
    /// <param name="pivotY">The target pivot Y</param>
    /// <returns></returns>
    internal static Vector2 GetLocalPivotPosition(this RectTransform transform, float pivotX, float pivotY) {
        Vector2 localPos = transform.localPosition;
        var rawOffset = transform.GetPivotOffsetTo(pivotX, pivotY);
        return localPos + rawOffset;
    }

    /// <summary>
    /// Gets snap candidates...
    /// This function is mostly a refactor for code cleanliness and will likely be hella difficult to understand.
    /// If you wanna understand, try to plug a certain case into the calculations
    ///   until you reach a single new SnapCandidate {...} construction.
    ///
    /// EXPLANATION (bullet points used for readability, please read linearly)
    /// - Imagine you want to overlap a segment on the candidate and a segment on the dragged item
    ///   - It counts when the two segments are touching, even if barely
    ///   - You need to offset the segments along their normals (I hope I'm right on the terminology),
    ///     then offset along their directions so that they "reach" each other
    /// - The "line ratio"s are values in [0, 1] that specify how much the segment passes through each item
    ///   - 0 is left/bottom, 0.5 is center, 1 is right/top
    /// - The value shift is the extra value added in case an offset between the two segments is desirable,
    ///   like having a gap when snapping two rectangles' facing edges.
    /// - The range padding intuitively extends the segments longer on both ends.
    /// - The produced snap candidates are to serve as targets to assist that snapping
    /// </summary>
    /// <param name="candidateTransforms">Array of candidates' RectTransforms</param>
    /// <param name="draggedItemTransform">RectTransform of the dragged item</param>
    /// <param name="axis">The axis that snapping performs on</param>
    /// <param name="candidateLineRatio">The pivot-like ratio of the candidate's segment for overlapping</param>
    /// <param name="draggedItemLineRatio">The pivot-like ratio of the dragged item's segment for overlapping</param>
    /// <param name="valueShift">The shift added to the snapping value</param>
    /// <param name="rangePadding">The added acceptable range on the other axis for snaps</param>
    /// <returns>The snap candidates</returns>
    private static IEnumerable<SnapCandidate> GetSnapCandidates(
        RectTransform[] candidateTransforms, RectTransform draggedItemTransform,
        RectTransform.Axis axis,
        float candidateLineRatio,
        float draggedItemLineRatio,
        float valueShift, float rangePadding
    ) {
        var isHorizontal = axis == RectTransform.Axis.Horizontal;
        return candidateTransforms.Select(t => {
            var candidatePoint = t.GetLocalPivotPosition(
                isHorizontal ? candidateLineRatio : 0.5f,
                isHorizontal ? 0.5f : candidateLineRatio
            );
            var draggedItemOffset = draggedItemTransform.GetPivotOffsetTo(
                isHorizontal ? draggedItemLineRatio : 0.5f,
                isHorizontal ? 0.5f : draggedItemLineRatio
            );
            var perfectCandidatePointOverlapTarget = isHorizontal
                ? (candidatePoint.x - draggedItemOffset.x)
                : (candidatePoint.y - draggedItemOffset.y);
            var rangeLowerPerfectEdges = isHorizontal
                ? t.GetLocalPivotPosition(0.5f, 0).y - draggedItemTransform.GetPivotOffsetTo(0.5f, 1).y
                : t.GetLocalPivotPosition(0, 0.5f).x - draggedItemTransform.GetPivotOffsetTo(1, 0.5f).x;
            var rangeUpperPerfectEdges = isHorizontal
                ? t.GetLocalPivotPosition(0.5f, 1).y - draggedItemTransform.GetPivotOffsetTo(0.5f, 0).y
                : t.GetLocalPivotPosition(1, 0.5f).x - draggedItemTransform.GetPivotOffsetTo(0, 0.5f).x;
            return new SnapCandidate {
                Axis = axis,
                Value = perfectCandidatePointOverlapTarget + valueShift,
                OtherAxisRange = Tuple.Create(
                    rangeLowerPerfectEdges - rangePadding,
                    rangeUpperPerfectEdges + rangePadding
                )
            };
        });
    }

    public static Vector2 Snap(HudModule hudMod, Vector2 newPos) {
        if (ThornModule.Instance == null) return newPos;
        var thorn = ThornModule.Instance;
        if (!(thorn.SnapEnabled.Value)) return newPos;

        var dragX = newPos.x;
        var dragY = newPos.y;
        float gap = thorn.SnapGap.Value;
        float activationDist = thorn.SnapActivationDistance.Value;
        float activationDistAlignment = thorn.SnapActivationDistanceAlignment.Value;
        var trans = hudMod.UIElement!.GetComponent<RectTransform>();

        var surface = hudMod.Surface.Value;
        HudModule[] sameSurfaceMods = [
            .. ModuleManager.Items
                .OfType<HudModule>()
                .Where(m => m.Surface.Value == surface && m != hudMod && m.IsEnabled)
        ];
        // Plugin.Log.LogInfo($"======\nother modules: {sameSurfaceMods.Stringify()}\n======");
        RectTransform[] transforms = sameSurfaceMods
            .Select(m => m.UIElement!.GetComponent<RectTransform>())
            .ToArray();

        var hAxis = RectTransform.Axis.Horizontal;
        var vAxis = RectTransform.Axis.Vertical;
        var xCenterTargets = GetSnapCandidates(transforms, trans, hAxis, 0.5f, 0.5f, 0, gap + activationDistAlignment);
        var yCenterTargets = GetSnapCandidates(transforms, trans, vAxis, 0.5f, 0.5f,  0, gap + activationDistAlignment);
        var xLeftAlignTargets = GetSnapCandidates(transforms, trans, hAxis, 0f, 0f,  0, gap + activationDistAlignment);
        var xRightAlignTargets = GetSnapCandidates(transforms, trans, hAxis, 1f, 1f, 0, gap + activationDistAlignment);
        var yBottomAlignTargets = GetSnapCandidates(transforms, trans, vAxis, 0f, 0f, 0, gap + activationDistAlignment);
        var yTopAlignTargets = GetSnapCandidates(transforms, trans, vAxis, 1f, 1f,  0, gap + activationDistAlignment);
        var xMeetLeftEdgeTargets = GetSnapCandidates(transforms, trans, hAxis, 0f, 1f, -gap, gap);
        var xMeetRightEdgeTargets = GetSnapCandidates(transforms, trans, hAxis, 1f, 0f, +gap, gap);
        var yMeetBottomEdgeTargets = GetSnapCandidates(transforms, trans, vAxis, 0f, 1f,  -gap, gap);
        var yMeetTopEdgeTargets = GetSnapCandidates(transforms, trans, vAxis, 1f, 0f, +gap, gap);

        SnapCandidate[] extraXCandidates = surface == HudSurface.Overlay ? [
            new SnapCandidate { // Screen center
                Axis = RectTransform.Axis.Horizontal,
                Value = 0,
                OtherAxisRange = Tuple.Create(-9999f, 9999f)
            }
        ] : [];
        SnapCandidate[] extraYCandidates = surface == HudSurface.Overlay ? [
            new SnapCandidate {
                Axis = RectTransform.Axis.Vertical,
                Value = 0,
                OtherAxisRange = Tuple.Create(-9999f, 9999f)
            }
        ] : [];

        SnapCandidate[] xTargets = [
            ..xCenterTargets, ..xLeftAlignTargets, ..xRightAlignTargets,
            ..xMeetLeftEdgeTargets, ..xMeetRightEdgeTargets,
            ..extraXCandidates,
        ];
        SnapCandidate[] yTargets = [
            ..yCenterTargets, ..yTopAlignTargets, ..yBottomAlignTargets,
            ..yMeetBottomEdgeTargets, ..yMeetTopEdgeTargets,
            ..extraYCandidates,
        ];

        var possibleXCandidates = xTargets
            .Where(cand => CheckCandidate(dragX, dragY, activationDist, cand))
            .OrderBy(cand => Math.Abs(cand.Value - dragX))
            .ToArray();

        var possibleYCandidates = yTargets
            .Where(cand => CheckCandidate(dragX, dragY, activationDist, cand))
            .OrderBy(cand => Math.Abs(cand.Value - dragY))
            .ToArray();

        var possibleXValues = possibleXCandidates.Select(cand => cand.Value).ToArray();
        var possibleYValues = possibleYCandidates.Select(cand => cand.Value).ToArray();
        var targetX = possibleXValues.Length > 0 ? possibleXValues[0] : dragX;
        var targetY = possibleYValues.Length > 0 ? possibleYValues[0] : dragY;

        return new Vector2(targetX, targetY);
    }
}
