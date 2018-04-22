using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JinRi.LogCenter
{
    [Serializable]
    public class ServerInfo
    {
        public string Code { get; set; }
        public string Exchange { get; set; }
        public string Queue { get; set; }
        public string RootingKey { get; set; }
        public string ExchangeType { get; set; }
        public ServerState State { get; set; }
    }
}
