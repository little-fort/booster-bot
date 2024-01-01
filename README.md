<p align="center">
 <img src="https://github.com/little-fort/booster-bot/assets/39720285/3fd898fc-9f52-4e6c-8045-ffd6ee7ae456" />
</p>

# BoosterBot for Marvel Snap
A bot that could ***hypothetically*** be used to farm boosters for any deck in Marvel Snap. Strictly for educational purposes, of course. Can also be paired with an Agatha deck to more effectively farm missions and seasonal ranks. The bot uses various UI reference points to determine game state and then simulates user clicks to perform relevant actions.

**NOTE:** Because this is simulating user input, the bot will keep giving the Snap window focus and moving the mouse pointer to various locations of the game screen. It cannot run in the background and it will be difficult to do anything else on the PC while the bot is running. For best results, turn it on when you're going to be AFK for a while.

## Features

- Randomly plays cards and progresses turns using any deck
- Will farm matches on loop until stopped
- Supports both Ranked and Conquest modes
- Simulates user input with randomness to prevent detection. Does not modify game files in any way
- No additional third-party software required 
- Portable executable that does not require installation. Simply download latest release, start Snap, and run `BoosterBot.exe`
- Runs under randomized process name to help prevent detection
- Option to specify which round to retreat after to maximize xp gain

## Prerequisites

This is only intended for use with the Steam version of Marvel Snap on Windows 10/11. No additional third-party software is required.

Game should be run in fullscreen mode at 1080p for best results. Although the bot *can* function in windowed mode, performance becomes very inconsistent. 

**With the official launch of the PC client, the bot will be non-functional when the game is in "landscape mode". As a workaround, you can disable "landscape" in the game settings to return to the old UI and the bot should function as intended.**

## Getting Started

### Simple

1. Download the [latest](https://github.com/little-fort/booster-bot/releases/latest) .zip from the Releases page.
2. Unpack the archive into directory of your choice. 
3. Start the game and wait until main menu is loaded.
4. Start `BoosterBot.exe`

**NOTE:** Due to the bot's nature (taking over cursor control and processing screen content on an endless loop), it may be flagged and quarantined by certain third-party antivirus programs. If you find the bot being halted repeatedly, you may need to whitelist the application directory in your antivirus software.

### Advanced

Source can be cloned directly. Project is built on .NET 6, so you will need [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/) or the [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) installed in order to run.

The .exe can also be run from the command line with the following options:

**Scaling** 

`--scaling, -scaling, -s`

Used to adjust display scale, if necessary. You can check your current display scale in the display properties under System > Display > Custom scaling. If the display where Marvel Snap will be running is currently set to 100% scale (Windows 10) or the custom scaling entry field is empty (Windows 11), this value does not need to be used. If you have a custom scale value set, divide it by 100 and then pass in the value as an argument. Should be the first thing to check if you are running the game on something other than a standard 1080p monitor and the bot is not working.

Usage: `BoosterBot.exe --scaling 2.75`

## Notes

- Bot averages 11-14 matches per hour, which translates into about 66-84 boosters per hour. Hard limit of 1000 boosters per day still applies.
- Bot will always play out matches to the end and will occasionally snap just for the sake of randomness.
- The game has bugs that will sometimes cause matches to hang at the end and not progress to the booster collection screen. The bot will try to detect matches that have gone on too long and auto-retreat, but this doesn't always work and sometimes the game will require a restart to unblock.
- Any deck will work fine, but there is no logic to the plays it attempts to make. It will just try to move and drop cards to random locations, regardless of board state.
- The process name will be masked with a randomly generated string each time to avoid detection by the game client. As a result, `BoosterBot.exe` will not appear in the list of active processes after starting the bot, but you will see something like `nskeqpsv.exe` running instead.

