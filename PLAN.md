# DadGamerMode — Plan / Backlog

Follow-ups from the 2026-06-23 source bug audit. Items here were deliberately **not** changed during the fix
passes (see `CHANGELOG.md`) and are parked so we don't re-investigate them from scratch each time.

## Deferred

### #10 — Item-weight patch scales every caller of the static weight helper
- **Where:** `Patches/OnWeightUpdatedPatch.cs`
- **What:** The postfix patches `EFT.InventoryLogic.InventoryEquipment.smethod_1(IEnumerable<Slot>)` — a *static*
  total-weight helper — and multiplies `__result` by the weight-reduction factor unconditionally. Because the
  method is static and takes an arbitrary slot enumeration, the reduction applies to **any** caller, not just the
  local player's carried weight.
- **Confirmed reach (via Assembly-CSharp decompile during the audit):** `smethod_1` is called by
  `InventoryEquipment.GetEquipmentWeight()` / `GetEquipmentWeightEliteSkill()`, which are reached from
  `Inventory.method_0` / `method_1` (per-inventory weight) and `HideoutMannequinEquipmentScreen.method_11`
  (a hideout UI weight readout). So the reduction also affects the hideout mannequin screen and, in principle,
  bot / other inventories' weight calcs.
- **Why deferred:** Impact is benign in single-player SPT — the mannequin screen is the player's own profile
  (scaling it is arguably consistent), there is no server-authoritative weight to desync, and no crash. A proper
  fix needs to confirm the slots belong to the local player before scaling, and there is no clean, verified
  owner / `PlayerProfile` signal available from inside this static postfix.
- **If we pick this up:** either (a) patch a higher-level, player-specific weight getter instead of the shared
  static helper, or (b) add a prefix that captures the `IEnumerable<Slot>` argument and verifies ownership (the
  slots' container traces to the local `Player`) before the postfix scales. Verify on a real build that bot
  encumbrance / the hideout mannequin weight is unaffected while the player's carried weight still drops.

## Intentionally left as-is (not bugs)
- **Unconditional patch enable** — all ModulePatches are enabled in `Plugin.cs` regardless of toggles; each patch
  re-reads its config value live and no-ops when off. This is the idiomatic SPT pattern, not a defect.
- **`dvize.GodModeTest` namespace** — mismatches the `dvize.DadGamerMode` assembly/branding but is purely
  cosmetic (BepInEx loads by the `[BepInPlugin]` GUID). Left alone to avoid a churny 12-file rename.
- **Duplicate `ConfigurationManagerAttributes`** — a global copy plus a nested copy in `VersionChecker`. No runtime
  effect (C# binds each usage to its nearest scope). Optional future single-source-of-truth refactor.
- **Energy/Hydration max-out event handlers re-enter** `ChangeEnergy`/`ChangeHydration` — verified harmless: EFT
  suppresses the change event on a zero delta, so the recursion terminates after one bounce. No fix needed.

## Notes
- Verify any change with `dotnet build DadGamerMode.csproj -c Release` (net472; refs resolved from `references/`).
- This fork targets **SPT 4.0.x / EFT build 40087**. Obfuscated `GClass*` / `Class*` names are build-specific and
  get renamed each EFT update — the per-patch comments record the current mappings.
