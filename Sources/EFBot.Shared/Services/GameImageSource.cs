using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using EFBot.Shared.Scaffolding;
using ReactiveUI;

namespace EFBot.Shared.Services
{
    public sealed class GameImageSource : DisposableReactiveObject, IGameImageSource
    {
        private IntPtr windowHandle;

        public GameImageSource()
        {
            Log.Instance.Debug($"Initializing new GameImage source...");

            Observable
                .Timer(DateTimeOffset.Now, TimeSpan.FromSeconds(1))
                .Select(x => FindWindow("Bluestacks"))
                .DistinctUntilChanged()
                .Subscribe(
                    hwnd =>
                    {
                        if (hwnd != IntPtr.Zero)
                        {
                            Log.Instance.Debug($"New game window found: 0x{hwnd.ToInt64():x8}");
                        }
                        else
                        {
                            Log.Instance.Debug($"Game window not found");
                        }
                        WindowHandle = hwnd;
                    })
                .AddTo(Anchors);
            
            Observable
                .Timer(DateTimeOffset.Now, TimeSpan.FromSeconds(1)).ToUnit()
                .Merge(this.WhenAnyValue(x => x.WindowHandle).ToUnit())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => { Source = CaptureWindow(WindowHandle); })
                .AddTo(Anchors);

            this.WhenAnyValue(x => x.WindowHandle)
                .Subscribe(
                    _ =>
                    {
                        this.RaisePropertyChanged(nameof(WindowRectangle));
                        this.RaisePropertyChanged(nameof(ButtonsArea));
                        this.RaisePropertyChanged(nameof(UnitNameAreas));
                        this.RaisePropertyChanged(nameof(UnitPriceAreas));
                        this.RaisePropertyChanged(nameof(RefreshButtonArea));
                        this.RaisePropertyChanged(nameof(IsForeground));
                    })
                .AddTo(Anchors);
        }

        private Bitmap source;

        public Bitmap Source
        {
            get { return source; }
            set { this.RaiseAndSetIfChanged(ref source, value); }
        }

        public IntPtr WindowHandle
        {
            get => windowHandle;
            set => this.RaiseAndSetIfChanged(ref windowHandle, value);
        }

        public Rectangle WindowRectangle => GetWindowRect(WindowHandle);

        public Rectangle ButtonsArea => CalculateButtonsArea(WindowRectangle);
        
        public Rectangle RefreshButtonArea => CalculateRefreshButtonArea(WindowRectangle);

        public bool IsForeground
        {
            get => WindowHandle != IntPtr.Zero && NativeMethods.GetForegroundWindow() == WindowHandle;
        }

        public Rectangle[] UnitNameAreas => CalculateUnitNameAreas(WindowRectangle);
        
        public Rectangle[] UnitPriceAreas => CalculateUnitPriceAreas(WindowRectangle);
        
        private Rectangle[] CalculateUnitNameAreas(Rectangle windowArea)
        {
            var firstArea = new RectangleF(
                    x: 90/533f,
                    y: 535/1030f,
                    width:  290/533f,
                    height: 35/1030f
                );
            var secondArea = new RectangleF(
                x: 90/533f,
                y: 625/1030f,
                width:  290/533f,
                height: 35/1030f
            );
            var thirdArea = new RectangleF(
                x: 90/533f,
                y: 715/1030f,
                width:  290/533f,
                height: 35/1030f
            );var fourthArea = new RectangleF(
                x: 90/533f,
                y: 805/1030f,
                width:  290/533f,
                height: 35/1030f
            );

            return new[]
            {
                firstArea.Multiply(windowArea).ToRectangle(),
                secondArea.Multiply(windowArea).ToRectangle(),
                thirdArea.Multiply(windowArea).ToRectangle(),
                fourthArea.Multiply(windowArea).ToRectangle(),
            };
        }
        
        private Rectangle[] CalculateUnitPriceAreas(Rectangle windowArea)
        {
            var firstArea = new RectangleF(
                x: 421/533f,
                y: 540/1030f,
                width:  85/533f,
                height: 58/1030f
            );
            var secondArea = new RectangleF(
                x: 421/533f,
                y: 630/1030f,
                width:  85/533f,
                height: 58/1030f
            );
            var thirdArea = new RectangleF(
                x: 421/533f,
                y: 720/1030f,
                width:  85/533f,
                height: 58/1030f
            );var fourthArea = new RectangleF(
                x: 421/533f,
                y: 810/1030f,
                width:  85/533f,
                height: 58/1030f
            );

            return new[]
            {
                firstArea.Multiply(windowArea).ToRectangle(),
                secondArea.Multiply(windowArea).ToRectangle(),
                thirdArea.Multiply(windowArea).ToRectangle(),
                fourthArea.Multiply(windowArea).ToRectangle(),
            };
        }

