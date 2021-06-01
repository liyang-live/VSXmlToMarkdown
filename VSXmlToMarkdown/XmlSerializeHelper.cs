using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VSXmlToMarkdown
{

    /// <summary>
    /// xml序列化和反序列化
    /// </summary>
    public sealed class XmlSerializeHelper
    {
        /// <summary>
        /// xml反序列化
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="xmlOrPath">xml字符或者xml文件路径</param>
        /// <returns></returns>
        public static T XmlDeserialize<T>(string xmlOrPath)
        {
            using (FileStream file = new FileStream(xmlOrPath, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                T obj = (T)serializer.Deserialize(file);
                file.Close();
                return obj;
            }
        }

        /// <summary>
        /// xml序列化
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="obj">带有参数的实体</param>
        /// <returns></returns>
        public static string XmlSerialize<T>(T obj)
        {
            using (StringWriter sw = new StringWriter())
            {
                XmlSerializer serializer = new XmlSerializer(obj.GetType());
                serializer.Serialize(sw, obj);
                sw.Close();
                return sw.ToString();
            }
        }


        public static string Serialize<T>(T obj)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            serializer.Serialize(writer, obj);
            string xml = writer.ToString();
            writer.Close();
            writer.Dispose();

            return xml;
        }

        public static T Deserialize<T>(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            StringReader reader = new StringReader(xml);
            T result = (T)(serializer.Deserialize(reader));
            reader.Close();
            reader.Dispose();

            return result;
        }

    }
}
