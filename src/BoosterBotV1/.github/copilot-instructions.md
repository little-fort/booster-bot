# BoosterBot AI Instructions

## Project Context
BoosterBot is a C# .NET 8.0 console application designed for game automation (Marvel Snap). It uses computer vision (OpenCV) for state detection and low-level Win32 API calls for input simulation.

## Architecture
- **Entry Point**: `Program.cs` handles CLI arguments, configuration loading (`appsettings.json`), localization, and bot initialization.
- **Bot Structure**:
  - `IBoosterBot`: Common interface for all bot types.
  - `BaseBot`: Abstract base class with shared logic (logging, pausing).
  - `ConquestBot`, `LadderBot`, `EventBot`, `RepairBot`: Specific implementations for different game modes.
- **Core Helpers**:
  - `GameUtilities.cs`: High-level game state detection and logic (e.g., `DetermineConquestGameState`).
  - `ImageUtilities.cs`: Image processing using `OpenCvSharp` (preprocessing) and `System.Drawing` (pixel comparison).
  - `MouseUtilities.cs`: Input simulation using `user32.dll` `SendInput`.
  - `ComponentMappings.cs`: Maps UI elements to screen coordinates and reference images.

## Key Patterns & Conventions

### State Detection
- Game state is determined by checking for the presence of specific UI elements (buttons, labels).
- Use `GameUtilities.CheckSimilarity` or `CheckMultipleSimilarities` to compare screen crops against reference images.
- **Pattern**: `CanIdentify[StateName]` methods in `GameUtilities` return an `IdentificationResult`.

### Image Recognition
- **Reference Images**: Stored in `reference/{lang}/`.
- **Preprocessing**: Images are often preprocessed (grayscale -> blur -> adaptive threshold) before comparison to handle noise.
- **Comparison**: `ImageUtilities.CalculateImageSimilarity` performs pixel-by-pixel comparison.
- **OCR**: Limited usage; primarily relies on image matching.

### Input Simulation
- **Mouse**: `MouseUtilities.MoveCard` simulates drag-and-drop with smooth movement and randomization to mimic human behavior.
- **Keyboard**: `HotkeyManager` handles global hotkeys (e.g., pause/resume).

### Configuration & Localization
- **Config**: `BotConfig` wraps `IConfiguration`. Settings are in `appsettings.json`.
- **Localization**: `LocalizationManager` loads strings from `Resources/Strings.{lang}.resx`. Always use localized strings for user-facing output.

### Logging & Debugging
- **Logging**: Use `Logger.Log` for all output. Logs are saved to `logs/`.
- **Screenshots**: `ImageUtilities.SaveImage` saves crops to `screens/` for debugging failed checks.
- **Process Masking**: The application can rename its executable at runtime (`MaskProcess`) to evade detection.

## Developer Workflows
- **Build**: `dotnet build`
- **Run**: `dotnet run [args]` (e.g., `dotnet run --mode ranked --turns 3`)
- **Debug**: Use VS Code launch configurations.
- **Adding New Checks**:
  1. Capture a reference image of the UI element.
  2. Save it to `reference/en/` (and other languages if text-based).
  3. Add the mapping to `ComponentMappings.cs`.
  4. Create a `CanIdentify...` method in `GameUtilities.cs`.

## External Dependencies
- **OpenCvSharp4**: For image processing.
- **System.Drawing.Common**: For bitmap manipulation.
- **Newtonsoft.Json**: For JSON handling.
