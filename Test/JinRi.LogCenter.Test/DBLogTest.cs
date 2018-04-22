using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using System.Reflection;
using System.Threading;

namespace JinRi.LogCenter.Test
{
    public class DBLogTest
    {
        [Fact]
        public void ProcessTest()
        {
            List<LogMessage> list = new List<LogMessage>();
            for (int i = 0; i < 100000 * 1; i++)
            {
                LogMessage log = new LogMessage();
                log.Ikey = Guid.NewGuid().ToString("N");
                log.Username = "test";
                log.ClientIP = "0.0.0.0";
                log.Content = "批量测试";
                log.Keyword = "测试";
                log.ServerIP = "127.0.0.1";
                log.OrderNo = "";
                log.Module = Assembly.GetExecutingAssembly().GetLoadedModules()[0].Name;
                log.LogType = "LogMessageDALTest";
                log.LogTime = DateTime.Now;
                log.IsHandle = false;
                list.Add(log);
                DBLog.Process(log);
            }
            Thread.Sleep(1000 * 60 * 60 * 3);
        }

        //[Fact]
        public void HandleTest()
        {
            List<LogMessage> list = new List<LogMessage>();
            for (int i = 0; i < 100000 * 10; i++)
            {
                LogMessage log = new LogMessage();
                log.Ikey = Guid.NewGuid().ToString("N");
                log.Username = "test";
                log.ClientIP = "0.0.0.0";
                log.Content = "批量测试";
                log.Keyword = "测试";
                log.ServerIP = "127.0.0.1";
                log.OrderNo = "";
                log.Module = Assembly.GetExecutingAssembly().GetLoadedModules()[0].Name;
                log.LogType = "LogMessageDALTest";
                log.LogTime = DateTime.Now;
                log.IsHandle = true;
                list.Add(log);
                DBLog.Process(log);
            }
            Thread.Sleep(1000 * 60 * 20);
        }

        /// <summary>
        /// 消费数据
        /// </summary>
        //[Fact]
        public void ConsumeTest()
        {
            DBLog.ConsumeForEach();
            //DBLog.ConsumeForEach();
            Thread.Sleep(1000 * 60 * 180);
        }
    }
}
