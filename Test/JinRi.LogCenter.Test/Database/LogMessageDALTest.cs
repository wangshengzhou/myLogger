using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using System.Reflection;
using System.Diagnostics;

namespace JinRi.LogCenter.Test.Database
{
    public class LogMessageDALTest
    {
        [Fact]
        public void TestDBConnectionShouldBeOnLine()
        {
            var result = DbHelper.ExecuteDataTable(DatabaseEnum.Log4Net_SELECT, CommandType.Text, "select getdate();");
            result.ShouldNotBeNull();
            result.Rows.ShouldNotBeNull();
            result.Rows.Count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void TestInsert()
        {
            LogMessage log = new LogMessage();
            log.Ikey = Guid.NewGuid().ToString("N");
            log.Username = "test";
            log.ClientIP = "0.0.0.0";
            log.Content = "单元测试";
            log.Keyword = "测试";
            log.ServerIP = "127.0.0.1";
            log.OrderNo = "";
            log.Module = Assembly.GetExecutingAssembly().GetLoadedModules()[0].Name;
            log.LogType = "LogMessageDALTest";
            log.LogTime = DateTime.Now;
            log.IsHandle = false;

            typeof(LogMessageDAL).InvokeMember("Insert", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic, null,
                LogMessageDAL.Instance, new object[] { log });

        }

        [Fact]
        public void TestDALInstanceShouldBeNotNull()
        {
            var instance = LogMessageDAL.Instance;
            instance.ShouldNotBeNull();
        }

        [Fact]
        public void TestInsertShouldBeSuccess()
        {
            IList<LogMessage> messages = new List<LogMessage>();
            for (int i = 0; i < 1000; i++)
            {
                messages.Add(new LogMessage
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
                });
            }
            var effectRowsCount = LogMessageDAL.Instance.Insert(messages);
            effectRowsCount.ShouldBeGreaterThan(0);
            effectRowsCount.ShouldBe(1000);
        }

        [Fact]
        public void TestQueryLogDBShouldBeSuccess()
        {
            var result = LogMessageDAL.QueryLogDB("select top 3 * from tbl_interface_handlelog_201709");
            result.ShouldNotBeNull();
            result.Rows.ShouldNotBeNull();
            result.Rows.Count.ShouldBe(3);
        }

        [Fact]
        public void TestGetLogTableSuffixShouldBeSuccess()
        {
            var result = LogMessageDAL.Instance.GetLogTableSuffix();
            result.ShouldNotBeNullOrEmpty();
            result.ShouldBe(DateTime.Now.ToString("_yyyyMM"));
            Trace.WriteLine(result);
        }
    }
}
