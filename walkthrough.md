# REviewer v1.1.2 Release

**Release Date:** January 31, 2026

---

## 🚀 Major Architectural Refactor
v1.1.0 marks a significant transition in the project's architecture, moving towards a more maintainable and scalable structure.

### 🏛️ MVVM Implementation
- **Decoupled Logic:** Transitioned `MainWindow` to use the **MVVM (Model-View-ViewModel)** pattern.
- **ViewModels:** Added `MainWindowViewModel`, `SRTViewModel`, `TrackerViewModel`, and `OverlayViewModel`.
- **Commanding:** Implemented `RelayCommand` for clean UI interaction.

### 🔌 New Modules & Services
- **REviewer-server:** A new Python-based backend server for advanced game data tracking and remote viewing.
- **REviewer.Tests:** Introduced a unit testing project focusing on core services (`GameStateService`, `TimerService`, `InventoryService`).
- **Core Namespace:** Moved configuration and foundational memory logic to a dedicated `Core` namespace.

---

## 🎮 Features & Improvements

### 🎨 Visuals & UI
- **Realistic ECG:** Improved health monitor animations for a more authentic Resident Evil feel.
- **Dynamic Timer Colors:** Timer now changes color based on speedrun pace/thresholds.
- **Theme Consistency:** Further refined the VSCode-inspired dark theme across all windows.

### 🏗️ Build Automation
- **`build_release.ps1`:** New automated PowerShell script that:
    - Automatically detects version from `app.config`.
    - Compiles x64 and x86 Single-File binaries.
    - Packages `data`, `saves`, and configs into a clean zip archive.
    - Excludes debug junk and logs.

---

## 🐛 Bug Fixes
- **Version Parsing:** Fixed a critical startup crash caused by parsing non-numeric version strings (e.g., `dev-1.1.0-alpha`).
- **Save/Load State:** Fixed a major logic bug where SRT stats (Deaths, Resets, Segments) were not being persisted or restored correctly. Counters now revert properly when reloading older saves.
- **Directory Crash:** Fixed `DirectoryNotFoundException` on startup by ensuring the `saves/` folder is automatically created if missing.
- **Service Decoupling:** Fixed circular dependencies that caused intermittent initialization failures.
- **Window Cleanup:** Improved the reliability of window closing when restarting the game tracking.

---

## 🚀 Getting Started

1. Download the latest release (`x64` or `x86`).
2. Run `REviewer.exe`.
3. The application will auto-generate required folders (`data`, `saves`).
4. (Optional) Explore the `REviewer-server` for remote tracking capabilities.

---

**Full Changelog:** [Compare with previous version](https://github.com/Namsku/REviewer/compare/v1.0.0...v1.1.0)

