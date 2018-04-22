using System;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using log4net;

namespace JinRi.LogCenter
{
    /// <summary>
    /// 数据缓冲池
    /// </summary>
    public class DataBufferPool : IDataBufferPool<object>
    {
        #region 变量

        private readonly ConcurrentQueue<IDataBuffer<object>> m_dataQueue = new ConcurrentQueue<IDataBuffer<object>>();
        private readonly Scheduler scheduler = new ActionScheduler();
        private readonly object m_newDataBufferLockObj = new object();
        private readonly object m_consumeLockObj = new object();
        private readonly int m_BufferSize;
        private readonly int m_DataCount;

        private IDataBuffer<object> m_dataBuffer;
        private TimeSpan m_AutoFlushTime;
        private bool m_isBlockMainThread = true;
        private bool m_isConsume = false;

        ILog log = AppSetting.Log(typeof(DataBufferPool));
        public event EventHandler<LogMessageEventArgs> OnDataHandle;

        #endregion

        #region 属性

        public int BufferCount
        {
            get
            {
                return m_dataQueue.Count;
            }
        }

        public int BufferCurrentDataCount
        {
            get
            {
                return m_dataBuffer.Count;
            }
        }


        public int AutoFlushLogSeconds
        {
            get
            {
                return m_AutoFlushTime.Seconds;
            }
        }

        public bool IsBlockWriteThread
        {
            get { return m_isBlockMainThread; }
        }

        public bool IsPoolFull
        {
            get
            {
                return BufferCount >= m_BufferSize;
            }
        }

        public bool IsPoolEmpty
        {
            get
            {
                return m_dataQueue.IsEmpty;
            }
        }

        #endregion

        /// <summary>
        /// 缓冲池
        /// </summary>
        /// <param name="dataBufferSize">缓冲池bufferPool的容量</param>
        /// <param name="dataBufferCount">buffer容量</param>
        public DataBufferPool(int dataBufferSize, int dataBufferCount)
            : this(dataBufferSize, dataBufferCount, true)
        {
        }

        /// <summary>
        /// 缓冲池
        /// </summary>
        /// <param name="dataBufferSize">缓冲池bufferPool的容量</param>
        /// <param name="dataBufferCount">单个缓冲buffer容量</param>
        /// <param name="isBlockMainThread">是否阻塞write线程</param>
        public DataBufferPool(int dataBufferSize, int dataBufferCount, bool isBlockMainThread)
            : this(dataBufferSize, dataBufferCount, TimeSpan.FromSeconds(60), isBlockMainThread)
        {
        }
        /// <summary>
        /// 缓冲池
        /// </summary>
        /// <param name="dataBufferSize">缓冲池bufferPool的容量</param>
        /// <param name="dataBufferCount">单个缓冲buffer容量</param>
        /// <param name="autoFlushSeconds">刷新时间<</param>
        public DataBufferPool(int dataBufferSize, int dataBufferCount, TimeSpan autoFlushSeconds)
            : this(dataBufferSize, dataBufferCount, autoFlushSeconds, true)
        {
        }

        /// <summary>
        /// 缓冲池
        /// </summary>
        /// <param name="dataBufferSize">缓冲池bufferPool的容量</param>
        /// <param name="dataBufferCount">单个缓冲buffer容量</param>
        /// <param name="autoFlushSeconds">刷新时间</param>
        /// <param name="isBlockMainThread">是否阻塞write线程</param>
        public DataBufferPool(int dataBufferSize, int dataBufferCount, TimeSpan autoFlushSeconds, bool isBlockMainThread)
        {
            m_isBlockMainThread = isBlockMainThread;
            m_BufferSize = dataBufferSize;
            m_DataCount = dataBufferCount;
            m_AutoFlushTime = autoFlushSeconds;
            m_dataBuffer = new DataBuffer(m_DataCount);
            //定时刷新
            scheduler.Start(autoFlushSeconds, () => AutoFlush());
        }

        public void Flush()
        {
            m_dataBuffer.Flush();
            AutoFlush();
        }

        public void Write(object data)
        {
            bool result = m_dataBuffer.Write(data);
            if (!result)
            {
                if (m_dataBuffer.IsFull)
                {
                    m_dataQueue.Enqueue(m_dataBuffer);
                    NewDataBuffer();
                }
                Write(data);
            }
            //消费者线程已启动，如果缓冲池满，则阻塞主线程
            while (IsBlockWriteThread && m_isConsume)
            {
                if (!IsPoolFull)
                {
                    break;
                }
                Thread.Sleep(100);
            }
        }

        public Task WriteAsync(object data, Action<object, Exception> action)
        {
            try
            {
                //Write(data);
                return Task.Run(() => Write(data));
            }
            catch (Exception ex)
            {
                try
                {
                    action(data, ex);
                    log.Error(ex);
                }
                catch (Exception)
                {
                    // ignored
                }
                return Task.FromResult(1);
            }
        }

        /// <summary>
        /// 保持仅有一个任务去消费
        /// </summary>
        /// <returns></returns>
        public Task TimerFlushAsync()
        {
            if (!m_isConsume)
            {
                lock (m_consumeLockObj)
                {
                    if (!m_isConsume)
                    {
                        m_isConsume = true;
                        return Task.Run(() => CallStack());
                    }
                }
            }
            return Task.FromResult(0);
        }



        /// <summary>
        /// 测试专用
        /// 多个任务启动，由于并发读，影响性能
        /// 始终保持一个任务在运行
        /// </summary>
        /// <param name="consumCount"></param>
        private void ConsumeTest(int consumCount)
        {
            Task[] taskArr = new Task[consumCount];
            for (int i = 0; i < consumCount; i++)
            {
                taskArr[i] = new Task(CallStack);
                taskArr[i].Start();
            }
        }

        #region 私有方法

        private void AutoFlush()
        {
            if (m_dataBuffer != null && m_dataBuffer.Count > 0)
            {
                m_dataQueue.Enqueue(m_dataBuffer);
                NewDataBuffer();
            }
        }

        private void NewDataBuffer()
        {
            lock (m_newDataBufferLockObj)
            {
                m_dataBuffer = new DataBuffer(m_DataCount);
            }
        }

        private void CallStack()
        {
            while (true)
            {
                if (m_dataQueue.Count > 0)
                {
                    IDataBuffer<object> dataBuffer = null;
                    if (m_dataQueue.TryDequeue(out dataBuffer))
                    {
                        if (dataBuffer != null)
                        {
                            Callback(this, new LogMessageEventArgs(dataBuffer));
                        }
                    }
                }

                if (m_dataQueue.IsEmpty)
                {
                    Thread.Sleep(100);
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }

        private void Callback(object sender, LogMessageEventArgs e)
        {
            if (OnDataHandle != null)
            {
                OnDataHandle(sender, e);
            }

        }
        #endregion
    }
}