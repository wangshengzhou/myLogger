using System;
using System.IO;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Web.Caching;
using System.Data.Common;
using log4net;
using System.Collections.Generic;

namespace JinRi.LogCenter
{
    public class ConnectionHelper
    {
        private static readonly ILog _logger = AppSetting.Log(typeof(ConnectionHelper));
        private static IDictionary<string, string> _ConnectionString = new Dictionary<string,string>();
        public static string GetConnectionString(string keyName, string decodeKey)
        {
            try
            {
                string connectionStr = null;
                if (!_ConnectionString.TryGetValue(keyName, out connectionStr))
                {
                    string strTmpValue = GetCustomConfigConnectionString(keyName);
                    if (string.IsNullOrEmpty(strTmpValue))
                    {
                        strTmpValue =  GetLocalConfigConnectionString(keyName);
                    }
                    if (string.IsNullOrEmpty(strTmpValue))
                    {
                        return string.Empty;
                    }
                    _ConnectionString.Add(keyName, strTmpValue);
                    return Decrypt(strTmpValue, decodeKey);
                }
                return Decrypt(connectionStr, decodeKey);
            }
            catch(Exception ex)
            {
                _logger.Error("GetConnectionString出现异常：" + ex.ToString());
                return string.Empty;
            }
        }

        /// <summary>
        /// 使用指定密钥解密
        /// </summary>
        /// <param name="encrypted">密文</param>
        /// <param name="key">密钥</param>
        /// <returns>明文</returns>
        private static string Decrypt(string encrypted, string key)
        {
            Encoding encoding = Encoding.Default;
            byte[] encryptedBytes = Convert.FromBase64String(encrypted);
            byte[] keyBytes = Encoding.Default.GetBytes(key);

            TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
            MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
            des.Key = hashmd5.ComputeHash(keyBytes);
            hashmd5 = null;
            des.Mode = CipherMode.ECB;
            byte[] bytes = des.CreateDecryptor().TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return encoding.GetString(bytes);
        }

        private static string GetLocalConfigConnectionString(string keyName)
        {
            ConnectionStringSettings setting = ConfigurationManager.ConnectionStrings[keyName];
            if (setting == null)
            {
                setting = ConfigurationManager.ConnectionStrings[keyName.Replace("_CMD", "").Replace("_SELECT", "")];
            }
            return setting.ConnectionString;
        }

        private static ConnectionStringSettings GetLocalConfigConnection(string keyName)
        {
            ConnectionStringSettings setting = ConfigurationManager.ConnectionStrings[keyName];
            if (setting == null)
            {
                setting = ConfigurationManager.ConnectionStrings[keyName.Replace("_CMD", "").Replace("_SELECT", "")];
            }
            return setting;
        }

        private static string GetCustomConfigConnectionString(string keyName)
        {
            ConnectionStringSettings setting = AppSetting.AppConfiguration.ConnectionStrings.ConnectionStrings[keyName];
            if (setting == null)
            {
                setting = AppSetting.AppConfiguration.ConnectionStrings.ConnectionStrings[keyName.Replace("_CMD", "").Replace("_SELECT", "")];
            }
            return setting.ConnectionString;
        }
    }
}
