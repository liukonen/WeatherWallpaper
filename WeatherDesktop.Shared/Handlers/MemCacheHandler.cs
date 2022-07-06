using System;
using System.Runtime.Caching;

namespace WeatherDesktop.Shared.Handlers
{

    public sealed class MemCacheHandler
    {
        private static readonly MemCacheHandler instance = new MemCacheHandler();

        private readonly string GetName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

        MemoryCache cache = MemoryCache.Default;
        private string TransformKey(string key) => $"{GetName}_{key}";

        public T GetItem<T>(string key) => (T)cache.Get(TransformKey(key));

        public void SetItem<T>(string key, T item, int minutes) => cache.Set(TransformKey(key), item, DateTime.Now.AddMinutes(minutes));

        public void SetItem<T>(string key, T item) => SetItem(TransformKey(key), item, 15);

        public Boolean Exists(string key) => cache.Contains(TransformKey(key));

        static MemCacheHandler()
        {
        }

        private MemCacheHandler()
        {
        }
        public static MemCacheHandler Instance
        {
            get => instance;
        }
    }
}
