using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace JinRi.LogCenter
{
    public class ClientHelper
    {
        /// <summary>
        /// 根据 User Agent 获取操作系统名称
        /// </summary>
        public static string GetCilentOSByUserAgent()
        {
            string osVersion = "未知";
            if (HttpContext.Current.Request == null) return osVersion;
            string userAgent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"];
            if (userAgent == null) return osVersion;
            if (userAgent.Contains("NT 6.0"))
            {
                osVersion = "Windows Vista/Server 2008";
            }
            else if (userAgent.Contains("NT 5.2"))
            {
                osVersion = "Windows Server 2003";
            }
            else if (userAgent.Contains("NT 5.1"))
            {
                osVersion = "Windows XP";
            }
            else if (userAgent.Contains("NT 5"))
            {
                osVersion = "Windows 2000";
            }
            else if (userAgent.Contains("NT 4"))
            {
                osVersion = "Windows NT4";
            }
            else if (userAgent.Contains("Me"))
            {
                osVersion = "Windows Me";
            }
            else if (userAgent.Contains("98"))
            {
                osVersion = "Windows 98";
            }
            else if (userAgent.Contains("95"))
            {
                osVersion = "Windows 95";
            }
            else if (userAgent.Contains("Mac"))
            {
                osVersion = "Mac";
            }
            else if (userAgent.Contains("Unix"))
            {
                osVersion = "UNIX";
            }
            else if (userAgent.Contains("Linux"))
            {
                osVersion = "Linux";
            }
            else if (userAgent.Contains("SunOS"))
            {
                osVersion = "SunOS";
            }
            return osVersion;
        }

        /// <summary>
        /// 获取客户端IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetClientIP()
        {
            string returnResult = string.Empty;
            try
            {
                if (HttpContext.Current.Request != null)
                {
                    returnResult = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (null == returnResult || returnResult == string.Empty)
                    {
                        returnResult = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                    }
                    if (null == returnResult || returnResult == string.Empty)
                    {
                        returnResult = HttpContext.Current.Request.UserHostAddress;
                    }
                    if (null == returnResult || returnResult == string.Empty || !RegexHelper.IsValidIP(returnResult))
                    {
                        return "0.0.0.0";
                    }
                }
                else
                {
                    returnResult = "0.0.0.0";
                }
            }
            catch
            {
                returnResult = "0.0.0.0";
            }
            return returnResult;
        }

        /// <summary>
        /// 获取MAC地址
        /// </summary>
        /// <returns></returns>
        public static string GetClientMAC()
        {
            return TestMac.GetMAC();
        }

        /// <summary>
        /// 获取主机头
        /// </summary>
        /// <returns></returns>
        public static string GetClientHost()
        {
            return HttpContext.Current.Request != null ?
                System.Web.HttpContext.Current.Request.Url.Host.ToString() : "";
        }

        /// <summary>  
        /// 获取浏览器版本号  
        /// </summary>  
        /// <returns></returns>  
        public static string GetBrowser()
        {
            if (System.Web.HttpContext.Current.Request != null)
            {
                HttpBrowserCapabilities bc = HttpContext.Current.Request.Browser;
                return bc.Browser + bc.Version;
            }
            else
            {
                return "";
            }
        }


        private class TestMac
        {

            private static Dictionary<string, string> remoteDicMAC = new Dictionary<string, string>();
            private static object remoteDicMACLock = new object();
            public static string GetMAC()
            {
                string strClientIP = GetClientIP();
                string mac = "";
                if (remoteDicMAC.TryGetValue(strClientIP, out mac))
                {
                    return mac;
                }
                lock (remoteDicMACLock)
                {
                    if (remoteDicMAC.TryGetValue(strClientIP, out mac))
                    {
                        return mac;
                    }
                    UInt32 ldest = inet_addr(strClientIP); //目的地的ip   
                                                           //Int32 lhost = inet_addr(""); //本地服务器的ip   
                    Int64 macinfo = 0;
                    Int32 len = 6;
                    int res = SendARP(ldest, 0, ref macinfo, ref len);
                    string mac_src = macinfo.ToString("X");
                    if (mac_src == "0")
                    {
                        remoteDicMAC[strClientIP] = String.Empty;
                        return String.Empty;
                    }
                    while (mac_src.Length < 12)
                    {
                        mac_src = mac_src.Insert(0, "0");
                    }
                    string mac_dest = "";
                    Regex rex = new Regex(@"(\w{2})");
                    if (rex.IsMatch(mac_src))
                    {
                        Stack<string> macList = new Stack<string>();
                        MatchCollection mCollect = rex.Matches(mac_src);
                        foreach (Match p in mCollect)
                        {
                            macList.Push(p.Value);
                        }
                        mac_dest = string.Join("-", macList.ToArray());
                    }
                    remoteDicMAC[strClientIP] = mac_dest;
                    return mac_dest;
                }
            }

            /// <summary>  
            /// 返回IP地址的MAC地址  
            /// </summary>  
            /// <param name="dest"></param>  
            /// <param name="host"></param>  
            /// <param name="mac"></param>  
            /// <param name="length"></param>  
            /// <returns></returns>  
            [DllImport("Iphlpapi.dll")]
            private static extern int SendARP(UInt32 dest, Int32 host, ref Int64 mac, ref Int32 length);

            /// <summary>  
            /// 通过IP返回IP的int数字表示法  
            /// </summary>  
            /// <param name="ip"></param>  
            /// <returns></returns>  
            [DllImport("Ws2_32.dll")]
            private static extern UInt32 inet_addr(string ip);
        }
    }
}
