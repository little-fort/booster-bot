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
            var region = ComponentMappings.GetBtnPlay(_lastCapture.Dimensions, _lastCapture.Center);
            using var cropped = ImageProcessor.CropRegion(_lastCapture.Screenshot, region);

            if (cropped.Empty())
            {
                DetectionResult = "Crop region is empty — check window dimensions";
                return;
            }

            using var preprocessed = ImageProcessor.Preprocess(cropped);

            var reference = ImageProcessor.GetReferenceImage(ReferenceKeys.MainPlay);
            var result = ImageProcessor.IsReferencePresent(preprocessed, reference, DetectionThresholds.ButtonHighConfidence);

            var confidence = ImageProcessor.GetMatchConfidence(preprocessed, reference);
            var matchText = result.Result ? "MATCH" : "NO MATCH";

            DetectionResult = $"{matchText} — Confidence: {confidence:P2} (threshold: {DetectionThresholds.ButtonHighConfidence:P2})";

            if (result.Logs.Count > 0)
                DetectionResult += "\n" + string.Join("\n", result.Logs);
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
