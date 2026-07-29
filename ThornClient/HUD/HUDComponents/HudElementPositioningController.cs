using System;
using System.Collections.Generic;
using System.Linq;
using NukeLib.UI;
using ThornClient.Core;
using ThornClient.System;
using ThornClient.System.ClickGUIComponents;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ThornClient.HUD.HUDComponents;

/// <summary>
/// Component to add to the GameObject that controls
/// </summary>
public class HudElementPositioningController : FreeMoveDragHandler, IEndDragHandler, IPointerEnterHandler,
    IPointerExitHandler {
    public HudModule? TargetModule;
    private GameObject? _dragOverlay;

    private struct PivotChoice {
        public string Path;
        public Button ButtonComp;
        public Image IconComp;
    }

    private string[] _pivotPositions =
        ["TopLeft", "TopRight", "BottomLeft", "BottomRight", "Top", "Bottom", "Left", "Right", "Center"];

    private static Dictionary<string, Vector2> _pivotValues = new Dictionary<string, Vector2> {
        { "TopLeft", new Vector2(0f, 1f) },
        { "TopRight", new Vector2(1f, 1f) },
        { "BottomLeft", new Vector2(0f, 0f) },
        { "BottomRight", new Vector2(1f, 0f) },
        { "Top", new Vector2(0.5f, 1f) },
        { "Bottom", new Vector2(0.5f, 0f) },
        { "Left", new Vector2(0f, 0.5f) },
        { "Right", new Vector2(1f, 0.5f) },
        { "Center", new Vector2(0.5f, 0.5f) },
    };

    private Dictionary<string, PivotChoice> _pivotChoices = [];

    private void Start() {
        _dragOverlay = gameObject.FindRecursive("Overlay");
        var colorizer = _dragOverlay?.FindRecursive("Image").GetOrAddComponent<Colorizer>();
        colorizer?.UpdateHighlight(true);
        _pivotChoices = [];
        foreach (var pivotName in _pivotPositions) {
            var pivotPath = $"Pivot/{pivotName}";
            var button = gameObject.FindRecursive(pivotPath, warnings: false);
            if (button == null) continue;
            var buttonComp = button.GetComponent<Button>();
            var iconComp = button.FindRecursive("Image")?.GetComponent<Image>();
            if (buttonComp == null || iconComp == null) continue;
            _pivotChoices.Add(pivotName, new PivotChoice {
                Path = pivotPath,
                ButtonComp = buttonComp,
                IconComp = iconComp,
            });
            buttonComp.onClick.AddListener(() => {
                var newPivot = _pivotValues[pivotName];
                if (TargetModule != null) TargetModule.PivotX.Value = newPivot.x;
                if (TargetModule != null) TargetModule.PivotY.Value = newPivot.y;
            });
        }

        if (TargetModule != null) {
            TargetModule.PivotX.OnValueChanged += UpdatePivotDisplay;
            TargetModule.PivotY.OnValueChanged += UpdatePivotDisplay;
        }

        UpdatePivotDisplay();
    }

    private void UpdatePivotDisplay(float _) {
        UpdatePivotDisplay();
    }

    private void UpdatePivotDisplay() {
        if (TargetModule == null) return;
        foreach (var pivotName in _pivotPositions) {
            try {
                if (!_pivotValues.ContainsKey(pivotName) || !_pivotChoices.ContainsKey(pivotName)) continue;
                var pivotVal = _pivotValues[pivotName];
                bool thisMatches = Mathf.Approximately(pivotVal.x, TargetModule.PivotX.Value) &&
                                   Mathf.Approximately(pivotVal.y, TargetModule.PivotY.Value);
                var iconComp = _pivotChoices[pivotName].IconComp;
                iconComp.color = thisMatches ? ThornModule.AccentColor : Color.white;
            } catch (Exception e) {
                Plugin.Log.LogWarning($"[HudElementPositioningController] Couldn't update pivot display: {e}");
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (TargetModule == null || target == null) return;
        // Use the target's position, as it's the element being moved.
        TargetModule.PositionX.Value = (float)Math.Round(target.localPosition.x, 1);
        TargetModule.PositionY.Value = (float)Math.Round(target.localPosition.y, 1);
    }

    public override void OnDrag(PointerEventData eventData) {
        if (target == null) return;
        float scaleFactor = _canvas != null ? _canvas.scaleFactor : 1f;
        Vector2 canvasDelta = eventData.delta / scaleFactor;
        target.anchoredPosition += canvasDelta;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (_dragOverlay == null) return;
        _dragOverlay.SetActive(true);
        _dragOverlay.transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (_dragOverlay == null) return;
        _dragOverlay.SetActive(false);
    }
}
