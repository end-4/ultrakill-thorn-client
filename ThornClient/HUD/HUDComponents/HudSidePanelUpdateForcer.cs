using NukeLib.Game;
using NukeLib.Utils;
using UnityEngine;

namespace ThornClient.HUD.HUDComponents;

/// <summary>
/// Forces HUDPos position update check when necessary
/// </summary>
public class HudSidePanelUpdateForcer : MonoBehaviour {
    public bool ForceActive = true;

    private void OnEnable() {
        PrefsHelper.Subscribe("weaponHoldPosition", ForceUpdate);
    }

    private void Start() {
        ForceUpdate();
    }

    private void OnDisable() {
        PrefsHelper.Unsubscribe("weaponHoldPosition", ForceUpdate);
    }

    public void ForceUpdate() {
        // Force visibility
        if (ForceActive) {
            var canvasComp = gameObject.GetComponent<Canvas>();
            if (canvasComp != null) canvasComp.enabled = true;
        }

        // Force position check
        var hudPos = gameObject.GetComponent<HUDPos>();
        if (hudPos == null) return;
        if (ForceActive) hudPos.active = true;
        hudPos.CheckPos();
        if (ForceActive) {
            ExecutionUtils.RunNextFrame(() => {
                var trans = gameObject.transform;
                trans.localPosition = new Vector3(
                    trans.localPosition.x, trans.localPosition.y, 1
                );
            });
        }
    }
}
