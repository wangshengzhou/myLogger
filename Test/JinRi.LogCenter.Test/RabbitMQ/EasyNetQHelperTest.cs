using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using System.Diagnostics;
using Newtonsoft.Json;
using EasyNetQ;
using System.Threading;

namespace JinRi.LogCenter.Test.RabbitMQ
{
    public class EasyNetQHelperTest
    {

        [Fact]
        public void TestRegister()
        {


        }

        [Fact]
        public void TestSendT()
        {
            ServerInfo server = RabbitMQConfig.ServerInfoList[0];
            for (int i = 0; i < 1000; i++)
            {
                var data = new DataObj { Index = i, Des = "测试" + i, CreateTime = DateTime.Now };
                EasyNetQHelper.SendAsync(server.Code, data);
            }
        }


        [Fact]
        public void TestSendList()
        {
            ServerInfo server = RabbitMQConfig.ServerInfoList[1];
            IList<DataObj> list = new List<DataObj>();
            for (int i = 0; i < 10; i++)
            {
                var data = new DataObj { Index = i, Des = "测试" + i, CreateTime = DateTime.Now };
                list.Add(data);
            }
            //DataObjCollection col = list;
            EasyNetQHelper.SendAsync(server.Code, list);

            Thread.Sleep(1000 * 60 * 10);
        }

        [Fact]
        public void TestReceiveT()
        {
            ServerInfo server = RabbitMQConfig.ServerInfoList[0];
            EasyNetQHelper.ReceiveAsyn<DataObj>(server.Code, x => Print(x));

            Thread.Sleep(1000 * 60 * 10);
        }

        [Fact]
        public void TestReceiveList()
        {
            ServerInfo server = RabbitMQConfig.ServerInfoList[1];
            EasyNetQHelper.ReceiveAsyn<IList<DataObj>>(server.Code, x => PrintList(x));

            Thread.Sleep(1000 * 60 * 10);
        }


        private void Print(IMessage<DataObj> data)
        {
            var body = data.Body;
            Debug.WriteLine(JsonConvert.SerializeObject(body));
        }

        private void PrintList(IMessage<IList<DataObj>> data)
        {
            var body = data.Body;
            foreach (var d in body)
            {
                Debug.WriteLine(JsonConvert.SerializeObject(d));
            }
        }
    }
}
