using System.Drawing;
using System.Text;
using System.Security.Cryptography;
using OpenCvSharp;
using BoosterBot.Models;

namespace BoosterBot;

internal class ImageUtilities
{
    #region Image Comparison

    /// <summary>
    /// A method to compare two images and return a similarity score based on how close they are to each other.
    /// </summary>
    /// <param name="area">The area that should be cropped from the target image.</param>
    /// <param name="refImage">The reference image that should be used for comparison.</param>
    /// <param name="targetScore">The threshold of similarity that must be met to return true.</param>
    /// <param name="image">The target image that is being evaluated.</param>
    /// <returns>A tuple containing a boolean indicating if the images are similar enough and a list of logs for debugging.</returns>
    public static IdentificationResult CheckImageAreaSimilarity(Rect area, string refImage, double targetScore = 0.95, string image = BotConfig.DefaultImageLocation)
    {
        var logs = new List<string>();

        // Get base crop
        var rect = new Rectangle() { X = area.Left, Y = area.Top, Width = area.Right - area.Left, Height = area.Bottom - area.Top };
        var crop = SaveImage(rect, image, refImage.Split('\\')[^1].Replace("-preproc", ""));

        // Preprocess image
        var preprocImagePath = crop.Replace(".png", "-preproc.png");
        var preprocImage = PreprocessImage(crop);
        preprocImage.ImWrite(preprocImagePath);

        // Compare pre-processed version against base
        logs.Add($"   CROP:    {Path.GetFileName(crop)}");
        logs.Add($"   PROC:    {Path.GetFileName(preprocImagePath)}");

        var similarity = CalculateImageSimilarity(refImage, preprocImagePath);

        logs.Add($"   SIMILAR: {Math.Round(similarity * 100, 1)}%");
        logs.Add($"   TARGET:  {targetScore * 100}%");
        logs.Add($"   RESULT:  {similarity >= targetScore}");

        // Small throttle to prevent blocks from too many calls in quick succession
        Thread.Sleep(250);

        return new IdentificationResult(similarity >= targetScore, logs);
    }


    /// <summary>
    /// A method to preprocess the cropped image so that the Tesseract OCR will return more consistent results.
    /// </summary>
    public static Mat PreprocessImage(string imagePath)
    {
        // Load the image
        Mat src = Cv2.ImRead(imagePath, ImreadModes.Color);

        // Convert the image to grayscale
        Mat gray = new Mat();
        Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

        // Apply Gaussian blur to reduce noise
        Mat blurred = new Mat();
        Cv2.GaussianBlur(gray, blurred, new OpenCvSharp.Size(5, 5), 0);

        // Apply adaptive thresholding to enhance contrast
        Mat thresh = new Mat();
        Cv2.AdaptiveThreshold(blurred, thresh, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 11, 2);

        return thresh;
    }

    private static string SaveImage(Rectangle crop, string image, string? name = null)
    {
        var destRect = new Rectangle(System.Drawing.Point.Empty, crop.Size);
        var cropImage = new Bitmap(destRect.Width, destRect.Height);
        using var graphics = Graphics.FromImage(cropImage);
        using var bitmap = new Bitmap(image);
        graphics.DrawImage(bitmap, destRect, crop, GraphicsUnit.Pixel);

        string file = @"screens\";
        file += (string.IsNullOrWhiteSpace(name)) ? $"snapcap-{DateTime.Now.ToString("yyyyMMddHHmmss")}.png" : $"{name}";
        cropImage.Save(file, System.Drawing.Imaging.ImageFormat.Png);

        return file;
    }

    public static double CalculateImageSimilarity(string image1, string image2)
    {
        using var bmp1 = new Bitmap(image1);
        using var bmp2 = new Bitmap(image2);
        return CalculateImageSimilarity(bmp1, bmp2);
    }

    public static double CalculateImageSimilarity(Bitmap image1, Bitmap image2)
    {
        int identicalPixelCount = 0;
        int totalPixels = image1.Width * image1.Height;

        for (int x = 0; x < image1.Width; x++)
        {
            for (int y = 0; y < image1.Height; y++)
            {
                if (image1.GetPixel(x, y).Equals(image2.GetPixel(x, y)))
                {
                    identicalPixelCount++;
                }
            }
        }

        return (double)identicalPixelCount / totalPixels;
    }

    public static string ComputeBitmapHash(Bitmap bitmap)
    {
        using MemoryStream stream = new MemoryStream();
        bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        var bytes = stream.ToArray();

        using SHA256 sha256Hash = SHA256.Create();
        byte[] data = sha256Hash.ComputeHash(bytes);

        var sb = new StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            sb.Append(data[i].ToString("x2"));
        }
        return sb.ToString();
    }

    #endregion

    #region OCR

    /// <summary>
    /// Takes a crop with the given dimensions from the specified image and saves it as an image to the disk. Used for debugging.
    /// </summary>
    private static void SaveImage(Rectangle crop, string image = BotConfig.DefaultImageLocation)
    {
        var destRect = new Rectangle(System.Drawing.Point.Empty, crop.Size);
        var cropImage = new Bitmap(destRect.Width, destRect.Height);
        using var graphics = Graphics.FromImage(cropImage);
        using var bitmap = new Bitmap(image);
        graphics.DrawImage(bitmap, destRect, crop, GraphicsUnit.Pixel);

        cropImage.Save(@"screens//snapcap-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png", System.Drawing.Imaging.ImageFormat.Png);
    }

    /// <summary>
    /// Takes in an expected value and compares it to what OCR returned to generate a similarity score based on how close the values are. Calculation is case-insensitive.
    /// </summary>
    private static double CalculateStringSimilarity(string expected, string ocr)
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
    private static int LevenshteinDistance(string source, string target)
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

    #endregion
}
