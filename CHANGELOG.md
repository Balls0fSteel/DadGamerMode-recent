# Changelog

## 2026-06-23_125949_1782212389
- Documented the source bug audit and added `PLAN.md` to track deferred follow-ups. The notable deferral is the
  item-weight patch (`Patches/OnWeightUpdatedPatch.cs`, audit item #10): it scales the shared static
  `InventoryEquipment.smethod_1`, so the reduction also applies to non-player callers (hideout mannequin screen,
  bot inventories). It is harmless in single-player and a correct fix needs slot-owner detection, so it was left
  as-is and recorded in `PLAN.md` rather than chased ad hoc. `PLAN.md` also records the items intentionally left
  unchanged (unconditional patch enable, the `dvize.GodModeTest` namespace, the duplicate
  `ConfigurationManagerAttributes`, and the verified-harmless Energy/Hydration event re-entrancy).

## 2026-06-23_125919_1782212359
- Bug audit fixes (batch 2 — robustness & cleanup):
  - **Reflection fields validated** (`Patches/InstantProductionPatch.cs`). If the obfuscated progress-field names
    (`GClass2438.Class1951_0` / `Class1951.Double_1`) ever fail to resolve on a future game build, the patch now
    logs a clear error and skips, instead of throwing an opaque NullReferenceException swallowed as "Unexpected
    error during CompleteProduction".
  - **Null guards** added to the per-frame `Update()` of `Features/MaxStamina.cs` and `Features/FallingDamage.cs`
    so a null `MainPlayer` can't spam NREs.
  - **Dead code removed**: the never-called `NoFallingDamageComponent.Disable()` method, and the unused
    `tmpDmg` / `healthController` static fields in `Patches/DestroyBodyPart.cs` and `Features/CODMode.cs`.
- Verified with `dotnet build DadGamerMode.csproj -c Release` (0 warnings, 0 errors) after each batch.

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
