using System;
using System.Collections.Generic;
using System.Linq;
using NukeLib.Game;
using NukeLib.Utils;
using UnityEngine;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Managers;
using ThornClient.System;
using Object = UnityEngine.Object;

namespace ThornClient.Modules.Render;

/// <summary>
/// Module that does stuff to the viewmodel
/// </summary>
public class ViewmodelTweaks : Module {
    // (Horrible) reference: https://github.com/daemon251/Ultrakill-DaemonWeaponUtils/blob/main/PluginConfig.cs

    public static ViewmodelTweaks? Instance;

    // Indices:
    // First  [2]:  0 = L/R pos, 1 = Middle pos
    // Second [5]: Weapon index: 0 = Revolver, ..., 4 = Rocket Launcher
    // Third  [2]:  0 = Base, 1 = Alt
    private static readonly Vector3[,,] BasePositions = new[,,] {
        {
            // L/R
            { new Vector3(+0.42f, -0.60f, +1.49f), new Vector3(+0.50f, -0.70f, +1.49f) }, // Revolver
            { new Vector3(+0.42f, -0.60f, +1.49f), new Vector3(+0.42f, -0.60f, +1.49f) }, // Shotgun
            { new Vector3(+0.00f, +0.00f, +0.00f), new Vector3(+0.00f, +0.00f, +0.00f) }, // Nailgun
            { new Vector3(+0.00f, +0.00f, +0.00f), new Vector3(+0.00f, +0.00f, +0.00f) }, // Railgun
            { new Vector3(+0.00f, +0.00f, +0.00f), new Vector3(+0.00f, +0.00f, +0.00f) }, // Rocket Launcher
        }, {
            // Middle
            { new Vector3(+0.09f, -0.90f, +1.49f), new Vector3(+0.09f, -0.90f, +1.49f) }, // Revolver
            { new Vector3(+0.00f, -0.85f, +1.65f), new Vector3(-0.18f, -0.80f, +1.65f) }, // Shotgun
            { new Vector3(-0.39f, -0.10f, +0.20f), new Vector3(-0.50f, -0.10f, +0.20f) }, // Nailgun
            { new Vector3(-0.60f, -0.40f, +0.10f), new Vector3(-0.60f, -0.40f, +0.10f) }, // Railgun
            { new Vector3(-0.37f, -0.15f, +0.20f), new Vector3(-0.37f, -0.15f, +0.20f) }, // Rocket Launcher
        },
    };

    // Same index rules as above
    private static readonly Vector3[,,] BaseAngles = new[,,] {
        {
            // L/R
            { new Vector3(00f, 90f, 20f), new Vector3(00f, 92f, 20f) }, // Revolver
            { new Vector3(00f, 90f, 20f), new Vector3(00f, 90f, 20f) }, // Shotgun
            { new Vector3(00f, 00f, 00f), new Vector3(00f, 00f, 00f) }, // Nailgun
            { new Vector3(00f, 00f, 00f), new Vector3(00f, 00f, 00f) }, // Railgun
            { new Vector3(00f, 00f, 00f), new Vector3(00f, 00f, 00f) }, // Rocket Launcher
        }, {
            // Middle
            { new Vector3(00f, 93f, 30f), new Vector3(00f, 93f, 25f) }, // Revolver
            { new Vector3(00f, 91f, 20f), new Vector3(00f, 90f, 25f) }, // Shotgun
            { new Vector3(00f, 00f, 00f), new Vector3(00f, 00f, 00f) }, // Nailgun
            { new Vector3(00f, 00f, 00f), new Vector3(00f, 00f, 00f) }, // Railgun
            { new Vector3(05f, 05f, 00f), new Vector3(05f, 05f, 00f) }, // Rocket Launcher
        }
    };

