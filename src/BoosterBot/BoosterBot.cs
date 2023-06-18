using OpenCvSharp;
using System.Diagnostics;
using System.Drawing;
using Tesseract;
using Point = System.Drawing.Point;
using Size = OpenCvSharp.Size;

namespace BoosterBot;

internal class BoosterBot
{
    private const string Image = "screen.png";
    private readonly double Scaling;
    private readonly bool Verbose;
    private readonly bool Autoplay;
    private readonly bool SaveScreens;
    private readonly int MaxAutoplay;

    private int Attempts { get; set; }
    private int Center { get; set; }
    private Rect Window { get; set; }
    private Dimension Screencap { get; set; }
    private Random Rand { get; set; }
    private Point ResetPoint { get; set; }
    private Point GameModesPoint { get; set; }
    private List<Point> Cards { get; set; }
    private List<Point> Locations { get; set; }
    private Stopwatch MatchTimer { get; set; }

    public BoosterBot(double scaling, bool verbose, bool autoplay, bool saveScreens, int maxAutoplay)
    {
        Scaling = scaling;
        Verbose = verbose;
        Autoplay = autoplay;
        Rand = new Random();
        SaveScreens = saveScreens;
        MaxAutoplay = maxAutoplay;
    }

    public void TestOcr()
    {
        GetPositions();
        Console.WriteLine("PROVING GROUNDS > SILVER = " + CalculateSimilarity("PROVING GROUNDS", "SILVER"));
        Console.WriteLine("PROVING GROUNDS > GOLD = " + CalculateSimilarity("PROVING GROUNDS", "GOLD"));
        Console.WriteLine("PROVING GROUNDS > INFINITE = " + CalculateSimilarity("PROVING GROUNDS", "INFINITE"));
        ReadArea(GetConquestBannerCrop(), export: true, expected: "PROVING GROUNDS");
        ReadArea(GetPlayButtonCrop(), export: true, expected: "PLAY"); ;
    }

    public void Run(bool conquest)
    {
        if (conquest)
            RunConquest();
        else
            RunLadder();
    }

    private void RunLadder()
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
                StartLadderMatch();

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

