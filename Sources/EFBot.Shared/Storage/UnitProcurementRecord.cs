using System;
using System.Reactive.Disposables;

namespace EFBot.Shared.Storage
{
    internal struct UnitProcurementRecord
    {
        public int Id { get; set; }
        
        public DateTime Timestamp { get; set; }
        
        public string UnitName { get; set; }
        
        public string UnitPrice { get; set; }
    }
}