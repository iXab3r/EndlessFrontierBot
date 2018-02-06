using System;
using System.Reactive.Disposables;
using System.Threading;
using WindowsInput;
using AutoIt;
using EFBot.Shared.Scaffolding;
using ReactiveUI;

namespace EFBot.Shared.Services {
    internal sealed class GameInputController : DisposableReactiveObject, IGameInputController
    {
        private static readonly TimeSpan ShortActionDelay = TimeSpan.FromSeconds(0.250);
        private static readonly TimeSpan ActionDelay = TimeSpan.FromSeconds(2);

        private readonly IGameImageSource gameSource;
        private readonly IUserInteractionsManager manager;

        public GameInputController(
            IGameImageSource gameSource,
            IUserInteractionsManager manager)
        {
            this.gameSource = gameSource;
            this.manager = manager;

            gameSource.WhenAnyValue(x => x.WindowRectangle)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(IsAvailable)))
                .AddTo(Anchors);
        }

        public bool IsAvailable => IsOperationPossible();

        public bool ClickOnRefreshButton()
        {
            Log.Instance.Debug("Trying to click on Refresh button");
            if (!IsOperationPossible())
            {
                return false;
            }

            using (SwitchWindowTo(gameSource.WindowHandle))
            {
                AutoItX.ControlSend(gameSource.WindowHandle, IntPtr.Zero, "R");
                manager.Delay(TimeSpan.FromMilliseconds(ActionDelay.TotalMilliseconds * 3));
            }
            
            return true;
        }
        
        public bool ClickOnButtonByIdx(int idx)
        {
            Log.Instance.Debug($"Trying to click on Unit button #{idx}");

            if (!IsOperationPossible())
            {
                return false;
            }
            
            var keyToPress = string.Empty;
            switch (idx)
            {
                    case 0:
                        keyToPress = "1";
                        break;
                    case 1: 
                        keyToPress = "2";
                        break;
                    case 2: 
                        keyToPress = "3";
                        break;
                    case 3:
                        keyToPress = "4";
                        break;
            }
            
            using (SwitchWindowTo(gameSource.WindowHandle))
            {
                AutoItX.ControlSend(gameSource.WindowHandle, IntPtr.Zero, keyToPress);
                manager.Delay(ActionDelay);
                AutoItX.ControlSend(gameSource.WindowHandle, IntPtr.Zero, "{ENTER}");
                manager.Delay(ActionDelay);
            }
            
            return true;
        }

        private string GetOperationStatus()
        {
            if (gameSource.WindowRectangle.IsEmpty)
            {
                return "Window area is not specified";
            }

            if (gameSource.WindowHandle == IntPtr.Zero)
            {
                return "Window not found";
            }

            return string.Empty;
        }

        private bool IsOperationPossible()
        {
            var result = GetOperationStatus();
            if (string.IsNullOrEmpty(result))
            {
                return true;
            }

            Log.Instance.Warn($"Operation is not possible: {result}");
            return false;
        }

        private IDisposable SwitchWindowTo(IntPtr targetWindow)
        {
            var activeWindow = AutoItX.WinGetHandle("");
            AutoItX.WinActivate(gameSource.WindowHandle);
            if (AutoItX.WinWaitActive(gameSource.WindowHandle, (int) ActionDelay.TotalMilliseconds) != 0)
            {
                Log.Instance.Warn($"Failed to activate game window in {ShortActionDelay}");
                return Disposable.Empty;
            }

            return Disposable.Create(
                () =>
                {
                    AutoItX.WinActivate(activeWindow);
                    manager.Delay(ActionDelay);
                });
        }
    }
}