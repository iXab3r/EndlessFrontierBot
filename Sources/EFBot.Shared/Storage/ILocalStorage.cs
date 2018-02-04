using EFBot.Shared.Scaffolding;

namespace EFBot.Shared.Storage {
    internal interface ILocalStorage : IDisposableReactiveObject {
        void Insert<T>(T record);
    }
}