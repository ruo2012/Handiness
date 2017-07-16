﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
namespace Handiness.Metadata
{
    /*-------------------------------------------------------------------------
 * 版权所有：王浪静
 * 作者：王浪静
 * 联系方式：932444208@qq.com
 * 创建时间： 2017/7/14 20:51:00
 * 版本号：v1.0
 * .NET 版本：4.0
 * 本类主要用途描述：用于将本地文件中的Schema信息读取到内存中，或者将内存中的Schema信息
 * 保存到本地文件中
 *  -------------------------------------------------------------------------*/
    /// <summary>
    /// 管理Schema信息
    /// </summary>
    public class SchemaBuffer
    {


        /*********************************/
        public SchemaBuffer(params String[] files)
        {
            IEnumerable<SchemaXml> schemaXmls = Load(files);

        }
        public ColumnSchema this[String columnKey, String tableKey = null, String fileName = null]
        {
            get
            {
                return this.GetColumnSchema(columnKey, tableKey, fileName);
            }
        }
        /// <summary>
        /// 从已有的Schema信息中返回一个满足条件的<see cref="ColumnSchema"/>信息，当条件不全时，返回遇到的第一个满足条件值
        /// </summary>
        /// <param name="columnKey">必须的</param>
        /// <param name="tableKey"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public ColumnSchema GetColumnSchema(String columnKey, String tableKey = null, String fileName = null)
        {
            return null;
        }
        public static IEnumerable<SchemaXml> Load(params String[] files)
        {
            foreach (String file in files)
            {
                if (!File.Exists(file)) break;
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(SchemaXml));
                using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    SchemaXml schema = xmlSerializer.Deserialize(stream) as SchemaXml;
                    if (schema != null) yield return schema;
                }
            }
        }
        /// <summary>
        /// 将内存中的Schema信息保存到本地磁盘上
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="schemas">schema信息</param>
        public static void Save(String path, params SchemaXml[] schemas)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(SchemaXml));
            path = path.EndsWith("\\") ? path : path + "\\";
            foreach (var schema in schemas)
            {
                String fileName = $"{schema.Name}.xml";
                String filePath = path + fileName;
                File.Delete(filePath);
                using (FileStream stream = new FileStream(filePath,
                    FileMode.Create, FileAccess.Write, FileShare.Read,
                    8096,
                    FileOptions.WriteThrough))
                {
                    TextWriter writer = new StreamWriter(stream);
                    xmlSerializer.Serialize(writer, schema);
                }
            }
        }
    }
}