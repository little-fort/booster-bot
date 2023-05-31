using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tesseract;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace BoosterBot;

internal class BoosterBot
{
    private const string Image = "screen.png";
    private readonly double Scaling;
    private readonly bool Verbose;
    private readonly bool Autoplay;
    private readonly bool SaveScreens;

    private int Attempts { get; set; }
    private int Center { get; set; }
    private Rect Window { get; set; }
    private Dimension Screencap { get; set; }
    private Random Rand { get; set; }
    private Point ResetPoint { get; set; }
    private List<Point> Cards { get; set; }
    private List<Point> Locations { get; set; }
    private Stopwatch MatchTimer { get; set; }

    public BoosterBot(double scaling, bool verbose, bool autoplay, bool saveScreens)
    {
        Scaling = scaling;
        Verbose = verbose;
        Autoplay = autoplay;
        Rand = new Random();
        SaveScreens = saveScreens;
    }

    public void Run()
    {
        MatchTimer = new Stopwatch();
        MatchTimer.Start();

        while (true)
        {
            GetPositions();
            var onMenu = IsOnMainMenu();
            Attempts++;

            if (onMenu || (Attempts > 2 && Attempts < 10))
            {
                if (onMenu)
                    Attempts = 3; // Make sure it doesn't loop through menu processing again

                var isMatch = ProcessMatch();

                if (isMatch)
                    ProcessRewards();
                else
                {
                    ResetClick();
                    Log("No match was detected.");
                }
            }
            else if (Attempts >= 10)
            {
                Log("Attempting blind 'next' click...");
                ClickNext();
                Thread.Sleep(10000);
                Attempts = 0;
            }
        }
    }

    private void GetPositions()
    {
        // Find game window and take screencap:
        Window = Utilities.GetGameWindowLocation();
        Screencap = Utilities.GetGameScreencap(Window, Scaling);

        // Calculate center position of game window:
        Center = Screencap.Width / 2;

        // Update card and location coordinates:
        Cards = new List<Point>
        {
            new Point
            {
                X = Window.Left + Center + 20,
                Y = Window.Bottom - 180
            },
            new Point
            {
                X = Window.Left + Center - 88,
                Y = Window.Bottom - 200
            },
            new Point
            {
                X = Window.Left + Center + 118,
                Y = Window.Bottom - 195
            },
            new Point
            {
                X = Window.Left + Center - 183,
                Y = Window.Bottom - 180
            }
        };

        Locations = new List<Point>()
        {
            new Point
            {
                X = Window.Left + Center - 170,
                Y = Window.Bottom - 340
            },
            new Point
            {
                X = Window.Left + Center + 20,
                Y = Window.Bottom - 340
            },
            new Point
            {
                X = Window.Left + Center + 200,
                Y = Window.Bottom - 340
            }
        };

        ResetPoint = new Point
        {
            X = Window.Left + 100,
            Y = Window.Bottom - 200
        };
    }

    private bool IsOnMainMenu()
    {
        Log("Checking if game is on main menu...");
        Utilities.Click(Window.Left + Center, Window.Bottom - 1);
        var crop = new Rect
        {
            Left = Center - 65,
            Right = Center + 60,
            Top = Screencap.Height - 255,
            Bottom = Screencap.Height - 160
        };
        var samples = ReadArea(crop);
        LogAttempts();

        if (samples.Any(x => x.ToLower().Contains("play")))
        {
            MatchTimer.Restart();
            Log("Main menu detected, starting matchmaking...");
            ClickPlay();
            Log("Waiting 25 seconds to allow for match to load...");
            Thread.Sleep(25000);

            LogAttempts();

            return true;
        }

        return false;
    }

