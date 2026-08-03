using System;
using System.Collections.Generic;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using ThornClient.Core.DataTypes;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;

namespace ThornClient.Modules.Player;

public class WeaponVariantBinds : Module {
    public static WeaponVariantBinds? Instance;
    public const int NumWeapons = 5;
    public const int NumVariants = 3;

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

    // Allocates a 5-row, 3-column array for Keybind objects
    public Setting<Keybind>[,] Binds = new Setting<Keybind>[5, 3];

    public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "gun");

    public WeaponVariantBinds() : base("thorn.weaponVariantBinds", "Weapon Variant Binds",
        "Allows you to bind keys to specific weapons",
        ModuleCategory.Player) {

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
        if (!IsEnabled || Time.timeScale == 0) return;
        if ((weaponIndex is >= NumWeapons or < 0) || (variantIndex is >= NumVariants or < 0) || gcon == null) return;
        var slotList = gcon.slots[weaponIndex];
        if (slotList == null) return;

        // This is not very clean, but the game gives us no choice
        // Reference + "I've seen worse": https://github.com/daemon251/Ultrakill-WeaponVariantBinds/blob/580ecf6f0e150495639bcaec6ee5f48193b76bed/PluginConfig.cs#L219
        for (int i = 0; i < slotList.Count; i++) {
            var weapon = slotList[i];
            int currVariant = -1;
            switch (weaponIndex) {
                case 0:
                    var rComp = weapon.GetComponent<Revolver>();
                    if (rComp != null) currVariant = rComp.gunVariation;
                    break;
                case 1:
                    var sComp = weapon.GetComponent<Shotgun>();
                    if (sComp != null) {
                        currVariant = sComp.variation;
                    } else {
                        var shComp = weapon.GetComponent<ShotgunHammer>();
                        if (shComp != null) currVariant = shComp.variation;
                    }

                    break;
                case 2:
                    var nComp = weapon.GetComponent<Nailgun>();
                    if (nComp != null) currVariant = (4 - nComp.variation) % 3;
                    break;
                case 3:
                    var raiComp = weapon.GetComponent<Railcannon>();
                    if (raiComp != null) currVariant = raiComp.variation;
                    break;
                case 4:
                    var rocComp = weapon.GetComponent<RocketLauncher>();
                    if (rocComp != null) currVariant = rocComp.variation;
                    break;
            }

            if (currVariant == variantIndex) {
                gcon.ForceWeapon(weapon);
                return;
            }
        }
    }
}
