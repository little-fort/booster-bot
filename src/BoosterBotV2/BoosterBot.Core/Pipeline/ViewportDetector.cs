using BoosterBot.Core.Models;
using OpenCvSharp;

namespace BoosterBot.Core.Pipeline;

public static class ViewportDetector
{
    public const int ReferenceHeight = 1080;

    // Marvel Snap renders at 16:9 on PC. On non-16:9 displays,
    // the game letterboxes (top/bottom) or pillarboxes (left/right).
    private const double GameAspectRatio = 16.0 / 9.0;
    private const double AspectTolerance = 0.02;

    public static GameViewport Detect(Mat screenshot)
    {
        double captureAspect = (double)screenshot.Width / screenshot.Height;

        int contentWidth, contentHeight, left, top;

        if (captureAspect > GameAspectRatio + AspectTolerance)
        {
            // Wider than 16:9 (e.g., ultrawide) → pillarbox
            contentHeight = screenshot.Height;
            contentWidth = (int)Math.Round(contentHeight * GameAspectRatio);
            left = (screenshot.Width - contentWidth) / 2;
            top = 0;
        }
        else if (captureAspect < GameAspectRatio - AspectTolerance)
        {
            // Taller than 16:9 (e.g., 16:10) → letterbox
            contentWidth = screenshot.Width;
            contentHeight = (int)Math.Round(contentWidth / GameAspectRatio);
            left = 0;
            top = (screenshot.Height - contentHeight) / 2;
        }
        else
        {
            // Already 16:9 — game fills the entire capture
            left = 0;
            top = 0;
            contentWidth = screenshot.Width;
            contentHeight = screenshot.Height;
        }

        return new GameViewport(left, top, contentWidth, contentHeight);
    }

    public static double GetScale(GameViewport viewport)
    {
        return (double)viewport.Height / ReferenceHeight;
    }
}
