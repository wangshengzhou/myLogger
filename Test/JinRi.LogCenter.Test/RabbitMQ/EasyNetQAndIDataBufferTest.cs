using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Threading;
using System.IO;

using EasyNetQ;
using log4net;
using Xunit;
using Shouldly;

namespace JinRi.LogCenter.Test.RabbitMQ
{
    public class EasyNetQAndIDataBufferTest
    {
        const int DataBufferPoolSize = 300;
        const int DataBufferSize = 400;
        ILog log = AppSetting.Log(typeof(EasyNetQAndIDataBufferTest));

        /// <summary>
        /// 系统测试
        /// </summary>
        [Fact]
        public void TestSystem()
        {
            //1、生产数据
            var task1 = ProduceAsync();
            //2、消费消费
            var task2 = ConsumeAsync();
            //3、查询数据
            //Task.WaitAll(task1, task2);
            //packageCount.ShouldBe(DataBufferPoolSize);
            //messageCount.ShouldBe(DataBufferPoolSize * DataBufferSize + 1);

            Thread.Sleep(1000 * 60 * 5);
        }

        /// <summary>
        /// 生产数据
        /// </summary>
        private Task ProduceAsync()
        {
            DataBufferPool pool = new DataBufferPool(AppSetting.DataBufferPoolSize, AppSetting.DataBufferSize);
            pool.OnDataHandle += OnWrite;
            pool.TimerFlushAsync();

            //1.写据数
            //var task1 = new TaskFactory().StartNew(() => Write(pool, AppSetting.DataBufferPoolSize * AppSetting.DataBufferSize + 1));
            var task1 = new TaskFactory().StartNew(() => Write(pool, DataBufferPoolSize * DataBufferSize + 1));
            return task1;
        }

        private void Write(DataBufferPool pool, int count)
        {
            for (int i = 0; i < count; i++)
            {
                pool.Write(new DataObj { Index = i, Des = "测试" + i, CreateTime = DateTime.Now });
            }
        }
        /// <summary>
        /// 批量生产消息,发消息队列
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWrite(object sender, LogMessageEventArgs e)
        {
            IDataBuffer<object> data = e.Message as IDataBuffer<object>;
            if (data != null)
            {
                var list = data.GetList().Cast<DataObj>().ToList();
                ServerInfo server = RabbitMQConfig.ServerInfoList[3];
                EasyNetQHelper.SendAsync(server.Code, list);
            }
        }

        private Task ConsumeAsync()
        {
            ServerInfo server = RabbitMQConfig.ServerInfoList[3];
            var task = EasyNetQHelper.ReceiveAsyn<IList<DataObj>>(server.Code, x => PrintList(x));
            return task;
        }

        int messageCount = 0;
        int packageCount = 0;
        private void PrintList(IMessage<IList<DataObj>> data)
        {
            packageCount++;
            var body = data.Body;
            foreach (var d in body)  // d  as DataObj
            {
                messageCount++;
                log.Info(JsonConvert.SerializeObject(d));
                //Debug.WriteLine(JsonConvert.SerializeObject(d));
            }
        }
    }
}
