using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace JinRi.LogCenter.Test.Logger
{
    public class DBLoggerTest
    {
        [Fact]
        public void TestHandleShouldBeSuccess()
        {
            Task.Run(() =>
            {
                for (int i = 0; i < 380; i++)
                {
                    var logMessage = new LogMessage
                    {
                        ClientIP = "ClientIP",
                        Content = "Content",
                        Ikey = Guid.NewGuid().ToString(),
                        IsHandle = true,
                        Keyword = "keyword",
                        LogTime = DateTime.Now,
                        LogType = "LogType",
                        Module = "Module",
                        OrderNo = "OrdeNo",
                        ServerIP = "ServerIP",
                        Username = "Username"
                    };
                    if (i % 2 == 1)
                    {
                        logMessage.IsHandle = false;
                        DBLog.Process(logMessage);
                    }
                    else
                    {
                        DBLog.Handle(logMessage);
                    }
                }
            });

            Task.Run(() =>
            {
                DBLog.ConsumeForEach();
            });

            Thread.Sleep(1000 * 60 * 10);
        }
    }
}
