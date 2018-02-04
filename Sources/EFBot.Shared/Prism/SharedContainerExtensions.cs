using System.Reactive.Concurrency;
using EFBot.Shared.GameLogic;
using EFBot.Shared.Services;
using EFBot.Shared.Storage;
using ReactiveUI;
using Unity;
using Unity.Extension;
using Unity.Injection;

namespace EFBot.Shared.Prism
{
    public sealed class SharedContainerExtensions : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Container
                .RegisterType(typeof (IFactory<,,>), typeof (Factory<,,>))
                .RegisterType(typeof (IFactory<,>), typeof (Factory<,>))
                .RegisterType(typeof (IFactory<>), typeof (Factory<>));
            
            Container
                .RegisterSingleton(typeof(IInputController), typeof(InputController))
                .RegisterSingleton(typeof(ILocalStorage), typeof(LocalStorage));

            Container
                .RegisterSingleton(typeof(IGameImageSource), typeof(GameImageSource));

            Container
                .RegisterType(typeof(IRecognitionEngine), typeof(RecognitionEngine))
                .RegisterType(typeof(IBotStrategy), typeof(BotStrategy));
        }
    }
}