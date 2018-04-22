using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JinRi.LogCenter
{
    public class RabbitMQConfig
    {
        static readonly IList<ServerInfo> serverList;
        static RabbitMQConfig()
        {
            serverList = new List<ServerInfo>();
            var section = AppSetting.AppConfiguration.GetSection("MQServers") as MQServersSection;
            foreach (var e in section.MQServers)
            {
                var mqServer = e as MQServerElement;
                if (mqServer == null) break;

                serverList.Add(new ServerInfo
                {
                    Code = mqServer.Code,
                    Exchange = mqServer.Exchange,
                    Queue = mqServer.Queue,
                    RootingKey = mqServer.RootingKey,
                    ExchangeType = mqServer.ExchangeType,
                    State = ServerState.Inactivated
                });
            }
        }

        public static IList<ServerInfo> ServerInfoList
        {
            get
            {
                return serverList;
            }
        }
    }
}
