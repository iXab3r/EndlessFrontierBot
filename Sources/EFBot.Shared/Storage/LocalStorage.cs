using System;
using System.IO;
using EFBot.Shared.Scaffolding;
using LiteDB;

namespace EFBot.Shared.Storage {
    internal sealed class LocalStorage : DisposableReactiveObject, ILocalStorage
    {
        private static readonly string DatabaseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage");
        private static readonly string DatabasePath = Path.Combine(DatabaseDirectory, "main.db");
        private readonly LiteDatabase database;

        public LocalStorage()
        {
            if (!Directory.Exists(DatabaseDirectory))
            {
                Directory.CreateDirectory(DatabaseDirectory);
            }
            
            database = new LiteDatabase(DatabasePath);
            database.AddTo(Anchors);
        }

        public void Insert<T>(T record)
        {
            var collection = database.GetCollection<T>();
            collection.Insert(record);
        }
    }
}