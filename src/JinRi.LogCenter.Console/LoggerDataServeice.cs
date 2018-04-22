using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JinRi.LogCenter.Console
{
    /// <summary>
    /// 帮助文档
    /// https://topshelf.readthedocs.io/en/latest/overview/commandline.html
    /// </summary>
    public class LoggerDataServeice : ILoggerDataServeice
    {
        ILog log = AppSetting.Log(typeof(LoggerDataServeice));
        public void Start()
        {
            try
            {
                log.Info("服务正在启动……");
                DBLog.ConsumeForEach();
                log.Info("服务启动完成！");
            }
            catch (Exception ex)
            {
                log.Error("服务启动异常，原因：" + ex.ToString());
            }
        }
        public void Stop()
        {
            log.Info("服务正在停止……");
            //...
            log.Info("服务正在停止完成");
        }
    }
}
