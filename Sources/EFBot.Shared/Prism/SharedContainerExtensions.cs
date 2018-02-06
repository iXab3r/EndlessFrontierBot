using System.Reactive.Concurrency;
using WindowsInput;
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
                .RegisterSingleton(typeof(IGameInputController), typeof(GameInputController))
                .RegisterSingleton(typeof(ILocalStorage), typeof(LocalStorage));

            Container
                .RegisterSingleton(typeof(IGameImageSource), typeof(GameImageSource));

            Container
                .RegisterType(typeof(IInputSimulator), new InjectionFactory(x => new InputSimulator()))
                .RegisterType(typeof(IInputSimulator), new InjectionFactory(x => new InputSimulator()))
                .RegisterType(typeof(IUserInputBlocker), typeof(FakeUserInputBlocker))
                .RegisterType(typeof(IUserInteractionsManager), typeof(UserInteractionsManager))
                .RegisterType(typeof(IGameInputController), typeof(GameInputController))
                .RegisterType(typeof(IRecognitionEngine), typeof(RecognitionEngine))
                .RegisterType(typeof(IBotVisionModel), typeof(BotVisionModel))
                .RegisterType(typeof(IBotStrategy), typeof(BotStrategy));
        }
    }
}