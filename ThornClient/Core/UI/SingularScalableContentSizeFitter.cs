using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ThornClient.Core.UI;

/// <summary>
/// A content size fitter that takes children scaling into account
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(LayoutElement))]
public class SingularScalableContentSizeFitter : ContentSizeFitter {
    /// <summary>
    /// Sets the size to fit, to be run after HandleSelfFittingAlongAxis
    /// </summary>
    /// <param name="axis"></param>
    protected virtual void HandleScalableFitting(int axis) {
        var fitMode = axis == 0 ? this.horizontalFit : this.verticalFit;
        if (fitMode == FitMode.Unconstrained) return;
        var trans = (RectTransform)transform;
        var childTransNotRect = trans.childCount > 0 ? trans.GetChild(0) : null;
        if (childTransNotRect == null || childTransNotRect is not RectTransform childTrans) return;
        var baseSize = (fitMode == FitMode.MinSize)
            ? LayoutUtility.GetMinSize(childTrans, axis)
            : LayoutUtility.GetPreferredSize(childTrans, axis);
        var scale = (axis == 0) ? childTrans.localScale.x : childTrans.localScale.y;
        var targetSize = baseSize * scale;
        trans.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, targetSize);
    }

    public override void SetLayoutHorizontal() {
        base.SetLayoutHorizontal();
        HandleScalableFitting(0);
    }

    public override void SetLayoutVertical() {
        base.SetLayoutVertical();
        HandleScalableFitting(1);
    }
}
