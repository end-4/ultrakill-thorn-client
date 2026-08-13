namespace ThornClient.System.ClickGUIComponents;

internal class IntSettingController: NumberSettingController<int> {
    protected override void UpdateScrub(int baseValue, float valueDiff) {
        if (TargetSetting != null) TargetSetting.Value = (int)(baseValue + valueDiff);
    }
}
