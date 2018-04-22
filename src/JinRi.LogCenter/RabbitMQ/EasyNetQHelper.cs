using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EasyNetQ;
using EasyNetQ.Loggers;
using EasyNetQ.Topology;
using log4net;

namespace JinRi.LogCenter
{
    public class EasyNetQHelper
    {
        private class ServerInner
        {
            public string ServerCode { get; set; }
            public string RootingKey { get; set; }
            public IQueue Queue { get; set; }
            public IExchange Exchage { get; set; }
        }

        private static readonly Dictionary<string, ServerInner> s_serverDic;
        private static readonly IBus s_bus = null;
        static ILog m_log = AppSetting.Log(typeof(EasyNetQHelper));
        private const string Prefix = "JinRi.LogCenter";
        static EasyNetQHelper()
        {
            try
            {
                string connectionString = AppSetting.RabbitMQHost;
                s_bus = RabbitHutch.CreateBus(connectionString, x => x.Register<IEasyNetQLogger, EasyNetQLogger>());
                m_log.Info("初始化IBus成功： connectionString=" + connectionString);
                //初始化队列路由bind
                s_serverDic = new Dictionary<string, ServerInner>();
                Register();
            }
            catch (Exception ex)
            {
                m_log.Error("初始化IBus异常：" + ex.ToString());
            }
        }

        private static void Register()
        {
            var slist = RabbitMQConfig.ServerInfoList;
            foreach (var server in slist)
            {
                var exchange = s_bus.Advanced.ExchangeDeclare(server.Exchange, server.ExchangeType);
                var queue = s_bus.Advanced.QueueDeclare(server.Queue);
                s_bus.Advanced.Bind(exchange, queue, server.RootingKey);
                s_serverDic[server.Code] = new ServerInner
                {
                    ServerCode = server.Code,
                    Exchage = exchange,
                    Queue = queue,
                    RootingKey = server.RootingKey
                };
            }
        }

        public static void Send(string queue, string message, bool isPersistent = false)
        {
            try
            {
                string exchangeName = string.Format("{0}.{1}.direct", Prefix, queue);
                string queueName = string.Format("{0}.{1}", Prefix, queue);
                string routingKey = string.Format("{0}.routingkey", queueName);

                var _exchange = s_bus.Advanced.ExchangeDeclare(exchangeName, ExchangeType.Direct);
                var _queue = s_bus.Advanced.QueueDeclare(queueName);
                s_bus.Advanced.Bind(_exchange, _queue, routingKey);

                MessageProperties messageProperties = new MessageProperties();
                messageProperties.DeliveryMode = isPersistent ? (byte)2 : (byte)1;

                var body = Encoding.UTF8.GetBytes(message);

                s_bus.Advanced.Publish(_exchange, routingKey, false, false, messageProperties, body);
            }
            catch (Exception ex)
            {
                m_log.Error("发布消息异常：" + ex.ToString());
            }
        }
        /// <summary>
        /// 泛型支持
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="code"></param>
        /// <param name="data"></param>
        /// <param name="isPersistent"></param>
        public static Task<int> SendAsync<T>(string code, T data, bool isPersistent = false) where T : class
        {
            try
            {
                if (!s_serverDic.ContainsKey(code)) return Task.FromResult(0);

                MessageProperties messageProperties = new MessageProperties();
                messageProperties.DeliveryMode = isPersistent ? (byte)2 : (byte)1;

                ServerInner server = s_serverDic[code];
                IMessage<T> message = new Message<T>(data, messageProperties);

                s_bus.Advanced.PublishAsync<T>(server.Exchage, server.RootingKey, false, false, message);
                return Task.FromResult(0);
            }
            catch (Exception ex)
            {
                m_log.Error("发布消息异常：" + ex.ToString());
                return Task.FromResult(1);
            }
        }

        public static void Receive(string queue, Action<string> func)
        {
            try
            {
                string queueName = string.Format("{0}.{1}", Prefix, queue);
                string _queueName = queue;
                var _queue = s_bus.Advanced.QueueDeclare(queueName);
                s_bus.Advanced.Consume(_queue, (body, properties, info) => Task.Factory.StartNew(() =>
                {
                    var message = Encoding.UTF8.GetString(body);
                    try
                    {
                        func(message);
                    }
                    catch (Exception ex)
                    {
                        m_log.Error(string.Format("调用{0}异常，异常信息：{1}", func.Method.Name, ex.ToString()));
                        //if (!_queueName.EndsWith("error", StringComparison.OrdinalIgnoreCase))
                        //{
                        //    _queueName = string.Format("{0}.error", _queueName);
                        //}
                        //Send(_queueName, message, true);
                    }
                }));
            }
            catch (Exception ex)
            {
                m_log.Error("消费消息异常：" + ex.ToString());
            }
        }

        public static Task ReceiveAsyn<T>(string code, Action<IMessage<T>> func) where T : class
        {
            try
            {
                if (!s_serverDic.ContainsKey(code)) return Task.FromResult(0);
                ServerInner server = s_serverDic[code];

                s_bus.Advanced.Consume<T>(server.Queue, (data, info) =>
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        func(data);
                    }
                    catch (Exception ex)
                    {
                        m_log.Error(string.Format("调用{0}异常，异常信息：{1}", func.Method.Name, ex.ToString()));
                    }

                }));
            }
            catch (Exception ex)
            {
                m_log.Error("消费消息异常：" + ex.ToString());
            }
            return Task.FromResult(0);
        }

    }
}
