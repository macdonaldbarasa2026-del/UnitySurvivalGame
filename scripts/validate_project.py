#!/usr/bin/env python3
"""Static validation of the UnitySurvivalGame project.

Runs locally and in GitHub Actions. It cannot replace a Unity compile, but it catches
broken scene/settings references and the common source mistakes that break CI builds.
"""
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
errors = []
checks = 0


def check(condition, message):
    global checks
    checks += 1
    if not condition:
        errors.append(message)


def text(relative_path):
    path = ROOT / relative_path
    if not path.is_file():
        errors.append(f"Missing file: {relative_path}")
        return ""
    return path.read_text(encoding="utf-8", errors="replace")


version = text("ProjectSettings/ProjectVersion.txt")
check(re.search(r"m_EditorVersion:\s*\S+", version) is not None, "ProjectVersion.txt is invalid")

settings = text("ProjectSettings/ProjectSettings.asset")
check("productName: Unity Survival" in settings, "Product name is not 'Unity Survival'")
check("com.mbgee.unitysurvivalgame" in settings, "Package ID is missing from ProjectSettings")
check("defaultScreenOrientation: 2" in settings, "Orientation is not forced to landscape")
check("activeInputHandler: 2" in settings, "Both input systems must be enabled for keyboard + touch")
check("AndroidMinSdkVersion: 23" in settings, "Android min SDK is not configured")

build_settings = text("ProjectSettings/EditorBuildSettings.asset")
scene_guid_match = re.search(r"path:\s*Assets/Scenes/Main\.unity\s*\n\s*guid:\s*([0-9a-f]{32})", build_settings)
check(scene_guid_match is not None, "Main.unity is not registered in EditorBuildSettings")
if scene_guid_match:
    scene_guid = scene_guid_match.group(1)
    scene_meta = text("Assets/Scenes/Main.unity.meta")
    check(f"guid: {scene_guid}" in scene_meta, "Scene GUID does not match Main.unity.meta")

bootstrap_meta = text("Assets/Scripts/Core/GameBootstrap.cs.meta")
bootstrap_guid_match = re.search(r"guid:\s*([0-9a-f]{32})", bootstrap_meta)
check(bootstrap_guid_match is not None, "GameBootstrap.cs.meta has no GUID")
if bootstrap_guid_match:
    scene = text("Assets/Scenes/Main.unity")
    check(bootstrap_guid_match.group(1) in scene, "Main.unity does not reference GameBootstrap script")

ci_build = text("Assets/Scripts/Editor/CiBuild.cs")
check("BuildDebug" in ci_build, "CiBuild.BuildDebug entry point is missing")
check("BuildRelease" in ci_build, "CiBuild.BuildRelease entry point is missing")
check("com.mbgee.unitysurvivalgame" in ci_build, "CiBuild package ID mismatch")

scripts = sorted((ROOT / "Assets" / "Scripts").rglob("*.cs"))
check(len(scripts) >= 15, f"Expected the full script set, found {len(scripts)} C# files")
for script in scripts:
    source = script.read_text(encoding="utf-8", errors="replace")
    relative = script.relative_to(ROOT).as_posix()
    check(source.count("{") == source.count("}"), f"Unbalanced braces in {relative}")
    check("namespace SurvivalGame" in source, f"Missing SurvivalGame namespace in {relative}")
    check("menuMenuName" not in source, f"Known CreateAssetMenu typo present in {relative}")

required = [
    "Assets/Scripts/Core/GameInput.cs",
    "Assets/Scripts/Core/GameBootstrap.cs",
    "Assets/Scripts/Environment/WorldGenerator.cs",
    "Assets/Scripts/Environment/WeatherSystem.cs",
    "Assets/Scripts/UI/GameUI.cs",
    "Assets/Scripts/UI/MobileControls.cs",
    "Assets/Scripts/Audio/ProceduralAudio.cs",
    "Assets/Scripts/Inventory/ItemDatabase.cs",
]
for relative in required:
    check((ROOT / relative).is_file(), f"Missing required script: {relative}")

all_files = [p for p in ROOT.rglob("*") if p.is_file() and ".git" not in p.parts]
secret_patterns = [
    (r"gho_[A-Za-z0-9]{20,}", "GitHub token"),
    (r"-----BEGIN (RSA |EC |OPENSSH )?PRIVATE KEY-----", "private key"),
    (r"AIza[0-9A-Za-z\-_]{30,}", "Google API key"),
]
for path in all_files:
    try:
        content = path.read_text(encoding="utf-8", errors="ignore")
    except OSError:
        continue
    for pattern, label in secret_patterns:
        if re.search(pattern, content):
            errors.append(f"Possible {label} committed in {path.relative_to(ROOT).as_posix()}")

print(f"Validated {checks} project checks across {len(scripts)} C# scripts.")
if errors:
    print("FAILED:")
    for error in errors:
        print("  - " + error)
    sys.exit(1)
print("All static project checks passed.")
