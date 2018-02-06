using EFBot.Shared.Scaffolding;

namespace EFBot.Shared.GameLogic {
    public interface IBotStrategy : IDisposableReactiveObject
    {
        bool IsEnabled { get; set; }
    }
}