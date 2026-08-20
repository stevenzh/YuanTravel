using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Collections;
using Arch.Common.Models;

namespace Arch.Common.Utils
{
    public class XMLTools
    {
        private XDocument xdoc = null;

        private string path;

        /// <summary>
        /// 实例化时，必须传递XML文档的完整路径
        /// </summary>
        /// <param name="xmlpath"></param>
        public XMLTools(string xmlpath)
        {
            xdoc = XDocument.Load(xmlpath);
            path = xmlpath;
        }

        public XmlDocument GetXmlDoc()
        {
            XmlDocument xdoc = new XmlDocument(); //创建XmlDocument对象; 
            //测试服务器测试
            xdoc.Load(Path.GetFullPath(path.Trim()));
            return xdoc;
        }

        /// <summary>
        /// 获取节点串联间的文本
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <returns></returns>
        public string GetValueByNodeName(string nodeName)
        {
            try
            {
                var value = (from temp in xdoc.Descendants()
                             where temp.Name == nodeName
                             select temp).FirstOrDefault();
                return value.Value;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取节点属性的值
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <param name="attributeName">属性名称</param>
        public string GetAttributeValue(string nodeName, string attributeName)
        {
            try
            {
                var node = (from temp in xdoc.Descendants()
                            where temp.Name == nodeName
                            select temp).FirstOrDefault();
                return node.Attribute(attributeName).Value;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 通过key获取标签元素之间的文本
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValueByKey(string key)
        {
            try
            {
                var result = (from temp in xdoc.Descendants()
                              where temp.Attribute("key") != null && temp.Attribute("key").Value == key
                              select temp).FirstOrDefault();
                return result.Value;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 通过key获取节点的中的value
        /// 因为考虑到会有多个属性
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Hashtable GetValueByXml(string key)
        {
            try
            {
                var value = (from temp in xdoc.Descendants()
                             where temp.Attribute("key") != null && temp.Attribute("key").Value == key
                             select temp).FirstOrDefault();
                Hashtable result = new Hashtable();
                foreach (XAttribute attr in value.Attributes())
                {
                    if (attr.Name == "key") { continue; }
                    result.Add(attr.Name, attr.Value);
                }
                return result;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取字典XML中的数据
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <returns></returns>
        public IList<KeyValueBean> GetDictionaryList(string nodeName)
        {
            XElement root = XElement.Load(path);
            if (nodeName.IsNullOrEmpty())
                return new List<KeyValueBean>();
            var result = from temp in root.Element(nodeName).Elements()
                         select new KeyValueBean()
                         {
                             Key = temp.Attribute("Key").Value,
                             Value = temp.Value
                         };
            return result.ToList();
        }

        //public List<SelectListItem> GetSelectListItemList(string nodeName)
        //{
        //    XElement root = XElement.Load(path);
        //    var result = from temp in root.Element(nodeName).Elements()
        //                 select new SelectListItem()
        //                 {
        //                     Value = temp.Attribute("Key").Value,
        //                     Text = temp.Value
        //                 };
        //    return result.ToList();
        //}
        /// <summary>
        /// 获取字典XML中某个节点的值
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <param name="key">键</param>
        /// <returns></returns>
        public KeyValueBean GetDictionary(String nodeName, string key)
        {
            XElement root = XElement.Load(path);
            var result = (from temp in root.Element(nodeName).Elements()
                          where temp.Attribute("Key").Value == key
                          select new KeyValueBean()
                          {
                              Key = temp.Attribute("Key").Value,
                              Value = temp.Value
                          }).FirstOrDefault();
            return result;
        }
        public List<KeyValueBean> GetDictionarys(string nodeName)
        {
            XElement root = XElement.Load(path);
            var result = (from temp in root.Element(nodeName).Elements()
                          select new KeyValueBean()
                          {
                              Key = temp.Attribute("Key").Value,
                              Value = temp.Value
                          }).ToList();
            return result;
        }
        /// <summary>
        /// 汉字转换PinYin
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public string ConvertPinYin(string word)
        {
            return ConvertPinYin(word, word.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="word"></param>
        /// <param name="Cnt">几个字母</param>
        /// <returns></returns>
        private string ConvertPinYin(string word, int Cnt)
        {
            string pinYin = string.Empty;
            if (string.IsNullOrEmpty(word))
                return string.Empty;

            string firstName = GetPinYin(word.Substring(0, 1));
            for (int i = 1; i < Cnt; i++)
            {
                pinYin += GetPinYin(word[i].ToString());
            }
            return firstName + pinYin;
        }


        private string GetPinYin(string word)
        {
            DataSet ds = new DataSet();
            XmlDocument xmlDoc = new XmlDocument();
            string pinYin = string.Empty;

            ds.ReadXml(Path.GetFullPath(path));
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                if (ds.Tables[0].Rows[i]["chinese"].Equals(word))
                {
                    pinYin = ds.Tables[0].Rows[i]["english"].ToString();
                    break;
                }
            }

            return pinYin;
        }

        /// <summary>
        /// 汉字转换PinYin  加空格
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public string ConvertPinYinTrim(string word)
        {
            return ConvertPinYinTrim(word, word.Length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="word"></param>
        /// <param name="Cnt">几个字母</param>
        /// <returns></returns>
        private string ConvertPinYinTrim(string word, int Cnt)
        {
            string pinYin = string.Empty;
            if (string.IsNullOrEmpty(word))
                return string.Empty;

            string firstName = GetPinYinTrim(word.Substring(0, 1));
            for (int i = 1; i < Cnt; i++)
            {
                pinYin += GetPinYinTrim(word[i].ToString());
            }
            return firstName + pinYin;
        }

        private string GetPinYinTrim(string word)
        {
            DataSet ds = new DataSet();
            XmlDocument xmlDoc = new XmlDocument();
            string pinYin = string.Empty;

            ds.ReadXml(Path.GetFullPath(path));
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                if (ds.Tables[0].Rows[i]["chinese"].Equals(word))
                {
                    pinYin = ds.Tables[0].Rows[i]["english"].ToString() + " ";
                    break;
                }
            }

            return pinYin;
        }
    }

}
