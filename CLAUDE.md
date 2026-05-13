# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```powershell
# Build
dotnet build src/BoosterBot.sln

# Run (from repo root)
dotnet run --project src/BoosterBot/BoosterBot.csproj -- --masked --mode conquest --tier pg

# Publish self-contained
dotnet publish src/BoosterBot/BoosterBot.csproj -o ./artifacts -c Release -r win-x64 --self-contained true
```

There are no tests in this project. The CI workflow (`build-booster-bot.yml`) runs on `windows-latest` and produces a self-contained win-x64 artifact.

## Architecture

BoosterBot is a .NET 8 console application that automates Marvel Snap gameplay using computer vision (OpenCV) for state detection and Win32 `SendInput` for mouse/keyboard simulation. It runs in an infinite loop: detect game state, perform the appropriate action, repeat.

### Bot Hierarchy

`IBoosterBot` -> `BaseBot` (shared loop infrastructure, logging, pause/resume) -> concrete bots:
- **ConquestBot** - Conquest mode (lobby selection, multi-round matches, ticket management)
- **LadderBot** - Ranked/Ladder mode
- **EventBot** - Limited-time event modes
- **RepairBot** - Guided UI recalibration for broken detection

### State Machine

`GameState` enum drives all bot logic. Each bot's `Run()` calls `GameUtilities.Determine[Mode]GameState()` which sequentially checks UI elements until one matches, returning the first matching state. The bot then dispatches to the appropriate handler method.

### Image Recognition Pipeline

1. `BotConfig.GetWindowPositions()` - Locates game window via Win32, captures screenshot to `screens/screen.png`
2. `ComponentMappings` - Maps game state checks to screen crop regions (`Rect`) and reference image paths
3. `ImageUtilities.CheckImageAreaSimilarity()` - Crops the region, preprocesses (grayscale -> Gaussian blur -> adaptive threshold via OpenCV), then pixel-compares against the reference image
4. Result: `IdentificationResult` (bool match + debug logs)

Detection confidence threshold defaults to 0.95 (0.9 in downscaled mode). Reference images live in `reference/{lang}/` with the `-preproc` suffix (already preprocessed).

### Input Simulation

- `SystemUtilities` - Win32 `SendInput` wrapper for clicks
- `MouseUtilities.MoveCard()` - Smooth drag-and-drop with random jitter to mimic human behavior
- `HotkeyManager` - Global hotkeys for pause (Ctrl+Alt+P) and quit (Ctrl+Alt+Q)
- All click points include randomized offsets via `BotConfig` properties (e.g., `PlayPoint`, `ResetPoint`)

### Process Masking

On startup (without `--masked`), the app copies itself to a randomly-named `.exe` and relaunches with `--masked`. The actual bot logic only runs in masked mode.

## Adding a New State Check

1. Capture a screenshot of the UI element in the game
2. Preprocess it (grayscale, blur, adaptive threshold) and save to `reference/en/` as `{name}-preproc.png`
3. Add the crop region method to `ComponentMappings.cs` (returns a `Rect` relative to `_config.Center` and `_config.Screencap`)
4. Add a `REF_*` property pointing to the reference image path
5. Add a `CanIdentify*` method in `GameUtilities.cs` using `CheckSimilarity` or `CheckMultipleSimilarities`
6. Wire it into the relevant `Determine*GameState()` method

## Localization

- App strings: `Resources/Strings.resx` (English) and `Strings.zh-CN.resx` (Chinese)
- Reference images are language-specific: `reference/en/` and `reference/zh/`
- `ComponentMappings` resolves the culture from `gameLanguage` in `appsettings.json`

## Configuration

`appsettings.json` is the runtime config file (copied to output). CLI args override appsettings values. Key settings: `scaling`, `downscaledMode`, `eventModeActive`, `defaultRunSettings.*`.

## General Guidelines

### 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them - don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

IMPORTANT: You MUST do research whenever a solution is not immediately clear or you have tried to solve the same problem multiple times without success. Do not assume that all of your training data is still correct and valid. If there is any uncertainty in your response, do research first. Quality is ALWAYS more important than speed.

### 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

### 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it - don't delete it.

When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

The test: Every changed line should trace directly to the user's request.

### 4. Goal-Driven Execution

**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:
- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:
```
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```

Strong success criteria let you loop independently. Weak criteria ("make it work") require constant clarification.

### 5. Memory

IMPORTANT: You MUST update the repo-specific memory files *as you are working*. Do not wait to update memory until the end of a session. When you learn something new, update the relevant files immediately. If the specified file does not exist, you should create it.

| Trigger | Action |
|---------|--------|
| User shares a fact about themselves | → Update `memory-profile.md` |
| User states a preference | → Update `memory-preferences.md` |
| A decision is made | → Update `memory-decisions.md` with date |
| Completing substantive work | → Add to `memory-sessions.md` |

**What to skip:** Quick factual questions, trivial tasks with no new info.

**DO NOT ASK. Just update the files when you learn something.**

When compacting, always preserve: the full list of modified files, any failing test names, the current feature branch name, and outstanding TODOs.

### 6. Code Comments

You should use comments intelligently throughout your code to help explain what the code does and why. For standard and/or boilerplate code, comments should be kept concise. For unique or novel solutions, more detailed comments should be used. 

When writing comments, assume that your target audience is an experienced developer looking at the code for the first time. Such a developer should be able to use comments to understand the code even if they are not deeply familiar with the framework.