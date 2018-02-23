using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using EFBot.Shared.Native;
using EFBot.Shared.Scaffolding;
using ReactiveUI;

namespace EFBot.Shared.Services
{
    public sealed class GameImageSource : DisposableReactiveObject, IGameImageSource
    {
        private readonly IWindowCaptureSource captureSource = new PrintWindowCaptureSource();

        private TimeSpan period;

        private Bitmap source;
        private IntPtr windowHandle;

        public GameImageSource()
        {
            Log.Instance.Debug($"Initializing new GameImage source...");
            
            var timer = this.WhenAnyValue(x => x.Period)
                .Throttle(TimeSpan.FromMilliseconds(250))
                .Select(
                    x => x > TimeSpan.Zero 
                        ?  Observable.Timer(DateTimeOffset.Now, x)
                        :  Observable.Never<long>())
                .Switch()
                .Publish();
            timer.Connect().AddTo(Anchors);
            
            timer
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

            timer
                .ToUnit()
                .Merge(this.WhenAnyValue(x => x.WindowHandle).ToUnit())
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

        public TimeSpan Period
        {
            get => period;
            set => this.RaiseAndSetIfChanged(ref period, value);
        }

        public Bitmap Source
        {
            get => source;
            set => this.RaiseAndSetIfChanged(ref source, value);
        }

        public IntPtr WindowHandle
        {
            get => windowHandle;
            set => this.RaiseAndSetIfChanged(ref windowHandle, value);
        }

        public Rectangle WindowRectangle => NativeMethods.GetAbsoluteClientRect(WindowHandle);

        public Rectangle RefreshButtonArea => CalculateRefreshButtonArea(WindowRectangle);

        public bool IsForeground => WindowHandle != IntPtr.Zero && NativeMethods.GetForegroundWindow() == WindowHandle;

        public Rectangle[] UnitNameAreas => CalculateUnitNameAreas(WindowRectangle);

        public Rectangle[] UnitPriceAreas => CalculateUnitPriceAreas(WindowRectangle);
        
        public Rectangle GameFieldArea => CalculateGameFieldArea(WindowRectangle);

        private Rectangle CalculateGameFieldArea(Rectangle windowArea)
        {
            var fieldArea = new RectangleF(
                0 / 533f,
                274 / 1030f,
                531 / 533f,
                132 / 1030f
            );
            return fieldArea.Multiply(windowArea).ToRectangle();
        }
        
        private Rectangle[] CalculateUnitNameAreas(Rectangle windowArea)
        {
            var firstArea = new RectangleF(
                90 / 533f,
                535 / 1030f,
                290 / 533f,
                35 / 1030f
            );
            var secondArea = new RectangleF(
                90 / 533f,
                625 / 1030f,
                290 / 533f,
                35 / 1030f
            );
            var thirdArea = new RectangleF(
                90 / 533f,
                715 / 1030f,
                290 / 533f,
                35 / 1030f
            );
            var fourthArea = new RectangleF(
                90 / 533f,
                805 / 1030f,
                290 / 533f,
                35 / 1030f
            );

            return new[]
            {
                firstArea.Multiply(windowArea).ToRectangle(),
                secondArea.Multiply(windowArea).ToRectangle(),
                thirdArea.Multiply(windowArea).ToRectangle(),
                fourthArea.Multiply(windowArea).ToRectangle()
            };
        }

        private Rectangle[] CalculateUnitPriceAreas(Rectangle windowArea)
        {
            var firstArea = new RectangleF(
                421 / 533f,
                540 / 1030f,
                85 / 533f,
                58 / 1030f
            );
            var secondArea = new RectangleF(
                421 / 533f,
                630 / 1030f,
                85 / 533f,
                58 / 1030f
            );
            var thirdArea = new RectangleF(
                421 / 533f,
                720 / 1030f,
                85 / 533f,
                58 / 1030f
            );
            var fourthArea = new RectangleF(
                421 / 533f,
                810 / 1030f,
                85 / 533f,
                58 / 1030f
            );

            return new[]
            {
                firstArea.Multiply(windowArea).ToRectangle(),
                secondArea.Multiply(windowArea).ToRectangle(),
                thirdArea.Multiply(windowArea).ToRectangle(),
                fourthArea.Multiply(windowArea).ToRectangle()
            };
        }

        private Rectangle CalculateRefreshButtonArea(Rectangle windowArea)
        {
            var relative = new RectangleF(
                359 / 534f,
                491 / 1031f,
                68 / 534f,
                23 / 1031f);

            var result = new Rectangle(
                (int) (windowArea.Width * relative.X),
                (int) (windowArea.Height * relative.Y),
                (int) (windowArea.Width * relative.Width),
                (int) (windowArea.Height * relative.Height)
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