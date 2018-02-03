namespace EFBot.Shared {
    public struct UnitShopUnit
    {
        public int Idx { get; set; }
        
        public string Price { get; set; }
        public override string ToString()
        {
            return $"{nameof(Idx)}: {Idx}, {nameof(Price)}: {Price}";
        }
    }
}