using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using System.Reflection;
using System.Threading;

namespace JinRi.LogCenter.Test.Database
{
    public class LogMessageRepeateTest
    {

        [Fact]
        public void TestRepeatBatchSaveLog()
        {
            List<LogMessage> list = new List<LogMessage>();
            for (int i = 0; i < 500; i++)
            {
                LogMessage log = new LogMessage();
                log.Ikey = Guid.NewGuid().ToString("N");
                log.Username = "test";
                log.ClientIP = "0.0.0.0";
                log.Content = "重试测试-批量测试";
                log.Keyword = "重试";
                log.ServerIP = "127.0.0.1";
                log.OrderNo = "";
                log.Module = Assembly.GetExecutingAssembly().GetLoadedModules()[0].Name;
                log.LogType = "LogMessageDALTest";
                log.LogTime = DateTime.Now;
                log.IsHandle = false;
                list.Add(log);
            }

            var retCount = LogMessageDAL.Instance.Insert(list);
            //retCount.ShouldBe(list.Count);
            Thread.Sleep(1000 * 60 * 10);
        }
    }
}
