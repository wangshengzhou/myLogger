using Newtonsoft.Json;
using System;

namespace JinRi.LogCenter
{
    [Serializable]
    public class LogMessage
    {


        #region 属性

        /// <summary>
        /// 日志编号
        /// </summary>
        public int ID
        {
            get; set;
        }

        /// <summary>
        /// ikey，表示一次请求的所有日志相同的Key值
        /// </summary>
        public string Ikey
        {
            get; set;
        }

        /// <summary>
        /// 请求者的用户名
        /// </summary>
        public string Username
        {
            get; set;
        }

        /// <summary>
        /// 日志时间
        /// </summary>
        public DateTime LogTime
        {
            get; set;
        }

        /// <summary>
        /// 请求者的IP地址
        /// </summary>
        public string ClientIP
        {
            get; set;
        }

        /// <summary>
        /// 服务器的IP地址
        /// </summary>
        public string ServerIP
        {
            get; set;
        }

        /// <summary>
        /// 日志所在的模块信息
        /// </summary>
        public string Module
        {
            get; set;
        }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo
        {
            get; set;
        }

        /// <summary>
        /// 日志类型
        /// </summary>
        public string LogType
        {
            get; set;
        }

        /// <summary>
        /// 供检索所用的关键词(主要用于统计数据)
        /// </summary>
        public string Keyword
        {
            get; set;
        }

        /// <summary>
        /// 日志信息
        /// </summary>
        public string Content
        {
            get; set;
        }

        /// <summary>
        /// 是输入输出日志，还是过程日志(true: 输入输出日志，反之)
        /// </summary>
        public bool IsHandle
        {
            get; set;
        }

        #endregion

        public override string ToString()
        {
            return string.Format("{0}:{1}, ", "Ikey", Ikey) +
                   string.Format("{0}:{1}, ", "Username", Username) +
                   string.Format("{0}:{1}, ", "LogTime", LogTime) +
                   string.Format("{0}:{1}, ", "ClientIp", ClientIP) +
                   string.Format("{0}:{1}, ", "ServerIP", ServerIP) +
                   string.Format("{0}:{1}, ", "Module", Module) +
                   string.Format("{0}:{1}, ", "OrderNo", OrderNo) +
                   string.Format("{0}:{1}, ", "LogType", LogType) +
                   string.Format("{0}:{1}, ", "Keyword", Keyword) +
                   string.Format("{0}:{1}", "IsHandle", IsHandle);
            //return JsonConvert.SerializeObject(this, new JsonSerializerSettings { Formatting = Formatting.Indented });
        }
    }

    public static class LogMessageFactory
    {
        public static LogMessage CreateHandleLogEntity(string logType, string iKey)
        {
            var handlelog = new LogMessage
            {
                Ikey = iKey,
                Username = "nouser",
                Module = "nomodule",
                LogType = logType,
                Content = "nocontent",
                IsHandle = false,
                Keyword = "nokeyword",
                OrderNo = "noorderno",
                ClientIP = ClientHelper.GetClientIP(),
                ServerIP = IPHelper.GetLocalIP(),
            };
            if (string.IsNullOrEmpty(iKey))
            {
                handlelog.Ikey = Guid.NewGuid().ToString();
            }
            return handlelog;
        }
    }
}
