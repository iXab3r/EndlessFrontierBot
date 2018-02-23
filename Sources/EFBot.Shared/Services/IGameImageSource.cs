using System;
using System.Drawing;
using EFBot.Shared.Scaffolding;

namespace EFBot.Shared.Services
{
    public interface IGameImageSource : IDisposableReactiveObject
    {
        Bitmap Source { get; }
        IntPtr WindowHandle { get; }
        Rectangle WindowRectangle { get; }
        Rectangle RefreshButtonArea { get; }
        bool IsForeground { get; }
        Rectangle[] UnitNameAreas { get; }
        Rectangle[] UnitPriceAreas { get; }
        Rectangle GameFieldArea { get; }

        TimeSpan Period { get; set; }
    }
}