using System;

namespace JinRi.LogCenter
{
    using EasyNetQ;

    /// <summary>
    /// noop logger
    /// </summary>
    public class EasyNetQLogger : IEasyNetQLogger
    {
        private static readonly log4net.ILog m_logger = AppSetting.Log(typeof(EasyNetQLogger));

        public void DebugWrite(string message)
        {
#if DEBUG
            m_logger.Debug(message);
#else
            return;
#endif
        }
        public void DebugWrite(string format, params object[] args)
        {
#if DEBUG
            m_logger.DebugFormat(format, args);
#else
                        return;
#endif
        }

        public void InfoWrite(string format, params object[] args)
        {
            m_logger.InfoFormat(format, args);
        }

        public void ErrorWrite(string format, params object[] args)
        {
            m_logger.ErrorFormat(format, args);
        }

        public void ErrorWrite(Exception exception)
        {
            m_logger.Error(exception.Message, exception);
        }
    }
}