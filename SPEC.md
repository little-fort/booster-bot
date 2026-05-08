# BoosterBot V2 - Project Specification

## 1. Overview

BoosterBot V2 is a ground-up redesign of the BoosterBot Marvel Snap automation tool. The existing V1 codebase (a .NET 8 console application) will be preserved in the repository for archival purposes. V2 introduces a modern GUI, cross-platform support, and a significantly optimized core engine.

### 1.1 Core Objectives

1. **Modern GUI** - Replace the console-only interface with a polished Avalonia desktop application
2. **Optimized Engine** - In-memory image processing, template matching, and focus-aware input simulation
3. **Dual-Mode Operation** - Full GUI experience and headless CLI mode for scripting/scheduling
4. **Cross-Platform Foundation** - Windows-first with macOS support as a secondary target
5. **Compact Overlay** - Lightweight on-screen overlay for single-monitor setups

### 1.2 Non-Goals

- Mobile support
- Web-based UI or remote control
- OCR-based text recognition (V2 remains image-template-based)
- Adding new game modes beyond what V1 supports (Conquest, Ladder, Event)

---

## 2. Architecture

### 2.1 Solution Structure

```
src/
├── BoosterBot.Core/           # Platform-agnostic bot engine
│   ├── Bots/                  # Bot implementations (Conquest, Ladder, Event)
│   ├── Pipeline/              # Screenshot capture, image processing, state detection
│   ├── Input/                 # Input simulation abstractions
│   ├── Configuration/         # Settings management
│   ├── Models/                # Shared data models, enums
│   └── Localization/          # String resources
│
├── BoosterBot.Desktop/        # Avalonia GUI application
│   ├── Views/                 # AXAML views
│   ├── ViewModels/            # MVVM ViewModels
│   ├── Controls/              # Custom controls (overlay, status indicators)
│   ├── Services/              # UI-specific services (navigation, theming)
│   └── Assets/                # Icons, images, styles
│
└── BoosterBot.Platform/       # Platform-specific implementations
    ├── Windows/               # Win32 capture, SendInput, hotkeys
    └── macOS/                 # macOS capture, input simulation (future)
```

### 2.2 Project Dependencies

```
BoosterBot.Desktop ──> BoosterBot.Core ──> BoosterBot.Platform
                                      └──> OpenCvSharp4
```

- **BoosterBot.Core** contains all bot logic, state machines, and image processing. It is platform-agnostic in its public API but delegates platform-specific operations (screen capture, input, window management) to `BoosterBot.Platform` via interfaces.
- **BoosterBot.Desktop** is the Avalonia application. It embeds Core and can run in either GUI mode or headless CLI mode.
- **BoosterBot.Platform** provides concrete implementations of platform abstractions (Win32 APIs, macOS APIs).

### 2.3 Key Architectural Principles

1. **In-memory pipeline** - No intermediate files. Screenshots are captured to `Bitmap`/`Mat` objects, processed in memory, and disposed after use. (Mirrors the TetraTalk V2 approach.)
2. **Focus-aware input** - Screen capture does NOT require game window focus. Focus is only acquired when sending mouse/keyboard input, and released immediately after. This allows users to multitask while the bot runs.
3. **Platform abstraction** - All OS-specific code lives behind interfaces in `BoosterBot.Platform`. Core logic never calls Win32 directly.
4. **Event-driven bot lifecycle** - Bots emit events (state changes, actions taken, errors) that the GUI subscribes to. The bot engine does not reference or depend on the UI.

### 2.4 Framework & Dependencies

| Dependency | Version | Purpose |
|---|---|---|
| .NET 10 | 10.0 | Target framework (current LTS at time of development) |
| Avalonia | 12.x | Cross-platform GUI framework |
| Avalonia.Themes.Fluent | 12.x | Fluent design theme |
| OpenCvSharp4 | 4.10+ | Image processing and template matching |
| CommunityToolkit.Mvvm | 8.x | Source-generated MVVM (INotifyPropertyChanged, RelayCommand) |
| Microsoft.Extensions.DependencyInjection | 10.x | DI container |
| Microsoft.Extensions.Configuration | 10.x | Settings management |
| Microsoft.Extensions.Hosting | 10.x | Host lifecycle, background services |

