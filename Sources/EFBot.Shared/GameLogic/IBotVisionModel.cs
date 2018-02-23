using System;
using System.Collections.ObjectModel;
using EFBot.Shared.Scaffolding;
using EFBot.Shared.Services;
using Emgu.CV;

namespace EFBot.Shared.GameLogic
{
    public interface IBotVisionModel : IDisposableReactiveObject
    {
        IImage BotImage { get; set; }
        
        ReadOnlyObservableCollection<RecognitionResult> BotVision { get; }
        
        UnitShopUnit[] AvailableUnits { get; set; }
        
        TimeSpan? TimeLeftTillRefresh { get; set; }

        string Text { get; set; }
        
        string Error { get; set; }
        
        double FramesPerSecond { get; }
    }
}