    private bool ProcessMatch()
    {
        List<string> samples;
        var isMatch = false;
        var snapped = false;

        for (int i = 0; i < 3; i++)
        {
            if (!isMatch)
                Log("Checking for active match...");

            GetPositions();

            // Set coordinates for "RETREAT" button:
            samples = ReadArea(GetRetreatCrop());

            if (samples.Any(x => x.ToLower().Contains("retreat")))
            {
                Attempts = 0;

                var ts = MatchTimer.Elapsed;
                var elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                Console.WriteLine("Match time: " + elapsedTime, true);
                if (ts.Minutes > 15)
                {
                    // Game is likely bugged out and need to attempt retreating:
                    Log("Match has gone longer than 15 minutes, attempting retreat...");
                    ClickRetreat();
                }
                else
                {
                    LogAttempts();

                    if (!isMatch)
                        Log($"Playing match...");

                    isMatch = true;

                    if (Autoplay)
                        PlayHand();

                    // Roll for an occasional SNAP:
                    if (!snapped && Rand.Next(100) < 5)
                        snapped = ClickSnap();

                    samples = ReadArea(GetRetreatCrop());
                    if (samples.Any(x => x.ToLower().Contains("retreat")))
                    {
                        ClickNext();
                        i = 0; // Ensure the variable does not keep incrementing until active match is no longer deteced
                    }

                    if (Autoplay)
                        Thread.Sleep(Rand.Next(3000, 5000)); 
                    else
                        Thread.Sleep(Rand.Next(5000, 10000)); // Wait 12-20 seconds to check again to avoid spamming the button
                }
            }
            else
            {
                var wait = Rand.Next(5000, 10000);
                Log($"Active match not detected, checking again in {wait / 1000} seconds...");
                Thread.Sleep(wait);
                ResetClick();
                Attempts++;
                LogAttempts();
            }
        }

        return isMatch;
    }

    private void ProcessRewards()
    {
        Log("Active match no longer detected. Checking for end of match...");
        ResetClick();

        List<string> samples;
        for (int i = 0; i < 5; i++)
        {
            GetPositions();
            samples = ReadArea(GetCollectCrop());

            // OCR has trouble with this one but determing if any text exists is good enough:
            if (samples.All(x => string.IsNullOrWhiteSpace(x)))
            {
                Log("Match end detected, collecting boosters...");
                Log("Clicking 'Collect Rewards' button...");
                ClickNext();
                Thread.Sleep(Rand.Next(10000, 15000));

                bool rewards = false;
                for (int j = 0; j < 5; j++)
                {
                    GetPositions();

                    // Attempt to read "NEXT" button (for some reason OCR is baffled by this one):
                    var btnSamples = ReadArea(GetNextCrop());

                    // Attept to read Victory/Defeat message
                    var msgSamples = ReadArea(GetVictoryDefeatCrop());

                    if (btnSamples.Any(x => !string.IsNullOrWhiteSpace(x)) || msgSamples.Any(x => !string.IsNullOrWhiteSpace(x)))
                    {
                        rewards = true;
                        Log("Exiting to main menu...");
                        j = i = 5;
                        ClickNext();
                        Thread.Sleep(Rand.Next(5000, 10000));
                    }
                    else
                    {
                        var wait = Rand.Next(5000, 10000);
                        Log($"Could not determine rewards status, checking again in {wait / 1000} seconds...");
                        ResetClick();
                        Thread.Sleep(wait);
                    }
                }

                if (!rewards)
                {
                    i = 5;
                    Log("Attempting exit click...");
                    Thread.Sleep(5000);
                    ResetClick();
                    ClickNext();
                }
            }
            else
            {
                Log("Could not determine match status, checking again in 5 seconds...");
                Thread.Sleep(5000);
                ResetClick();
            }
        }
    }

    private Rect GetRetreatCrop() => new Rect
    {
        Left = Center - 400,
        Right = Center - 260,
        Top = Screencap.Height - 95,
        Bottom = Screencap.Height - 35
    };

    private Rect GetCollectCrop() => new Rect
    {
        Left = Center + 215,
        Right = Center + 380,
        Top = Screencap.Height - 95,
        Bottom = Screencap.Height - 45
    };

    private Rect GetNextCrop() => new Rect
    {
        Left = Center + 255,
        Right = Center + 430,
        Top = Screencap.Height - 95,
        Bottom = Screencap.Height - 25
    };

    private Rect GetVictoryDefeatCrop() => new Rect
    {
        Left = Center - 110,
        Right = Center + 110,
        Top = 190,
        Bottom = 250
    };

    private void ResetClick() => Utilities.Click(ResetPoint);

