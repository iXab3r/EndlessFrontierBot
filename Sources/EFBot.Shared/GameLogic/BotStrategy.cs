using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using EFBot.Shared.Prism;
using EFBot.Shared.Scaffolding;
using EFBot.Shared.Services;
using EFBot.Shared.Storage;
using ReactiveUI;

namespace EFBot.Shared.GameLogic
{
    internal sealed class BotStrategy : DisposableReactiveObject, IBotStrategy
    {
        private readonly ILocalStorage localStorage;
        private readonly BotVisionModel botVisionModel;

        private readonly IGameInputController controller;
        private bool isEnabled;

        private readonly static TimeSpan SamplingPeriod = TimeSpan.FromSeconds(10);
        
        public BotStrategy(
            ILocalStorage localStorage,
            IGameImageSource gameSource,
            IFactory<IGameInputController, IGameImageSource> controllerFactory,
            BotVisionModel botVisionModel)
        {
            this.localStorage = localStorage;
            this.botVisionModel = botVisionModel;
            this.controller = controllerFactory.Create(gameSource);

            botVisionModel
                .WhenAnyValue(x => x.AvailableUnits)
                .Sample(SamplingPeriod)
                .Where(x => IsEnabled)
                .Subscribe(ProcessNextUnitsPack)
                .AddTo(Anchors);
            
            botVisionModel
                .WhenAnyValue(x => x.TimeLeftTillRefresh)
                .Sample(SamplingPeriod)
                .Where(x => IsEnabled)
                .Where(x => x != null && x.Value == TimeSpan.Zero)
                .Subscribe(x => HandlePackRefreshAvailability())
                .AddTo(Anchors);
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set { this.RaiseAndSetIfChanged(ref isEnabled, value); }
        }
        
        private void HandlePackRefreshAvailability()
        {
            Log.Instance.Debug("Clicking on Refresh button...");
            controller.ClickOnRefreshButton();
        }

        private void ProcessNextUnitsPack(UnitShopUnit[] units)
        {
            Log.Instance.Debug($"New UnitPack detected:\n\t{units.DumpToTable()}");

            foreach (var unit in units.Where(
                x => x.Price == "80" || 
                     x.Price == "15000" || 
                     x.Price == "300" || 
                     x.Price == "700" || 
                     x.Price == "4000"))
            {
                Log.Instance.Debug($"Clicking on Unit {unit}...");
                if (controller.ClickOnButtonByIdx(unit.Idx))
                {
                    var boughtUnit = new UnitProcurementRecord()
                    {
                        UnitName = string.IsNullOrEmpty(unit.Name) ? "(unknown)" : unit.Name,
                        Timestamp = DateTime.Now,
                        UnitPrice = unit.Price,
                    };
                    localStorage.Insert(boughtUnit);
                    return;
                }
            }
        }
    }
}