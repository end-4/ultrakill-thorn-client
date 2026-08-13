namespace ThornClient.System.ClickGUIComponents;

internal class FloatSettingController: NumberSettingController<float> {
    protected override void UpdateScrub(float baseValue, float valueDiff) {
        if (TargetSetting != null) TargetSetting.Value = baseValue + valueDiff;
    }
}
