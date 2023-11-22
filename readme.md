# MBM-Mods
*SoapBoxHero's mods for MonsterBlackMarket*

## Installation
1. Backup your game files.
1. Install bepinex [6.0.0-pre.1](https://github.com/BepInEx/BepInEx/releases/tag/v6.0.0-pre.1)
1. (Optional) Download [Configmanager UI](https://github.com/SoapSoapSoapS/BepInEx.ConfigurationManager6.Core/releases/tag/v1.0) to change settings in game.
1. [Download the plugins](https://github.com/SoapSoapSoapS/MBM-Mods/releases/latest) you want to use + the tools plugin (the tools plugin is required for the others to work).
1. Add the .dll files you just downloaded to `<YourGameFolder>/BepInEx/Plugins`
1. Run the game.
   - If using Configmanager, just press F1 in game to change settings.
   - If not, stop the game and change settings in `<YourGameFolder>/BepInEx/Config/sss.plugin.<pluginName>.cfg`
---

## PunnettRebalance
A genetic rework to make the game harder.

### Features:

- 50/50 inheritance, each gene is inherited from either the mother or father.
   - In base game, they are added together and capped at +- 5% change from parents. 
   - This change caps default genetic enhancements, makes the special Dragon trait more important, makes balance more interesting.

### Config:

```
[PunnettInheritance]

## Enables 50/50 genetic inheritance, unless a special inheritance trait applies.
# Setting type: Boolean
# Default value: true
EnableGenetics = true

## Enables custom names and name inheritance
# Setting type: Boolean
# Default value: true
EnableNaming = true
```

---
## Restless
QOL changes for the impatient.

### Features:

- Custom override to unit Rest Time.
- Allow dragging units in stopped time.

### Config:

```
[DragInStoppedTime]

## Allows dragging units in stopped time.
# Setting type: Boolean
# Default value: false
Enable = false

[RestTime]

## The time in seconds that a unit will rest before starting a new activity.
# Setting type: Single
# Default value: 15
# Acceptable value range: From 1 to 64
Seconds = 15

## Allow custom RestTime.
# Setting type: Boolean
# Default value: false
Enable = false
```

---

## Keybinds
Keybinds to set time speed.

### Features:

- Customizeable keybinds (single key only) for each time speed higher than 1x.

### Config:

```
[TimeControls]

## Keybind for 1.5x speed
# Setting type: KeyCode
# Default value: None
Speed1_5 = None

## Keybind for 2x speed
# Setting type: KeyCode
# Default value: None
Speed2 = None

## Keybind for 3x speed
# Setting type: KeyCode
# Default value: None
Speed3 = None

## Keybind for 4x speed
# Setting type: KeyCode
# Default value: None
Speed4 = None

## Keybind for 5x speed
# Setting type: KeyCode
# Default value: None
Speed5 = None
```