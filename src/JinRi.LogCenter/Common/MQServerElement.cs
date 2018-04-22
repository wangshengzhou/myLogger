using System;
using System.Configuration;
using System.Xml;

namespace JinRi.LogCenter
{
    /// <summary>
    /// 服务器节点对象
    /// </summary>
    public class MQServerElement : ConfigurationElement
    {
        public MQServerElement()
        {
        }

        /// <summary>
        /// 服务器唯一标示
        /// </summary>
        [ConfigurationProperty("Code", DefaultValue = "服务器唯一标识", IsRequired = true, IsKey = true)]
        public string Code
        {
            get
            {
                return (string)this["Code"];
            }
            set
            {
                this["Code"] = value;
            }
        }

        /// <summary>
        /// 服务器IP地址
        /// </summary>
        [ConfigurationProperty("Exchange",  IsRequired = true)]
        public string Exchange
        {
            get
            {
                return (string)this["Exchange"];
            }
            set
            {
                this["Exchange"] = value;
            }
        }

        /// <summary>
        /// 服务端口号
        /// </summary>
        [ConfigurationProperty("Queue",  IsRequired = true)]
        public string Queue
        {
            get
            {
                return (string)this["Queue"];
            }
            set
            {
                this["Queue"] = value;
            }
        }

        /// <summary>
        /// 采用协议
        /// </summary>
        [ConfigurationProperty("RootingKey",  IsRequired = true)]
        public string RootingKey
        {
            get
            {
                return (string)this["RootingKey"];
            }
            set
            {
                this["RootingKey"] = value;
            }
        }

        /// <summary>
        /// 服务器性能指数
        /// </summary>
        [ConfigurationProperty("ExchangeType",  IsRequired = true)]
        public string ExchangeType
        {
            get
            {
                return (string)this["ExchangeType"];
            }
            set
            {
                this["ExchangeType"] = value;
            }
        }

        protected override bool IsModified()
        {
            bool ret = base.IsModified();

            // Enter your custom processing code here.
            Console.WriteLine("UrlConfigElement.IsModified() called.");
            return ret;
        }
    }
}
