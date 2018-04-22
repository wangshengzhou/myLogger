using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

using log4net;
using System.Threading.Tasks;
using EasyNetQ;

namespace JinRi.LogCenter
{
    public static class DBLog
    {
        private static readonly string s_localServerIP;
        private static readonly IDataBufferPool<LogMessage> s_logHandlePool;
        private static readonly IDataBufferPool<LogMessage> s_logProcessPool;
        private static readonly ILog m_localLog = AppSetting.Log(typeof(DBLog));

        public static int LogHandleCount = 0;
        public static int LogProcessCount = 0;
        private static int s_PublihServerIndex = AppSetting.ServerCodeInt;
        static DBLog()
        {
            try
            {
                s_localServerIP = IPHelper.GetLocalIP();

                s_logHandlePool = new DataBufferPool(AppSetting.DataBufferPoolSize, AppSetting.DataBufferSize,
                    TimeSpan.FromSeconds(AppSetting.AutoFlushSecond));
                s_logHandlePool.OnDataHandle += OnSendRequest;
                s_logHandlePool.TimerFlushAsync();

                s_logProcessPool = new DataBufferPool(AppSetting.DataBufferPoolSize, AppSetting.DataBufferSize,
                    TimeSpan.FromSeconds(AppSetting.AutoFlushSecond), false);
                s_logProcessPool.OnDataHandle += OnSendRequest;
                s_logProcessPool.TimerFlushAsync();

                m_localLog.Info("初始化DataBufferPool成功");
            }
            catch (Exception ex)
            {
                m_localLog.Error(ex);
            }
        }

        public static void Process(this LogMessage logMessage)
        {
            s_logProcessPool.WriteAsync(logMessage, (data, ex) =>
            {
                LogMessage message = data as LogMessage;
                if (message != null)
                {
                    message.Content = string.Format("ex: {0}, log: {1}", ex.ToString(), logMessage.Content);
                }
                //发送失败，直接写本地库
                InsertLog(logMessage);
            });
        }
        public static void Handle(this LogMessage logMessage)
        {
            s_logHandlePool.WriteAsync(logMessage, (data, ex) =>
            {
                LogMessage message = data as LogMessage;
                if (message != null)
                {
                    message.Content = string.Format("ex: {0}, log: {1}", ex.ToString(), logMessage.Content);
                }
                //发送失败，直接写本地库
                InsertLog(logMessage);
            });
        }


        public static void Process(string userName, string iKey, string clientIP, string module, string orderNo,
            string logType, string content, string keyword, DateTime ctime)
        {

            LogMessage logMessage = new LogMessage
            {
                Ikey = iKey,
                Username = userName,
                Module = module,
                LogType = logType,
                Content = content,
                IsHandle = false,
                Keyword = keyword,
                OrderNo = orderNo,
                ClientIP = clientIP,
                ServerIP = s_localServerIP,
                LogTime = ctime
            };

            Process(logMessage);
        }

        /// <summary>
        /// 写入日志(执行过程)
        /// </summary>
        /// <param name="userName">用户账号</param>
        /// <param name="iKey">GUID</param>
        /// <param name="clientIP">客户端IP</param>
        /// <param name="module">操作模块(类名)</param>
        /// <param name="orderNo">订单号</param>
        /// <param name="logType">日志类型</param>
        /// <param name="content">日志内容</param>
        /// <param name="keyword">关键字(可用于统计信息的记录)</param>
        public static void Process(string userName, string iKey, string clientIP, string module, string orderNo,
            string logType, string content, string keyword = "")
        {
            Process(userName, iKey, clientIP, module, orderNo, logType, content, keyword, DateTime.Now);
        }