        private Rectangle CalculateButtonsArea(Rectangle windowArea)
        {
            var relative = new RectangleF(
                    x: 295/377f,
                    y: 383/741f,
                width:  (364 - 295)/377f,
                height: (620 - 383)/741f);

            var result = new Rectangle(
                x: (int)(windowArea.Width * relative.X),
                y: (int)(windowArea.Height * relative.Y),
                width:  (int)(windowArea.Width * relative.Width),
                height: (int)(windowArea.Height * relative.Height)
                );

            return result;
        }
        
        private Rectangle CalculateRefreshButtonArea(Rectangle windowArea)
        {
            var relative = new RectangleF(
                x: 255/377f,
                y: 355/741f,
                width:  (295 - 255)/377f,
                height: (370 - 355)/741f);

            var result = new Rectangle(
                x: (int)(windowArea.Width * relative.X),
                y: (int)(windowArea.Height * relative.Y),
                width:  (int)(windowArea.Width * relative.Width),
                height: (int)(windowArea.Height * relative.Height)
            );

            return result;
        }

        private IntPtr FindWindow(string procName)
        {
            var proc = Process.GetProcessesByName(procName).FirstOrDefault(x => x.MainWindowHandle != IntPtr.Zero);
            if (proc == null)
            {
                return IntPtr.Zero;
            }

            return proc.MainWindowHandle;
        }

        private Rectangle GetWindowRect(IntPtr hwnd)
        {
            RECT rc;
            GetWindowRect(hwnd, out rc);
            return rc;
        }

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

        private static Bitmap CaptureWindow(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                RECT rc;
                GetWindowRect(hwnd, out rc);

                var bmp = new Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb);
                var gfxBmp = Graphics.FromImage(bmp);
                var hdcBitmap = gfxBmp.GetHdc();

                PrintWindow(hwnd, hdcBitmap, 0);

                gfxBmp.ReleaseHdc(hdcBitmap);
                gfxBmp.Dispose();

                return bmp;
            }
            catch (Exception e)
            {
                Log.HandleException(e);
            }

            return null;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public RECT(RECT Rectangle) : this(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom) { }
            public RECT(int Left, int Top, int Right, int Bottom)
            {
                X = Left;
                Y = Top;
                this.Right = Right;
                this.Bottom = Bottom;
            }

            public int X { get; set; }

            public int Y { get; set; }

            public int Left
            {
                get => X;
                set => X = value;
            }

            public int Top
            {
                get => Y;
                set => Y = value;
            }

            public int Right { get; set; }

            public int Bottom { get; set; }

            public int Height
            {
                get => Bottom - Y;
                set => Bottom = value + Y;
            }

            public int Width
            {
                get => Right - X;
                set => Right = value + X;
            }

            public Point Location
            {
                get => new Point(Left, Top);
                set
                {
                    X = value.X;
                    Y = value.Y;
                }
            }

            public Size Size
            {
                get => new Size(Width, Height);
                set
                {
                    Right = value.Width + X;
                    Bottom = value.Height + Y;
                }
            }

            public static implicit operator Rectangle(RECT Rectangle)
            {
                return new Rectangle(Rectangle.Left, Rectangle.Top, Rectangle.Width, Rectangle.Height);
            }
            public static implicit operator RECT(Rectangle Rectangle)
            {
                return new RECT(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom);
            }
            public static bool operator ==(RECT Rectangle1, RECT Rectangle2)
            {
                return Rectangle1.Equals(Rectangle2);
            }
            public static bool operator !=(RECT Rectangle1, RECT Rectangle2)
            {
                return !Rectangle1.Equals(Rectangle2);
            }

            public override string ToString()
            {
                return "{Left: " + X + "; " + "Top: " + Y + "; Right: " + Right + "; Bottom: " + Bottom + "}";
            }

            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }

            public bool Equals(RECT Rectangle)
            {
                return Rectangle.Left == X && Rectangle.Top == Y && Rectangle.Right == Right && Rectangle.Bottom == Bottom;
            }

            public override bool Equals(object Object)
            {
                if (Object is RECT)
                {
                    return Equals((RECT) Object);
                }

                if (Object is Rectangle)
                {
                    return Equals(new RECT((Rectangle) Object));
                }

                return false;
            }
        }
    }
}