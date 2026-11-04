using BoosterBot.Core.Models;
using BoosterBot.Core.Pipeline;
using BoosterBot.Platform.Windows;
using OpenCvSharp;

namespace BoosterBot.Desktop;

/// <summary>
/// Developer CLI for driving the game and maintaining reference images.
///
/// All coordinates are "reference coordinates": pixels in a 1920x1080 frame laid over the
/// game's 16:9 viewport (see <see cref="ViewportDetector"/>). The same numbers address the
/// same UI element at any window size, so values read off a 1080p screenshot can be fed
/// straight back into click/snip commands.
///
/// Commands can be chained with a standalone "+":  dev click 960 880 + wait 1500 + capture shot.png
/// </summary>
internal static class DevCommands
{
    private const string Usage = """
        BoosterBot dev tools. Coordinates are 1080p reference pixels over the game's 16:9 viewport.
        Chain commands with " + ". Commands that read a screenshot capture the live game unless --file is given.

          capture <out.png>                              Save a live capture of the game window
          annotate <out.png> [--file f]                  Draw every catalog crop region onto a screenshot
          snip <x> <y> <w> <h> <out.png> [--file f] [--zoom n] [--regions]
                                                         Zoomed crop with a 10px reference grid
          inspect <key> <out.png> [--file f] [--ref-dir d]
                                                         Padded crop | preprocessed | reference, with best match boxed
          test [--file f] [--key k] [--ref-dir d]        Score references against a screenshot
          eval <manifest.txt> [--ref-dir d]              Score every reference against a labeled screenshot set
          save-ref <key> --ref-dir d [--file f] [--culture c] [--dx n] [--dy n]
                                                         Regenerate a reference from a screenshot (dx/dy shift the
                                                         crop, in reference px, when the element sits off its region)
          click <x> <y>                                  Click at reference coordinates
          drag <x1> <y1> <x2> <y2>                       Drag between reference coordinates
          hold <x> <y> <ms>                              Press and hold (e.g., "HOLD TO CONFIRM" buttons)
          key <escape|enter|space>                       Send a key to the game
          wait <ms>                                      Pause between chained commands

        --ref-dir is the folder that contains the culture folders (en/, zh/).
        """;

    private static readonly Win32WindowManager WindowManager = new();
    private static readonly Win32ScreenCapture ScreenCapture = new(WindowManager);
    private static readonly Win32InputSimulator Input = new(WindowManager);

    public static int Run(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine(Usage);
            return 1;
        }

        // Split "a b + c d" into ["a","b"], ["c","d"] and run each in order
        var commands = new List<string[]>();
        var current = new List<string>();
        foreach (var arg in args)
        {
            if (arg == "+") { commands.Add([.. current]); current.Clear(); }
            else current.Add(arg);
        }
        commands.Add([.. current]);

