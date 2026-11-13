# MediaRemote

A tiny Windows tray app that turns an Xbox controller (connected over Bluetooth
or USB) into a media remote: play/pause, skip tracks, volume, and mute — all
global, so they work no matter which app has focus.

## How it works

- **Controller input** is read via **XInput** (`XInputGetState`), not the newer
  `Windows.Gaming.Input` WinRT API. That's a deliberate choice: `Gamepad` and
  `RawGameController` only deliver live button state to the foreground window
  for classic Win32 apps, so a background-only tray app like this one would see
  the controller but never get a single button press from it. XInput has no
  such restriction, which is why background gamepad-remapping tools generally
  use it too.
- **Play / Pause / Next / Previous** go through
  `GlobalSystemMediaTransportControlsSessionManager`, so they control whatever
  app currently owns the system media session (Spotify, a browser tab, etc.)
  directly — not by faking a keypress.
- **Volume up/down and mute** have no equivalent session-level API, so these
  simulate the same virtual keys (`VK_VOLUME_UP` / `VK_VOLUME_DOWN` /
  `VK_VOLUME_MUTE`) a hardware volume button sends. That's the standard
  approach, not a workaround.

## Controls (defaults)

| Button       | Action         |
|--------------|----------------|
| A            | Play / Pause   |
| B            | Next track     |
| X            | Previous track |
| Y            | Mute           |
| Left bumper  | Volume down    |
| Right bumper | Volume up      |

These can be changed at any time from the tray menu — see
[Remapping controls](#remapping-controls) below.

The tray icon's right-click menu has:
- **Controller: Connected / Disconnected** — a status label (not clickable)
  showing whether a controller is currently detected.
- **Enable / Disable** — toggles whether button presses do anything. Disabling
  keeps the app running but stops it from acting on the controller at all, so
  it won't interfere with normal controller use (e.g. gaming).
- **Remap...** — opens a window to reassign which button triggers each
  action.
- **Close** — exits the app.

## Remapping controls

Right-click the tray icon and choose **Remap...**. Each action has a dropdown
for the button that triggers it. Picking a button that's already assigned to
another action swaps the two, so the mapping always stays one-to-one — you
can never end up with two actions on the same button, or a button that does
nothing.

- **Save** applies the change immediately and writes it to
  `%AppData%\MediaRemote\mapping.json`, so it persists across restarts.
- **Restore Defaults** resets the dropdowns back to the factory mapping above
  (still requires **Save** to apply).
- **Cancel** discards any changes made in the window.

Only A, B, X, Y, and the two bumpers can be assigned — the D-pad, Start,
Back, and thumbstick clicks are intentionally left alone.

## Requirements

- Windows 11 (uses `Windows.Media.Control`, available since Windows 10
  1809+).
- [.NET 10 SDK](https://dotnet.microsoft.com/download) to build.
- An Xbox controller, paired over Bluetooth (or wired/via adapter) and showing
  up as a normal game controller in Windows.

## Build instructions

```sh
cd src
dotnet build
```

The built app lands at
`src/bin/Debug/net10.0-windows10.0.19041.0/MediaRemote.exe`.

For a self-contained release build (no .NET runtime required on the target
machine):

```sh
cd src
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The output goes to
`src/bin/Release/net10.0-windows10.0.19041.0/win-x64/publish/MediaRemote.exe`.

### Running tests

```sh
dotnet test
```

This runs the `tests/MediaRemote.Tests` xUnit project, which covers the
button-mapping logic (`ButtonMapping`) in isolation — the actual XInput
polling, WinRT media session calls, and UI aren't practically unit-testable,
so those are verified manually (see each PR's test plan).

## Running

Just launch `MediaRemote.exe`. It has no window — look for its icon in the
system tray. There's currently no installer or start-on-login setup; run it
manually, or add a shortcut to `shell:startup` if you want it to launch
automatically.
