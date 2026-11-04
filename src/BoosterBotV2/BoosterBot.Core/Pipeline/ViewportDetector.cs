using BoosterBot.Core.Models;
using OpenCvSharp;

namespace BoosterBot.Core.Pipeline;

public static class ViewportDetector
{
    public const int ReferenceHeight = 1080;

    // In portrait mode the game scales its UI by height and centers it horizontally in every
    // window shape (measured 2026-11: windowed 600x800, 1000x800, 600x1000, 810x1080). The one
    // exception is fullscreen on a display taller than the game's frame (e.g., 2560x1600), where
    // the game draws black letterbox bars above and below. Their size does NOT follow 16:9
    // geometry (a real 2560x1600 capture had 53px bars, content 2560x1494), so they are measured.
    private const double GameAspectRatio = 16.0 / 9.0;

    // A bar row has every pixel pure black; allow a little headroom for capture noise
    private const byte BarMaxBrightness = 8;

    // Letterboxing is symmetric. Requiring it avoids trimming a frame whose top or bottom just
    // happens to be dark (fades, transitions).
    private const int BarSymmetryTolerance = 2;

    public static GameViewport Detect(Mat screenshot)
    {
        int w = screenshot.Width, h = screenshot.Height;
        var (top, bottom) = MeasureBars(screenshot);

        // Ignore slivers: dark screens can have a black edge row or two (real bars are 3%+ of height)
        if (top > 0 && top >= h / 100 && Math.Abs(top - bottom) <= BarSymmetryTolerance && top + bottom < h / 2)
        {
            int contentHeight = h - top - bottom;
            int contentWidth = Math.Min(w, (int)Math.Round(contentHeight * GameAspectRatio));
            return new GameViewport((w - contentWidth) / 2, top, contentWidth, contentHeight);
        }

        return Pillarbox(w, h);
    }

    // Counts fully black rows at the top and bottom edges of the capture
    private static (int Top, int Bottom) MeasureBars(Mat screenshot)
    {
        using var gray = ImageProcessor.ConvertToGrayPublic(screenshot);
        using var rowMax = new Mat();
        Cv2.Reduce(gray, rowMax, ReduceDimension.Column, ReduceTypes.Max, MatType.CV_8U);

        int h = rowMax.Rows, top = 0, bottom = 0;
        while (top < h && rowMax.At<byte>(top, 0) <= BarMaxBrightness) top++;
        while (bottom < h - top && rowMax.At<byte>(h - 1 - bottom, 0) <= BarMaxBrightness) bottom++;
        return (top, bottom);
    }

    // Full height, horizontally centered. For captures wider than 16:9 (ultrawide) the content
    // width is reported as the 16:9 frame; center and height (all regions depend on) are unaffected.
    private static GameViewport Pillarbox(int width, int height)
    {
        int contentWidth = Math.Min(width, (int)Math.Round(height * GameAspectRatio));
        return new GameViewport((width - contentWidth) / 2, 0, contentWidth, height);
    }

    public static double GetScale(GameViewport viewport)
    {
        return (double)viewport.Height / ReferenceHeight;
    }
}
