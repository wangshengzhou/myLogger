using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Shouldly;

namespace JinRi.LogCenter.Test
{
    public class RabbitMQConfigTest
    {
        [Fact]
        public void ServerInfoList()
        {
            IList<ServerInfo> list = RabbitMQConfig.ServerInfoList;
            list.ShouldNotBeNull();
            list.Count.ShouldBe(6);
        }
    }
}
