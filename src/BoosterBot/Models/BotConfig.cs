using BoosterBot.Helpers;
using Microsoft.Extensions.Configuration;
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
        private readonly bool _useEvent;
        private readonly bool _downscaled;
        private readonly string _logPath;
        private readonly bool _constantSnapping;
		private readonly IConfiguration _settings;
        private readonly LocalizationManager _localizer;

        public LocalizationManager Localizer { get => _localizer; }
        public double Scaling { get => _scaling; }
        public bool Verbose { get => _verbose; }
        public bool Autoplay { get => _autoplay; }
        public bool SaveScreens { get => _saveScreens; }
        public bool Downscaled { get => _downscaled; }
        public string LogPath { get => _logPath; }
        public bool ConstantSnapping { get => _constantSnapping; }
		public IConfiguration Settings { get => _settings; }
        public int Center { get; set; }
        public int vCenter { get; set; }
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
                var yBound = (int)(100 * Scaling);
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
        private Point BaseEventBannerPoint { get; set; }
        public Point EventBannerPoint
        {
            get
            {
                var rand = new Random();
                var bound = (int)(50 * Scaling);
                return new Point
                {
                    X = BaseEventBannerPoint.X + rand.Next(-bound, bound),
                    Y = BaseEventBannerPoint.Y + rand.Next(-bound, bound)
                };
            }
        }
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
        private Point BaseClaimPoint { get; set; }
        public Point ClaimPoint
        {
            get
            {
                var rand = new Random();
                var bound = (int)(10 * Scaling);
                return new Point
                {
                    X = BaseClaimPoint.X + rand.Next(-bound, bound),
                    Y = BaseClaimPoint.Y + rand.Next(-bound, bound)
                };
            }
        }
        public Point RetreatPoint { get; set; }
        public Point RetreatConfirmPoint { get; set; }
		public Point ConcedePoint { get; set; }
		public Point ConcedeConfirmPoint { get; set; }
        public Point LaneColorPoint1 { get; set; }
        public Point LaneColorPoint2 { get; set; }
        public Point LaneColorPoint3 { get; set; }
        public List<Point> Cards { get; set; }
        public List<Point> Locations { get; set; }

        public BotConfig(IConfiguration settings, LocalizationManager localizer, double scaling, bool verbose, bool autoplay, bool saveScreens, string logPath, bool useEvent, bool downscaled, bool constantSnapping)
        {
            _settings = settings;
            _localizer = localizer;
            _scaling = scaling;
            _verbose = verbose;
            _autoplay = autoplay;
            _saveScreens = saveScreens;
            _downscaled = downscaled;
            _logPath = logPath;
			_constantSnapping = constantSnapping;
			_useEvent = useEvent;
        }

        public int Scale(int x) => (int)(Scaling * x);

        public void GetWindowPositions()
        {
            // Find game window and take screencap:
            Window = SystemUtilities.GetGameWindowLocation();
            Screencap = SystemUtilities.GetGameScreencap(Window, Scaling);

            // Calculate center position of game window:
            Center = Screencap.Width / 2;
            vCenter = Screencap.Height / 2;

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

            LaneColorPoint1 = new Point
            {
                X = Center - 186,
                Y = vCenter + 27
            };

            LaneColorPoint2 = new Point
            {
                X = Center,
                Y = vCenter + 15
            };

            LaneColorPoint3 = new Point
            {
                X = Center + 188,
                Y = vCenter + 27
            };

            BaseResetPointLeft = new Point
            {
                X = Window.Left + Scale(100),
                Y = Window.Bottom - Scale(100)
            };

            BaseResetPointRight = new Point
            {
                X = Window.Right - Scale(100),
                Y = Window.Bottom - Scale(100)
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

            BaseEventBannerPoint = new Point
            {
                X = Window.Left + Center,
                Y = Window.Top + Scale(330)
            };

            BaseConquestBannerPoint = new Point
            {
                X = Window.Left + Center,
                Y = Window.Top + (_useEvent ? Scale(540) : Scale(330)) // Adjust click point for LTM
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

            BaseClaimPoint = new Point
            {
                X = Window.Left + Center,
                Y = Window.Bottom - Scale(135)
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
