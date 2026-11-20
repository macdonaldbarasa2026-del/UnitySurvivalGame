# Unity Survival

An offline-first 3D survival game for Android, built with Unity 6. The whole world is generated
procedurally at runtime, so the project needs no downloaded models, textures, audio or other assets.

- Package ID: `com.mbgee.unitysurvivalgame`
- Game title: `Unity Survival`
- Orientation: landscape (auto-rotate between landscape orientations)
- Unity version: `6000.0.32f1`
- Platforms: Android (ARM64, API 23+), desktop for development

## What is in the game

- Procedural terrain with mountains, a river, a lake, forests, rocks, fibre plants and berry bushes
- Third-person character controller with camera orbit, sprinting, jumping and procedural motion
- Survival stats: health, stamina, hunger, thirst and body temperature with environmental damage
- Enemies with patrol / chase / attack AI, wildlife that flees, and loot drops
- Grid inventory, consumables and crafting (stone axe, bandage, torch)
- Melee combat with blocking and resource harvesting
- Day/night cycle, temperature changes and procedural rain cycles
- A cabin shelter and a campfire that provides warmth and can be refuelled
- Main menu, in-game HUD, inventory screen, pause, game-over and victory screens
- Sound effects and ambient music generated from raw samples at runtime — fully offline
- Keyboard/mouse plus an on-screen joystick, look pad, action buttons and pinch zoom for touch devices

## Controls

| Action | Desktop | Touch |
|---|---|---|
| Move | WASD / arrows | Left joystick |
| Look | Mouse | Right look pad |
| Sprint | Shift | RUN button |
| Jump | Space | JUMP button |
| Attack / harvest | Left mouse / F | ATTACK button |
| Block | Right mouse / Q | BLOCK button |
| Interact | E | USE button |
| Inventory / crafting | Tab or I | BAG button |
| Pause | Esc or P | MENU button |
| Zoom | Scroll wheel | Pinch |

## Opening the project

1. Install Unity `6000.0.32f1` with the **Android Build Support**, Android SDK/NDK and OpenJDK modules.
2. Open this folder in Unity Hub.
3. Open `Assets/Scenes/Main.unity` and press Play.
4. Everything is assembled by `GameBootstrap` at runtime. The optional
   `Tools > Survival Game > Setup Complete Starter Scene` menu still exists for legacy authoring.

## Automated builds (GitHub Actions)

`.github/workflows/android-build.yml` does the following on every push to `main`:

1. Static project validation (`scripts/validate_project.py`).
2. Offline C# compile check (`scripts/compile_check.sh` with `tests/UnityStubs.cs`).
3. Two Unity builds using [GameCI unity-builder](https://game.ci/docs/github/builder):
   `UnitySurvival-debug.apk` and `UnitySurvival-release.apk`.
4. APK integrity verification and upload as a GitHub Actions artifact.

Pushing a `v*` tag additionally publishes a GitHub Release with both APKs attached.

### Required repository secrets

The GitHub-hosted runner needs a Unity license to run the Editor headlessly. Add these in
**Settings → Secrets and variables → Actions**:

| Secret | Purpose |
|---|---|
| `UNITY_LICENSE` | Contents of your Unity Personal `.ulf` license file (required) |
| `UNITY_EMAIL` | Unity account email |
| `UNITY_PASSWORD` | Unity account password |
| `UNITY_SERIAL` | Only for Pro/Plus/Enterprise serial licensing |
| `ANDROID_KEYSTORE_NAME` | Optional: filename for your release keystore |
| `ANDROID_KEYSTORE_BASE64` | Optional: base64 of the release keystore |
| `ANDROID_KEYSTORE_PASS` | Optional: keystore password |
| `ANDROID_KEYALIAS_NAME` | Optional: key alias |
| `ANDROID_KEYALIAS_PASS` | Optional: key password |

To obtain `UNITY_LICENSE`, follow the GameCI activation guide: generate an activation (`.alf`)
file in CI, upload it at <https://license.unity3d.com>, then paste the resulting `.ulf` file
contents into the secret. Never commit credentials or keystores to this repository.

Without `UNITY_LICENSE` the build job stops early with an explicit error; no APK is produced.
That is intentional so a failed build is never mistaken for a successful one.

## Installing on Android

1. Download `UnitySurvival-release.apk` (or the debug build for testing) from the GitHub Release
   or from the Actions artifact.
2. Transfer it to the phone and open it in a file manager.
3. Allow installation from unknown sources when prompted.
4. Install, then launch **Unity Survival**. The game needs no internet connection.

Requirements: Android 8.0 (API 23) or newer, ARM64 device, roughly 150 MB free space.

## Repository layout

```
Assets/Scripts/          Gameplay, UI, environment, inventory, audio, input
Assets/Scenes/Main.unity Minimal bootstrap scene
ProjectSettings/         Android player, graphics and quality settings
scripts/                 Local validation and compile checks
tests/UnityStubs.cs      Minimal Unity API stubs for offline compiler checks
.github/workflows/       Android APK build and release pipeline
```
