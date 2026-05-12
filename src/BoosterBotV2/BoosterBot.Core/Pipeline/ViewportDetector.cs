using BoosterBot.Core.Models;
using OpenCvSharp;

namespace BoosterBot.Core.Pipeline;

public static class ViewportDetector
{
    public const int ReferenceHeight = 1080;

    private const int BrightnessThreshold = 15;
    private const int ScanBandHeight = 40;

    public static GameViewport Detect(Mat screenshot)
    {
        using var gray = ImageProcessor.ConvertToGrayPublic(screenshot);

        int left = FindLeftEdge(gray);
        int right = FindRightEdge(gray);
        int top = FindTopEdge(gray, left, right);
        int bottom = FindBottomEdge(gray, left, right);

        if (right - left < 100 || bottom - top < 100)
            return new GameViewport(0, 0, screenshot.Width, screenshot.Height);

        return new GameViewport(left, top, right - left, bottom - top);
    }

    public static double GetScale(GameViewport viewport)
    {
        return (double)viewport.Height / ReferenceHeight;
    }

    private static int FindLeftEdge(Mat gray)
    {
        int midY = gray.Height / 2;
        int bandTop = Math.Max(0, midY - ScanBandHeight / 2);
        int bandBottom = Math.Min(gray.Height, midY + ScanBandHeight / 2);

        for (int x = 0; x < gray.Width / 2; x++)
        {
            double avg = ColumnAverage(gray, x, bandTop, bandBottom);
            if (avg > BrightnessThreshold)
                return x;
        }
        return 0;
    }

    private static int FindRightEdge(Mat gray)
    {
        int midY = gray.Height / 2;
        int bandTop = Math.Max(0, midY - ScanBandHeight / 2);
        int bandBottom = Math.Min(gray.Height, midY + ScanBandHeight / 2);

        for (int x = gray.Width - 1; x >= gray.Width / 2; x--)
        {
            double avg = ColumnAverage(gray, x, bandTop, bandBottom);
            if (avg > BrightnessThreshold)
                return x + 1;
        }
        return gray.Width;
    }

    private static int FindTopEdge(Mat gray, int left, int right)
    {
        int midX = (left + right) / 2;
        int bandLeft = Math.Max(left, midX - ScanBandHeight / 2);
        int bandRight = Math.Min(right, midX + ScanBandHeight / 2);

        for (int y = 0; y < gray.Height / 2; y++)
        {
            double avg = RowAverage(gray, y, bandLeft, bandRight);
            if (avg > BrightnessThreshold)
                return y;
        }
        return 0;
    }

    private static int FindBottomEdge(Mat gray, int left, int right)
    {
        int midX = (left + right) / 2;
        int bandLeft = Math.Max(left, midX - ScanBandHeight / 2);
        int bandRight = Math.Min(right, midX + ScanBandHeight / 2);

        for (int y = gray.Height - 1; y >= gray.Height / 2; y--)
        {
            double avg = RowAverage(gray, y, bandLeft, bandRight);
            if (avg > BrightnessThreshold)
                return y + 1;
        }
        return gray.Height;
    }

    private static double ColumnAverage(Mat gray, int x, int yStart, int yEnd)
    {
        double sum = 0;
        int count = 0;
        for (int y = yStart; y < yEnd; y++)
        {
            sum += gray.At<byte>(y, x);
            count++;
        }
        return count > 0 ? sum / count : 0;
    }

    private static double RowAverage(Mat gray, int y, int xStart, int xEnd)
    {
        double sum = 0;
        int count = 0;
        for (int x = xStart; x < xEnd; x++)
        {
            sum += gray.At<byte>(y, x);
            count++;
        }
        return count > 0 ? sum / count : 0;
    }
}
