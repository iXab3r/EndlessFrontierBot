using System;
using System.Drawing;
using System.Windows.Input;
using WindowsInput;
using WindowsInput.Native;
using ReactiveUI;

namespace EFBot.Shared.Services {
    internal sealed class InputController : ReactiveObject
    {
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
            if (!IsOperationPossible())
            {
                return false;
            }
            
            manager.SendKey(VirtualKeyCode.VK_R);
            return true;
        }

        public bool ClickOnButtonByIdx(int idx)
        {
            if (!IsOperationPossible())
            {
                return false;
            }

            var keyToPress = VirtualKeyCode.CANCEL;
            switch (idx)
            {
                    case 0:
                        keyToPress = VirtualKeyCode.VK_1;
                        break;
                    case 1: 
                        keyToPress = VirtualKeyCode.VK_2;
                        break;
                    case 2: 
                        keyToPress = VirtualKeyCode.VK_3;
                        break;
                    case 3:
                        keyToPress = VirtualKeyCode.VK_4;
                        break;
            }
            
            manager.SendKey(keyToPress);
            manager.Delay(TimeSpan.FromSeconds(2));
            manager.SendKey(VirtualKeyCode.RETURN);

            return true;
        }

        private bool IsOperationPossible()
        {
            if (gameSource.WindowRectangle.IsEmpty)
            {
                return false;
            }

            if (!gameSource.IsForeground())
            {
                return false;
            }

            return true;
        }
    }
}