    // Same index rules as above
    private static readonly Vector3[,,] BaseScales = new[,,] {
        {
            // L/R
            { new Vector3(0.29f, 0.29f, 0.33f), new Vector3(0.30f, 0.30f, 0.40f) }, // Revolver
            { new Vector3(0.29f, 0.29f, 0.33f), new Vector3(1.00f, 1.00f, 1.00f) }, // Shotgun
            { new Vector3(1.00f, 1.00f, 1.00f), new Vector3(1.00f, 1.00f, 1.00f) }, // Nailgun
            { new Vector3(1.00f, 1.00f, 1.00f), new Vector3(1.00f, 1.00f, 1.00f) }, // Railgun
            { new Vector3(1.00f, 1.00f, 1.00f), new Vector3(1.00f, 1.00f, 1.00f) }, // Rocket Launcher
        }, {
            // Middle
            { new Vector3(0.29f, 0.29f, 0.33f), new Vector3(0.30f, 0.30f, 0.40f) }, // Revolver
            { new Vector3(0.29f, 0.29f, 0.33f), new Vector3(1.00f, 1.00f, 1.00f) }, // Shotgun
            { new Vector3(1.00f, 1.00f, 1.00f), new Vector3(1.00f, 1.00f, 1.00f) }, // Nailgun
            { new Vector3(1.00f, 1.00f, 1.00f), new Vector3(1.00f, 1.00f, 1.00f) }, // Railgun
            { new Vector3(1.00f, 1.00f, 1.20f), new Vector3(1.00f, 1.00f, 1.20f) }, // Rocket Launcher
        }
    };

    // General
    public Setting<bool> Bobbing;
    public Setting<bool> WeaponEdges;
    public Setting<float> WeaponEdgeThickness;
    public Setting<Color> WeaponFillColor;

    // Transforms: Global
    public Setting<float> ModelRollOffset;
    public Setting<float> ModelPitchOffset;
    public Setting<float> ModelYawOffset;
    public Setting<float> ModelPosXOffset;
    public Setting<float> ModelPosYOffset;
    public Setting<float> ModelPosZOffset;
    public Setting<float> ModelScaleOffset;

    // Transforms: Weapon-specifics. 8 indices go in this order: revolver, alt revolver, shotgun, alt shotgun, nailgun, alt nailgun, railgun, rocket launcher
    public Setting<float>[] WeaponModelRollOffsets = new Setting<float>[8];
    public Setting<float>[] WeaponModelPitchOffsets = new Setting<float>[8];
    public Setting<float>[] WeaponModelYawOffsets = new Setting<float>[8];
    public Setting<float>[] WeaponModelPosXOffsets = new Setting<float>[8];
    public Setting<float>[] WeaponModelPosYOffsets = new Setting<float>[8];
    public Setting<float>[] WeaponModelPosZOffsets = new Setting<float>[8];
    public Setting<float>[] WeaponModelScaleOffsets = new Setting<float>[8];

    private int GetSpecificWeaponVariantOffsetIndex(int weaponIndex, int altVariantIndex) {
        return weaponIndex switch {
            0 => // Revolver
                altVariantIndex // 0 or 1
            ,
            1 => // Shotgun
                2 + altVariantIndex // 2 or 3
            ,
            2 => // Nailgun
                4 + altVariantIndex // 4 or 5
            ,
            3 => // Railgun
                6,
            4 => // Rocket Launcher
                7,
            _ => -1
        };
    }

    public static readonly string[] SettingWeaponNameMap = new[] {
        "Revolver", "Slab Revolver", "Shotgun", "Jackhammer", "Nailgun", "Sawblade Launcher", "Railgun",
        "Rocket Launcher"
    };

    private static Material?[] _matCache = new Material[3];

    public static Material? GetVariantMat(int variantIndex) {
        if (variantIndex < 0 || variantIndex > 2 || Instance == null) return null;
        if (_matCache[variantIndex] != null) return _matCache[variantIndex];
        var baseMat = EffectManager.GetMaterial("GeometryWireframeMaterial");
        if (baseMat == null) {
            Plugin.Log.LogError("[ViewmodelTweaks] Failed to load GeometryWireframeMaterial asset!");
            return null;
        }

        _matCache[variantIndex] = new Material(baseMat);
        Instance.UpdateVariantColors();
        return _matCache[variantIndex];
    }

