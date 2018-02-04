using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using EFBot.Shared.Prism;
using EFBot.Shared.Scaffolding;
using EFBot.Shared.Services;
using ReactiveUI;

namespace EFBot.Shared.GameLogic
{
    internal sealed class BotStrategy : DisposableReactiveObject, IBotStrategy
    {
        private readonly BotController botController;

        private readonly IInputController controller;

        private readonly static TimeSpan SamplingPeriod = TimeSpan.FromSeconds(10);
        
        public BotStrategy(
            IGameImageSource gameSource,
            IFactory<IInputController, IGameImageSource> controllerFactory,
            BotController botController)
        {
            this.botController = botController;
            this.controller = controllerFactory.Create(gameSource);

            botController
                .WhenAnyValue(x => x.AvailableUnits)
                .Sample(SamplingPeriod)
                .DistinctUntilChanged(new LambdaComparer<UnitShopUnit[]>((x, y) => x.SequenceEqual(y, UnitShopUnit.Comparer)))
                .Subscribe(ProcessNextUnitsPack)
                .AddTo(Anchors);
            
            botController
                .WhenAnyValue(x => x.TimeLeftTillRefresh)
                .Sample(SamplingPeriod)
                .Where(x => x != null && x.Value == TimeSpan.Zero)
                .Subscribe(x => HandlePackRefreshAvailability())
                .AddTo(Anchors);
        }

        private void HandlePackRefreshAvailability()
        {
            Log.Instance.Debug("Clicking on Refresh button...");
            controller.ClickOnRefreshButton();
        }

        private void ProcessNextUnitsPack(UnitShopUnit[] units)
        {
            Log.Instance.Debug($"New UnitPack detected:\n\t{units.DumpToTable()}");

            foreach (var unit in units.Where(x => x.Price == "80" || x.Price == "15000" || x.Price == "300" || x.Price == "700" || x.Price == "4000"))
            {
                Log.Instance.Debug($"Clicking on Unit {unit}...");
                controller.ClickOnButtonByIdx(unit.Idx);
                
                Thread.Sleep(1000);
            }
        }
    }
}