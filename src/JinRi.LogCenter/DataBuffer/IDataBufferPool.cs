using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace JinRi.LogCenter
{
    /// <summary>
    /// 数据缓冲器，管理池
    /// </summary>
    public interface IDataBufferPool<in T>
    {
        /// <summary>
        /// buffer的装载数据的数量
        /// </summary>
        int BufferCurrentDataCount { get; }
        /// <summary>
        /// buffer的数量
        /// </summary>
        int BufferCount { get; }
        /// <summary>
        /// 刷新时间，单位秒
        /// </summary>
        int AutoFlushLogSeconds { get; }
        /// <summary>
        /// 刷新数据
        /// </summary>
        void Flush();

        bool IsPoolFull { get; }
        bool IsPoolEmpty { get; }
        bool IsBlockWriteThread { get; }
        /// <summary>
        /// 写人数据
        /// </summary>
        /// <param name="data"></param>
        void Write(T data);
        Task TimerFlushAsync();
        Task WriteAsync(T data, Action<object,Exception> action);
        event EventHandler<LogMessageEventArgs> OnDataHandle;
    }
}