    /// <summary>
    /// Simulates attempting to play four cards in your hand to random locations.
    /// </summary>
    private void PlayHand()
    {
        Utilities.PlayCard(Cards[3], Locations[Rand.Next(3)], ResetPoint);
        Utilities.PlayCard(Cards[2], Locations[Rand.Next(3)], ResetPoint);
        Utilities.PlayCard(Cards[1], Locations[Rand.Next(3)], ResetPoint);
        Utilities.PlayCard(Cards[0], Locations[Rand.Next(3)], ResetPoint);

        ResetClick();
    }

    /// <summary>
    /// SNAP
    /// </summary>
    /// <returns></returns>
    private bool ClickSnap()
    {
        Utilities.Click(Window.Left + Center + Rand.Next(-20, 20), 115 + Rand.Next(-20, 20));
        Log("OH SNAP!");

        return true;
    }

    /// <summary>
    /// Simulates clicking the "Play" button while on the main menu.
    /// </summary>
    private void ClickPlay() => Utilities.Click(Window.Left + Center + Rand.Next(-20, 20), Window.Bottom - 200 + Rand.Next(-20, 20));

    /// <summary>
    /// Simulates clicking the "Next"/"Collect Rewards" button while in a match.
    /// </summary>
    private void ClickNext() => Utilities.Click(Window.Left + Center + 300 + Rand.Next(-20, 20), Window.Bottom - 60 + Rand.Next(-10, 10));

    /// <summary>
    /// Simulates clicks to from a match.
    /// </summary>
    public void ClickRetreat()
    {
        GetPositions();

        var pnt = new Point // Retreat
        {
            X = Window.Left + Center - 300,
            Y = Window.Bottom - 70
        };
        Utilities.Click(pnt);

        pnt = new Point // Retreat Now
        {
            X = Window.Left + Center - 100,
            Y = Window.Bottom - 280
        };
        Utilities.Click(pnt);

        Thread.Sleep(10000);
    }

    /// <summary>
    /// Used to log processing attempts by the main methods.
    /// </summary>
    private void LogAttempts()
    {
        if (Attempts > 0)
            Log($"Attempts: {Attempts}", true);
    }

    /// <summary>
    /// Prints the given text to the console.
    /// </summary>
    private void Log(string text, bool onlyVerbose = false)
    {
        if (!onlyVerbose || Verbose)
            Console.WriteLine(text);
    }

    private List<string> ReadArea(Rect crop, string image = Image, int sampleCount = 5, bool export = false)
    {
        var samples = new List<string>();
        for (int x = 0; x < sampleCount; x++)
            samples.Add(ReadArea(crop.Left, crop.Top, crop.Right - crop.Left, crop.Bottom - crop.Top, export));

        return samples;
    }

    /// <summary>
    /// Takes a crop of the specified image and attempts to parse out any text.
    /// </summary>
    private string ReadArea(int x, int y, int width, int height, bool export, string image = Image)
    {
        // Initialize engine and load image
        using var engine = new TesseractEngine(@"tessdata", "eng", EngineMode.Default); // @"./tessdata" should be the path to the tessdata directory.
        using var img = new Bitmap(image);

        // Create crop of relevant area
        var rect = new Rectangle() { X = x, Y = y, Width = width, Height = height };
        using var crop = img.Clone(rect, img.PixelFormat);

        // Convert Bitmap to Pix
        var converter = new BitmapToPixConverter();
        using var pix = converter.Convert(crop);

        // Process image
        using var page = engine.Process(pix);

        if (export || SaveScreens)
            SaveImage(rect); // Export crop for debugging

        // Read from image:
        var result = page.GetText()?.Trim();
        Log($"OCR RESULT: {result}", true); // Print read result for debugging
        return result;
    }

    /// <summary>
    /// Takes a crop with the given dimensions from the specified image and saves it as an image to the disk. Used for debugging.
    /// </summary>
    private static void SaveImage(Rectangle crop, string image = Image)
    {
        var destRect = new Rectangle(Point.Empty, crop.Size);
        var cropImage = new Bitmap(destRect.Width, destRect.Height);
        using var graphics = Graphics.FromImage(cropImage);
        using var bitmap = new Bitmap(image);
        graphics.DrawImage(bitmap, destRect, crop, GraphicsUnit.Pixel);

        // Create directory if it doesn't exist
        var dir = "screens";
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        cropImage.Save(@"screens//snapcap-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png", System.Drawing.Imaging.ImageFormat.Png);
    }
}
