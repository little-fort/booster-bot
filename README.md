<p align="center">
 <img src="https://github.com/TurmanTech/booster-bot/assets/39720285/0bbc5968-6c91-4bc7-95ed-99727e48062e" />
</p>

# BoosterBot for Marvel SNAP
A bot that could ***hypothetically*** be used to farm boosters for any deck in Marvel SNAP. Strictly for educational purposes, of course. Can also be paired with an Agatha deck to effectively farm missions and seasonal ranks. The bot uses OCR powered by [Tesseract](https://github.com/charlesw/tesseract/) to detect on-screen controls and then performs relevant actions.

## Features

- Randomly plays cards and progresses turns using any deck
- Will farm matches on loop until stopped 
- Simulates user input with randomness to prevent detection. Does not modify game files in any way
- No additional third-party software required 
- Portable executable that does not require installation. Simply download latest release, start SNAP, and run the .exe

## Prerequisites

This is only intended for use with the Steam version of Marvel SNAP on Windows 10/11. No additional third-party software is required.

## Getting Started

### Simple

1. Download the latest .zip from the Releases page.
2. Unpack the archive into directory of your choice. 
3. Start the game and wait until main menu is loaded.
4. Start BoosterBot.exe 

### Advanced

Source can be cloned directly. Project is built on .NET 6, so you will need [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/) or the [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) installed in order to run. Project also makes use of the [Tesseract](https://github.com/charlesw/tesseract/) library, which may require you to download the [Visual Studio 2019 Runtime](https://visualstudio.microsoft.com/downloads/).

The .exe can also be run from the command line with the following options:

**Scaling** 

`--scaling, -scaling, -s`

Used to adjust display scale, if necessary. You can check your current display scale in the display properties under System > Display > Custom scaling. If the display where Marvel SNAP will be running is currently set to 100% scale (Windows 10) or the custom scaling entry field is empty (Windows 11), this value does not need to be used. If you have a custom scale value set, divide it by 100 and then pass in the value as an argument. Should be the first thing to check if you are running the game on something other than a standard 1080p monitor and the bot is not working.

Usage: `BoosterBot.exe --scaling 2.75`

**Verbose** 

`--verbose, -verbose, -v`

Enables full log details in the console window. Typically only used to debug issues with the OCR failing to recognize words on screen. If the `OCR RESULT` entries are failing to print words like 'Play' or 'Retreat', the OCR may be failing. Next step would be to enable the screencap log and verify that the images being scanned by OCR contain the relevant text.

**No Autoplay** 

`--noautoplay, -noautoplay, -na`

Disables the feature that attempts to play cards to the board. Useful if you're running an Agatha deck but you want to take over manual control when she plays herself early. Or if you want even more guarantee that you'll lose games, I guess.

**Save Screens** 

`--savescreens, -savescreens, -ss`

The OCR system works by taking a screenshot of the game (and only the game) at regular intervals and scanning certain areas of the screencap for certain text. Each new screenshot overwrites the last one so there is only one image at a time in the working directory. By enabling this feature, ***ALL*** screenshots taken by the bot are preserved in the `screens` subfolder of the working directory. Useful for debugging OCR if the game is consistently failing to read text from the game window. However, should be used with caution, as it can create a huge number of images if the bot is left unattended for a long time while this option is enabled.

## Notes

- Bot averages 11-14 matches per hour, which translates into about 66-84 boosters per hour. Hard limit of 1000 boosters per day still applies.
- Bot will always play out matches to the end, and will occasionally snap just for the sake of randomness.
- The game will sometimes hang at the end and not progress to the booster collection screen, so the bot will detect matches that have gone on too long and auto-retreat.
- Any deck will work fine, but there is no logic to the plays it attempts to make. It will just try to move and drop cards to random locations, regardless of board state.

