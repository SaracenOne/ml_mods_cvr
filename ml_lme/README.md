# Leap Motion Extension
This mod allows you to use your Leap Motion controller for hands and fingers visual tracking.

[![](.github/img_01.png)](https://youtu.be/nak1C8uibgc)

# Installation
* Install [latest Ultraleap Gemini tracking software](https://developer.leapmotion.com/tracking-software-download)
* Install [latest MelonLoader](https://github.com/LavaGang/MelonLoader)
* Get [latest release DLL](../../../releases/latest):
  * Put `ml_lme.dll` in `Mods` folder of game

# Usage
## Settings
Available mod's settings in `Settings - Implementation - Leap Motion Tracking`:
* **Enable tracking:** enable hands tracking from Leap Motion data, disabled by default.
* **Tracking mode:** set Leap Motion tracking mode, available values: `Screentop`, `Desktop` (by default), `HMD`.
* **Desktop offset X/Y/Z:** offset position for body attachment, (0, -45, 30) by default.
* **Attach to head:** attach hands transformation to head instead of body, disabled by default.
* **Head offset X/Y/Z:** offset position for head attachment (`Attach to head` is **`true`**), (0, -30, 15) by default.
* **Offset angle:** rotation around X axis, useful for neck mounts, 0 by default.
* **Fingers tracking only:** apply only fingers tracking, disabled by default.
* **Model visibility:** show Leap Motion controller model, useful for tracking visualizing, disabled by default.
