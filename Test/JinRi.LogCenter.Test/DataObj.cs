using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JinRi.LogCenter.Test
{
    [Serializable]
    public class DataObj
    {
        public int Index { get; set; }
        public string Des { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
