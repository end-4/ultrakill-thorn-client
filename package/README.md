# Thorn Core

[GitHub](https://github.com/end-4/ultrakill-thorn-client)  ||  [Wiki](https://github.com/end-4/ultrakill-thorn-client/wiki)

## Powerful config system

- Supports all the usual basic data types (string, bool, numbers, enums), plus:
  - Colors with rainbow pulse
  - Enemy types
  - Keybinds with modifiers (e.g. Ctrl+A instead of just A)
  - Any custom data you can serialize to JSON, and allows custom UI
- Compact UI, great for tuning settings live (no Esc and multiple clicks)
- Hot reload on disk change

Comparison with PluginConfig and Configgy: see [Thorn Wiki](https://github.com/end-4/ultrakill-thorn-client/wiki#config-system)

## Screenshots

<img src="https://github.com/user-attachments/assets/1707ce0b-a4de-41f8-a0f9-be1dcc8aa992" />

## Usage

```csharp

public class HatsModConfig : Module 
{
    public static Setting<bool> FunkyMode = null!;
    public static Setting<EnhancedColor> HatColor = null!;
    public static Setting<FilePath> HatTexture = null!;

    public HatsModConfig() : base("myHatsMod.config", "Hats Mod", "Adds custom hat to enemies", ModuleCategory.Gameplay) 
    {
        FunkyMode = CreateSetting("funkyMode", "Funky Mode", "Funny effects", false);
        HatColor = CreateSetting("color", "Hat Color", "The color for the custom hat", new EnhancedColor(0.66f, 1f, 0.97f, 0.5f));
        HatTexture = CreateSetting("texture", "Hat Texture", "The texture for the custom hat", HatsMod.DefaultHatTexturePath);
    }
}

[BepInDependency("com.github.end-4.thornClient")]
public class HatsMod : BaseUnityPlugin 
{
    public const string DefaultHatTexturePath = "/path/to/texture.png";

    private void Update() 
    {
        Logger.LogInfo($"Funky Mode: {HatsModConfig.FunkyMode.Value}");
        Logger.LogInfo($"Hat Color: {HatsModConfig.HatColor.Value.GetCurrentColor()}");
        Logger.LogInfo($"Hat Texture: {HatsModConfig.HatTexture.Value.ToString()}");
    }
}
```

For more details, see [Thorn Wiki](https://github.com/end-4/ultrakill-thorn-client/wiki/Modules)

## Copyleft

Thorn's **code** is released under the **LGPL-3.0** license, which means you are free to use its config system in your mod, whether or not it's open source; modifications to Thorn's code itself however are required to be open-sourced.

**Assets** are released under the **CC BY-SA 4.0** license. This means you are free to reuse them as long as you give attribution and maintain the same license for derivative works.

## Mods that use Thorn

- [SpawnerArmExtras](https://thunderstore.io/c/ultrakill/p/BillyMod/SpawnerArmExtras/)
- [BillionNemesis](https://thunderstore.io/c/ultrakill/p/BillyMod/BillionNemesis/)
- [LGBTQ Hitscan](https://thunderstore.io/c/ultrakill/p/achelia/LGBTQ_Hitscan/)
- [Crossover](https://thunderstore.io/c/ultrakill/p/end_4/Crossover/)

## Technical description

Thorn as a whole is an utility mod, with its own config system featuring a compact UI to house its 50+ modules. This package contains the base system without those modules.