        foreach (var command in commands)
        {
            try
            {
                Execute(command);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in '{string.Join(' ', command)}': {ex.Message}");
                return 1;
            }
        }
        return 0;
    }

    private static void Execute(string[] cmd)
    {
        var a = new ArgReader(cmd);
        switch (a.Positional(0))
        {
            case "capture": Capture(a.Positional(1)); break;
            case "annotate": Annotate(a.Positional(1), a.Option("--file")); break;
            case "snip":
                Snip(a.Int(1), a.Int(2), a.Int(3), a.Int(4), a.Positional(5), a.Option("--file"),
                    int.Parse(a.Option("--zoom") ?? "4"), a.Flag("--regions"));
                break;
            case "inspect": Inspect(a.Positional(1), a.Positional(2), a.Option("--file"), a.Option("--ref-dir")); break;
            case "test": Test(a.Option("--file"), a.Option("--key"), a.Option("--ref-dir")); break;
            case "eval": Eval(a.Positional(1), a.Option("--ref-dir")); break;
            case "save-ref":
                SaveRef(a.Positional(1), a.Option("--ref-dir") ?? throw new ArgumentException("save-ref requires --ref-dir"),
                    a.Option("--file"), a.Option("--culture") ?? "en",
                    int.Parse(a.Option("--dx") ?? "0"), int.Parse(a.Option("--dy") ?? "0"));
                break;
            case "click": Click(a.Int(1), a.Int(2)); break;
            case "drag": Drag(a.Int(1), a.Int(2), a.Int(3), a.Int(4)); break;
            case "hold": Hold(a.Int(1), a.Int(2), a.Int(3)); break;
            case "key": SendKey(a.Positional(1)); break;
            case "wait": Thread.Sleep(a.Int(1)); break;
            default: Console.WriteLine(Usage); throw new ArgumentException($"Unknown command '{a.Positional(0)}'");
        }
    }

    #region Screenshots

    private static void Capture(string outPath)
    {
        using var scene = LoadScene(null);
        WriteImage(outPath, scene);
        var vp = ViewportDetector.Detect(scene);
        Console.WriteLine($"Captured {scene.Width}x{scene.Height} (viewport L={vp.Left} T={vp.Top} {vp.Width}x{vp.Height}) -> {outPath}");
    }

    private static void Annotate(string outPath, string? file)
    {
        using var scene = LoadScene(file);
        var vp = ViewportDetector.Detect(scene);
        var s = ViewportDetector.GetScale(vp);

        // Several references share a region; draw each region once, labeled by its ComponentMappings method
        foreach (var group in ReferenceCatalog.All.GroupBy(d => d.GetRegion.Method.Name))
        {
            var exact = group.First().GetRegion(vp);
            var padded = group.First().GetSearchRegion(vp);
            Cv2.Rectangle(scene, ToCv(padded), new Scalar(0, 255, 255), 1);
            Cv2.Rectangle(scene, ToCv(exact), new Scalar(0, 255, 0), 2);
            Cv2.PutText(scene, group.Key.Replace("Get", ""), new Point(exact.Left, exact.Top - 4),
                HersheyFonts.HersheySimplex, 0.4 * Math.Max(1, s), new Scalar(0, 255, 0), 1, LineTypes.AntiAlias);
        }

        WriteImage(outPath, scene);
        Console.WriteLine($"Annotated -> {outPath}");
    }

    private static void Snip(int x, int y, int w, int h, string outPath, string? file, int zoom, bool showRegions)
    {
        using var scene = LoadScene(file);
        var vp = ViewportDetector.Detect(scene);
        var s = ViewportDetector.GetScale(vp);

        using var crop = ImageProcessor.CropRegion(scene, RefToImage(vp, s, x, y, w, h));
        using var zoomed = new Mat();
        Cv2.Resize(crop, zoomed, new Size(w * zoom, h * zoom), interpolation: InterpolationFlags.Nearest);

        // Grid lines every 10 reference px (labels every 50) so coordinates can be read off the image
        int firstX = (int)Math.Ceiling(x / 10.0) * 10, firstY = (int)Math.Ceiling(y / 10.0) * 10;
        for (int gx = firstX; gx < x + w; gx += 10)
        {
            bool major = gx % 50 == 0;
            int px = (gx - x) * zoom;
            Cv2.Line(zoomed, new Point(px, 0), new Point(px, zoomed.Height), major ? new Scalar(0, 0, 255) : new Scalar(80, 80, 80), 1);
            if (major) Cv2.PutText(zoomed, gx.ToString(), new Point(px + 2, 12), HersheyFonts.HersheySimplex, 0.4, new Scalar(0, 0, 255), 1);
        }
        for (int gy = firstY; gy < y + h; gy += 10)
        {
            bool major = gy % 50 == 0;
            int py = (gy - y) * zoom;
            Cv2.Line(zoomed, new Point(0, py), new Point(zoomed.Width, py), major ? new Scalar(0, 0, 255) : new Scalar(80, 80, 80), 1);
            if (major) Cv2.PutText(zoomed, gy.ToString(), new Point(2, py - 2), HersheyFonts.HersheySimplex, 0.4, new Scalar(0, 0, 255), 1);
        }

        // Optionally outline catalog regions (converted back to reference coordinates)
        if (showRegions)
        {
            foreach (var group in ReferenceCatalog.All.GroupBy(d => d.GetRegion.Method.Name))
            {
                var r = group.First().GetRegion(vp);
                var rx = (int)Math.Round((r.Left - vp.CenterX) / s) + RefCenterX - x;
                var ry = (int)Math.Round((r.Top - vp.Top) / s) - y;
                var rw = (int)Math.Round(r.Width / s);
                var rh = (int)Math.Round(r.Height / s);
                Cv2.Rectangle(zoomed, new OpenCvSharp.Rect(rx * zoom, ry * zoom, rw * zoom, rh * zoom), new Scalar(0, 255, 0), 2);
                Cv2.PutText(zoomed, group.Key.Replace("Get", ""), new Point(rx * zoom + 2, ry * zoom - 4),
                    HersheyFonts.HersheySimplex, 0.45, new Scalar(0, 255, 0), 1);
            }
        }

        WriteImage(outPath, zoomed);
        Console.WriteLine($"Snip ({x},{y}) {w}x{h} @ {zoom}x -> {outPath}");
    }

    #endregion

    #region References

    private static void Test(string? file, string? key, string? refDir)
    {
        LoadReferences(refDir);
        using var scene = LoadScene(file);
        var vp = ViewportDetector.Detect(scene);
        var s = ViewportDetector.GetScale(vp);

        var defs = key is null
            ? ReferenceCatalog.All
            : [ReferenceCatalog.Find(key) ?? throw new ArgumentException($"Unknown reference '{key}'")];

        Console.WriteLine($"Scene {scene.Width}x{scene.Height}, viewport L={vp.Left} T={vp.Top} {vp.Width}x{vp.Height}, scale {s:F3}\n");
        Console.WriteLine($"{"Reference",-44} {"Score",7} {"Limit",6}  {"dx",3} {"dy",3}  {"Ref",-3}  Region");

        var rows = new List<(ReferenceDefinition Def, double Score, int Dx, int Dy, string Kind)>();
        foreach (var def in defs)
        {
            var (score, dx, dy) = Score(scene, vp, s, def);
            rows.Add((def, score, dx, dy, ReferenceKind(def.Key)));
        }

        foreach (var r in rows.OrderByDescending(r => r.Score))
        {
            var mark = r.Score >= r.Def.Threshold ? "*" : " ";
            Console.WriteLine($"{mark}{r.Def.Key,-43} {r.Score,7:P1} {r.Def.Threshold,6:P0}  {r.Dx,3} {r.Dy,3}  {r.Kind,-3}  {r.Def.GetRegion.Method.Name}");
        }
        Console.WriteLine("\n* = at/above threshold. dx/dy = best-match offset from the region's expected spot (reference px).");
        Console.WriteLine("Ref: V2 = grayscale (regenerated), V1 = binary placeholder (will not match).");
    }

    /// <summary>
    /// Scores every reference against a labeled set of screenshots to show how well each one
    /// separates "element present" from "element absent". The manifest has one line per screenshot:
    ///   path/to/screenshot.png: key-a key-b ?key-c   (keys without "-preproc"; paths relative to the manifest)
    /// Listed keys are present. A "?" prefix means "don't care" (e.g., a Ladder reference matching
    /// a Conquest screen, which never matters because each bot mode only checks its own references).
    /// Every other key is treated as absent from that screenshot.
    /// </summary>
    private static void Eval(string manifestPath, string? refDir)
    {
        LoadReferences(refDir);
        var baseDir = Path.GetDirectoryName(Path.GetFullPath(manifestPath))!;
        static string Normalize(string k) => k.EndsWith("-preproc") ? k : k + "-preproc";

        var samples = new List<(string Name, HashSet<string> Present, HashSet<string> Ignored)>();
        foreach (var raw in File.ReadAllLines(manifestPath))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;
            int colon = line.IndexOf(':');
            var tokens = line[(colon + 1)..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            samples.Add((line[..colon].Trim(),
                [.. tokens.Where(t => !t.StartsWith('?')).Select(Normalize)],
                [.. tokens.Where(t => t.StartsWith('?')).Select(t => Normalize(t[1..]))]));
        }

        foreach (var key in samples.SelectMany(s => s.Present.Concat(s.Ignored)).Distinct())
            if (ReferenceCatalog.Find(key) is null)
                throw new ArgumentException($"Manifest references unknown key '{key}'");

        // Score every (screenshot, reference) pair once
        var scores = new Dictionary<(string Sample, string Key), double>();
        foreach (var (name, _, _) in samples)
        {
            using var scene = LoadScene(Path.Combine(baseDir, name));
            var vp = ViewportDetector.Detect(scene);
            var s = ViewportDetector.GetScale(vp);
            foreach (var def in ReferenceCatalog.All)
                scores[(name, def.Key)] = Score(scene, vp, s, def).Score;
        }

        Console.WriteLine($"{samples.Count} screenshots. pos = lowest score where present; neg = highest score where absent.\n");
        Console.WriteLine($"{"Reference",-40} {"Ref",-3} {"#pos",4} {"pos min",8} {"neg max",8} {"margin",7} {"limit",6}  Result   Worst negative");
        int failures = 0;
        foreach (var def in ReferenceCatalog.All)
        {
            var pos = samples.Where(x => x.Present.Contains(def.Key)).Select(x => (x.Name, Score: scores[(x.Name, def.Key)])).ToList();
            var neg = samples.Where(x => !x.Present.Contains(def.Key) && !x.Ignored.Contains(def.Key))
                .Select(x => (x.Name, Score: scores[(x.Name, def.Key)])).ToList();
            var name = def.Key.Replace("-preproc", "");

            // Both sets are needed to judge separation; report gaps instead of guessing
            if (pos.Count == 0 || neg.Count == 0)
            {
                var gap = pos.Count == 0 ? "no positive samples" : "no negative samples";
                var negText = neg.Count == 0 ? "-" : neg.Max(x => x.Score).ToString("P1");
                var posText = pos.Count == 0 ? "-" : pos.Min(x => x.Score).ToString("P1");
                Console.WriteLine($"{name,-40} {ReferenceKind(def.Key),-3} {pos.Count,4} {posText,8} {negText,8} {"-",7} {def.Threshold,6:P0}  {gap}");
                continue;
            }

            var negMax = neg.MaxBy(x => x.Score);
            var posMin = pos.MinBy(x => x.Score);
            bool ok = posMin.Score >= def.Threshold && negMax.Score < def.Threshold;
            if (!ok) failures++;
            Console.WriteLine($"{name,-40} {ReferenceKind(def.Key),-3} {pos.Count,4} {posMin.Score,8:P1} {negMax.Score,8:P1} {posMin.Score - negMax.Score,7:P1} {def.Threshold,6:P0}  {(ok ? "OK" : "FAIL"),-6}   {Path.GetFileNameWithoutExtension(negMax.Name)}"
                + (posMin.Score < def.Threshold ? $"  (weakest positive: {Path.GetFileNameWithoutExtension(posMin.Name)})" : ""));
        }

        // Non-zero exit so scripts (and chained commands) can detect a regression
        if (failures > 0)
            throw new InvalidOperationException($"{failures} reference(s) failed evaluation");
    }

    private static void Inspect(string key, string outPath, string? file, string? refDir)
    {
        LoadReferences(refDir);
        var def = ReferenceCatalog.Find(key) ?? throw new ArgumentException($"Unknown reference '{key}'");

        using var scene = LoadScene(file);
        var vp = ViewportDetector.Detect(scene);
        var s = ViewportDetector.GetScale(vp);

        using var crop = ImageProcessor.CropRegion(scene, def.GetSearchRegion(vp));
        using var pre = ImageProcessor.Preprocess(crop, s);
        var reference = ImageProcessor.GetReferenceImage(def.Key);
        var (score, loc) = ImageProcessor.FindBestMatch(pre, reference);

        // Panels at a common 1080p scale, enlarged 3x for viewing
        const int z = 3;
        using var raw = new Mat();
        Cv2.Resize(crop, raw, new Size(pre.Width * z, pre.Height * z), interpolation: InterpolationFlags.Nearest);
        using var preBgr = ToBgr(pre);
        Cv2.Rectangle(preBgr, new OpenCvSharp.Rect(loc.X, loc.Y, reference.Width, reference.Height), new Scalar(0, 255, 0), 1);
        using var preZ = new Mat();
        Cv2.Resize(preBgr, preZ, new Size(pre.Width * z, pre.Height * z), interpolation: InterpolationFlags.Nearest);
        using var refBgr = ToBgr(reference);
        using var refZ = new Mat();
        Cv2.Resize(refBgr, refZ, new Size(reference.Width * z, reference.Height * z), interpolation: InterpolationFlags.Nearest);

        int width = Math.Max(raw.Width, refZ.Width) + 20;
        int height = raw.Height + preZ.Height + refZ.Height + 80;
        using var canvas = new Mat(height, width, MatType.CV_8UC3, new Scalar(40, 40, 40));
        int yPos = 0;
        foreach (var (panel, label) in new[] { (raw, "padded crop"), (preZ, $"preprocessed (best match boxed, {score:P1})"), (refZ, "reference") })
        {
            Cv2.PutText(canvas, label, new Point(10, yPos + 18), HersheyFonts.HersheySimplex, 0.5, new Scalar(255, 255, 255), 1);
            using (var roi = new Mat(canvas, new OpenCvSharp.Rect(10, yPos + 24, panel.Width, panel.Height)))
                panel.CopyTo(roi);
            yPos += panel.Height + 26;
        }

        WriteImage(outPath, canvas);
        Console.WriteLine($"{def.Key}: {score:P2} (match at {loc.X},{loc.Y}; expected ~{def.PadX},{def.PadY}) -> {outPath}");
    }

    private static void SaveRef(string key, string refDir, string? file, string culture, int dx, int dy)
    {
        var def = ReferenceCatalog.Find(key) ?? throw new ArgumentException($"Unknown reference '{key}'");

        using var scene = LoadScene(file);
        var vp = ViewportDetector.Detect(scene);
        var s = ViewportDetector.GetScale(vp);

        // Optionally shift the region when the element sits off its usual spot
        int ox = (int)Math.Round(dx * s), oy = (int)Math.Round(dy * s);
        static Core.Models.Rect Shift(Core.Models.Rect r, int x, int y) => new() { Left = r.Left + x, Right = r.Right + x, Top = r.Top + y, Bottom = r.Bottom + y };
        var region = Shift(def.GetRegion(vp), ox, oy);
        var search = Shift(def.GetSearchRegion(vp), ox, oy);

        // Preprocess the whole search window, then cut the reference out of it. Blurring the exact
        // crop on its own would treat its edges differently than detection (which blurs the padded
        // window), lowering scores for faint elements.
        using var padded = ImageProcessor.CropRegion(scene, search);
        using var pre = ImageProcessor.Preprocess(padded, s);
        var inner = new OpenCvSharp.Rect(
            (int)Math.Round((region.Left - search.Left) / s), (int)Math.Round((region.Top - search.Top) / s),
            (int)Math.Round(region.Width / s), (int)Math.Round(region.Height / s));
        using var reference = new Mat(pre, inner).Clone();

        var path = Path.Combine(refDir, culture, def.Key + ".png");
        WriteImage(path, reference);

        // Sanity check: the new reference should match the scene it came from almost perfectly
        var score = ImageProcessor.GetMatchConfidence(pre, reference);
        Console.WriteLine($"Saved {path} ({reference.Width}x{reference.Height}), self-match {score:P2}");
    }

    private static (double Score, int Dx, int Dy) Score(Mat scene, GameViewport vp, double s, ReferenceDefinition def)
    {
        using var crop = ImageProcessor.CropRegion(scene, def.GetSearchRegion(vp));
        using var pre = ImageProcessor.Preprocess(crop, s);
        var (score, loc) = ImageProcessor.FindBestMatch(pre, ImageProcessor.GetReferenceImage(def.Key));
        return (score, loc.X - def.PadX, loc.Y - def.PadY);
    }

    // V1 placeholders are strictly black/white; regenerated V2 references contain gray levels
    private static string ReferenceKind(string key)
    {
        var reference = ImageProcessor.GetReferenceImage(key);
        using var midTones = new Mat();
        Cv2.InRange(reference, new Scalar(1), new Scalar(254), midTones);
        return Cv2.CountNonZero(midTones) == 0 ? "V1" : "V2";
    }

    // Reload on every command: a chained save-ref or a different --ref-dir must not be
    // scored against images cached by an earlier command
    private static void LoadReferences(string? refDir)
    {
        ImageProcessor.ClearReferenceCache();
        ImageProcessor.LoadReferenceImages(refDir ?? Path.Combine(AppContext.BaseDirectory, "Assets", "Reference"), "en");
    }

    private static void WriteImage(string path, Mat image)
    {
        if (!Cv2.ImWrite(path, image))
            throw new IOException($"Failed to write image: {path} (does the folder exist?)");
    }

    #endregion

    #region Input

    private static void Click(int x, int y)
    {
        var (sx, sy) = RefToScreen(x, y);
        Input.Click(sx, sy);
        Console.WriteLine($"Clicked ref ({x},{y}) -> screen ({sx},{sy})");
    }

    private static void Drag(int x1, int y1, int x2, int y2)
    {
        var (sx1, sy1) = RefToScreen(x1, y1);
        var (sx2, sy2) = RefToScreen(x2, y2);
        Input.DragAndDrop(sx1, sy1, sx2, sy2);
        Console.WriteLine($"Dragged ref ({x1},{y1}) -> ({x2},{y2})");
    }

    // A stationary "drag" keeps the button pressed for roughly the requested duration
    private static void Hold(int x, int y, int ms)
    {
        var (sx, sy) = RefToScreen(x, y);
        Input.DragAndDrop(sx, sy, sx, sy, new Core.Input.DragOptions(Steps: Math.Max(1, ms / 20), StepDelayMs: 20, AddJitter: false));
        Console.WriteLine($"Held ref ({x},{y}) for ~{ms}ms");
    }

    private static void SendKey(string name)
    {
        int vk = name.ToLowerInvariant() switch
        {
            "escape" or "esc" => 0x1B,
            "enter" => 0x0D,
            "space" => 0x20,
            _ => throw new ArgumentException($"Unsupported key '{name}'")
        };
        Input.SendKey(vk);
        Console.WriteLine($"Sent key {name}");
    }

    #endregion

    #region Helpers

    /// <summary>Loads a screenshot from disk, or captures the live game window. Always returns 3-channel BGR.</summary>
    private static Mat LoadScene(string? file)
    {
        if (file is not null)
        {
            var image = Cv2.ImRead(file, ImreadModes.Color);
            if (image.Empty()) throw new FileNotFoundException($"Could not load image: {file}");
            return image;
        }

        if (WindowManager.FindGameWindow() is null)
            throw new InvalidOperationException("Game window not found");

        using var ctx = ScreenCapture.Capture();
        return ToBgr(ctx.Screenshot);
    }

    // Reference x is measured from the 1920-wide frame's center (960), matching ComponentMappings,
    // so it stays correct when the viewport is narrower than 1920 reference px (portrait windows).
    private const int RefCenterX = 960;

    private static int RefXToImage(GameViewport vp, double s, int x) => vp.CenterX + (int)Math.Round((x - RefCenterX) * s);
    private static int RefYToImage(GameViewport vp, double s, int y) => vp.Top + (int)Math.Round(y * s);

    private static (int X, int Y) RefToScreen(int x, int y)
    {
        var window = WindowManager.FindGameWindow() ?? throw new InvalidOperationException("Game window not found");
        // Use a live capture: whether letterbox bars exist can only be told from the pixels
        using var scene = LoadScene(null);
        var vp = ViewportDetector.Detect(scene);
        var s = ViewportDetector.GetScale(vp);
        return (window.Bounds.Left + RefXToImage(vp, s, x), window.Bounds.Top + RefYToImage(vp, s, y));
    }

    private static Core.Models.Rect RefToImage(GameViewport vp, double s, int x, int y, int w, int h) => new()
    {
        Left = RefXToImage(vp, s, x),
        Top = RefYToImage(vp, s, y),
        Right = RefXToImage(vp, s, x + w),
        Bottom = RefYToImage(vp, s, y + h)
    };

    private static OpenCvSharp.Rect ToCv(Core.Models.Rect r) => new(r.Left, r.Top, r.Width, r.Height);

    private static Mat ToBgr(Mat source)
    {
        var bgr = new Mat();
        switch (source.Channels())
        {
            case 1: Cv2.CvtColor(source, bgr, ColorConversionCodes.GRAY2BGR); break;
            case 4: Cv2.CvtColor(source, bgr, ColorConversionCodes.BGRA2BGR); break;
            default: source.CopyTo(bgr); break;
        }
        return bgr;
    }

    /// <summary>Minimal reader: positional args, "--name value" options, and "--flag" switches.</summary>
    private sealed class ArgReader(string[] args)
    {
        private static readonly HashSet<string> Flags = ["--regions"];

        private readonly List<string> _positional = Collect(args);

        private static List<string> Collect(string[] args)
        {
            var list = new List<string>();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("--"))
                {
                    if (!Flags.Contains(args[i])) i++; // skip the option's value
                }
                else list.Add(args[i]);
            }
            return list;
        }

        public string Positional(int index) => index < _positional.Count
            ? _positional[index]
            : throw new ArgumentException($"Missing argument #{index}");

        public int Int(int index) => int.Parse(Positional(index));

        public string? Option(string name)
        {
            int i = Array.IndexOf(args, name);
            if (i < 0) return null;

            // A present-but-valueless option must not silently fall back to the default
            // (e.g., a trailing "--file" would otherwise capture the live game instead)
            if (i + 1 >= args.Length || args[i + 1].StartsWith("--"))
                throw new ArgumentException($"Option '{name}' requires a value");
            return args[i + 1];
        }

        public bool Flag(string name) => args.Contains(name);
    }

    #endregion
}
