using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace JinRi.LogCenter
{
    /// <summary>
    /// Êý¾Ý»º³åÆ÷
    /// </summary>
    public interface IDataBuffer<T> where T : class
    {
        WaitCallback Callback { get; }
        string BufferId { get; }
        int BufferSize { get; }
        int Count { get; }
        bool IsFull { get; }
        void Flush();
        bool Write(T data);
        List<T> GetList();
        IEnumerator<T> GetEnumerator();
    }
}



