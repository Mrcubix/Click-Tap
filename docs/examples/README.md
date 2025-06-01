## Example Setting Files

This directory contains examples of settings files as well as, in this README, some explanations on how they behave.

### Installation

Drop these settings files in the same directory as OpenTabletDriver's `settings.json` file.
The Setting file should be named `Click-Tap.json` in order for the plugin to be able to find & process it.

![Settings File Location](/docs/examples/img/settings_file.png)

Once placed properly, Open the file in any text editor change the `Name` keypair value with your own tablet name.
(Use the name found at the bottom tight of OpenTabletDriver's Tablet Selection Dropdown)

```json
{
  "Version": 2,
  "Profiles": [
    {
      "Name": "Wacom PTK-640",
...
```

For example, if your tablet is a Wacom CTH-480, change the `Name` keypair value to `Wacom CTH-480`.

```json
{
  "Version": 2,
  "Profiles": [
    {
      "Name": "Wacom CTH-480",
...
```

Or if you have a Gaomon S620, change the `Name` keypair value to `Gaomon S620`:

```json
{
  "Version": 2,
  "Profiles": [
    {
      "Name": "Gaomon S620",
...
```

### Explanations

#### Left Click Tip & Right - Middle Click Pen Button

When not pushing any of 2 pen buttons & using the pen tip, the left click will be triggered. 

If you push the first pen button, then use the pen tip, a Right Click will be triggered.
If you push the second pen button, then use the pen tip, a Middle Click will be triggered.

#### Left Click Eraser & Right - Middle Click Pen Button

Same as previous, except using the eraser instead of the pen tip.

#### Right - Middle Click Pen Button

Same as [First Example](#left-click-tip--right---middle-click-pen-button), except using the pen tip without pressing any pen button won't trigger anything.

#### Left Click Tip & Right - Middle Aux Keys

Same as [First Example](#left-click-tip--right---middle-click-pen-button), except that the right & middle click are triggered using the aux keys.