> **Note on MVVM framework**: The Conduit project uses manual MVVM (`INotifyPropertyChanged` + custom `RelayCommand`). For V2, we'll use `CommunityToolkit.Mvvm` instead - it generates the same boilerplate via source generators with zero runtime overhead, reducing ViewModel code by ~40% while maintaining the same simple patterns. This is a lightweight addition, not a heavy framework like ReactiveUI.

---

## 3. Core Engine (`BoosterBot.Core`)

### 3.1 Image Processing Pipeline

The V1 pipeline writes screenshots to disk, crops to files, and compares pixel-by-pixel. V2 replaces this with a fully in-memory pipeline using OpenCV template matching.

#### 3.1.1 Screen Capture

```
IScreenCapture.Capture() → ScreenCaptureContext (IDisposable)
    └── Contains: Bitmap, WindowRect, Dimensions
```

- **Windows**: Uses `PrintWindow` with `PW_RENDERFULLCONTENT` to capture DirectX content without requiring focus (proven in TetraTalk V2)
- **macOS** (future): `CGWindowListCreateImage` with `kCGWindowListOptionIncludingWindow`
- The captured bitmap is wrapped in `ScreenCaptureContext`, a disposable container that ensures cleanup

#### 3.1.2 Image Processing

```
ImageProcessor (static utility class)
    ├── CropRegion(Bitmap source, Rect region) → Mat
    ├── Preprocess(Mat source) → Mat  [grayscale → Gaussian blur → adaptive threshold]
    ├── IsReferencePresent(Mat scene, Mat template, double threshold) → bool
    └── GetMatchConfidence(Mat scene, Mat template) → double
```

- **Template matching** via `Cv2.MatchTemplate(scene, template, result, TemplateMatchModes.CCoeffNormed)` replaces V1's pixel-by-pixel comparison
- Reference images loaded once at startup into a static `Dictionary<string, Mat>` cache
- All intermediate `Mat` objects are properly disposed via `using` blocks

#### 3.1.3 Detection Thresholds

Thresholds are defined per-component rather than scattered as magic numbers:

```csharp
public static class DetectionThresholds
{
    public const double Default = 0.90;
    public const double ButtonHighConfidence = 0.95;
    public const double ButtonLowConfidence = 0.80;
    public const double LobbyIndicator = 0.85;
}
```

> **Note**: Template matching (CCoeffNormed) produces different correlation scores than V1's pixel-ratio metric. All thresholds will need recalibration during development. The values above are starting points.

### 3.2 State Machine

V2 retains the `GameState` enum-driven state machine but refactors it for clarity and extensibility.

#### 3.2.1 GameState Enum

The V1 enum has 21 states with some redundancy. V2 consolidates where possible:

```csharp
public enum GameState
{
    // Shared states
    Unknown,
    MainMenu,
    EventMenu,
    Reconnecting,

    // Active match states (all modes)
    Matchmaking,
    ActiveMatch,
    MidTurn,
    MatchEnd,
    MatchEndRewards,

    // Conquest-specific
    ConquestLobby,         // Tier identified separately via ConquestTier enum
    ConquestNoTickets,
    ConquestPrematch,
    ConquestRoundEnd,
    ConquestPostMatchLoss,
    ConquestPostMatchWin,
    ConquestTicketClaim,
}

public enum ConquestTier
{
    ProvingGrounds,
    Silver,
    Gold,
    Infinite
}
```

