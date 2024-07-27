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
                var xBound = (int)(100 * Scaling);
                var yBound = (int)(500 * Scaling);
                points[0] = new Point
                {
                    X = BaseResetPointLeft.X + rand.Next(xBound),
                    Y = BaseResetPointLeft.Y - rand.Next(yBound)
                };
                points[1] = new Point
                {
                    X = BaseResetPointRight.X - rand.Next(xBound),
                    Y = BaseResetPointRight.Y - rand.Next(yBound)
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
                var bound = (int)(50 * Scaling);
                return new Point
                {
                    X = BaseConquestBannerPoint.X + rand.Next(-bound, bound),
                    Y = BaseConquestBannerPoint.Y + rand.Next(-bound, bound)
                };
            }
        }
        private Point BaseSnapPoint { get; set; }
        public Point SnapPoint
        {
            get
            {
                var rand = new Random();
                var bound = (int)(20 * Scaling);
                return new Point
                {
                    X = BaseSnapPoint.X + rand.Next(-bound, bound),
                    Y = BaseSnapPoint.Y + rand.Next(-bound, bound)
                };
            }
        }
        private Point BasePlayPoint { get; set; }
        public Point PlayPoint
        {
            get
            {
                var rand = new Random();
                var bound = (int)(20 * Scaling);
                return new Point
                {
                    X = BasePlayPoint.X + rand.Next(-bound, bound),
                    Y = BasePlayPoint.Y + rand.Next(-bound, bound)
                };
            }
        }
        private Point BaseCancelPoint { get; set; }
        public Point CancelPoint
        {
            get
            {
                var rand = new Random();
                var bound = (int)(10 * Scaling);
                return new Point
                {
                    X = BaseCancelPoint.X + rand.Next(-bound, bound),
                    Y = BaseCancelPoint.Y + rand.Next(-bound, bound)
                };
            }
        }
        private Point BaseNextPoint { get; set; }
        public Point NextPoint
        {
            get
            {
                var rand = new Random();
                var xBound = (int)(20 * Scaling);
                var yBound = (int)(10 * Scaling);
                return new Point
                {
                    X = BaseNextPoint.X + rand.Next(-xBound, xBound),
                    Y = BaseNextPoint.Y + rand.Next(-yBound, yBound)
                };
            }
        }
        public Point RetreatPoint { get; set; }
        public Point RetreatConfirmPoint { get; set; }
		public Point ConcedePoint { get; set; }
		public Point ConcedeConfirmPoint { get; set; }
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

        public int Scale(int x) => (int)(Scaling * x);

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
                    X = Window.Left + Center + Scale(20),
                    Y = Window.Bottom - Scale(180)
                },
                new Point
                {
                    X = Window.Left + Center - Scale(88),
                    Y = Window.Bottom - Scale(200)
                },
                new Point
                {
                    X = Window.Left + Center + Scale(118),
                    Y = Window.Bottom - Scale(195)
                },
                new Point
                {
                    X = Window.Left + Center - Scale(183),
                    Y = Window.Bottom - Scale(180)
                }
            };

            Locations = new List<Point>()
            {
                new Point
                {
                    X = Window.Left + Center - Scale(170),
                    Y = Window.Bottom - Scale(340)
                },
                new Point
                {
                    X = Window.Left + Center + Scale(20),
                    Y = Window.Bottom - Scale(340)
                },
                new Point
                {
                    X = Window.Left + Center + Scale(200),
                    Y = Window.Bottom - Scale(340)
                }
            };

            BaseResetPointLeft = new Point
            {
                X = Window.Left + Scale(100),
                Y = Window.Bottom - Scale(200)
            };

            BaseResetPointRight = new Point
            {
                X = Window.Right - Scale(100),
                Y = Window.Bottom - Scale(200)
            };

            ClearErrorPoint = new Point
            {
                X = Window.Left + Center,
                Y = Window.Bottom - Scale(50)
            };

            GameModesPoint = new Point
            {
                X = Window.Left + Center + Scale(175),
                Y = Window.Bottom - Scale(50)
            };

            BaseConquestBannerPoint = new Point
            {
                X = Window.Left + Center,
                Y = Window.Top + Scale(540) // Scale(330) // Temporary adjustment during Deadpool's Diner event
            };

            BaseSnapPoint = new Point
            {
                X = Window.Left + Center,
                Y = Window.Top + Scale(115)
            };

            BasePlayPoint = new Point
            {
                X = Window.Left + Center,
                Y = Window.Bottom - Scale(200)
            };

            BaseCancelPoint = new Point
            {
                X = Window.Left + Center,
                Y = Window.Bottom - Scale(60)
            };

            BaseNextPoint = new Point
            {
                X = Window.Left + Center + Scale(300),
                Y = Window.Bottom - Scale(60)
            };

            RetreatPoint = new Point
            {
                X = Window.Left + Center - Scale(300),
                Y = Window.Bottom - Scale(70)
            };

            RetreatConfirmPoint = new Point
            {
                X = Window.Left + Center - Scale(100),
                Y = Window.Bottom - Scale(280)
            };

			ConcedePoint = new Point
			{
				X = Window.Left + Center - Scale(300),
				Y = Window.Bottom - Scale(70)
			};

			ConcedeConfirmPoint = new Point
			{
				X = Window.Left + Center + Scale(100),
				Y = Window.Bottom - Scale(280)
			};
		}
	}
}
