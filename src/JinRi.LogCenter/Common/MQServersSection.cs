using System;
using System.Configuration;
using System.Xml;

namespace JinRi.LogCenter
{
    /// <summary>
    /// 服务器节点配置域
    /// </summary>
    public class MQServersSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public MQServersCollection MQServers
        {
            get
            {
                MQServersCollection col = (MQServersCollection)base[""];
                return col;
            }
        }
    }
}
