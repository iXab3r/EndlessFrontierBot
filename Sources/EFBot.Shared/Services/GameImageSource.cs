using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using EFBot.Shared.Native;
using EFBot.Shared.Scaffolding;
using ReactiveUI;

namespace EFBot.Shared.Services
{
    public sealed class GameImageSource : DisposableReactiveObject, IGameImageSource
    {
        private IntPtr windowHandle;

        private IWindowCaptureSource captureSource = new PrintWindowCaptureSource();
        
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
                .Subscribe(x => { Source = captureSource.Capture(WindowHandle); })
                .AddTo(Anchors);

            this.WhenAnyValue(x => x.WindowHandle)
                .Subscribe(
                    _ =>
                    {
                        this.RaisePropertyChanged(nameof(WindowRectangle));
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

        public Rectangle WindowRectangle => NativeMethods.GetAbsoluteClientRect(WindowHandle);

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
        
        private Rectangle CalculateRefreshButtonArea(Rectangle windowArea)
        {
            var relative = new RectangleF(
                x: 359/534f,
                y: 491/1031f,
                width:  68/534f,
                height: 23/1031f);

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

    }
}