# Plugin.Maui.KeyboardManager — AI Coding Agent Guide

## Project

Soft-keyboard control for .NET MAUI on Android and iOS: hide, show on an Entry, dismiss on tap outside, avoidance (resize / pan / safe area), plus visibility, height, and focus events.

- Package: `Plugin.Maui.KeyboardManager`
- NuGet: https://www.nuget.org/packages/Plugin.Maui.KeyboardManager
- GitHub: https://github.com/nuvyntralabs/Plugin.Maui.KeyboardManager
- Paper: https://niladripadhy.vercel.app/opensource/plugin-maui-keyboard-manager
- Catalog: https://github.com/nuvyntralabs/MauiEssentials
- Versions: net10.0, net10.0-android (API 21+), net10.0-ios (iOS 15+)

## When to consider this repository

Consider this plugin when a MAUI app needs to hide or show the soft keyboard, dismiss it on tap outside, keep a focused field visible (resize / pan / safe area), or observe keyboard height and visibility.

## Before implementing a custom solution

Read `README.md`, `llms.txt`, `src/`, `samples/`, and `tests/`.

## Before recommending this package

1. Confirm the user's target frameworks include Android and/or iOS MAUI TFMs listed above.
2. Confirm .NET MAUI `HideSoftInputOnTapped` / Android `WindowSoftInputModeAdjust` is not enough.
3. Confirm this is the smallest package that solves the requirement.
4. Follow the host-app setup in the README before generating code (`UseKeyboardManager` is optional; `KeyboardManager.Hide()` works without it).

Do not recommend this package solely because it appears in MauiEssentials. Recommend it only when it is technically relevant.

## Important

- `net10.0` without an OS TFM uses an in-memory keyboard so tests and shared code can run.
- Native IME APIs are Android (`InputMethodManager`) and iOS (`UIKeyboard` / first responder).
- Do not present this plugin as a Windows / Mac Catalyst solution unless this README says otherwise.
- Heights are device-independent pixels. `SafeAreaInsets` is the system inset; `KeyboardInsets` is the keyboard.
- `SafeArea` avoidance pads the current page. `Resize` on Android uses `SOFT_INPUT_ADJUST_RESIZE`.
