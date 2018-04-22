using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace JinRi.LogCenter
{
    /// <summary>
    /// ���ݻ������������
    /// </summary>
    public interface IDataBufferPool<in T>
    {
        /// <summary>
        /// buffer��װ�����ݵ�����
        /// </summary>
        int BufferCurrentDataCount { get; }
        /// <summary>
        /// buffer������
        /// </summary>
        int BufferCount { get; }
        /// <summary>
        /// ˢ��ʱ�䣬��λ��
        /// </summary>
        int AutoFlushLogSeconds { get; }
        /// <summary>
        /// ˢ������
        /// </summary>
        void Flush();

        bool IsPoolFull { get; }
        bool IsPoolEmpty { get; }
        bool IsBlockWriteThread { get; }
        /// <summary>
        /// д������
        /// </summary>
        /// <param name="data"></param>
        void Write(T data);
        Task TimerFlushAsync();
        Task WriteAsync(T data, Action<object,Exception> action);
        event EventHandler<LogMessageEventArgs> OnDataHandle;
    }
}