    private void RunConquest()
    {
        while (true)
        {
            GetPositions();
            Attempts++;

            if (IsOnMainMenu())
            {
                // Reset menu
                ResetMenu();
                Thread.Sleep(1000);

                // Click over to the Game Modes tab
                Log("Navigating to Game Modes tab...");
                Utilities.Click(GameModesPoint);
                Thread.Sleep(1000);
                Utilities.Click(GameModesPoint);
                Thread.Sleep(1000);

                // Select Conquest
                Log("Navigating to Conquest menu...");
                ClickConquest();
                Thread.Sleep(1000);

                // Select Proving Grounds
                ClickProvingGrounds();

                // Wait for matchmaking
                Thread.Sleep(25000);

                Attempts = 3;
            }

            if ((Attempts > 2 && Attempts < 8))
            {
                MatchTimer = new Stopwatch();
                MatchTimer.Start();

                var isMatch = ProcessMatch(true);

                if (isMatch)
                {
                    ClickNext();
                    Thread.Sleep(2000);
                    ProcessRewards();
                    
                    // Click through post-match screens
                    if (!ReadArea(GetConquestLobbySelectionCrop()))
                        ClickPlay();
                    
                    Thread.Sleep(2000);

                    if (!ReadArea(GetConquestLobbySelectionCrop()))
                        ClickPlay();

                    Thread.Sleep(2000);
                }
                else
                {
                    ResetClick();
                    Log("No match was detected.");
                }
            }
            else if (Attempts > 8)
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

        GameModesPoint = new Point
        {
            X = Window.Left + Center + 175,
            Y = Window.Bottom - 10
        };
    }

    private bool IsOnMainMenu()
    {
        Log("Checking if game is on main menu...");
        Utilities.Click(Window.Left + Center, Window.Bottom - 1);
        var foundText = ReadArea(GetPlayButtonCrop());
        LogAttempts();

        // return samples.Any(x => x.ToLower().Contains("play"));
        //return samples.Any(x => !string.IsNullOrWhiteSpace(x));
        return foundText;
    }

    private void StartLadderMatch()
    {
        MatchTimer.Restart();
        Log("Main menu detected, starting matchmaking...");
        ClickPlay();
        Log("Waiting 25 seconds to allow for match to load...");
        Thread.Sleep(25000);

        LogAttempts();
    }

    private bool ProcessMatch(bool alwaysSnap = false)
    {
        List<string> samples;
        var isMatch = false;
        var snapped = false;

        for (int i = 0; i < 5; i++)
        {
            if (!isMatch)
                Log("Checking for active match...");

            GetPositions();

            // Set coordinates for "RETREAT" button:
            //samples = ReadArea(GetRetreatCrop());

            //if (samples.Any(x => x.ToLower().Contains("retreat")))
            //if (samples.Any(x => !string.IsNullOrWhiteSpace(x.ToLower())))
            if (ReadArea(GetRetreatCrop()))
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
                    if (!snapped && (alwaysSnap || Rand.Next(100) < 5))
                        snapped = ClickSnap();

                    //samples = ReadArea(GetRetreatCrop());
                    //if (samples.Any(x => x.ToLower().Contains("retreat")))
                    if (ReadArea(GetRetreatCrop(), expected: "RETREAT"))
                    {
                        ClickNext();
                        Thread.Sleep(Rand.Next(400, 600));
                        ClickNext();
                        i = 0; // Ensure the variable does not keep incrementing until active match is no longer deteced
                    }

                    if (Autoplay)
                        Thread.Sleep(Rand.Next(3000, 5000)); 
                    else
                        Thread.Sleep(Rand.Next(4000, 7000)); // Wait 4-7 seconds to check again to avoid spamming the button
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
            //samples = ReadArea(GetCollectCrop());

            // OCR has trouble with this one but determing if any text exists is good enough:
            //if (samples.All(x => string.IsNullOrWhiteSpace(x)))
            if (ReadArea(GetCollectCrop()))
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

                    //if (btnSamples.Any(x => !string.IsNullOrWhiteSpace(x)) || msgSamples.Any(x => !string.IsNullOrWhiteSpace(x)))
                    if (btnSamples || msgSamples)
                    {
                        rewards = true;
                        Log("Exiting to main menu...");
                        j = i = 5;
                        ClickNext();
                        Thread.Sleep(Rand.Next(10000, 15000));
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

    private Rect GetPlayButtonCrop() => new Rect
    {
        Left = Center - 65,
        Right = Center + 60,
        Top = Screencap.Height - 245,
        Bottom = Screencap.Height - 175
    };

    private Rect GetRetreatCrop() => new Rect
    {
        Left = Center - 400,
        Right = Center - 260,
        Top = Screencap.Height - 100,
        Bottom = Screencap.Height - 30
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

    private Rect GetConquestBannerCrop() => new Rect
    {
        Left = Center - 110,
        Right = Center + 100,
        Top = 15,
        Bottom = 60
    };

    private Rect GetConquestLobbySelectionCrop() => new Rect
    {
        Left = Center - 160,
        Right = Center + 160,
        Top = 120,
        Bottom = 170
    };

    private void ResetClick() => Utilities.Click(ResetPoint);

    private void ResetMenu() => Utilities.Click(Window.Left + Center, Window.Bottom - 1);

    /// <summary>
    /// Simulates attempting to play four cards in your hand to random locations.
    /// </summary>
    private void PlayHand()
    {
        if (MaxAutoplay > 0) Utilities.PlayCard(Cards[3], Locations[Rand.Next(3)], ResetPoint);
        if (MaxAutoplay > 1) Utilities.PlayCard(Cards[2], Locations[Rand.Next(3)], ResetPoint);
        if (MaxAutoplay > 2) Utilities.PlayCard(Cards[1], Locations[Rand.Next(3)], ResetPoint);
        if (MaxAutoplay > 3) Utilities.PlayCard(Cards[0], Locations[Rand.Next(3)], ResetPoint);

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
    /// Simulates clicking the Conquest mode from the Game Modes tab
    /// </summary>
    private void ClickConquest() => Utilities.Click(Window.Left + Center + Rand.Next(-20, 20), 330 + Rand.Next(-20, 20));

    /// <summary>
    /// Swipes over the Conquest carousel to Proving Grounds and then Enters the mode
    /// </summary>
    private void ClickProvingGrounds()
    {
        Thread.Sleep(1000);

        // The carousel defaults to the highest diffulty level that you have tickets for. Need to first swipe back over to Proving Grounds
        Log("Making sure lobby type is set to Proving Grounds...");
        for (int x = 0; x < 5; x++)
        {
            Utilities.Drag(
                startX: Window.Left + Center - 250,
                startY: Window.Bottom / 2,
                endX: Window.Left + Center + 250,
                endY: Window.Bottom / 2
            );
            Thread.Sleep(1000);
        }

        // Attempt to verify that Proving Grounds is selected:
        var crop = GetConquestBannerCrop();
        //var lobby = ReadArea(GetConquestLobbyTypeCrop());
        //foreach (var text in lobby)
        //{
        var isProving = ReadArea(crop, expected: "PROVING GROUNDS");
        if (isProving)
            Log("Confirmed Proving Grounds lobby...");
        else
        {
            var isSilver = ReadArea(crop, expected: "SILVER"); // CalculateSimilarity("SILVER", text) > 60.0;
            var isGold = ReadArea(crop, expected: "GOLD"); // CalculateSimilarity("GOLD", text) > 60.0;
            var isInfinite = ReadArea(crop, expected: "INFINITE"); // CalculateSimilarity("INFINITE", text) > 60.0;
            if (isSilver || isGold || isInfinite)
            {
                Log("\n\n############## WARNING ##############");
                Log($"Detected active Conquest lobby in a tier higher than Proving Grounds. BoosterBot will stop running to avoid consuming Conquest tickets.");
                Log("Finish your current Conquest matches and restart when Proving Grounds is accessible again.");
                Log("\nPress [Enter] to exit...");
                Console.ReadLine();
                Environment.Exit(0);
            }
        }
        //}

        // Click once to hit "Enter"
        Log("Starting matchmaking...");
        ClickPlay();
        Thread.Sleep(1000);

        // Click again to hit "Play"
        ClickPlay();
        Thread.Sleep(1000);

        // One more time just to be safe:
        ClickPlay();
        Thread.Sleep(1000);

        // Confirm deck:
        Utilities.Click(Window.Left + Center + 100, Window.Bottom - 345);
    }

    /// <summary>
    /// Simulates clicks to from a match.
    /// </summary>
    private void ClickRetreat()
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
            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {text}");
    }

    private bool ReadArea(Rect crop, string image = Image, int sampleCount = 5, bool export = false, string expected = "")
    {
        var baseImg = ReadArea(crop.Left, crop.Top, crop.Right - crop.Left, crop.Bottom - crop.Top, export, expected: expected);
        var preprocImg = ReadArea(crop.Left, crop.Top, crop.Right - crop.Left, crop.Bottom - crop.Top, export, expected: expected, preproc: true);

        return baseImg || preprocImg;
    }
    /*{
        var samples = new List<string>();
        for (int x = 0; x < sampleCount; x++)
            samples.Add(ReadArea(crop.Left, crop.Top, crop.Right - crop.Left, crop.Bottom - crop.Top, export, expected: expected));

        return samples;
    }*/

    /// <summary>
    /// Takes a crop of the specified image and attempts to parse out any text.
    /// </summary>
    private bool ReadArea(int x, int y, int width, int height, bool export, string image = Image, string expected = "", bool preproc = false)
    {
        // Preprocess image
        if (preproc)
        {
            var preprocImagePath = image.Replace(".png", "-preproc.png");
            var preprocImage = PreprocessImage(image);
            preprocImage.ImWrite(preprocImagePath);
            image = preprocImagePath;
        }

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
        using var page = engine.Process(pix, PageSegMode.SingleLine);

        if (export || SaveScreens)
            SaveImage(rect, image); // Export crop for debugging

        // Read from image:
        var result = page.GetText()?.Trim();
        var similarity = CalculateSimilarity(expected, result);
        var log = $"OCR RESULT: {result}";

        if (!string.IsNullOrWhiteSpace(expected))
            log += $" [Expected: {expected}][Similarity: {CalculateSimilarity(expected, result)}]";

        Log(log, true); // Print read result for debugging
        return string.IsNullOrWhiteSpace(expected) ? result.Trim().Length > 0 : similarity > 60.0;
    }

    /// <summary>
    /// A method to preprocess the cropped image so that the Tesseract OCR will return more consistent results.
    /// </summary>
    public Mat PreprocessImage(string imagePath)
    {
        // Load the image
        Mat src = Cv2.ImRead(imagePath, ImreadModes.Color);

        // Convert the image to grayscale
        Mat gray = new Mat();
        Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

        // Apply Gaussian blur to reduce noise
        Mat blurred = new Mat();
        Cv2.GaussianBlur(gray, blurred, new Size(5, 5), 0);

        // Apply adaptive thresholding to enhance contrast
        Mat thresh = new Mat();
        Cv2.AdaptiveThreshold(blurred, thresh, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 11, 2);

        return thresh;
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

    /// <summary>
    /// Takes in an expected value and compares it to what OCR returned to generate a similarity score based on how close the values are. Calculation is case-insensitive.
    /// </summary>
    private double CalculateSimilarity(string expected, string ocr)
    {
        int maxLen = Math.Max(expected.Length, ocr.Length);
        if (maxLen == 0) 
            return 1.0; // If both strings are empty, they are 100% similar

        int dist = LevenshteinDistance(expected.ToUpper(), ocr.ToUpper());

        return (1.0 - (double)dist / maxLen) * 100; // Percentage of similarity
    }


    /// <summary>
    /// Used to calculate the Levenshtein distance—the minimum number of single-character edits (insertions, deletions, or substitutions) required to change one word into the other.
    /// </summary>
    private int LevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source))
        {
            if (string.IsNullOrEmpty(target)) 
                return 0;

            return target.Length;
        }
        if (string.IsNullOrEmpty(target)) return source.Length;

        var matrix = new int[source.Length + 1, target.Length + 1];

        // Initialize the first column
        for (var i = 0; i <= source.Length; i++)
            matrix[i, 0] = i;

        // Initialize the first row
        for (var j = 0; j <= target.Length; j++)
            matrix[0, j] = j;

        for (var i = 1; i <= source.Length; i++)
        {
            for (var j = 1; j <= target.Length; j++)
            {
                var cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[source.Length, target.Length];
    }

}
