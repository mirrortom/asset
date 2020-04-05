using System;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;

using AssetInfo.BLL;
using AssetInfo.Entity;
using System.Collections.Generic;

namespace XUnitTestAssetInfo
{
    public class AssetBllTest
    {
        readonly ITestOutputHelper log;
        public AssetBllTest(ITestOutputHelper output)
        {
            log = output;
        }

        [Fact]
        public void List()
        {
            AssetM para = new AssetM();
            List<AssetM> data = AssetBll.List(para);
            foreach (var item in data)
            {
                log.WriteLine(item.Title);
            log.WriteLine(item.Ctime.ToString());
                log.WriteLine("-------------");
            }
            
        }
        [Fact]
        public void HistoryList()
        {
            AssetM para = new AssetM();
            para.Id = "5ce71c0d131f46cf9c36d1d68e48b763";
            List<AssetM> data = AssetBll.HistoryList(para);
            foreach (var item in data)
            {
                log.WriteLine(item.Title);
                log.WriteLine(item.Ctime.ToString());
                log.WriteLine(item.ItemCode);
                log.WriteLine("-------------");
            }
        }
    }
}
