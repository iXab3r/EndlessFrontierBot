using System;
using System.Linq;
using System.Reactive.Linq;
using EFBot.Shared.Services;
using ReactiveUI;

namespace EFBot.Shared
{
    public sealed class BotStrategy : ReactiveObject
    {
        private readonly BotController botController;

        private readonly InputController controller;
        
        public BotStrategy(
            GameImageSource gameSource,
            BotController botController)
        {
            this.botController = botController;
            this.controller = new InputController(gameSource);

            botController
                .WhenAnyValue(x => x.AvailableUnits)
                .Where(x => controller.IsAvailable)
                .Sample(TimeSpan.FromSeconds(5))
                .Subscribe(ProcessNextUnitsPack);
            
            
            botController
                .WhenAnyValue(x => x.TimeLeftTillRefresh)
                .Where(x => controller.IsAvailable)
                .Where(x => x != null && x.Value == TimeSpan.Zero)
                .Sample(TimeSpan.FromSeconds(5))
                .Subscribe(x => HandlePackRefreshAvailability());
        }

        private void HandlePackRefreshAvailability()
        {
            controller.ClickOnRefreshButton();
        }

        private void ProcessNextUnitsPack(UnitShopUnit[] units)
        {
            var unit = units.FirstOrDefault(x => x.Price == "15000");
            if (unit.Price == null)
            {
                return;
            }
            
            controller.ClickOnButtonByIdx(unit.Idx);
        }
    }
}