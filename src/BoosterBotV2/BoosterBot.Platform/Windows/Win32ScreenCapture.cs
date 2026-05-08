using System.Drawing;
using System.Drawing.Imaging;
using BoosterBot.Core.Models;
using BoosterBot.Core.Pipeline;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace BoosterBot.Platform.Windows;

public class Win32ScreenCapture : IScreenCapture
{
    private readonly Win32WindowManager _windowManager;
    private readonly double _scaling;

    public Win32ScreenCapture(Win32WindowManager windowManager, double scaling = 1.0)
    {
        _windowManager = windowManager;
        _scaling = scaling;
    }

    public ScreenCaptureContext Capture()
    {
        var windowRect = _windowManager.GetWindowPosition();
        var (bitmap, dimensions) = CaptureGameWindow(windowRect);

        var mat = BitmapConverter.ToMat(bitmap);
        bitmap.Dispose();

        return new ScreenCaptureContext(mat, dimensions, windowRect);
    }

    private (Bitmap bitmap, Dimension dimensions) CaptureGameWindow(Core.Models.Rect windowRect)
    {
        int width = (int)((windowRect.Right - windowRect.Left) * _scaling);
        int height = (int)((windowRect.Bottom - windowRect.Top) * _scaling);

        if (width <= 0 || height <= 0)
            throw new InvalidOperationException("Invalid window dimensions");

        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

        using (var graphics = Graphics.FromImage(bitmap))
        {
            var hdc = graphics.GetHdc();
            try
            {
                NativeMethods.PrintWindow(
                    _windowManager.FindGameWindow()?.Handle ?? throw new InvalidOperationException("Game window not found"),
                    hdc,
                    NativeMethods.PW_RENDERFULLCONTENT);
            }
            finally
            {
                graphics.ReleaseHdc(hdc);
            }
        }

        var dimensions = new Dimension { Width = width, Height = height };
        return (bitmap, dimensions);
    }
}
