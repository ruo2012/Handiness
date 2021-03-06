﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Data;
using Handiness.Services;
using Handiness.Adaptive;
using System.Reflection;
using System.IO;
namespace Handiness.Metadata
{
    public abstract class MetadataProvider : IMetadataProvider
    {
        public abstract String Version { get; }
        public abstract String Explain { get; }
        public String DatabaseName => this.GetDatabaseName();
        /********************/

        private DbConnection _connection = null;
        protected virtual DbConnection Connection { get => this._connection; set => this._connection = value; }
        protected abstract String GetDatabaseName();

        /********************/

        protected MetadataProvider() { }

        /********************/
        public virtual void Close()
        {
            if (this._connection.State == System.Data.ConnectionState.Open)
            {
                this._connection.Close();
            }
        }
        public virtual void Dispose()
        {
            this.Close();
            this._connection.Dispose();
        }
        public virtual void Open(String connectionString)
        {
            connectionString = connectionString.Trim();
             if (String.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException(TextResources.ErrorOfConnectionStringWithEmpty);
            }
            switch (this.Connection.State)
            {
                case ConnectionState.Open:
                    {
                        if (this.Connection.ConnectionString.ToLower() != connectionString.ToLower())
                        {
                            this.Connection.Close();
                            this.Connection.ConnectionString = connectionString;
                            this.Connection.Open();
                        }
                    }break;
                default:
                    {
                        this._connection.ConnectionString = connectionString;
                        this._connection.Open();
                    }
                    break;
            }
        }

        /********************/
        public abstract IEnumerable<ColumnSchema> GetColumnSchemas(String tableName);
        public abstract IEnumerable<TableSchema> GetTableSchemas();

        #region 列信息获取操作
        /// <summary>
        /// 根据列的元数据信息判断此列是否为主键
        /// </summary>
        protected abstract Boolean IsPrimaryKey(DataRow row);
        /// <summary>
        /// 根据列的元数据信息获取此列的长度信息
        /// </summary>
        protected abstract String GetLength(DataRow row);
        /// <summary>
        ///  根据列的元数据信息判断此列是否可为空
        /// </summary>
        protected abstract Boolean IsNullable(DataRow row);
        /// <summary>
        /// 将原始的元数据信息转换成<see cref="ColumnSchema"/> 实例
        /// </summary>
        protected abstract ColumnSchema ColumnMetadataToSechma(DataRow row);
        /// <summary>
        /// 将原始的元数据信息转换成<see cref="TableSchema"/> 实例
        /// </summary>
        protected abstract TableSchema TableMetadataToSechma(DataRow row);
        /// <summary>
        /// 根据列的元数据信息获取此列的注释
        /// </summary>
        protected abstract String GetExplain(DataRow row);
        /// <summary>
        /// 获取表的注释信息
        /// </summary>
        protected abstract String GetTableExplain(DataRow row);
        #endregion


        /// <summary>
        /// 用以导出指定GUID的数据库适配层的<see cref="IMetadataProvider"/>接口的实现类的实例
        /// </summary>
        /// <param name="adaptiveGuid">适配层的Guid</param>
        /// <param name="directory">指定的目录，默认为本程序集所在目录</param>
        /// <returns>返回实现此接口类的实例</returns>
        public static IMetadataProvider ExportMetadataProvider(String adaptiveGuid, String directory = null)
        {
            IMetadataProvider instance = null;
            if (String.IsNullOrWhiteSpace(adaptiveGuid))
            {
                throw new ArgumentException(TextResources.ErrorOfALNameGuidWithEmpty);
            }
            directory = directory ?? Path.GetDirectoryName(directory ?? Assembly.GetExecutingAssembly().Location);
            DirectoryCatalog searchCatalog = new DirectoryCatalog(directory, TextResources.PatternOfALDllName);
            instance=InstanceExportService.GetExport<IMetadataProvider>(searchCatalog, adaptiveGuid);
            return instance;
        }
        /// <summary>
        /// 用以导出指定目录下所有<see cref="IMetadataProvider"/>接口的实现类的实例集合
        /// </summary>
        /// <param name="directory">指定的目录，默认为本程序集所在目录</param>
        public static IEnumerable<IMetadataProvider> ExportMetadataProviders(String directory = null)
        {
            List<IMetadataProvider> providerList = new List<IMetadataProvider>();
            directory =Path.GetDirectoryName(directory ?? Assembly.GetExecutingAssembly().Location);
            DirectoryCatalog searchCatalog = new DirectoryCatalog(directory, TextResources.PatternOfALDllName);
            var adaptives=AdaptiveSeacher.ExportAdaptiveExplain(searchCatalog);
            foreach (var adaptive in adaptives)
            {
              var provider=InstanceExportService.GetExport<IMetadataProvider>(searchCatalog, adaptive.Guid);
                if (provider != null)
                {
                    providerList.Add(provider);
                }
            }
            return providerList;
        }
    }
}
