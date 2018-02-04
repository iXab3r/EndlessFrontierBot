using System.Collections.Generic;
using EFBot.Shared.Scaffolding;

namespace EFBot.Shared.GameLogic {
    public struct UnitShopUnit
    {
        public static readonly IEqualityComparer<UnitShopUnit> Comparer = new LambdaComparer<UnitShopUnit>((x, y) => x.Idx == y.Idx && x.Price == y.Price);
        
        public int Idx { get; set; }
        
        public string Price { get; set; }
        
        public override string ToString()
        {
            return $"{nameof(Idx)}: {Idx}, {nameof(Price)}: {Price}";
        }
    }
}