using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JinRi.LogCenter
{
    public class LogMessageEventArgs : EventArgs
    {
        public LogMessageEventArgs(object data)
        {
            Message = data;
        }
        public object Message { get; set; }
    }
}
