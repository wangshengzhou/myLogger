using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.IO;
using log4net.Config;
using log4net;

namespace JinRi.LogCenter
{
    public class AppSetting
    {
        private class LogWrap
        {
            ILog _log;
            public LogWrap(string fileName)
            {
                XmlConfigurator.ConfigureAndWatch(new FileInfo(fileName));
            }

            public ILog Log
            {
                get
                {
                    if (_log == null)
                    {
                        _log = LogManager.GetLogger(typeof(AppSetting));
                    }
                    return _log;
                }
            }

            public ILog GetLog(Type t)
            {
                return LogManager.GetLogger(t);
            }
        }


        #region 初始化配置节点

        private static string s_configFilename = null;
        private static Configuration s_config;
        private static readonly LogWrap appLog;


        static AppSetting()
        {
            SetConfigFilename();
            if (!string.IsNullOrEmpty(s_configFilename))
            {
                ExeConfigurationFileMap configMap = new ExeConfigurationFileMap { ExeConfigFilename = s_configFilename };
                s_config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
                appLog = new LogWrap(Log4NetFile);
                appLog.Log.Info("初始化配置文件成功");
            }
        }


        private static void SetConfigFilename()
        {
            s_configFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogCenter.config");
            if (!File.Exists(s_configFilename))
            {
                s_configFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "LogCenter.config");
            }
            if (!File.Exists(s_configFilename))
            {
                s_configFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "LogCenter.config");
            }
            if (!File.Exists(s_configFilename))
            {
                throw new FileNotFoundException("未找到配置文件[LogCenter.config]");
            }
        }

        public static Configuration AppConfiguration
        {
            get
            {
                return s_config;
            }
        }

        public static ILog Log(Type t)
        {
            return appLog.GetLog(t);
        }


        public static void TestWrite(string message)
        {
            appLog.Log.Info(message);
        }

        #endregion

        #region AppSetting 节点

        /// <summary>
        /// 数据缓冲器大小
        /// </summary>
        public static int DataBufferSize
        {
            get
            {
                string val = GetAppValue("DataBufferSize");
                int ival = 300;
                int.TryParse(val, out ival);
                return ival;
            }
        }


        /// <summary>
        /// 获取MQ消息服务器队列索引
        /// </summary>
        public static int ServerCodeInt
        {
            get
            {
                string val = GetAppValue("ServerCode");
                int ival = 0;
                int.TryParse(val, out ival);
                return ival;
            }
        }

        public static int AutoFlushSecond
        {
            get
            {
                string val = GetAppValue("AutoFlushSecond");
                int ival = 60;
                int.TryParse(val, out ival);
                return ival;
            }
        }

        /// <summary>
        /// 数据库提供程序名称
        /// </summary>
        public static int DataBufferPoolSize
        {
            get
            {
                string val = GetAppValue("DataBufferPoolSize");
                int ival = 100;
                int.TryParse(val, out ival);
                return ival;
            }
        }

        /// <summary>
        /// 日志配置文件
        /// </summary>
        public static string Log4NetFile
        {
            get
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, GetAppValue("log4net.Config"));
            }
        }

        public static string RabbitMQHost
        {
            get
            {
                return GetAppValue("RabbitMQHost");
            }
        }
        /// <summary>
        /// 数据库提供程序名称
        /// </summary>
        public static string EncryptKey
        {
            get
            {
                return GetAppValue("EncryptKey");
            }
        }
        /// <summary>
        /// 数据库提供程序名称
        /// </summary>
        public static string ProviderName
        {
            get
            {
                return GetAppValue("ProviderName");
            }
        }



        #endregion

        #region 辅助函数

        /// <summary>
        /// 获取配置项的值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetAppValue(string name)
        {
            Configuration config = AppConfiguration;
            if (config != null)
            {
                KeyValueConfigurationElement elm = config.AppSettings.Settings[name];
                if (elm != null)
                {
                    return elm.Value;
                }
            }
            return "";
        }

        #endregion
    }
}
