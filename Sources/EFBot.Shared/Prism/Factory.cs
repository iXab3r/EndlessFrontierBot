using Unity;

namespace EFBot.Shared.Prism
{
    internal sealed class Factory<T> : IFactory<T>
    {
        private readonly IUnityContainer container;

        public Factory(IUnityContainer container)
        {
            this.container = container;
        }

        public T Create()
        {
            return container.Resolve<T>();
        }
    }
}
