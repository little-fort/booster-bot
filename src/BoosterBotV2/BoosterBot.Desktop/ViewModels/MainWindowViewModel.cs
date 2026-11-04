using System.IO;
using Avalonia.Media.Imaging;
using BoosterBot.Core.Models;
using BoosterBot.Core.Pipeline;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenCvSharp;

namespace BoosterBot.Desktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IScreenCapture _screenCapture;
    private readonly IWindowManager _windowManager;

    private ScreenCaptureContext? _lastCapture;

    [ObservableProperty]
    private string _statusText = "Ready — open Marvel Snap and click Capture";

    [ObservableProperty]
    private Bitmap? _capturedImage;

    [ObservableProperty]
    private string _detectionResult = "";

    [ObservableProperty]
    private Bitmap? _debugCropped;

    [ObservableProperty]
    private Bitmap? _debugPreprocessed;

    [ObservableProperty]
    private Bitmap? _debugReference;

    public MainWindowViewModel(IScreenCapture screenCapture, IWindowManager windowManager)
    {
        _screenCapture = screenCapture;
        _windowManager = windowManager;
    }

    [RelayCommand]
    private void CaptureScreen()
    {
        try
        {
            var window = _windowManager.FindGameWindow();
            if (window is null)
            {
                StatusText = "Game window not found — is Marvel Snap running?";
                return;
            }

            _lastCapture?.Dispose();
            _lastCapture = _screenCapture.Capture();

            CapturedImage = MatToAvaloniaBitmap(_lastCapture.Screenshot);
            StatusText = $"Captured: {_lastCapture.Dimensions.Width}x{_lastCapture.Dimensions.Height} — Center: {_lastCapture.Center}";
            DetectionResult = "";
        }
        catch (Exception ex)
        {
            StatusText = $"Capture failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private void TestDetection()
    {
        if (_lastCapture is null)
        {
            DetectionResult = "No capture available — click Capture first";
            return;
        }

        try
        {
            var viewport = ViewportDetector.Detect(_lastCapture.Screenshot);
            var scale = ViewportDetector.GetScale(viewport);
            var definition = ReferenceCatalog.Find(ReferenceKeys.MainPlay)!;
            var region = definition.GetSearchRegion(viewport);

            using var cropped = ImageProcessor.CropRegion(_lastCapture.Screenshot, region);

            if (cropped.Empty())
            {
                DetectionResult = "Crop region is empty — check window dimensions";
                return;
            }

            // Preprocess normalizes to 1080p (grayscale + downscale + blur),
            // so the reference at 1080p matches directly — no scaling needed.
            using var preprocessed = ImageProcessor.Preprocess(cropped, scale);
            var reference = ImageProcessor.GetReferenceImage(ReferenceKeys.MainPlay);

            DebugCropped = MatToAvaloniaBitmap(cropped);
            DebugPreprocessed = MatToAvaloniaBitmap(preprocessed);
            DebugReference = MatToAvaloniaBitmap(reference);

            var confidence = ImageProcessor.GetMatchConfidence(preprocessed, reference);
            var matchText = confidence >= definition.Threshold ? "MATCH" : "NO MATCH";

            DetectionResult = $"{matchText} — Confidence: {confidence:P2} (threshold: {definition.Threshold:P2})"
                + $"\nViewport: L={viewport.Left} T={viewport.Top} {viewport.Width}x{viewport.Height} (scale: {scale:F3})"
                + $"\nCrop region: L={region.Left} T={region.Top} R={region.Right} B={region.Bottom} ({region.Width}x{region.Height})"
                + $"\nCapture: {_lastCapture.Dimensions.Width}x{_lastCapture.Dimensions.Height}"
                + $"\nPreprocessed: {preprocessed.Width}x{preprocessed.Height}"
                + $"\nRef: {reference.Width}x{reference.Height}";
        }
        catch (Exception ex)
        {
            DetectionResult = $"Detection failed: {ex.Message}";
        }
    }

    private static Bitmap? MatToAvaloniaBitmap(Mat mat)
    {
        if (mat.Empty()) return null;

        Cv2.ImEncode(".png", mat, out var buffer);
        using var stream = new MemoryStream(buffer);
        return new Bitmap(stream);
    }
}
