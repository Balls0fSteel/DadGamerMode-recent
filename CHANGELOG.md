# Changelog

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