        /// <summary>
        /// 写入日志(请求值/返回值)
        /// </summary>
        /// <param name="userName">用户账号</param>
        /// <param name="iKey">GUID</param>
        /// <param name="clientIP">客户端IP</param>
        /// <param name="module">操作模块(类名)</param>
        /// <param name="orderNo">订单号</param>
        /// <param name="logType">日志类型</param>
        /// <param name="content">日志内容</param>
        /// <param name="keyword">关键字(可用于统计信息的记录)</param>
        public static void Handle(string userName, string iKey, string clientIP, string module, string orderNo, string logType, string content, string keyword = "")
        {
            LogMessage logMessage = new LogMessage
            {
                Ikey = iKey,
                Username = userName,
                Module = module,
                LogType = logType,
                Content = content,
                IsHandle = true,
                Keyword = keyword,
                OrderNo = orderNo,
                ClientIP = clientIP,
                ServerIP = s_localServerIP,
                LogTime = DateTime.Now
            };
            Handle(logMessage);
        }

        //立即批量提交本地缓冲池
        /// <summary>
        /// 立即批量提交本地缓冲池
        /// </summary>
        public static void FlushLogMessage()
        {
            try
            {
                s_logHandlePool.Flush();
                s_logProcessPool.Flush();
                while (true)
                {
                    if (s_logHandlePool.IsPoolEmpty)
                    {
                        break;
                    }
                    Thread.Sleep(100);
                }
                while (true)
                {
                    if (s_logProcessPool.IsPoolEmpty)
                    {
                        break;
                    }
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                m_localLog.Error(ex);
            }
        }

        #region 私有方法

        //直接插入日志表
        /// <summary>
        /// 直接插入日志表
        /// </summary>
        /// <param name="logMessage"></param>
        /// <param name="isHandle"></param>
        private static void InsertLog(LogMessage logMessage)
        {
            InsertLog(new List<LogMessage> { logMessage });
        }

        private static void InsertLog(IList<LogMessage> list)
        {
            LogMessageDAL.Instance.Insert(list);
        }

        /// <summary>
        /// 记录发送消息日志到库
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="content"></param>
        /// <param name="isHandle"></param>
        /// <param name="isSucess"></param>
        private static void RecordLogCenterState(string logType, string content, bool isHandle, bool isSucess)
        {
            LogMessage logMessage = new LogMessage()
            {
                Ikey = Guid.NewGuid().ToString(),
                LogType = logType,
                Module = "",
                Content = content,
                IsHandle = isHandle,
                Username = "nouser",
                Keyword = isSucess ? "Success" : "Failed",
                OrderNo = "",
                ClientIP = "",
                ServerIP = s_localServerIP,
                LogTime = DateTime.Now
            };
            InsertLog(logMessage);
        }

        private static async void OnSendRequest(object sender, LogMessageEventArgs e)
        {
            IDataBuffer<object> buffer = e.Message as IDataBuffer<object>;
            if (buffer == null) return;
            var list = buffer.GetList().Cast<LogMessage>().ToList();
            if (!list.Any()) return;

            bool isHandle = list[0].IsHandle;
            try
            {
                ServerInfo server = RabbitMQConfig.ServerInfoList[s_PublihServerIndex];
                var flag = await EasyNetQHelper.SendAsync(server.Code, list);
                if (flag > 0)
                {
                    //插入本地数据库
                    InsertLog(list);
                    string content = string.Format("本次批量插入{0}条数据，Task响应：{1}，IsHandle：{2}", list.Count, flag, isHandle);
                    RecordLogCenterState("分布式日志1.0", content, isHandle, false);
                }
            }
            catch (Exception ex)
            {
                m_localLog.Error(ex);
                string content = string.Format("批量插入数据异常，原因：{0}，IsHandle：{1}", ex.ToString(), isHandle);
                RecordLogCenterState("分布式日志1.0", content, isHandle, false);
            }
        }

        private static Task ConsumeAsync()
        {
            ServerInfo server = RabbitMQConfig.ServerInfoList[s_PublihServerIndex];
            var task = EasyNetQHelper.ReceiveAsyn<IList<LogMessage>>(server.Code, x => WriteToDb(x));
            return task;
        }

        private static void WriteToDb(IMessage<IList<LogMessage>> data)
        {
            InsertLog(data.Body);
        }

        public static void ConsumeForEach()
        {
            foreach (var server in RabbitMQConfig.ServerInfoList)
            {
                EasyNetQHelper.ReceiveAsyn<IList<LogMessage>>(server.Code, x => WriteToDb(x));
            }
        }

        #endregion
    }
}
