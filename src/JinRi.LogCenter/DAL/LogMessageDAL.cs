using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using log4net;
using System.Runtime.Caching;

using System.Linq;

namespace JinRi.LogCenter
{
    public class LogMessageDAL : DbBase
    {

        private static readonly string m_localServerIP = IPHelper.GetLocalIP();
        private static readonly ILog Logger = AppSetting.Log(typeof(LogMessageDAL));
        private readonly static LogMessageDAL m_LogMessageDAL = new LogMessageDAL();
        private IDataBufferPool<object> _repeatBatchSaveHandlePool;
        private IDataBufferPool<object> _repeatBatchSaveProcessPool;

        private LogMessageDAL()
        {
            _repeatBatchSaveHandlePool = new DataBufferPool(20, 5, TimeSpan.FromSeconds(120));
            _repeatBatchSaveHandlePool.OnDataHandle += OnSendRequest;
            _repeatBatchSaveHandlePool.TimerFlushAsync();

            _repeatBatchSaveProcessPool = new DataBufferPool(20, 5, TimeSpan.FromSeconds(120), false);
            _repeatBatchSaveProcessPool.OnDataHandle += OnSendRequest;
            _repeatBatchSaveProcessPool.TimerFlushAsync();
        }

        public static LogMessageDAL Instance
        {
            get
            {
                return m_LogMessageDAL;
            }
        }


        public int Insert(IList<LogMessage> message)
        {
            return Insert(GetLogMessageTable(message), message[0].IsHandle);
        }

        public int Insert(DataTable table, bool isHandle)
        {
            int count = InsertInternal(table, isHandle);
            if (count == 0)
            {
                lock (m_repeatBatchSaveTimesLockObj)
                {
                    m_repeatBatchSaveTimes[table.TableName] = 1;
                }
                if (isHandle)
                {
                    _repeatBatchSaveHandlePool.Write(table);
                }
                else
                {
                    _repeatBatchSaveProcessPool.Write(table);
                }
            }
            return count;
        }

        private int InsertInternal(DataTable table, bool isHandle)
        {
            int count = 0;
            string tableName = "";
            try
            {
                using (SqlBulkCopy bult =
                     new SqlBulkCopy(ConnectionStringFactory.CreateConnectionString(DatabaseEnum.Log4Net_CMD))
                     {
                         BatchSize = 800
                     })
                {

                    tableName = "dbo.tbl_Interface_ProcessLog";
                    if (isHandle)
                    {
                        tableName = "dbo.tbl_Interface_HandleLog";
                    }
                    tableName = string.Format("{0}{1}", tableName, GetLogTableSuffix());

                    bult.ColumnMappings.Add("IKey", "ikey");
                    bult.ColumnMappings.Add("Username", "username");
                    bult.ColumnMappings.Add("LogTime", "logtime");
                    bult.ColumnMappings.Add("ClientIP", "clientip");
                    bult.ColumnMappings.Add("Module", "module");
                    bult.ColumnMappings.Add("OrderNo", "orderno");
                    bult.ColumnMappings.Add("LogType", "logtype");
                    bult.ColumnMappings.Add("Content", "content");
                    bult.ColumnMappings.Add("ServerIP", "ServerIP");
                    bult.ColumnMappings.Add("Keyword", "KeyWord");

                    bult.DestinationTableName = tableName;
                    bult.WriteToServer(table);
                    count = table.Rows.Count;
                    Logger.Info(string.Format("批量成功往{0}-{1}表插入{2}条数据", tableName, table.TableName, count));
                }
            }
            catch (Exception ex)
            {
                count = 0;
                Logger.Error(ex.ToString());
            }
            return count;
        }



        public static DataTable QueryLogDB(string sql)
        {
            try
            {
                DataSet dataset = new DataSet();
                SqlConnection conn = new SqlConnection(ConnectionStringFactory.CreateConnectionString(DatabaseEnum.Log4Net_CMD));
                SqlCommand command = conn.CreateCommand();
                command.CommandText = sql;
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(dataset);
                return dataset.Tables[0];
            }
            catch
            {
                return null;
            }
        }

        private static object m_repeatBatchSaveTimesLockObj = new object();
        private static Dictionary<string, int> m_repeatBatchSaveTimes = new Dictionary<string, int>(100);


