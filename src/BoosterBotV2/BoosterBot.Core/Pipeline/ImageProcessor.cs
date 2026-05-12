using BoosterBot.Core.Models;
using OpenCvSharp;

namespace BoosterBot.Core.Pipeline;

public static class ImageProcessor
{
    private static readonly Dictionary<string, Mat> _referenceImages = new();
    private static bool _referencesLoaded;

    public static void LoadReferenceImages(string basePath, string culture)
    {
        if (_referencesLoaded) return;

        var dir = Path.Combine(basePath, culture);
        if (!Directory.Exists(dir))
            throw new DirectoryNotFoundException($"Reference image directory not found: {dir}");

        foreach (var file in Directory.GetFiles(dir, "*.png"))
        {
            var key = Path.GetFileNameWithoutExtension(file);
            var mat = Cv2.ImRead(file, ImreadModes.Grayscale);
            if (!mat.Empty())
                _referenceImages[key] = mat;
        }

        _referencesLoaded = true;
    }

    public static Mat GetReferenceImage(string key)
    {
        if (_referenceImages.TryGetValue(key, out var mat))
            return mat;

        throw new FileNotFoundException($"Reference image not found in cache: {key}");
    }

    public static Mat CropRegion(Mat source, Models.Rect area)
    {
        var roi = new OpenCvSharp.Rect(area.Left, area.Top, area.Width, area.Height);

        roi.X = Math.Max(0, roi.X);
        roi.Y = Math.Max(0, roi.Y);
        roi.Width = Math.Min(roi.Width, source.Width - roi.X);
        roi.Height = Math.Min(roi.Height, source.Height - roi.Y);

        if (roi.Width <= 0 || roi.Height <= 0)
            return new Mat();

        var cropped = new Mat(source, roi);
        var result = cropped.Clone();
        cropped.Dispose();
        return result;
    }

    public static Mat Preprocess(Mat source)
    {
        var gray = new Mat();
        var code = source.Channels() switch
        {
            4 => ColorConversionCodes.BGRA2GRAY,
            3 => ColorConversionCodes.BGR2GRAY,
            _ => throw new ArgumentException($"Unexpected channel count: {source.Channels()}")
        };
        Cv2.CvtColor(source, gray, code);

        var blurred = new Mat();
        Cv2.GaussianBlur(gray, blurred, new OpenCvSharp.Size(5, 5), 0);
        gray.Dispose();

        var thresh = new Mat();
        Cv2.AdaptiveThreshold(blurred, thresh, 255,
            AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 11, 2);
        blurred.Dispose();

        return thresh;
    }

    public static PipelineResult<bool> IsReferencePresent(Mat captured, Mat reference, double threshold = DetectionThresholds.Default)
    {
        var logs = new List<string>();

        using var capturedGray = ConvertToGray(captured);
        using var referenceGray = ConvertToGray(reference);

        if (capturedGray.Empty() || referenceGray.Empty())
            return PipelineResult<bool>.Fail(["Empty image provided"]);

        if (referenceGray.Width > capturedGray.Width || referenceGray.Height > capturedGray.Height)
            return PipelineResult<bool>.Fail(["Reference image is larger than captured region"]);

        using var result = new Mat();
        Cv2.MatchTemplate(capturedGray, referenceGray, result, TemplateMatchModes.CCoeffNormed);
        Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out _);

        logs.Add($"  SCORE: {maxVal:P2}");
        logs.Add($"  LIMIT: {threshold:P2}");
        logs.Add($"  MATCH: {maxVal >= threshold}");

        return PipelineResult<bool>.Ok(maxVal >= threshold, logs);
    }

    public static double GetMatchConfidence(Mat captured, Mat reference)
    {
        using var capturedGray = ConvertToGray(captured);
        using var referenceGray = ConvertToGray(reference);

        if (capturedGray.Empty() || referenceGray.Empty())
            return 0.0;

        if (referenceGray.Width > capturedGray.Width || referenceGray.Height > capturedGray.Height)
            return 0.0;

        using var result = new Mat();
        Cv2.MatchTemplate(capturedGray, referenceGray, result, TemplateMatchModes.CCoeffNormed);
        Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out _);

        return maxVal;
    }

    public static Mat NormalizeToReferenceScale(Mat source, double scale)
    {
        if (Math.Abs(scale - 1.0) < 0.01)
            return source.Clone();

        var targetWidth = (int)Math.Round(source.Width / scale);
        var targetHeight = (int)Math.Round(source.Height / scale);

        if (targetWidth <= 0 || targetHeight <= 0)
            return source.Clone();

        var resized = new Mat();

        if (source.Channels() == 1)
        {
            // Binary/preprocessed image: Area interpolation produces smooth anti-aliased
            // edges during downscaling, then re-threshold to get clean binary output
            // that approximates what native-resolution preprocessing would produce.
            Cv2.Resize(source, resized, new OpenCvSharp.Size(targetWidth, targetHeight),
                interpolation: InterpolationFlags.Area);
            var clean = new Mat();
            Cv2.Threshold(resized, clean, 128, 255, ThresholdTypes.Binary);
            resized.Dispose();
            return clean;
        }

        var interpolation = scale > 1.0
            ? InterpolationFlags.Area
            : InterpolationFlags.Cubic;
        Cv2.Resize(source, resized, new OpenCvSharp.Size(targetWidth, targetHeight),
            interpolation: interpolation);
        return resized;
    }

    internal static Mat ConvertToGrayPublic(Mat source) => ConvertToGray(source);

    private static Mat ConvertToGray(Mat source)
    {
        if (source.Channels() == 1)
            return source.Clone();

        var gray = new Mat();
        var code = source.Channels() == 4
            ? ColorConversionCodes.BGRA2GRAY
            : ColorConversionCodes.BGR2GRAY;
        Cv2.CvtColor(source, gray, code);
        return gray;
    }
}
