using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using log4net;

namespace JinRi.LogCenter.Test
{

    public class AppSettingTest
    {
        [Fact]
        public void TestAppConfiguration()
        {
            var config = AppSetting.AppConfiguration;

            config.ShouldNotBeNull();
            AppSetting.DataBufferSize.ShouldBe(300);
            AppSetting.DataBufferPoolSize.ShouldBe(100);
            Debug.WriteLine(AppSetting.DataBufferSize);
        }

        [Fact]
        public void TestLogWrite()
        {
            AppSetting.TestWrite("测试log是否正常");
        }

        [Fact]
        public void TestWrite()
        {
            for (int i = 0; i < 10; i++)
            {
                var log = AppSetting.Log(typeof(AppSettingTest));
                log.Info("测试log是否正常");
            }
        }
    }
}