This reduces 21 states to 16 by:
- Merging `LADDER_MATCH` / `CONQUEST_MATCH` into `ActiveMatch` (bots differentiate via their own context)
- Merging `LADDER_MATCHMAKING` / `CONQUEST_MATCHMAKING` into `Matchmaking`
- Merging `LADDER_MATCH_END` / `CONQUEST_MATCH_END` into `MatchEnd`
- Extracting tier as a separate enum from the state

#### 3.2.2 State Detection

```csharp
public interface IGameStateDetector
{
    GameStateResult DetectState(ScreenCaptureContext capture);
}

public record GameStateResult(
    GameState State,
    ConquestTier? Tier = null,
    double Confidence = 0.0
);
```

State detection is a pure function: capture in, result out. No side effects, no file I/O. The detector checks regions sequentially in priority order and short-circuits on the first match above threshold.

### 3.3 Bot Architecture

#### 3.3.1 Interface & Base Class

```csharp
public interface IBot
{
    BotMode Mode { get; }
    BotStatus Status { get; }
    BotStatistics Statistics { get; }

    Task StartAsync(CancellationToken ct);
    void Pause();
    void Resume();

    event EventHandler<BotStateChangedEventArgs> StateChanged;
    event EventHandler<BotActionEventArgs> ActionPerformed;
    event EventHandler<BotLogEventArgs> LogEmitted;
}
```

Key changes from V1:
- **Async-first**: `StartAsync` with `CancellationToken` replaces blocking `Run()` loop
- **Event-driven**: UI subscribes to events rather than polling
- **Observable status**: `BotStatus` (Running, Paused, Stopped, Error) exposed as a property
- **Statistics tracking**: Match count, win/loss, uptime, actions performed

#### 3.3.2 Concrete Bots

| Bot | Mode | V2 Changes |
|---|---|---|
| `ConquestBot` | Conquest | Configurable tier selection, ticket management, snap probability |
| `LadderBot` | Ladder | Configurable snap probability |
| `EventBot` | Event | Auto-detect event type from menu state |

`RepairBot` is removed as a bot type. Its reference image capture functionality becomes a standalone calibration tool in the GUI (see Section 4.4).

### 3.4 Input Simulation

```csharp
public interface IInputSimulator
{
    void Click(Point position);
    void DragAndDrop(Point from, Point to, DragOptions? options = null);
    void SendKey(VirtualKey key);
}

public record DragOptions(
    int Steps = 50,
    TimeSpan? StepDelay = null,
    bool AddJitter = true
);
```

