namespace EFBot.Shared.Services {
    internal interface IInputController {
        bool IsAvailable { get; }
        bool ClickOnRefreshButton();
        bool ClickOnButtonByIdx(int idx);
    }
}