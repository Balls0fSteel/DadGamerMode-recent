# DadGamerMode - SPT 4.0.x update notes

Updated from SPT 3.9 (EFT build 30626) to **SPT 4.0.13 (EFT build 0.16.9.0 / 40087)**.
Plugin version bumped 1.9.3 -> 1.9.4.

## Obfuscated-name / API remap (30626 -> 40087)

| Old (3.9) | New (4.0.13) | Where |
|-----------|--------------|-------|
| `DamageInfo` | `DamageInfoStruct` | ApplyDamage, DestroyBodyPart, CODMode |
| `ActiveHealthController.ApplyDamage(ref float, EBodyPart, DamageInfo)` | `ApplyDamage(EBodyPart, float, DamageInfoStruct)` | ApplyDamage |
| `healthController.Dictionary_0` | `Dictionary_0_1` (on base `GClass3009`1`) | CODMode |
| `ActiveHealthController.GClass2429` (abstract effect) | `ActiveHealthController.GClass3008` | CODMode |
| `GInterface252 / GInterface253` effect markers | replaced with type checks: `is ActiveHealthController.Bleeding` + type-name `Fracture`/`Pain`/`Tremor` | CODMode |
| `Player.BeingHitAction(DamageInfo,...)` | `(DamageInfoStruct,...)` | CODMode |
| `GClass1931` (production controller) | `GClass2431` | InstantProduction |
| `GClass1937` (producing item) | `GClass2438` | InstantProduction |
| `GClass1937.Class1666` / field `class1666_0` / field `double_1` | `GClass2438.Class1951` / `Class1951_0` / `Double_1` (a.k.a. property `Progress`) | InstantProduction |
| `EquipmentClass.method_10(IEnumerable<Slot>)` | `InventoryEquipment.smethod_1(IEnumerable<Slot>)` | OnWeightUpdated |
| `AreaData.method_0(int)` | `AreaData.method_0(int, bool)` | InstantConstruction |
| `[assembly: TarkovVersion(30626)]` | `40087` | Properties/AssemblyInfo.cs |

## Behavioural improvements (more resilient to future EFT patches)
- **Weight reduction** (OnWeightUpdatedPatch) now uses a Harmony **postfix** that scales the
  computed weight, instead of reimplementing the calculation with obfuscated helper delegates.
- **COD Mode** effect removal now matches effects by their stable named types
  (`Bleeding` base + `Fracture`/`Pain`/`Tremor`) instead of `GInterfaceNNN` markers.

## Building
- Open `DadGamerMode.csproj` (SDK-style, targets `net472`) and build **Release**, or run
  `dotnet build -c Release`.
- All references resolve from the local `references\` folder (game `Assembly-CSharp.dll`,
  `spt-*.dll`, `BepInEx.dll`, `0Harmony.dll`, `Comfort*.dll`, `UnityEngine*.dll`, plus the
  game's `mscorlib/System/System.Core/netstandard` BCL). No targeting pack required.
- Output: `bin\Release\dvize.DadGamerMode.dll` -> copy into `<SPT>\BepInEx\plugins\`.

## Housekeeping
- The `SPT\` folder (your copied game install) is **only** used as the source of the reference
  DLLs, which are now in `references\`. You can delete `SPT\` to reclaim disk space.
