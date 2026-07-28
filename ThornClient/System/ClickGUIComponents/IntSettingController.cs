using NotImplementedException = System.NotImplementedException;

namespace ThornClient.System.ClickGUIComponents;

public class IntSettingController: NumberSettingController<int> {
    protected override void UpdateScrub(int baseValue, float valueDiff) {
        if (TargetSetting != null) TargetSetting.Value = (int)(baseValue + valueDiff);
    }
}
