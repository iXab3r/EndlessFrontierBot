using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using EFBot.Shared.Scaffolding;
using EFBot.Shared.Services;
using ReactiveUI;

namespace EFBot.Shared
{
    public sealed class BotStrategy : ReactiveObject
    {
        private readonly BotController botController;

        private readonly InputController controller;

        private readonly static TimeSpan SamplingPeriod = TimeSpan.FromSeconds(5);
        
        public BotStrategy(
            GameImageSource gameSource,
            BotController botController)
        {
            this.botController = botController;
            this.controller = new InputController(gameSource);

            botController
                .WhenAnyValue(x => x.AvailableUnits)
                .Where(x => controller.IsAvailable)
                .Sample(SamplingPeriod)
                .DistinctUntilChanged()
                .Subscribe(ProcessNextUnitsPack);
            
            botController
                .WhenAnyValue(x => x.TimeLeftTillRefresh)
                .Where(x => controller.IsAvailable)
                .Where(x => x != null && x.Value == TimeSpan.Zero)
                .Sample(SamplingPeriod)
                .Subscribe(x => HandlePackRefreshAvailability());
        }

        private void HandlePackRefreshAvailability()
        {
            Log.Instance.Debug("Clicking on Refresh button...");
            controller.ClickOnRefreshButton();
            Thread.Sleep(5000);
        }

        private void ProcessNextUnitsPack(UnitShopUnit[] units)
        {
            Log.Instance.Debug($"New UnitPack detected:\n\t{units.DumpToTable()}");
            
            var unit = units.FirstOrDefault(x => x.Price == "15000");
            if (unit.Price == null)
            {
                return;
            }
            
            Log.Instance.Debug($"Clicking on Unit {unit}...");
            controller.ClickOnButtonByIdx(unit.Idx);
            Thread.Sleep(5000);
        }
    }
}