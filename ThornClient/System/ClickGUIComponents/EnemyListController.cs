using System;
using System.Linq;
using NukeLib.UI;
using ThornClient.Core;
using ThornClient.Core.DataTypes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ThornClient.Managers;

namespace ThornClient.System.ClickGUIComponents;

internal class EnemyListController : MonoBehaviour {
    private bool _doneSetup = false;
    public bool IsPopup = true;
    public Setting<EnemyList>? TargetList;
    private Transform? _falseList;
    private Transform? _trueList;

    private void Start() {
        gameObject.FindRecursive("Header").AddComponent<TitlebarDragHandler>();
        SetupModules();
    }

    public void SetupModules() {
        if (_doneSetup) return;
        _doneSetup = true;

        if (TargetList == null) return;

        // Header
        var categoryText = gameObject.FindRecursive("Header/TitleName").GetComponent<TextMeshProUGUI>();
        categoryText.text = TargetList.Name;

        // Back button
        var backBtn = gameObject.FindRecursive("Header/TitleButton").GetComponent<Button>();
        backBtn.onClick.AddListener(ClickGUI.NavigateBack);

        // Populate the lists
        _falseList = gameObject.FindRecursive("Columns/FalseScrollView/Viewport/Content").transform;
        _trueList = gameObject.FindRecursive("Columns/TrueScrollView/Viewport/Content").transform;

        PopulateLists();
        TargetList.OnValueChanged += _ => PopulateLists();
    }

    private void OnDestroy() {
        if (TargetList != null) {
            TargetList.OnValueChanged -= _ => PopulateLists();
        }
    }

    private void PopulateLists() {
        if (TargetList == null || _falseList == null || _trueList == null) return;

        // Clear existing items
        foreach (Transform child in _falseList) Destroy(child.gameObject);
        foreach (Transform child in _trueList) Destroy(child.gameObject);

        var enemyTypes = (EnemyType[])Enum.GetValues(typeof(EnemyType));
        var sortedEnemyTypes = enemyTypes.OrderBy(e => e.ToString());

        foreach (var enemyType in sortedEnemyTypes) {
            bool isIncluded = TargetList.Value.Includes(enemyType);
            var parentList = isIncluded ? _trueList : _falseList;

            var item = Instantiate(AssetManager.Get<GameObject>(ClickGUI.BundleKey, "EnemyListItem"), parentList);
            var icoCon = item.FindRecursive("Icon").AddComponent<EnemyIconController>();
            icoCon.enemyType = enemyType;
            item.FindRecursive("Name").GetComponent<TextMeshProUGUI>().text = enemyType.ToString();
			item.FindRecursive("ToggleButton/Icon").GetComponent<Image>().sprite = AssetManager.Get<Sprite>(ClickGUI.BundleKey, isIncluded ? "minus" : "plus");
            var button = item.FindRecursive("ToggleButton").GetComponent<Button>();
            button.onClick.AddListener(() => OnItemClick(enemyType));
            item.UnfuckLayoutHack();
        }

        _falseList.gameObject.UnfuckLayoutHack();
        _trueList.gameObject.UnfuckLayoutHack();
        gameObject.UnfuckLayoutHack();
    }

    private void OnItemClick(EnemyType enemyType) {
        if (TargetList == null) return;
        var newList = TargetList.Value.Clone();
        newList.Toggle(enemyType);
        TargetList.Value = newList;
    }
}
