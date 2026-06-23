# DadGamerMode

A quality-of-life / "easy mode" plugin for [SPT (Single Player Tarkov)](https://www.sp-tarkov.com/) that
layers a set of **Call of Duty–style, casual-friendly features** on top of Escape from Tarkov — regenerating
health, godmode, infinite stamina/energy/hydration, instant hideout crafting, faster reloads, and more.

It's built for the "dad gamer": the player who has an hour after the kids are asleep and just wants to drop
into a raid and have fun without the full hardcore grind. Toggle as much or as little as you like from the
in-game **F12 menu** — every feature is off by default.

> **Status: reviving an abandoned mod.** DadGamerMode was originally written by **[dvize](https://github.com/dvize/DadGamerMode)**
> and went stale across several Tarkov updates. This fork brings it back to life and updates it for the
> current SPT release. See [Credits](#credits) and [CHANGELOG.md](CHANGELOG.md).

---

## Compatibility

| | |
|---|---|
| **Plugin version** | `1.9.4` |
| **SPT version** | `4.0.x` (tested against **SPT 4.0.13**) |
| **EFT build** | `0.16.9.0` / **40087** |
| **Loader** | BepInEx 5 |

> ⚠️ The plugin is locked to a specific EFT build (`40087`). On startup it checks your running game build
> against the one it was compiled for; if they don't match it logs an error and shows a warning in the F12
> menu rather than risk crashing or corrupting your profile. If you've updated Tarkov/SPT, grab a matching
> build of the plugin (or rebuild it — see [Building](#building)).

---

## Installation

1. Download the latest release `.zip` (or build it yourself — see below).
2. Extract it into your SPT install folder. The DLL needs to land here:
   ```
   <SPT>\BepInEx\plugins\dvize.DadGamerMode.dll
   ```
3. Launch SPT, start a raid, and press **F12** to open the BepInEx configuration menu.
4. Enable the features you want. Everything is **off by default**.

---

## Features

All settings live in the **F12** menu, grouped into three sections.

### 1. Health
| Setting | What it does |
|---|---|
| **Godmode** | Makes you invincible to everything *except* fall damage. |
| **Keep 1 Health** | Body parts can't be destroyed — they stop at 1 HP instead of blacking out. |
| **Keep 1 Health Selection** | Choose which parts this applies to: `All` or `Head And Thorax` (the lethal ones). |
| **Ignore Headshot Damage** | Headshots deal no damage. |
| **Percentage Headshot Damage** | Apply a custom damage % to the head only (overrides the all-body percentage). |
| **% Damage Received (Only Head)** | Damage threshold for headshots when the above is enabled (0–100%). |
| **% Damage Received (All Body)** | Scale *all* incoming damage by a percentage (e.g. 50% = take half damage). |

### 2. COD Mode
Classic Call of Duty–style **automatic health regeneration**.
| Setting | What it does |
|---|---|
| **CODMode** | Gradually heals all damage over time — including bleeds and fractures. |
| **CODMode Heal Rate** | How fast you heal (0–100). |
| **CODMode Heal Wait** | Seconds you must go without taking damage before regen kicks in (0–600). |
| **CODMode Bleeding Damage** | Keep taking bleed/fracture damage even with COD Mode on (regen still applies). |

### 3. QOL (Quality of Life)
| Setting | What it does |
|---|---|
| **No Falling Damage** | Removes fall damage. |
| **Infinite Stamina** | Stamina never drains. |
| **Infinite Energy** | Energy never drains — no need to eat. |
| **Infinite Hydration** | Hydration never drains — no need to drink. |
| **Instant Production** | Hideout crafts complete instantly. |
| **Instant Construction** | Hideout area upgrades complete instantly. |
| **Enable Reload/Unload Mag Speed** | Master toggle for the two mag-speed sliders below. |
| **Reload Mag Speed** | Magazine reload speed multiplier (smaller = faster). |
| **Unload Mag Speed** | Magazine unload speed multiplier (smaller = faster). |
| **Enemy Damage Multiplier** | Multiply the damage *you* deal to enemies (1–20×, default 1). |
| **Item Total Weight %** | Scale the total weight you carry (0–100%, where 100 = normal). **Set this before entering a raid.** |

---

## Building

The project is an SDK-style C# project targeting **.NET Framework 4.7.2** (the framework Tarkov's Mono runtime
uses).

```bash
dotnet build -c Release
```

or open `DadGamerMode.csproj` in Visual Studio / Rider and build **Release**.

Output:
```
bin\Release\dvize.DadGamerMode.dll   ->   copy into <SPT>\BepInEx\plugins\
```

### References
All assembly references resolve from the local **`references\`** folder — the game's `Assembly-CSharp.dll`,
the `spt-*.dll` client assemblies, `BepInEx.dll`, `0Harmony.dll`, `Comfort*.dll`, `UnityEngine*.dll`, plus
the game's own `mscorlib` / `System` / `System.Core` / `netstandard` BCL. `FrameworkPathOverride` points the
compiler at that folder, so the project builds with no machine-specific paths and **no .NET Framework
targeting pack required**.

> **Note:** `references\` and `SPT\` are **git-ignored** and not shipped in this repo (they contain
> copyrighted game/loader binaries). To build, populate `references\` with the matching DLLs from your own
> SPT install. The `SPT\` folder, if present, is only used as a source for those DLLs and can be deleted to
> reclaim disk space.

### Packaging
`PackageMods.ps1` is a post-build helper that zips the compiled DLL into the correct
`BepInEx\plugins\` folder structure for distribution. The paths inside it are hard-coded to the original
author's dev environment (`F:\SPT-AKI-DEV\...`) — adjust them to your setup before using it.

---

## How it works

DadGamerMode is a [BepInEx](https://docs.bepinex.dev/) plugin that uses [Harmony](https://harmony.pmlpro.com/)
patches (via SPT's reflection helpers) to hook into the game:

- **`Patches/`** — Harmony patches against EFT internals: `ApplyDamage` and `DestroyBodyPart` (health/godmode),
  `OnWeightUpdatedPatch` (weight scaling), `InstantProductionPatch` and `InstantConstructionPatch` (hideout).
- **`Features/`** — per-raid Unity components enabled when a raid starts: `CODMode`, `MaxStamina`, `Energy`,
  `Hydration`, `FallingDamage`, `MagReloadSpeed`.
- **`VersionChecker/`** — validates the running EFT build against the one the plugin was compiled for.

Because EFT ships obfuscated, class/method names change every game update, which is why this kind of mod
needs periodic maintenance. See [SPT4_UPDATE_NOTES.md](SPT4_UPDATE_NOTES.md) for the full name-remapping
table from the most recent update.

---

## Disclaimer

This is a **single-player** mod for SPT. It has nothing to do with — and must never be used on — live
Escape from Tarkov or BSG's official servers. Use at your own risk; it's a cheat/sandbox tool by design.

---

## Credits

- **Author (this fork):** [Balls0fSteel](https://github.com/Balls0fSteel) — revival and SPT 4.0.x update.
- **Original author:** [dvize](https://github.com/dvize/DadGamerMode) — created DadGamerMode.

## License

[MIT](LICENSE.txt).
