using System;
using System.Collections.ObjectModel;
using EFBot.Shared.Scaffolding;
using EFBot.Shared.Services;
using Emgu.CV;

namespace EFBot.Shared.GameLogic
{
    public interface IBotVisionModel : IDisposableReactiveObject
    {
        IImage BotImage { get; }
        
        ReadOnlyObservableCollection<RecognitionResult> BotVision { get; }
        
        UnitShopUnit[] AvailableUnits { get; }
        
        TimeSpan? TimeLeftTillRefresh { get; }

        string Text { get; }
        
        string Error { get; }
        
        double FramesPerSecond { get; }
        
        bool IsTrackingEnabled { get; set; }
    }
}