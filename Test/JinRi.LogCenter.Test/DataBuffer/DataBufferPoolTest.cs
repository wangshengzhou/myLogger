using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Shouldly;

namespace JinRi.LogCenter.Test
{
    /// <summary>
    /// 测试并发访问缓冲区
    /// 单个线程写，多个线程读
    /// </summary>
    public class DataBufferPoolTest
    {
        const int dataBufferSize = 1000;
        const int dataBufferCount = 3;

        [Fact]
        public void TestWrite()
        {
            DataBufferPool pool = new DataBufferPool(dataBufferSize, dataBufferCount, false);
            pool.OnDataHandle += WriteFile;

            //1.写据数
            var task1 = new TaskFactory().StartNew(() => Write(pool, dataBufferSize * dataBufferCount + 1));
            Thread.Sleep(50);

            //2.查看bufferpool状态
            var task2 = new TaskFactory().StartNew(() => Show(pool));

            Task.WaitAll(task1, task2);
            Debug.WriteLine("结束   ,BufferCount=" + pool.BufferCount + "   ,BufferCurrentDataCount=" + pool.BufferCurrentDataCount);
            //3.断言
            pool.BufferCount.ShouldBe(dataBufferSize);

            //4.消费数据
            var task3 = pool.TimerFlushAsync();
            //刷新数据
            //Debug.WriteLine("刷新数据...");
            //pool.Flush();

            //测试是否重复写入池中
            new TaskFactory().StartNew(() => Empty(pool));

            Thread.Sleep(1000 * 60 * 60 * 1);
        }

        [Fact]
        public void TestWrite2()
        {
            DataBufferPool pool = new DataBufferPool(dataBufferSize, dataBufferCount);
            pool.OnDataHandle += WriteFile;
            pool.TimerFlushAsync();

            //1.写据数
            var task1 = new TaskFactory().StartNew(() => Write(pool, dataBufferSize * dataBufferCount + 1));

            Task.WaitAll(task1);
            Debug.WriteLine("结束   ,BufferCount=" + pool.BufferCount + "   ,BufferCurrentDataCount=" + pool.BufferCurrentDataCount);


            //测试是否重复写入池中
            new TaskFactory().StartNew(() => Empty(pool));

            Thread.Sleep(1000 * 60 * 60 * 1);
        }

        /// <summary>
        /// 写数据
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="count"></param>
        private void Write(DataBufferPool pool, int count)
        {
            for (int i = 0; i < count; i++)
            {
                pool.Write(new DataObj { Index = i, Des = "测试" + i, CreateTime = DateTime.Now });
            }
        }

        /// <summary>
        /// 判断数据是否重复写到池中
        /// </summary>
        /// <param name="pool"></param>
        private void Empty(DataBufferPool pool)
        {
            while (true)
            {
                if (pool.IsPoolEmpty)
                {
                    break;
                }
            }
            Task.Delay(1000 * 5).Wait();
            //断言
            tmpList.Count.ShouldBe(dataBufferSize * dataBufferCount + 1);

            new TaskFactory().StartNew(() =>
            {
                File.AppendAllText(CreateFile(), JsonConvert.SerializeObject(
                tmpList.ToLookup(x => x.Index)
                    .Select(x => new { Index = x.Key, Count = x.Count() })
                    .OrderByDescending(x => x.Count).OrderBy(x => x.Index).ToList()));
            });
        }

        private void Show(DataBufferPool pool)
        {
            int i = 1;
            while (true)
            {
                Debug.WriteLine("第" + i + "轮   ,BufferCount=" + pool.BufferCount + "   ,BufferCurrentDataCount=" + pool.BufferCurrentDataCount);
                Thread.Sleep(5);
                i++;
                if (pool.BufferCount == dataBufferSize)
                {
                    break;
                }
            }
            Debug.WriteLine("第" + i + "轮   ,BufferCount=" + pool.BufferCount + "   ,BufferCurrentDataCount=" + pool.BufferCurrentDataCount);
        }

        List<DataObj> tmpList = new List<DataObj>();
        private void WriteFile(object sender, LogMessageEventArgs e)
        {
            string path = CreateFile();
            IDataBuffer<object> data = e.Message as IDataBuffer<object>;
            if (data != null)
            {
                foreach (var d in data)
                {
                    //File.AppendAllText(path, JsonConvert.SerializeObject(d) + Environment.NewLine);
                    Debug.WriteLine(JsonConvert.SerializeObject(d));
                    tmpList.Add(d as DataObj);
                }
            }
        }

        [Fact]
        public string CreateFile()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestLog", "data.txt");
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            return path;
        }
       
    }
}

