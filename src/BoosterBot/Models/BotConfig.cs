using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoosterBot.Models
{
    internal class BotConfig
    {
        public const string DefaultImage = "screen.png";
        private readonly double _scaling;
        private readonly bool _verbose;
        private readonly bool _autoplay;
        private readonly bool _saveScreens;

        public double Scaling { get => _scaling; }
        public bool Verbose { get => _verbose; }
        public bool Autoplay { get => _autoplay; }
        public bool SaveScreens { get => _saveScreens; }
        public int Center { get; set; }
        public Rect Window { get; set; }
        public Dimension Screencap { get; set; }
        public Point ResetPoint { get; set; }
        public Point ClearErrorPoint { get; set; }
        public List<Point> Cards { get; set; }
        public List<Point> Locations { get; set; }

        public BotConfig(double scaling, bool verbose, bool autoplay, bool saveScreens)
        {
            _scaling = scaling;
            _verbose = verbose;
            _autoplay = autoplay;
            _saveScreens = saveScreens;
        }

        public void GetWindowPositions()
        {
            // Find game window and take screencap:
            Window = SystemUtilities.GetGameWindowLocation();
            Screencap = SystemUtilities.GetGameScreencap(Window, Scaling);

            // Calculate center position of game window:
            Center = Screencap.Width / 2;

            // Update card and location coordinates:
            Cards = new List<Point>
            {
                new Point
                {
                    X = Window.Left + Center + 20,
                    Y = Window.Bottom - 180
                },
                new Point
                {
                    X = Window.Left + Center - 88,
                    Y = Window.Bottom - 200
                },
                new Point
                {
                    X = Window.Left + Center + 118,
                    Y = Window.Bottom - 195
                },
                new Point
                {
                    X = Window.Left + Center - 183,
                    Y = Window.Bottom - 180
                }
            };

            Locations = new List<Point>()
            {
                new Point
                {
                    X = Window.Left + Center - 170,
                    Y = Window.Bottom - 340
                },
                new Point
                {
                    X = Window.Left + Center + 20,
                    Y = Window.Bottom - 340
                },
                new Point
                {
                    X = Window.Left + Center + 200,
                    Y = Window.Bottom - 340
                }
            };

            ResetPoint = new Point
            {
                X = Window.Left + 100,
                Y = Window.Bottom - 200
            };

            ClearErrorPoint = new Point
            {
                X = Window.Left + Center,
                Y = Window.Bottom - 110
            };
        }
    }
}
