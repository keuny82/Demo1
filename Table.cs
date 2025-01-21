// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using System.Reflection;
//using System.Web;
//using System.Web.UI.WebControls;
//using WebTool.Utils;

namespace Demo1
{
    public interface ITableLoader
    {
        bool Load();
    }

    public class TableLoaderContainer : ThreadSafeSingleton<TableLoaderContainer>
    {
        private string m_TestTablePath = string.Empty;

        public string GetTableSrc()
        {
            if (!string.IsNullOrEmpty(m_TestTablePath))
                return m_TestTablePath;

            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

            m_TestTablePath = directory.FullName + "\\table\\";
            return m_TestTablePath;
        }

        private List<ITableLoader> m_loaders = new List<ITableLoader>();

        public object RegistLoaderContentExplainTableLoader { get; internal set; }

        public void RegistLoader(ITableLoader loader)
        {
            m_loaders.Add(loader);
        }

        public string Load()
        {
            System.Text.StringBuilder resultMessage = new System.Text.StringBuilder();

            foreach (var loader in m_loaders)
            {
                if (!loader.Load())
                {
                    resultMessage.Append(loader.GetType().Name + "Load Fail!!!");/* 로드 실패!*/
                    resultMessage.Append(@"\r\n");
                }
            }

            return resultMessage.ToString();
        }
    }
}
