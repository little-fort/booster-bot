using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace BoosterBot
{
    internal class BotConfig
    {
        public const string DefaultImageLocation = @"screens\screen.png";
        private readonly double _scaling;
        private readonly bool _verbose;
        private readonly bool _autoplay;
        private readonly bool _saveScreens;
        private readonly string _logPath;

        public double Scaling { get => _scaling; }
        public bool Verbose { get => _verbose; }
        public bool Autoplay { get => _autoplay; }
        public bool SaveScreens { get => _saveScreens; }
        public string LogPath { get => _logPath; }
        public int Center { get; set; }
        public Rect Window { get; set; }
        public Dimension Screencap { get; set; }
        private Point BaseResetPointLeft { get; set; }
        private Point BaseResetPointRight { get; set; }
        public Point ResetPoint 
        {
            get
            {
                var rand = new Random();
                var points = new Point[2];
                points[0] = new Point
                {
                    X = BaseResetPointLeft.X + rand.Next(100),
                    Y = BaseResetPointLeft.Y - rand.Next(500)
                };
                points[1] = new Point
                {
                    X = BaseResetPointRight.X - rand.Next(100),
                    Y = BaseResetPointRight.Y - rand.Next(500)
                };

                return points[rand.Next(2)];
            }
        }
        public Point ClearErrorPoint { get; set; }
        public Point GameModesPoint { get; set; }
        private Point BaseConquestBannerPoint { get; set; }
        public Point ConquestBannerPoint
        {
            get
            {
                var rand = new Random();
                return new Point
                {
                    X = BaseConquestBannerPoint.X + rand.Next(-50, 50),
                    Y = BaseConquestBannerPoint.Y + rand.Next(-50, 50)
                };
            }
        }
        public List<Point> Cards { get; set; }
        public List<Point> Locations { get; set; }

        public BotConfig(double scaling, bool verbose, bool autoplay, bool saveScreens, string logPath)
        {
            _scaling = scaling;
            _verbose = verbose;
            _autoplay = autoplay;
            _saveScreens = saveScreens;
            _logPath = logPath;
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

            BaseResetPointLeft = new Point
            {
                X = Window.Left + 100,
                Y = Window.Bottom - 200
            };

            BaseResetPointRight = new Point
            {
                X = Window.Right - 100,
                Y = Window.Bottom - 200
            };

            ClearErrorPoint = new Point
            {
                X = Window.Left + Center,
                Y = Window.Bottom - 110
            };

            GameModesPoint = new Point
            {
                X = Window.Left + Center + 175,
                Y = Window.Bottom - 50
            };

            BaseConquestBannerPoint = new Point
            {
                X = Window.Left + Center,
                Y = 330
            };
        }
    }
}
