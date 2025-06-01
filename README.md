A re-implmentation of Wacom's Click & Tap feature as a plugin for use with OpenTabletDriver.

## Dependencies

- OpenTabletDriver 0.6.4.0
- .NET 6.0 (for OpenTabletDriver 0.6.4.0)
- .NET 8.0 (for the Editor App in ClickTap.UX) (Will be required in the future for OpenTabletDriver 0.6.5.0)

## Installation

1. Download ClickTap-0.6.x.zip,
2. Open OpenTabletDriver's Plugin Manager,
3. On Windows, you may Drag & Drop the zip file while on Unix-based platforms, you will need to go to `File` > `Install Plugins...`,
4. Close the Plugin Manager,
5. Switch to the `Tools` tab,
6. Enable `Click & Tap Daemon`,

### For Specific Tablets

7. Switch to the `Filters` tab,
8. Enable `Click & Tap`.

### Configuring Bindings

Unfortunately, The UI Framework OpenTabletDriver uses it too complicated to properly use,
So i built a separate app, using the future UI Framework for OpenTabletDriver 0.7, Avalonia.

1. Download the App for your platform (For example on windows 64 bits, download `ClickTap.UX-win-x64.zip`),
2. Extract the App anywhere (Most likely next to OpenTabletDriver),
3. Ensure that OpenTabletDriver is running,
4. Open `ClickTap.UX.exe`,

From there, you should see this :

![Bindings Overview](/docs/readme/img/bindings_overview.png)

> [!WARNING]
> If you see a connecting screen, make sure that OpenTabletDriver is running (or at least the Daemon).

> [!WARNING]
> If the app just closes or do not open, Open an Issue with OTD's diagnostics (Help > Export Diagnostics). & Stacktrace otentially obtained from The event viewer on Windows, or your terminal on linux

From there, this interface should be very familiar to you.
Don't forget to Apply, then save if you want changes to be persistent.

If you want to rollback to the settings you had before applying, you may just restart OpenTabletDriver.

## Known Issues

1. In 0.6.x, Plugins with extra settings like my own `Scroll Binding` bindings, don't have any way to be configured from inside the app.
If you plan on using `Scroll Binding` however, Use `Legacy Scroll Binding` instead in the type & configure it in OTD's Tool tab.

This is due to an external issue on my Avalonia port of some UI Controls & would require a lot more work to implement.
See https://github.com/Mrcubix/OpenTabletDriver.External/issues/1 for more details.

You may additionally copy the store from OTD's settings to ClickTap's settings. 
(The settings file can be found in ./userdata/settings.json (Windows) or in your local appdata directory)

```json
{
  "Store": {
    "Path": "ScrollBinding.NewBulletproofScrollBinding",
    "Settings": [
      {
        "Property": "Delay",
        "Value": 15
      },
      {
        "Property": "Amount",
        "Value": 120
      },
      {
        "Property": "Property",
        "Value": "Scroll Forward"
      }
    ],
    "Enable": true
  }
}
```

THIS ALSO MEANS THAT MULTI-KEY BINDINGS WILL NOT WORK, unless you copy the settings from OpenTabletDriver to ClickTap's settings, like for `Scroll Binding`.

2. The implementation for both the UX & the plugin is still fragile, and might be prone to breaking, if such things occur, 
Open an Issue with OTD's diagnostics (Help > Export Diagnostics). & Stacktrace potentially obtained from The event viewer on Windows, or your terminal on Linux

3. I'm unable to test MacOS builds due to not having an Apple device or a VM running MacOS, and for a reason i'm currently unable to figure out, the app will not run.

## Packaging

You may run any of the packaging scripts from the root of the repo.
The linux packaging script, need to be run in a Unix-based environment. (Linux, MacOS, maybe WSL?)