using System;
using System.Threading;
using WindowsInput;
using AutoIt;
using EFBot.Shared.Scaffolding;
using ReactiveUI;

namespace EFBot.Shared.Services {
    internal sealed class InputController : ReactiveObject, IInputController
    {
        private static readonly TimeSpan ActionDelay = TimeSpan.FromSeconds(1);

        private readonly GameImageSource gameSource;
        private readonly UserInteractionsManager manager;

        public InputController(GameImageSource gameSource)
        {
            this.gameSource = gameSource;
            manager = new UserInteractionsManager(new InputSimulator(), new FakeUserInputBlocker());

            gameSource.WhenAnyValue(x => x.WindowRectangle)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(IsAvailable)));
        }

        public bool IsAvailable => IsOperationPossible();

        public bool ClickOnRefreshButton()
        {
            Log.Instance.Debug("Trying to click on Refresh button");
            if (!IsOperationPossible())
            {
                return false;
            }

            var activeWindow = AutoItX.WinGetHandle("");
            AutoItX.WinActivate(gameSource.WindowHandle);
            AutoItX.ControlSend(gameSource.WindowHandle, IntPtr.Zero, "R");
            AutoItX.WinActivate(activeWindow);

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
            
            var activeWindow = AutoItX.WinGetHandle("");
            AutoItX.WinActivate(gameSource.WindowHandle);
            AutoItX.ControlSend(gameSource.WindowHandle, IntPtr.Zero, keyToPress);
            AutoItX.ControlSend(gameSource.WindowHandle, IntPtr.Zero, "{ENTER}");
            AutoItX.WinActivate(activeWindow);
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
    }
}