    private void UpdateVariantColors() {
        for (int i = 0; i < 3; i++) {
            var mat = _matCache[i];
            if (mat == null) continue;
            mat.SetColor("_WireframeColor", ColorUtils.GetWeaponVariantColor(i));
            mat.SetColor("_FillColor", WeaponFillColor.Value);
            mat.SetFloat("_WireframeThickness", WeaponEdgeThickness.Value);
        }
    }

    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "axes");

    public ViewmodelTweaks() : base(
        "thorn.viewmodelTweaks",
        "Viewmodel Tweaks",
        "The viewmodel is your hands, basically.",
        ModuleCategory.Render
    ) {
        if (Instance != null) return;
        Instance = this;

        CreateHeader("generalHeader", "General");
        Bobbing = CreateSetting("bobbing", "Bobbing", "Whether your hands wiggle when walking", true);
        WeaponEdges = CreateSetting("weaponEdges", "Edges skin",
            "Similar to the popular ULTRAEDGES skin. Cleaner but also more detailed. NOTE: to un-apply, you need to restart the mission",
            false);
        WeaponEdgeThickness = CreateSetting("weaponEdgeThickness", "Edge thickness",
            "Thickness of the edges", 2f);
        WeaponFillColor = CreateSetting("weaponFillColor", "Edges: fill color",
            "Color to fill surfaces between the lines", Color.black);

        CreateHeader("transformHeader", "Transformations");
        var globalTransGroup = CreateGroup("globalTransform", "Global", "Transformations that apply to all weapons");
        ModelRollOffset = CreateSetting("modelRollOffset", "Roll", "Tilting left/right", 0f, globalTransGroup);
        ModelPitchOffset = CreateSetting("modelPitchOffset", "Pitch", "Pointing up/down", 0f, globalTransGroup);
        ModelYawOffset = CreateSetting("modelYawOffset", "Yaw", "Turning left/right", 0f, globalTransGroup);
        ModelPosXOffset = CreateSetting("modelPosXOffset", "Offset X", "Left/Right position", 0f, globalTransGroup);
        ModelPosYOffset = CreateSetting("modelPosYOffset", "Offset Y", "Up/Down position", 0f, globalTransGroup);
        ModelPosZOffset = CreateSetting(
            "modelPosZOffset",
            "Offset Z",
            "Forward/Backward position",
            0f,
            globalTransGroup
        );
        ModelScaleOffset = CreateSetting("modelScaleOffset", "Scale", "Uniform scale offset", 0f, globalTransGroup);

        for (int i = 0; i < WeaponModelRollOffsets.Length; i++) {
            var currGroup =
                CreateGroup($"transGroup{i}", SettingWeaponNameMap[i],
                    $"Transformations that apply to {SettingWeaponNameMap[i]}");
            WeaponModelRollOffsets[i] =
                CreateSetting($"weapon{i}ModelRollOffset", "Roll", "Tilting left/right", 0f, currGroup);
            WeaponModelPitchOffsets[i] =
                CreateSetting($"weapon{i}ModelPitchOffset", "Pitch", "Pointing up/down", 0f, currGroup);
            WeaponModelYawOffsets[i] =
                CreateSetting($"weapon{i}ModelYawOffset", "Yaw", "Turning left/right", 0f, currGroup);
            WeaponModelPosXOffsets[i] = CreateSetting($"weapon{i}ModelPosXOffset", "Offset X", "Left/Right position",
                0f, currGroup);
            WeaponModelPosYOffsets[i] =
                CreateSetting($"weapon{i}ModelPosYOffset", "Offset Y", "Up/Down position", 0f, currGroup);
            WeaponModelPosZOffsets[i] = CreateSetting($"weapon{i}ModelPosZOffset", "Offset Z",
                "Forward/Backward position", 0f, currGroup);
            WeaponModelScaleOffsets[i] = CreateSetting($"weapon{i}ModelScaleOffset", "Scale", "Uniform scale offset",
                0f, currGroup);
        }

        SceneUtils.SafeSceneLoadedNoParam += () => {
            if (gc != null) gc.OnWeaponChange -= UpdateCurrentNextFrame;
            if (fc != null) fc.FistIconUpdated -= UpdateCurrentNextFrame;
        };
    }

    private static FistControl? fc => FistControl.Instance;
    private static GunControl? gc => GunControl.Instance;
    private static PrefsManager? prefs => PrefsManager.Instance;

    /// <inheritdoc />
    protected override void OnEnable() {
        SceneUtils.SafeSceneLoadedNoParam += OnSceneLoadedWhenEnabled;
        ModelRollOffset.OnValueChanged += UpdateCurrent;
        ModelPitchOffset.OnValueChanged += UpdateCurrent;
        ModelYawOffset.OnValueChanged += UpdateCurrent;
        ModelPosXOffset.OnValueChanged += UpdateCurrent;
        ModelPosYOffset.OnValueChanged += UpdateCurrent;
        ModelPosZOffset.OnValueChanged += UpdateCurrent;
        ModelScaleOffset.OnValueChanged += UpdateCurrent;
        for (int i = 0; i < WeaponModelRollOffsets.Length; i++) {
            WeaponModelRollOffsets[i].OnValueChanged += UpdateCurrent;
            WeaponModelPitchOffsets[i].OnValueChanged += UpdateCurrent;
            WeaponModelYawOffsets[i].OnValueChanged += UpdateCurrent;
            WeaponModelPosXOffsets[i].OnValueChanged += UpdateCurrent;
            WeaponModelPosYOffsets[i].OnValueChanged += UpdateCurrent;
            WeaponModelPosZOffsets[i].OnValueChanged += UpdateCurrent;
            WeaponModelScaleOffsets[i].OnValueChanged += UpdateCurrent;
        }

        if (!SceneUtils.IsInGame()) {
            SceneUtils.SafeSceneLoadedNoParam += SubscribeStuffWhenSafe;
        } else {
            SubscribeStuffWhenSafe();
            UpdateCurrentNextFrame();
        }

        Bobbing.OnChanged += UpdateBobbing;
        WeaponEdges.OnChanged += UpdateCurrentColor;
        WeaponFillColor.OnChanged += UpdateVariantColors;
        WeaponEdgeThickness.OnChanged += UpdateVariantColors;
    }

    private void SubscribeStuffWhenSafe() {
        if (!SceneUtils.IsInGame()) return;
        if (fc != null) {
            // Hooking icon update is weird, but it only exposes this. I hate patches...
            fc.FistIconUpdated += UpdateCurrentNextFrame;
        }

        if (gc != null) {
            gc.OnWeaponChange += UpdateCurrentNextFrame;
            UpdateBobbing();
            SceneUtils.SafeSceneLoadedNoParam -= SubscribeStuffWhenSafe;
        }
    }

    /// <inheritdoc />
    protected override void OnDisable() {
        WeaponEdges.OnChanged -= UpdateCurrentColor;
        WeaponEdgeThickness.OnChanged -= UpdateVariantColors;
        WeaponFillColor.OnChanged -= UpdateVariantColors;
        Bobbing.OnChanged -= UpdateBobbing;
        SceneUtils.SafeSceneLoadedNoParam -= OnSceneLoadedWhenEnabled;
        ModelRollOffset.OnValueChanged -= UpdateCurrent;
        ModelPitchOffset.OnValueChanged -= UpdateCurrent;
        ModelYawOffset.OnValueChanged -= UpdateCurrent;
        ModelPosXOffset.OnValueChanged -= UpdateCurrent;
        ModelPosYOffset.OnValueChanged -= UpdateCurrent;
        ModelPosZOffset.OnValueChanged -= UpdateCurrent;
        ModelScaleOffset.OnValueChanged -= UpdateCurrent;
        for (int i = 0; i < WeaponModelRollOffsets.Length; i++) {
            WeaponModelRollOffsets[i].OnValueChanged -= UpdateCurrent;
            WeaponModelPitchOffsets[i].OnValueChanged -= UpdateCurrent;
            WeaponModelYawOffsets[i].OnValueChanged -= UpdateCurrent;
            WeaponModelPosXOffsets[i].OnValueChanged -= UpdateCurrent;
            WeaponModelPosYOffsets[i].OnValueChanged -= UpdateCurrent;
            WeaponModelPosZOffsets[i].OnValueChanged -= UpdateCurrent;
            WeaponModelScaleOffsets[i].OnValueChanged -= UpdateCurrent;
        }

        // We disable on scene load instead to ensure consistency
        // if (gc != null) {
        //     gc.OnWeaponChange -= UpdateCurrentNextFrame;
        // }

        SceneUtils.SafeSceneLoadedNoParam -= SubscribeStuffWhenSafe;
        ResetAllTransforms();
    }

    private int GetMiddleIndex() {
        if (prefs == null) return 0;
        return prefs.GetInt("weaponHoldPosition") == 1 ? 1 : 0;
    }

    private void OnSceneLoadedWhenEnabled() {
        UpdateCurrentNextFrame();
        // It seems we need to resubscribe to GunControl stuff every scene.
        // Hacky but works.
        Toggle();
        Toggle();
        if (gc != null) ExecutionUtils.RunNextFrame(() => gc.ForceWeapon(gc.currentWeapon));
    }

    private void UpdateCurrentNextFrame(GameObject _) {
        UpdateCurrentNextFrame();
    }

    private void UpdateCurrentNextFrame(int _) {
        UpdateCurrentNextFrame();
    }

    private void UpdateCurrentNextFrame() {
        if (!SceneUtils.IsInGame()) return;
        ExecutionUtils.RunNextFrame(UpdateCurrent);
    }

    private void UpdateCurrent(GameObject _) {
        UpdateCurrent();
    }

    private void UpdateCurrent(float _) {
        UpdateCurrent();
    }

    // TODO make it work for dual wield too
    private void UpdateCurrent() {
        if (!SceneUtils.IsInGame()) return;
        // Note currentSlotIndex is 1-based for some fucking reason
        // Plugin.Log.LogInfo(
        //     $"viewmodel..., [{gc?.currentSlotIndex}, {gc?.currentVariationIndex}], gc {gc}, prefs {prefs}");
        if (gc == null || prefs == null || gc.currentSlotIndex >= 6 || gc.currentVariationIndex >= 3) return;
        UpdateCurrentColor();
        // Plugin.Log.LogInfo("viewmodel updating");
        int middle = GetMiddleIndex(); // 0 middle, 1 right, 2 left
        // Plugin.Log.LogInfo($"Middle {middle}");
        int wi = gc.currentSlotIndex - 1;
        int altVariationIndex = 0;
        GameObject? weapon = gc.currentWeapon;
        // Plugin.Log.LogInfo($"Weapon {weapon}");
        if (weapon == null) return;
        switch (wi) {
            case 0:

                var rcomp = weapon.GetComponent<Revolver>();
                if (rcomp?.altVersion ?? false) altVariationIndex = 1;
                break;
            case 1:
                var scomp = weapon.GetComponent<ShotgunHammer>();
                if (scomp != null) altVariationIndex = 1; // If null we know there's only Shotgun, no hammer
                break;
            case 2:
                var ncomp = weapon.GetComponent<Nailgun>();
                if (ncomp?.altVersion ?? false) altVariationIndex = 1;
                break;
        }

        int weaponSettingIndex = GetSpecificWeaponVariantOffsetIndex(wi, altVariationIndex);
        WeaponPos wpos = weapon.GetComponent<WeaponPos>();

        // Base
        Vector3 basePos = BasePositions[middle, wi, altVariationIndex];
        Vector3 baseAngle = BaseAngles[middle, wi, altVariationIndex];
        Vector3 baseScale = BaseScales[middle, wi, altVariationIndex];

        // Target
        Vector3 posOffset = new Vector3(ModelPosXOffset.Value, ModelPosYOffset.Value, ModelPosZOffset.Value);
        Vector3 angleOffset =
            new Vector3(ModelPitchOffset.Value, ModelYawOffset.Value, ModelRollOffset.Value);
        float scaleOffset = ModelScaleOffset.Value;

        if (weaponSettingIndex != -1) {
            posOffset += new Vector3(WeaponModelPosXOffsets[weaponSettingIndex].Value,
                WeaponModelPosYOffsets[weaponSettingIndex].Value, WeaponModelPosZOffsets[weaponSettingIndex].Value);
            angleOffset += new Vector3(WeaponModelPitchOffsets[weaponSettingIndex].Value,
                WeaponModelYawOffsets[weaponSettingIndex].Value, WeaponModelRollOffsets[weaponSettingIndex].Value);
            scaleOffset += WeaponModelScaleOffsets[weaponSettingIndex].Value;
        }

        float offsetMultiplier = IsEnabled ? 1 : 0;
        Vector3 targetPos = basePos + posOffset * offsetMultiplier;
        Vector3 targetAngle = baseAngle + angleOffset * offsetMultiplier;
        Vector3 targetScale = baseScale + Vector3.one * scaleOffset * offsetMultiplier;

        // Set
        SetTransform(weapon, targetPos, targetAngle, targetScale);
        // Plugin.Log.LogInfo($"Weapon {wi} [alt:{altVariationIndex}] [middle:{middle}]");
        // Plugin.Log.LogInfo($"local pos -> {targetPos}");
        // Plugin.Log.LogInfo($"local angle -> {targetAngle}");
        // Plugin.Log.LogInfo($"local scale -> {baseScale}");
    }

    private void UpdateCurrentColor() {
        if (!WeaponEdges.Value || gc == null || fc == null || HookArm.Instance == null) return;

        // Apply gun
        var weaponVariantIndex = GunHelper.GetVariation(gc.currentWeapon, gc.currentSlotIndex - 1);
        ApplyWireframe(gc.currentWeapon, weaponVariantIndex);

        // Apply arm. Note (variant / 2) transforms 0 -> 0 and 1 -> 2, because 2nd fist is red but 2nd variant is green
        var armVariantIndex = PlayerPrefs.GetInt("CurArm", 0);
        ApplyWireframe(fc.currentArmObject, armVariantIndex * 2);

        // Apply whiplash. It's green -> variant 1
        ApplyWireframe(HookArm.Instance.gameObject, 1);

        UpdateVariantColors();
    }

    private void ApplyWireframe(GameObject obj, int variantIndex) {
        if (obj == null) return;
        var mat = GetVariantMat(variantIndex);
        var comps = obj.GetComponentsInChildren<Renderer>(true)
            ?.Where(r => r is (MeshRenderer or SkinnedMeshRenderer))?
            .ToArray() ?? [];

        foreach (var comp in comps) {
            if (comp == null) continue;

            List<Material> sharedMats = [];
            comp.GetSharedMaterials(sharedMats);
            int count = sharedMats.Count;
            if (count == 0) continue;

            // Check if we really have to update
            bool needsUpdate = false;
            for (int i = 0; i < count; i++) {
                if (sharedMats[i] != mat) {
                    needsUpdate = true;
                    break;
                }
            }

            // Apply change if needed
            if (needsUpdate) {
                Material[] newMats = new Material[count];
                for (int i = 0; i < count; i++) {
                    newMats[i] = mat;
                }

                comp.sharedMaterials = newMats;
            }
        }
    }

    private void SetTransform(GameObject weapon, Vector3 targetPos, Vector3 targetAngle, Vector3 targetScale) {
        var trans = weapon.transform;
        // wpos.currentDefault = targetPos;
        trans.localPosition = targetPos;
        trans.localEulerAngles = targetAngle;
        trans.localScale = targetScale;
    }

    private void ResetAllTransforms() {
        int middleIndex = GetMiddleIndex();
        ResetTransforms<Revolver>(middleIndex, 0);
        ResetTransforms<Shotgun>(middleIndex, 1);
        ResetTransforms<ShotgunHammer>(middleIndex, 1);
        ResetTransforms<Nailgun>(middleIndex, 2);
        ResetTransforms<Railcannon>(middleIndex, 3);
        ResetTransforms<RocketLauncher>(middleIndex, 4);
    }

    private void ResetTransforms<T>(int middleIndex, int weaponIndex) where T : MonoBehaviour {
        if (middleIndex >= 2 || weaponIndex >= 5) return;
        var comps = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var comp in comps) {
            int altVariantIndex = 0;
            if (comp is ShotgunHammer || comp is Revolver { altVersion: true } || comp is Nailgun { altVersion: true })
                altVariantIndex = 1;
            // Plugin.Log.LogInfo($"Resetting {comp} whose indices are [{middleIndex},{weaponIndex},{altVariantIndex}]");
            Vector3 basePos = BasePositions[middleIndex, weaponIndex, altVariantIndex];
            Vector3 baseAngle = BaseAngles[middleIndex, weaponIndex, altVariantIndex];
            Vector3 baseScale = BaseScales[middleIndex, weaponIndex, altVariantIndex];
            SetTransform(comp.gameObject, basePos, baseAngle, baseScale);
        }
    }

    private void UpdateBobbing() {
        if (!SceneUtils.IsSafe() || gc == null) return;
        var comp = gc.GetComponent<WalkingBob>();
        if (comp == null) return;
        comp.enabled = Bobbing.Value;
    }
}
