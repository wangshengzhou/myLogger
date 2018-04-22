using System;
using System.Configuration;

namespace JinRi.LogCenter
{
    /// <summary>
    /// 定义Server节点集合
    /// </summary>
    public class MQServersCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new MQServerElement();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MQServerElement)element).Code;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }
        protected override string ElementName
        {
            get
            {
                return "add";
            }
        }

        public MQServerElement this[int index]
        {
            get
            {
                return (MQServerElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }
    }
}
