using System;
using System.Reflection;

namespace JinRi.LogCenter
{
    public class Null
    {
        private static readonly DateTime MinDateTime = new DateTime(1911, 11, 11, 11, 11, 11);
        public static short NullShort
        {
            get
            {
                return 0;
            }
        }

        public static int NullInteger
        {
            get
            {
                return 0;
            }
        }

        public static int NullLong
        {
            get
            {
                return 0;
            }
        }

        public static byte NullByte
        {
            get
            {
                return 0;
            }
        }

        public static float NullSingle
        {
            get
            {
                return 0f;
            }
        }

        public static double NullDouble
        {
            get
            {
                return 0d;
            }
        }

        public static decimal NullDecimal
        {
            get
            {
                return 0m;
            }
        }

        public static DateTime NullDate
        {
            get
            {
                return MinDateTime;
            }
        }

        public static string NullString
        {
            get
            {
                return "";
            }
        }

        public static bool NullBoolean
        {
            get
            {
                return false;
            }
        }

        public static Guid NullGuid
        {
            get
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        /// ��ȡĳ��ֵ���͵�Ĭ��ֵ
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetNull(Type type)
        {
            return SetNull(DBNull.Value, type);
        }

        /// <summary>
        /// �������objValueΪ�գ��򸳸����͵�Ĭ��ֵ
        /// </summary>
        /// <param name="objValue"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object SetNull(object objValue, Type type)
        {
            object returnValue = null;
            if (objValue == DBNull.Value)
            {
                if (type == typeof(short))
                {
                    returnValue = NullShort;
                }
                else if (type == typeof(byte))
                {
                    returnValue = NullByte;
                }
                else if (type == typeof(int))
                {
                    returnValue = NullInteger;
                }
                else if (type == typeof(long))
                {
                    returnValue = NullLong;
                }
                else if (type == typeof(float))
                {
                    returnValue = NullSingle;
                }
                else if (type == typeof(double))
                {
                    returnValue = NullDouble;
                }
                else if (type == typeof(decimal))
                {
                    returnValue = NullDecimal;
                }
                else if (type == typeof(DateTime))
                {
                    returnValue = NullDate;
                }
                else if (type == typeof(string))
                {
                    returnValue = NullString;
                }
                else if (type == typeof(bool))
                {
                    returnValue = NullBoolean;
                }
                else if (type == typeof(Guid))
                {
                    returnValue = NullGuid;
                }
                else //complex object
                {
                    returnValue = null;
                }
            }
            else //return value
            {
                returnValue = objValue;
            }
            return returnValue;
        }

        public static object SetNull(PropertyInfo objPropertyInfo)
        {
            object returnValue = null;
            switch (objPropertyInfo.PropertyType.ToString())
            {
                case "System.Int16":
                    returnValue = NullShort;
                    break;
                case "System.Int32":
                case "System.Int64":
                    returnValue = NullInteger;
                    break;
                case "system.Byte":
                    returnValue = NullByte;
                    break;
                case "System.Single":
                    returnValue = NullSingle;
                    break;
                case "System.Double":
                    returnValue = NullDouble;
                    break;
                case "System.Decimal":
                    returnValue = NullDecimal;
                    break;
                case "System.DateTime":
                    returnValue = NullDate;
                    break;
                case "System.String":
                case "System.Char":
                    returnValue = NullString;
                    break;
                case "System.Boolean":
                    returnValue = NullBoolean;
                    break;
                case "System.Guid":
                    returnValue = NullGuid;
                    break;
                default:
                    //Enumerations default to the first entry
                    Type pType = objPropertyInfo.PropertyType;
                    if (pType.BaseType.Equals(typeof(Enum)))
                    {
                        Array objEnumValues = Enum.GetValues(pType);
                        Array.Sort(objEnumValues);
                        returnValue = Enum.ToObject(pType, objEnumValues.GetValue(0));
                    }
                    else //complex object
                    {
                        returnValue = null;
                    }
                    break;
            }
            return returnValue;
        }

        public static bool IsNull(ValueType objField)
        {
            bool isNull = false;
            if (objField != null)
            {
                if (objField is int)
                {
                    isNull = (int)objField == NullInteger;
                }
                else if (objField is short)
                {
                    isNull = (short)objField == NullShort;
                }
                else if (objField is long)
                {
                    isNull = (long)objField == NullLong;
                }
                else if (objField is byte)
                {
                    isNull = (byte)objField == NullByte;
                }
                else if (objField is float)
                {
                    isNull = (float)objField == NullSingle;
                }
                else if (objField is double)
                {
                    isNull = (double)objField == NullDouble;
                }
                else if (objField is decimal)
                {
                    isNull = (decimal)objField == NullDecimal;
                }
                else if (objField is DateTime)
                {
                    DateTime objDate = (DateTime)objField;
                    isNull = objDate.Date.Equals(NullDate.Date) ||
                        objDate.Date.Equals(DateTime.MinValue);
                }
                else //complex object
                {
                    isNull = false;
                }
            }
            else
            {
                isNull = true;
            }
            return isNull;
        }

        /// <summary>
        /// �ж�ֵobjField���Ƿ�Ϊ��ֵ
        /// </summary>
        /// <param name="objField"></param>
        /// <returns></returns>
        public static bool IsNull(object objField)
        {
            bool isNull = false;
            if (objField != null)
            {
                if (objField is int)
                {
                    isNull = objField.Equals(NullInteger);
                }
                else if (objField is short)
                {
                    isNull = objField.Equals(NullShort);
                }
                else if (objField is long)
                {
                    isNull = objField.Equals(NullLong);
                }
                else if (objField is byte)
                {
                    isNull = objField.Equals(NullByte);
                }
                else if (objField is float)
                {
                    isNull = objField.Equals(NullSingle);
                }
                else if (objField is double)
                {
                    isNull = objField.Equals(NullDouble);
                }
                else if (objField is decimal)
                {
                    isNull = objField.Equals(NullDecimal);
                }
                else if (objField is DateTime)
                {
                    DateTime objDate = (DateTime)objField;
                    isNull = objDate.Date.Equals(NullDate.Date) ||
                        objDate.Date.Equals(DateTime.MinValue);
                }
                else if (objField is string)
                {
                    isNull = objField.Equals(NullString);
                }
                else if (objField is bool)
                {
                    isNull = objField.Equals(NullBoolean);
                }
                else if (objField is Guid)
                {
                    isNull = objField.Equals(NullGuid);
                }
                else //complex object
                {
                    isNull = false;
                }
            }
            else
            {
                isNull = true;
            }
            return isNull;
        }
    }
}