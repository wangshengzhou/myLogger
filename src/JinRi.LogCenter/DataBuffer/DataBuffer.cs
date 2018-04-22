using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Collections.Concurrent;

namespace JinRi.LogCenter
{
    /// <summary>
    /// Êý¾Ý»º³åÆ÷
    /// </summary>
    public class DataBuffer : IDataBuffer<object>
    {
        private readonly WaitCallback m_callback;
        private readonly int m_bufferSize;
        private readonly ConcurrentBag<object> m_buffer;

        public WaitCallback Callback
        {
            get
            {
                return m_callback;
            }
        }

        public int BufferSize
        {
            get
            {
                return m_bufferSize;
            }
        }

        public int Count
        {
            get
            {
                return m_buffer.Count;
            }
        }

        public string BufferId
        {
            get; set;
        }


        public DataBuffer(int bufferSize)
            : this(null, bufferSize)
        {
        }

        public DataBuffer(WaitCallback callback, int bufferSize)
        {
            m_callback = callback;
            m_bufferSize = bufferSize;
            m_buffer = new ConcurrentBag<object>();
            BufferId = Guid.NewGuid().ToString();
        }

        public bool IsFull
        {
            get
            {
                return Count == m_bufferSize;
            }
        }

        public void Flush()
        {
            if (Callback != null)
            {
                ThreadPool.QueueUserWorkItem(Callback, this);
            }
        }

        public bool Write(object data)
        {

            if (!IsFull)
            {
                m_buffer.Add(data);
                return true;
            }
            else
            {
                return false;
            }
        }

        public IEnumerator<object> GetEnumerator()
        {
            return m_buffer.GetEnumerator();
        }

        public List<object> GetList()
        {
            return m_buffer.Select(x => x).ToList();
        }
    }
}