# Changelog

## 2026-06-23_125849_1782212329
- Bug audit fixes (batch 1 — behavioral). Fixed five medium-severity issues found in a full source audit:
  - **Headshot % damage no longer stacks with the all-body %** (`Patches/ApplyDamage.cs`). For head hits the
    all-body multiplier is now skipped when "Percentage Headshot Damage" is enabled, so the head value OVERRIDES
    the body value as the config text promises (previously they multiplied, e.g. 50% × 50% = 25%).
  - **"Keep 1 Health" at-minimum protection now respects the toggle and the body-part selection**
    (`Patches/ApplyDamage.cs`). The trailing at-minimum guard is gated on `Keep1Health` and only blocks Head/Chest
    under "Head And Thorax" — previously it made *every* blacked-out limb invincible, even with Keep 1 Health off.
    Also removed the no-op `currentHealth.Current = 3f` writes (a `ValueStruct` copy — they did nothing) and the
    misleading "below 3f" comment.
  - **COD Mode regen is now framerate-independent** (`Features/CODMode.cs`). Healing runs on a fixed 1-second
    real-time interval instead of "every 60 rendered frames", and clamps each part to its maximum so it can't
    overshoot. Heal rate now means roughly HP/second per limb (was FPS-dependent).
  - **Mag reload/unload speed no longer leaks or clobbers config** (`Features/MagReloadSpeed.cs`). The real engine
    `BaseLoadTime`/`BaseUnloadTime` are snapshotted before being overwritten and restored on disable and on raid
    exit (`OnDestroy`); the user's saved slider values are no longer overwritten with hardcoded 0.85/0.3.
  - **EFT version check is now actually run** (`Plugin.cs`). `Awake()` creates the logger first, then calls
    `CheckEftVersion(...)` and aborts binding/patching on a build mismatch (it was previously dead code, so all
    Harmony patches installed regardless of the running game version).

## 2026-06-23_120701_1782209221
- Reverted the earlier decision to ignore `references/`. The folder holds the build dependencies
  (Unity engine modules, EFT `Assembly-CSharp.dll`, SPT/BepInEx/Harmony loaders) the `.csproj`
  references, so the project does not build without it. Removed the `/references` rule from
  `.gitignore` and re-tracked all 115 DLL/XML files (~44 MB) so clones build out of the box.

## 2026-06-23_120409_1782209049
- Set authorship to **Balls0fSteel** (this fork): updated README credits, `LICENSE.txt` copyright holder
  (added Balls0fSteel 2024-2026, retained dvize for the original), and `Properties/AssemblyInfo.cs`
  (`AssemblyCompany` + `AssemblyCopyright`). Original author dvize remains credited throughout.

## 2026-06-23_120217_1782208937
- Added `README.md` documenting the mod: overview ("CoD-style easy mode" features for SPT), revival/fork
  status (originally by dvize), compatibility (SPT 4.0.x / EFT build 40087, plugin v1.9.4), installation,
  the full F12 feature/config reference (Health, COD Mode, QOL sections), build instructions, packaging
  notes, an architecture overview, disclaimer, credits, and license.
- Stopped tracking the `references/` folder (binary game/loader deps). It was committed before being added
  to `.gitignore`, so git kept tracking all 91 files; removed from the index with `git rm -r --cached`
  while keeping them on disk. The `/references` ignore rule now takes effect.
