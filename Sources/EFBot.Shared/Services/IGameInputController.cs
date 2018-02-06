namespace EFBot.Shared.Services {
    internal interface IGameInputController {
        bool IsAvailable { get; }
        bool ClickOnRefreshButton();
        bool ClickOnButtonByIdx(int idx);
    }
}