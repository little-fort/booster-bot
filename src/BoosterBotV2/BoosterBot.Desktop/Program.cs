using Avalonia;
using BoosterBot.Core.Pipeline;
using BoosterBot.Platform.Windows;
using OpenCvSharp;
using CvSize = OpenCvSharp.Size;

namespace BoosterBot.Desktop;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Contains("--test"))
        {
            RunDetectionTest(args);
            return;
        }
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();

    static void RunDetectionTest(string[] args)
    {
        var referencePath = Path.Combine(AppContext.BaseDirectory, "Assets", "Reference");
        ImageProcessor.LoadReferenceImages(referencePath, "en");

        Console.WriteLine("=== BoosterBot V2 Detection Test ===\n");

        // Load screenshot from file or capture live
        var fileIdx = Array.IndexOf(args, "--file");
        Mat screenshot;
        if (fileIdx >= 0 && fileIdx + 1 < args.Length)
        {
            var filePath = args[fileIdx + 1];
            screenshot = Cv2.ImRead(filePath, ImreadModes.Color);
            if (screenshot.Empty())
            {
                Console.WriteLine($"ERROR: Could not load image: {filePath}");
                return;
            }
            Console.WriteLine($"Loaded:   {filePath}");
        }
        else
        {
            var wm = new Win32WindowManager();
            var capture = new Win32ScreenCapture(wm);
            var window = wm.FindGameWindow();
            if (window is null)
            {
                Console.WriteLine("ERROR: Game window not found");
                return;
            }
            var ctx = capture.Capture();
            screenshot = ctx.Screenshot;
            Console.WriteLine($"Captured live from game window");
        }

        var viewport = ViewportDetector.Detect(screenshot);
        var scale = ViewportDetector.GetScale(viewport);

        Console.WriteLine($"Image:    {screenshot.Width}x{screenshot.Height}");
        Console.WriteLine($"Viewport: L={viewport.Left} T={viewport.Top} {viewport.Width}x{viewport.Height}");
        Console.WriteLine($"Scale:    {scale:F3}");

        var paddedRegion = ComponentMappings.PadForSearch(
            ComponentMappings.GetBtnPlay(viewport), scale);
        using var cropped = ImageProcessor.CropRegion(screenshot, paddedRegion);

        if (cropped.Empty())
        {
            Console.WriteLine("ERROR: Crop region is empty");
            return;
        }

        var reference = ImageProcessor.GetReferenceImage(ReferenceKeys.MainPlay);
        Console.WriteLine($"Cropped:  {cropped.Width}x{cropped.Height}");
        Console.WriteLine($"Ref:      {reference.Width}x{reference.Height}");

        // Production pipeline: grayscale → normalize to 1080p → blur → match
        using var preprocessed = ImageProcessor.Preprocess(cropped, scale);
        var confidence = ImageProcessor.GetMatchConfidence(preprocessed, reference);
        var result = confidence >= DetectionThresholds.ButtonHighConfidence ? "PASS" : "FAIL";

        Console.WriteLine($"\nPreprocessed: {preprocessed.Width}x{preprocessed.Height}");
        Console.WriteLine($"Confidence:   {confidence:P2}");
        Console.WriteLine($"Threshold:    {DetectionThresholds.ButtonHighConfidence:P2}");
        Console.WriteLine($"Result:       {result}");

        // Save new reference if requested
        if (args.Contains("--save-ref"))
        {
            Console.WriteLine("\n--- Saving reference ---");
            var exactRegion = ComponentMappings.GetBtnPlay(viewport);
            using var exactCrop = ImageProcessor.CropRegion(screenshot, exactRegion);
            using var newRef = ImageProcessor.Preprocess(exactCrop, scale);
            var savePath = Path.Combine(referencePath, "en", "btn-main-play-preproc.png");
            Cv2.ImWrite(savePath, newRef);
            Console.WriteLine($"  Saved: {savePath} ({newRef.Width}x{newRef.Height})");
        }

        Console.WriteLine("\nDone.");
    }
}
