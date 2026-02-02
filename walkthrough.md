# REviewer v1.1.3-alpha

## Major Changes

- **Memory Address Overflow Fix**: Refactored all memory address-related fields from `int` to `nint` (IntPtr). This prevents a silent `OverflowException` (and subsequent initialization failure) when game processes are loaded at memory addresses above 2GB (0x7FFFFFFF), common on 64-bit systems.
- **Improved Initialization Diagnostics**: Added try-catch blocks and logging to `MappingGameVariables` to provide better visibility into why initialization might fail in the future.

## Previous Versions

### REviewer v1.1.2-alpha

- **Save/Load State Fix**: Fixed a critical bug where SRT stats (Deaths, Resets, Saves, etc.) were not being correctly restored from older saves due to `Math.Max` logic and missing field mappings.
- **Directory Crash Fix**: Added checks to ensure the `saves/` directory exists before file operations, preventing `DirectoryNotFoundException`.

### REviewer v1.1.1-alpha
- Initial release of the new architecture.

## 🐛 Bug Fixes
- **Version Parsing:** Fixed a critical startup crash caused by parsing non-numeric version strings (e.g., `dev-1.1.0-alpha`).
- **Save/Load State:** Fixed a major logic bug where SRT stats (Deaths, Resets, Segments) were not being persisted or restored correctly. Counters now revert properly when reloading older saves.
- **Directory Crash:** Fixed `System.IO.DirectoryNotFoundException` when the `saves/` folder was missing.

## 📺 Demo
![SRT Preview](file:///C:/Users/namsku/.gemini/antigravity/brain/8c29f391-7abb-422d-8ba0-7b01a5a8be14/uploaded_media_1769871010442.png)