        /// <summary>
        /// pool事件处理程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSendRequest(object sender, LogMessageEventArgs e)
        {
            IDataBuffer<object> buffer = e.Message as IDataBuffer<object>;
            if (buffer != null && buffer.Count > 0)
            {
                List<DataTable> list = buffer.GetList().Cast<DataTable>().ToList();
                RepeatBatchSaveLog(list, list[0].TableName.StartsWith("HandleLog"));
            }
        }

        /// <summary>
        /// 补偿机制，重试5次
        /// </summary>
        /// <param name="dataBuffer"></param>
        /// <param name="isHandle"></param>
        private void RepeatBatchSaveLog(List<DataTable> list, bool isHandle)
        {
            foreach (DataTable table in list)
            {
                string key = table.TableName;
                if (!m_repeatBatchSaveTimes.ContainsKey(key)) return;

                int count = m_repeatBatchSaveTimes[key];
                bool ret = InsertInternal(table, isHandle) > 0;
                if (!ret)
                {
                    if (count < 5)
                    {
                        m_repeatBatchSaveTimes[key] = count + 1;
                        if (isHandle)
                            _repeatBatchSaveHandlePool.Write(table);
                        else
                            _repeatBatchSaveProcessPool.Write(table);
                    }
                    else
                    {
                        lock (m_repeatBatchSaveTimesLockObj)
                        {
                            m_repeatBatchSaveTimes.Remove(key);
                        }
                        Logger.Info(string.Format("日志补偿保存，表名：【{0}】，更新次数：【{1}】，更新结果：失败-超过补偿次数，影响数据条数：【{2}】", key, count + 1, table.Rows.Count));
                    }
                    Logger.Info(string.Format("日志补偿保存，表名：【{0}】，更新次数：【{1}】，更新结果：【{2}】", key, count + 1, ret));
                }
                else
                {
                    lock (m_repeatBatchSaveTimesLockObj)
                    {
                        m_repeatBatchSaveTimes.Remove(key);
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private int Insert(LogMessage log)
        {
            int ret = 0;
            string tableName = "";
            try
            {
                tableName = "dbo.tbl_Interface_ProcessLog";
                if (log.IsHandle)
                {
                    tableName = "dbo.tbl_Interface_HandleLog";
                }
                tableName = string.Format("{0}{1}", tableName, GetLogTableSuffix());
                string sql = string.Format(@"insert into {0}(IKey, Username, LogTime, ClientIP, ServerIP, Module, Keyword, OrderNo, LogType, Content) 
                    values (@IKey, @Username, @LogTime, @ClientIP, @ServerIP, @Module, @Keyword, @OrderNo, @LogType, @Content)", tableName);

                List<DbParameter> paraList = new List<DbParameter>();
                AddParameter(paraList, "@IKey", log.Ikey);
                AddParameter(paraList, "@Username", log.Username);
                AddParameter(paraList, "@LogTime", log.LogTime);
                AddParameter(paraList, "@ClientIP", log.ClientIP);
                AddParameter(paraList, "@ServerIP", log.ServerIP);
                AddParameter(paraList, "@Module", log.Module);
                AddParameter(paraList, "@Keyword", log.Keyword);
                AddParameter(paraList, "@OrderNo", log.OrderNo);
                AddParameter(paraList, "@LogType", log.LogType);
                AddParameter(paraList, "@Content", log.Content);

                ret = DbHelper.ExecuteNonQuery(DatabaseEnum.Log4Net_CMD, CommandType.Text, sql, paraList.ToArray());
            }
            catch (Exception ex)
            {
                //丢弃日志
                Logger.Fatal(string.Format("往{0}表插入数据出现严重错误，{1}", tableName, log.ToString()), ex);
            }
            return ret;
        }

        private DataTable GetLogMessageTable(IList<LogMessage> message)
        {
            string tableName = message[0].IsHandle ? "HandleLog_" : "ProcessLog_";
            DataTable table = new DataTable(tableName + Guid.NewGuid().ToString("N"));

            #region 构建列

            DataColumn col = new DataColumn("IKey", typeof(String));
            col.DefaultValue = "";
            col.MaxLength = 100;
            table.Columns.Add(col);

            col = new DataColumn("Username", typeof(String));
            col.DefaultValue = "";
            col.MaxLength = 50;
            table.Columns.Add(col);

            col = new DataColumn("LogTime", typeof(DateTime));
            col.DefaultValue = DateTime.Now;
            table.Columns.Add(col);

            col = new DataColumn("ClientIP", typeof(String));
            col.DefaultValue = "";
            col.MaxLength = 50;
            table.Columns.Add(col);

            col = new DataColumn("ServerIP", typeof(String));
            col.DefaultValue = "";
            col.MaxLength = 25;
            table.Columns.Add(col);

            col = new DataColumn("Module", typeof(String));
            col.DefaultValue = "";
            col.MaxLength = 255;
            table.Columns.Add(col);

            col = new DataColumn("Keyword", typeof(String));
            col.DefaultValue = "";
            col.MaxLength = 255;
            table.Columns.Add(col);

            col = new DataColumn("OrderNo", typeof(String));
            col.DefaultValue = "";
            col.MaxLength = 50;
            table.Columns.Add(col);

            col = new DataColumn("LogType", typeof(String));
            col.DefaultValue = "";
            col.MaxLength = 100;
            table.Columns.Add(col);

            col = new DataColumn("Content", typeof(String));
            col.DefaultValue = "";
            table.Columns.Add(col);

            #endregion

            foreach (var log in message)
            {
                if (log != null)
                {
                    DataRow newRow = table.NewRow();
                    newRow["IKey"] = GetDefaultString(GetSubstring(log.Ikey, newRow.Table.Columns["IKey"].MaxLength), "noIkey");
                    newRow["Username"] = GetDefaultString(GetSubstring(log.Username, newRow.Table.Columns["Username"].MaxLength), "nouser");
                    newRow["LogTime"] = log.LogTime;
                    newRow["ClientIP"] = GetDefaultString(GetSubstring(log.ClientIP, newRow.Table.Columns["ClientIP"].MaxLength), "noip");
                    newRow["ServerIP"] = GetDefaultString(GetSubstring(log.ServerIP, newRow.Table.Columns["ServerIP"].MaxLength), "noip");
                    //newRow["ServerIP"] = GetDefaultString(GetSubstring(m_localServerIP, newRow.Table.Columns["ServerIP"].MaxLength), "noip");

                    newRow["Module"] = GetDefaultString(GetSubstring(log.Module, newRow.Table.Columns["Module"].MaxLength), "nomodule");
                    newRow["Keyword"] = GetDefaultString(GetSubstring(log.Keyword, newRow.Table.Columns["Keyword"].MaxLength), "nokeyword");
                    newRow["OrderNo"] = GetDefaultString(GetSubstring(log.OrderNo, newRow.Table.Columns["OrderNo"].MaxLength), "noorderno");
                    newRow["LogType"] = GetDefaultString(GetSubstring(log.LogType, newRow.Table.Columns["LogType"].MaxLength), "nologtype");
                    newRow["Content"] = GetDefaultString(log.Content, "nocontent");
                    table.Rows.Add(newRow);
                }
            }

            return table;
        }

        public string GetLogTableSuffix()
        {
            string cacheKey = "JinRi.LogCenter.LogMessageDAL.TableSuffix";
            string cacheVal = MemoryCache.Default.Get(cacheKey) as string;
            if (cacheVal == null)
            {
                DateTime now = GetLogServerTime();
                int leftSeconds = 60;
                cacheVal = string.Format("_{0:yyyyMM}", now);
                DateTime monEnd = new DateTime(now.Year, now.Month, 1, 0, 0, 0);
                monEnd = monEnd.AddMonths(1);
                if ((monEnd - now).TotalSeconds < leftSeconds)
                {
                    return cacheVal;
                }
                MemoryCache.Default.Set(cacheKey, cacheVal, monEnd.AddSeconds(-1 * leftSeconds));
            }
            return cacheVal;
        }

        public static DateTime GetLogServerTime()
        {
            return (DateTime)DbHelper.ExecuteScalar(DatabaseEnum.Log4Net_CMD, CommandType.Text, "select getdate()");
        }

        private string GetDefaultString(string input, string defaultValue)
        {
            return input ?? defaultValue;
        }

        private string GetSubstring(string input, int len)
        {
            if (!string.IsNullOrEmpty(input))
            {
                byte[] buff = System.Text.Encoding.GetEncoding("GB2312").GetBytes(input);
                if (buff.Length > len)
                {
                    input = System.Text.Encoding.GetEncoding("GB2312").GetString(buff, 0, len).Trim();
                }
            }
            return input;
        }

    }
}
