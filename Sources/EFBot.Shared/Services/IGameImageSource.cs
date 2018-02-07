using System;
using System.Drawing;

namespace EFBot.Shared.Services {
    public interface IGameImageSource {
        Bitmap Source { get; set; }
        IntPtr WindowHandle { get; set; }
        Rectangle WindowRectangle { get; }
        Rectangle RefreshButtonArea { get; }
        bool IsForeground { get; }
        
        Rectangle[] UnitNameAreas { get; }
        Rectangle[] UnitPriceAreas { get; }
    }
}