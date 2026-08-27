using NukeLib.Game;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;

namespace ThornClient.Modules.Utility;

/// <summary>
/// Module that allows you to bind keys to specific weapon variants
/// </summary>
public class WeaponVariantBinds : Module {
    /// <summary>
    /// The singleton instance
    /// </summary>
    public static WeaponVariantBinds? Instance;

    /// <summary>
    /// Number of weapons available
    /// </summary>
    public const int NumWeapons = 5;

    /// <summary>
    /// Number of variants available
    /// </summary>
    public const int NumVariants = 3;

    /// <summary>
    /// Gun setting IDs
    /// </summary>
    public static readonly string[,] GunIds = new[,] {
        { "revolver_piercer", "revolver_marksman", "revolver_sharpshooter" },
        { "shotgun_core_eject", "shotgun_pump_charge", "shotgun_sawed_on" },
        { "nailgun_attractor", "nailgun_overheat", "nailgun_jumpstart" },
        { "railcannon_electric", "railcannon_screwdriver", "railcannon_malicious" },
        { "rocket_freezeframe", "rocket_srs", "rocket_firestarter" }
    };

    private static readonly string[,] GunNamesRaw = new[,] {
        {
            "Piercer Revolver",
            "Marksman Revolver",
            "Sharpshooter Revolver"
        }, {
            "Core Eject Shotgun (or Hammer)",
            "Pump Charge Shotgun (or Hammer)",
            "Sawed-on Shotgun (or Hammer)"
        }, {
            "Attractor Nailgun (or Sawblade Launcher)",
            "Overheat Nailgun (or Sawblade Launcher)",
            "Jumpstart Nailgun (or Sawblade Launcher)"
        }, {
            "Electric Railcannon",
            "Screwdriver Railcannon",
            "Malicious Railcannon"
        }, {
            "Freezeframe Rocket Launcher",
            "S.R.S. Cannon Rocket Launcher",
            "Firestarter Rocket Launcher"
        }
    };

    private static readonly string[,] GunNames = new[,] {
        {
            "<color=#9c9c9c>Revolver   :</color> <color=#40e7ff>Piercer</color>",
            "<color=#9c9c9c>Revolver   :</color> <color=#44ff45>Marksman</color>",
            "<color=#9c9c9c>Revolver   :</color> <color=#f00>Sharpshooter</color>"
        }, {
            "<color=#ffffff>Shotgun    :</color> <color=#40e7ff>Core</color>",
            "<color=#ffffff>Shotgun    :</color> <color=#44ff45>Pump</color>",
            "<color=#ffffff>Shotgun    :</color> <color=#f00>Sawed-on</color>"
        }, {
            "<color=#9c9c9c>Nailgun    :</color> <color=#40e7ff>Attractor</color>",
            "<color=#9c9c9c>Nailgun    :</color> <color=#44ff45>Overheat</color>",
            "<color=#9c9c9c>Nailgun    :</color> <color=#f00>Jumpstart</color>"
        }, {
            "<color=#ffffff>Railcannon :</color> <color=#40e7ff>Electric</color>",
            "<color=#ffffff>Railcannon :</color> <color=#44ff45>Screwdriver</color>",
            "<color=#ffffff>Railcannon :</color> <color=#f00>Malicious</color>"
        }, {
            "<color=#9c9c9c>Rocket     :</color> <color=#40e7ff>Freezeframe</color>",
            "<color=#9c9c9c>Rocket     :</color> <color=#44ff45>S.R.S.</color>",
            "<color=#9c9c9c>Rocket     :</color> <color=#f00>Firestarter</color>"
        }
    };

    /// <summary>
    /// Keybinds for the weapon variants
    /// </summary>
    public Setting<Keybind>[,] Binds = new Setting<Keybind>[5, 3];

    /// <summary>
    /// Icon of the module
    /// </summary>
    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "ab_key");

    /// <summary>
    /// Constructor
    /// </summary>
    public WeaponVariantBinds() : base("thorn.weaponVariantBinds", "Weapon Variant Binds",
        "Allows you to bind keys to specific weapons",
        ModuleCategory.Utility) {
        if (Instance != null) return;
        Instance = this;

        // Create the bind settings
        for (int i = 0; i < NumWeapons; i++) {
            for (int j = 0; j < NumVariants; j++) {
                Binds[i, j] = CreateSetting(GunIds[i, j], GunNames[i, j], $"Keybind to switch to {GunNamesRaw[i, j]}",
                    new Keybind(KeyCode.None));
                var i1 = i;
                var j1 = j;
                Binds[i, j].OnPress += () => Switch(i1, j1);
            }
        }
    }

    private GunControl? gcon => GunControl.Instance;

    private void Switch(int weaponIndex, int variantIndex) {
        if (!IsEnabled || (ClickGUI.Instance?.IsEnabled ?? false)) return;
        if ((weaponIndex is >= NumWeapons or < 0) || (variantIndex is >= NumVariants or < 0) || gcon == null || gcon.slots.Count <= weaponIndex) return;
        var slotList = gcon.slots[weaponIndex];
        if (slotList == null) return;

        for (int i = 0; i < slotList.Count; i++) {
            var weapon = slotList[i];
            int currVariant = GunHelper.GetVariation(weapon, weaponIndex);
            if (currVariant == variantIndex) {
                gcon.ForceWeapon(weapon);
                return;
            }
        }
    }
}