- **Windows**: `SendInput` for both mouse and keyboard (replaces V1's mixed `mouse_event`/`SendInput` approach)
- All input methods acquire focus via `SetForegroundWindow` before sending, then release
- Randomized timing and jitter are configurable via `DragOptions`
- Click point randomization (V1's `±20px` offsets) is handled at the bot level, not in the input simulator

### 3.5 Window Management

```csharp
public interface IWindowManager
{
    WindowInfo? FindGameWindow();
    ScreenCaptureContext CaptureWindow(WindowInfo window);
    void FocusWindow(WindowInfo window);
    bool IsWindowVisible(WindowInfo window);
}

public record WindowInfo(
    IntPtr Handle,
    string ProcessName,
    Rect Bounds,
    double ScalingFactor
);
```

- Window handle is cached and refreshed on a configurable interval (e.g., 10 seconds) or when capture fails
- `ScalingFactor` is detected from the display rather than requiring manual configuration
- Process name matching supports configurable names (for the game's various executable names across regions)

### 3.6 Configuration

V2 replaces the flat `appsettings.json` with a structured settings model:

```csharp
public class BotSettings
{
    public BotMode DefaultMode { get; set; } = BotMode.Conquest;
    public ConquestTier MaxConquestTier { get; set; } = ConquestTier.ProvingGrounds;
    public int RetreatAfterTurn { get; set; } = 0;  // 0 = disabled
    public double SnapProbability { get; set; } = 0.465;
    public string GameLanguage { get; set; } = "en";
    public string AppLanguage { get; set; } = "en-US";
    public bool SaveDebugScreenshots { get; set; } = false;
    public bool VerboseLogging { get; set; } = false;
    public ProcessMaskingOptions ProcessMasking { get; set; } = new();
    public OverlayOptions Overlay { get; set; } = new();
}
```

Settings are loaded from `appsettings.json` and can be overridden by CLI arguments (when running headless) or the GUI settings panel.

---

## 4. Desktop Application (`BoosterBot.Desktop`)

### 4.1 Technology Choices

- **Avalonia 12.x** with Fluent theme (dark mode default, matching the Conduit project's aesthetic)
- **CommunityToolkit.Mvvm** for source-generated ViewModels
- **DI via Microsoft.Extensions.DependencyInjection** for service registration
- Cross-platform desktop target: Windows first, macOS second

### 4.2 Application Layout

The main window provides a single-screen dashboard for bot control and monitoring:

```
┌──────────────────────────────────────────────────────┐
│  BoosterBot V2                            [_][□][X]  │
├──────────────────────────────────────────────────────┤
│                                                      │
│  ┌─── Bot Control ────────────────────────────────┐  │
│  │                                                │  │
│  │  Mode: [Conquest ▼]    Tier: [PG ▼]            │  │
│  │  Retreat After Turn: [0 ▼]                     │  │
│  │                                                │  │
│  │  [ ▶ Start ]  [ ⏸ Pause ]  [ ■ Stop ]         │  │
│  │                                                │  │
│  └────────────────────────────────────────────────┘  │
│                                                      │
│  ┌─── Status ─────────────────────────────────────┐  │
│  │  State: Conquest Lobby (PG)                    │  │
│  │  Status: Running                               │  │
│  │  Uptime: 01:23:45                              │  │
│  │  Matches: 12  |  Wins: 8  |  Losses: 4        │  │
│  └────────────────────────────────────────────────┘  │
│                                                      │
│  ┌─── Activity Log ───────────────────────────────┐  │
│  │  [14:23:01] Match started                      │  │
│  │  [14:23:15] Playing cards: Turn 1              │  │
│  │  [14:24:02] Retreating (turn 4)                │  │
│  │  [14:24:10] Match ended - Loss                 │  │
│  │  [14:24:12] Returning to lobby                 │  │
│  │                                          [Clear]  │
│  └────────────────────────────────────────────────┘  │
│                                                      │
│  ┌─── Quick Settings ─────────────────────────────┐  │
│  │  Snap Probability: [====●=====] 46.5%          │  │
│  │  [ ] Save debug screenshots                    │  │
│  │  [ ] Verbose logging                           │  │
│  │  [Calibration Tool]     [Settings]             │  │
│  └────────────────────────────────────────────────┘  │
│                                                      │
└──────────────────────────────────────────────────────┘
```

### 4.3 Compact Overlay

For single-monitor setups, a compact always-on-top overlay replaces the main window:

```
┌─────────────────────────────────┐
│  BoosterBot  ●Running  ⏸  ■  ↗ │
│  CQ/PG  M:12  W:8  L:4  1:23  │
└─────────────────────────────────┘
```

- Always-on-top, semi-transparent, draggable
- Positioned at a screen edge by default (user-repositionable)
- Minimal footprint: ~300x60px
- Shows: mode, status indicator, pause/stop buttons, expand button
- Second row: abbreviated stats (mode/tier, matches, wins, losses, uptime)
- Click the expand button (↗) to restore the main window
- Overlay visibility toggled from main window or via global hotkey

### 4.4 Calibration Tool

Replaces V1's `RepairBot`. Provides a guided wizard for capturing and validating reference images:

1. **Select Component** - Choose which UI element to calibrate (play button, retreat button, etc.)
2. **Capture** - Bot captures the current screen and highlights the expected region
3. **Preview** - Shows the captured region alongside the preprocessed version and current reference
4. **Test** - Runs detection against the live game to validate the new reference image
5. **Save** - Overwrites the reference image

This is a modal dialog/panel within the main window rather than a separate bot mode.

### 4.5 Settings Panel

Full settings accessible via a dedicated panel or dialog:

- **Bot Settings**: Default mode, tier, retreat turn, snap probability
- **Display**: Theme (dark/light), overlay options, window position memory
- **Game**: Game language, process name(s) to detect
- **Advanced**: Detection thresholds, timing parameters, debug options
- **About**: Version info, links

### 4.6 System Tray

- Minimize to system tray (tray icon with context menu)
- Context menu: Show/Hide, Pause/Resume, Stop, Quit
- Tray icon reflects bot status (color or icon change)

### 4.7 MVVM Structure

```
ViewModels/
├── MainWindowViewModel.cs        # Top-level, orchestrates sub-ViewModels
├── BotControlViewModel.cs        # Mode selection, start/stop/pause
├── BotStatusViewModel.cs         # Live state, statistics display
├── ActivityLogViewModel.cs       # Scrolling log entries
├── SettingsViewModel.cs          # Settings panel
├── CalibrationViewModel.cs       # Calibration wizard
└── OverlayViewModel.cs           # Compact overlay state
```

### 4.8 Responsive Layout

Following the Conduit pattern, the main window adapts its layout based on window size:
- **Normal** (default): Vertical stack as shown in 4.2
- **Wide** (>900px width): Side-by-side layout with controls on the left, log on the right

---

## 5. Headless / CLI Mode

The Desktop application supports a headless mode for script and scheduler invocation:

```powershell
# Launch GUI (default)
BoosterBot.exe

# Launch headless (no window)
BoosterBot.exe --headless --mode conquest --tier pg --retreat 4

# Launch headless with quiet output
BoosterBot.exe --headless --mode ladder --quiet
```

### 5.1 CLI Arguments

| Argument | Short | Description | Default |
|---|---|---|---|
| `--headless` | `-H` | Run without GUI | false |
| `--mode` | `-m` | Bot mode: conquest, ladder, event | conquest |
| `--tier` | `-t` | Max conquest tier: pg, silver, gold, infinite | pg |
| `--retreat` | `-r` | Retreat after turn N (0 = disabled) | 0 |
| `--snap` | `-s` | Snap probability (0.0 - 1.0) | 0.465 |
| `--verbose` | `-v` | Verbose logging | false |
| `--quiet` | `-q` | Suppress console output | false |
| `--save-screens` | | Save debug screenshots | false |
| `--config` | `-c` | Path to config file | appsettings.json |

### 5.2 Headless Behavior

- No Avalonia UI initialized (skip `AppBuilder`)
- Console output for logging (respects `--quiet` and `--verbose`)
- Responds to `Ctrl+C` for graceful shutdown
- Exit codes: 0 = normal exit, 1 = error, 2 = game window not found
- Process masking still applies unless `--no-mask` is specified

---

## 6. Process Masking

V2 retains V1's process masking strategy with minor improvements:

- Self-copy to randomly-named executable on launch
- Purge stale copies from previous runs
- `--masked` flag to indicate running as masked process
- New: `--no-mask` flag to disable masking (useful for debugging)
- New: Configurable via settings (enable/disable, custom name prefix)

---

## 7. Localization

### 7.1 Application Strings

- Continue using .NET `ResourceManager` with `.resx` files
- Supported cultures: `en-US` (English), `zh-CN` (Simplified Chinese)
- Log files always use English (neutral culture) regardless of app language

### 7.2 Reference Images

- Organized by language: `Assets/Reference/{culture}/`
- Naming convention: `{component}-{variant}-preproc.png`
- Loaded into memory cache at startup, keyed by component name
- Calibration tool supports generating reference images for any language

---

## 8. Build & CI/CD

### 8.1 Build Commands

```powershell
# Build entire solution
dotnet build src/BoosterBot.sln

# Run GUI (development)
dotnet run --project src/BoosterBot.Desktop

# Run headless (development)
dotnet run --project src/BoosterBot.Desktop -- --headless --mode conquest

# Publish self-contained
dotnet publish src/BoosterBot.Desktop -o ./artifacts -c Release -r win-x64 --self-contained true

# Publish for macOS (future)
dotnet publish src/BoosterBot.Desktop -o ./artifacts -c Release -r osx-arm64 --self-contained true
```

### 8.2 CI/CD Pipeline

Update GitHub Actions workflow to:
- Build on `windows-latest` (primary) and `macos-latest` (secondary, when macOS support is added)
- Produce self-contained artifacts per platform
- Retain manual dispatch with version/label inputs

---

## 9. Development Roadmap

### Phase 1: Foundation
- [ ] Create solution structure (`Core`, `Desktop`, `Platform` projects)
- [ ] Set up Avalonia Desktop project with Fluent theme
- [ ] Implement platform abstractions (`IScreenCapture`, `IInputSimulator`, `IWindowManager`)
- [ ] Implement Windows platform layer (PrintWindow capture, SendInput)
- [ ] Port `ImageProcessor` with in-memory pipeline and template matching
- [ ] Port reference image loading and caching

### Phase 2: Bot Engine
- [ ] Port `GameState` enum and state detection logic
- [ ] Implement `IGameStateDetector` with template matching
- [ ] Port `IBot` interface and `BaseBot` with async lifecycle
- [ ] Port `ConquestBot` with event-driven architecture
- [ ] Port `LadderBot`
- [ ] Port `EventBot`
- [ ] Recalibrate detection thresholds for template matching

### Phase 3: GUI
- [ ] Build main window layout (bot control, status, activity log)
- [ ] Implement `BotControlViewModel` (mode selection, start/stop/pause)
- [ ] Implement `BotStatusViewModel` (live state updates, statistics)
- [ ] Implement `ActivityLogViewModel` (scrolling log display)
- [ ] Wire up bot events to UI updates
- [ ] Implement settings panel

### Phase 4: Overlay & Polish
- [ ] Build compact overlay window
- [ ] Implement system tray integration
- [ ] Build calibration tool (replacing RepairBot)
- [ ] Add global hotkey support
- [ ] Responsive layout for different window sizes
- [ ] Process masking

### Phase 5: CLI & Distribution
- [ ] Implement headless mode (skip Avalonia init, console-only output)
- [ ] CLI argument parsing
- [ ] Update CI/CD pipeline for V2
- [ ] Localization (port existing English + Chinese resources)

### Phase 6: Cross-Platform (Future)
- [ ] Implement macOS platform layer (`BoosterBot.Platform/macOS/`)
- [ ] macOS screen capture via CoreGraphics
- [ ] macOS input simulation
- [ ] Test and validate on macOS
- [ ] Add macOS CI/CD pipeline

---

## 10. Migration Notes

### 10.1 V1 Preservation

The existing V1 source code is moved to the `src/BoosterBotV1/`. V2 projects are to be created in the `src/BoosterBotV2/` directory. The final output of the V2 executable should still be `BoosterBot.exe` to maintain consistency for users, but the internal structure and namespaces will reflect the new architecture.

### 10.2 Reference Image Compatibility

V1 reference images (already preprocessed) can be reused in V2. The preprocessing pipeline (grayscale -> Gaussian blur -> adaptive threshold) is identical. However, since V2 uses template matching instead of pixel comparison, threshold values will need recalibration.

### 10.3 Breaking Changes from V1

- Configuration format changes (structured settings vs. flat JSON)
- CLI argument names updated for consistency
- `RepairBot` replaced by GUI calibration tool
- Hotkey bindings may change (configurable in V2)
- Process masking behavior configurable (was always-on in V1)
