using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetInfo.Help
{
    /// <summary>
    /// 公共缓存
    /// </summary>
    public static class CacheHelp
    {
        public static MemoryCache cache;
        static CacheHelp()
        {
            MemoryCacheOptions mcop = new MemoryCacheOptions();
            cache = new MemoryCache(mcop);
        }
    }
}
