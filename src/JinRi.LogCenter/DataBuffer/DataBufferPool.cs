using System;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using log4net;

namespace JinRi.LogCenter
{
    /// <summary>
    /// ���ݻ����
    /// </summary>
    public class DataBufferPool : IDataBufferPool<object>
    {
        #region ����

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

        #region ����

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
        /// �����
        /// </summary>
        /// <param name="dataBufferSize">�����bufferPool������</param>
        /// <param name="dataBufferCount">buffer����</param>
        public DataBufferPool(int dataBufferSize, int dataBufferCount)
            : this(dataBufferSize, dataBufferCount, true)
        {
        }

        /// <summary>
        /// �����
        /// </summary>
        /// <param name="dataBufferSize">�����bufferPool������</param>
        /// <param name="dataBufferCount">��������buffer����</param>
        /// <param name="isBlockMainThread">�Ƿ�����write�߳�</param>
        public DataBufferPool(int dataBufferSize, int dataBufferCount, bool isBlockMainThread)
            : this(dataBufferSize, dataBufferCount, TimeSpan.FromSeconds(60), isBlockMainThread)
        {
        }
        /// <summary>
        /// �����
        /// </summary>
        /// <param name="dataBufferSize">�����bufferPool������</param>
        /// <param name="dataBufferCount">��������buffer����</param>
        /// <param name="autoFlushSeconds">ˢ��ʱ��<</param>
        public DataBufferPool(int dataBufferSize, int dataBufferCount, TimeSpan autoFlushSeconds)
            : this(dataBufferSize, dataBufferCount, autoFlushSeconds, true)
        {
        }

        /// <summary>
        /// �����
        /// </summary>
        /// <param name="dataBufferSize">�����bufferPool������</param>
        /// <param name="dataBufferCount">��������buffer����</param>
        /// <param name="autoFlushSeconds">ˢ��ʱ��</param>
        /// <param name="isBlockMainThread">�Ƿ�����write�߳�</param>
        public DataBufferPool(int dataBufferSize, int dataBufferCount, TimeSpan autoFlushSeconds, bool isBlockMainThread)
        {
            m_isBlockMainThread = isBlockMainThread;
            m_BufferSize = dataBufferSize;
            m_DataCount = dataBufferCount;
            m_AutoFlushTime = autoFlushSeconds;
            m_dataBuffer = new DataBuffer(m_DataCount);
            //��ʱˢ��
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
            //�������߳������������������������������߳�
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
        /// ���ֽ���һ������ȥ����
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
        /// ����ר��
        /// ����������������ڲ�������Ӱ������
        /// ʼ�ձ���һ������������
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

        #region ˽�з���

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