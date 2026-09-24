## 0.5.0

- Splitted Thorn's config system and the built-in modules into separate packages: Thorn and Thorn Core.
  The former contains the modules and the latter contains the config system.
    - Regular users: no action is required if you use a mod manager. Otherwise get the Thorn Core package.
    - Mod developers utilizing the config system: it's recommended to declare Thorn Core
      instead of Thorn as the dependency. This gives your users a clean version of Thorn without
      the modules, in case they don't want them.
- Enemy tracers: fixed threshold being off by 1, added enemy type blacklist
- Fixed custom style ranks (from Ultraskins or whatever) being oversized
- Made HUD widget snapping on the sides slightly more consistent
- Fixed duplicate cheatiness notifications

## 0.4.0

- Added EnhancedColor data type. Most customizable colors can now have rainbow pulse
- New modules: Visible Portals, Extra Binds, Gravity Tweak
- New HUD widgets: Custom Label
- Added foreground (text/icon) color customization to stat HUD elements
- Fixed Railcannon charge meter jumping from 80% to full
- Viewmodel Tweaks: Edges skin: made it work with spawner arms and fixed applied to Piercer's charge effect/panel
- Weapon Variant Binds: added spawner arms
- ClickGUI: Raised the layer so the tabs are clickable when Polarite is installed and spawns a blocking notification
  panel; made Back navigation (Esc/Mouse4) on ClickGUI pop panels before going to previous layer

## 0.3.0

- Added snapping for HUD widgets
- Added support for custom setting UI elements
- New HUD modules: Alerts, Freshness, Style Multiplier, Style Feed, Style Rank, Style Points (of current rank)
- Added an option for Run In Background to only affect menus (useful for large Angry downloads)
- Fixed Freezeframe rocket ride hints flipped when doing 8-2 challenge

## 0.2.0

- Added config profiles
- New modules: Boss Bars
- New HUD modules: Rocket Fuel, Clock
- Fixed Weapon Variant Binds not working during hitstops
- Bounded value indicators:
    - Added vertical line and crosshair circle style
    - Made horizontal progress bar HUD elements have customizable length and can have icon/numbers hidden. You can now
      have clean horizontal crosshair indicators!
- Tweaked some icons for consistency & readability

## 0.1.4

- New modules: Force Enrage, Resize Enemies, Edges (World)
- Made viewmodel tweaks edges work with arms
- Added 12h/24h option for ClickGUI top bar clock
- Fixed hasToggling constructor param for Modules not working
- Fixed tooltip positioning that made buttons unclickable in certain cases
- Fixed very inconsistent icon colorizing
- Fixed dropdown texts being empty

## 0.1.3

- Added FixPluginTypesSerialization dependency (basically makes dependent mods like Billion Nemesis load properly
  without Angry already installed)
- Fixed weird bounded value hud module style selection
- Use a new menu for big enums (tmp dropdown sucks)
- Prevent setting group buttons from spawning many children window
- Made HUD elements scalable
- UI: Only colorize monochrome icons, underline active modules

## 0.1.2

- ClickGUI: prevented cheat binds and esc pause from triggering
- Fixed battery icon error spam for desktop users without a battery
- Added UI hint for enum substitutions
- Fixed a bug where you could move the drag area of a disabled HUD module's config window

_0.1.1 omitted because it was used for a description edit on Thunderstore_

## 0.1.0

- Initial release
