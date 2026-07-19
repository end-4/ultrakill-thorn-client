using Notiffy.API;
using NukeLib.UI;
using ThornClient.Core;
using ThornClient.Core.DataTypes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThornClient.System.ClickGUIComponents;

public class EnemyListSettingController : MonoBehaviour {
    public Setting<EnemyList>? TargetSetting;

    private void Start() {
        if (TargetSetting == null) return;
        GetComponent<Button>().onClick.AddListener(OpenList);
        SetupEnemies();
        TargetSetting.OnValueChanged += UpdateColumns;
    }

    private void SetupEnemies() {
        NotificationSystem.NotifySend("thorn", "TODO");
    }

    private void OnDestroy() {
        if (TargetSetting != null) {
            TargetSetting.OnValueChanged -= UpdateColumns;
        }
        var btn = GetComponent<Button>();
        if (btn != null) btn.onClick.RemoveListener(OpenList);
    }

    private void OpenList() {
        NotificationSystem.NotifySend("thorn", "TODO");
    }

    private void UpdateColumns(EnemyList enemyList) {
        NotificationSystem.NotifySend("thorn", "TODO");
    }
}
