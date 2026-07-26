using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ThornClient.System.ClickGUIComponents;

public class InputFocusGrab : MonoBehaviour {
    private TMP_InputField? _input;

    private void OnEnable() {
        _input = gameObject.GetComponent<TMP_InputField>();
        if (_input == null) return;
        _input.onSelect.AddListener(Grab);
        _input.onDeselect.AddListener(Release);
    }

    private void OnDisable() {
        if (_input == null) return;
        _input.onSelect.RemoveListener(Grab);
        _input.onDeselect.RemoveListener(Release);
    }

    private void Grab(string _) {
        ThornClient.Managers.InputManager.BlockInput = true;
    }

    private void Release(string _) {
        ThornClient.Managers.InputManager.BlockInput = false;
    }
}
