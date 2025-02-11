﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VSXmlToMarkdown
{
    public partial class Main : Form
    {
        //使用Windows Api AnimateWindow
        [DllImport("user32.dll", EntryPoint = "AnimateWindow")]
        private static extern bool AnimateWindow(IntPtr handle, int ms, int flags);

        private string[] m_FilenNames;
        public static List<string> m_fileList = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Main"/> class.
        /// </summary>
        /// <example>
        ///    <code>
        ///    </code>
        /// </example>
        /// <exception cref="System.Exception">
        /// </exception>
        public Main()
        {
            InitializeComponent();
        }

        private void btnBroswer_Click(object sender, EventArgs e)
        {


            MarkDownHelper.m_LogList = new List<string>();

            openFileDialog1.Filter = "Special Files(*.xml)|*.xml";
            openFileDialog1.Multiselect = true;

            DialogResult result = openFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {

                txtFile.Text = string.Join(";", openFileDialog1.FileNames);

                m_FilenNames = openFileDialog1.FileNames;
                string path1 = AppDomain.CurrentDomain.BaseDirectory + "/doc";
                if (!Directory.Exists(path1))
                {
                    Directory.CreateDirectory(path1);
                }

                Directory.Delete(path1, true);

                foreach (var item in m_FilenNames)
                {
                    try
                    {
                        MarkDownHelper.Generate(item);
                    }
                    catch (Exception ex)
                    {
                        MarkDownHelper.m_LogList.Add("");
                        MarkDownHelper.m_LogList.Add("发生异常，异常XML：" + item);
                        MarkDownHelper.m_LogList.Add("异常信息：" + ex.ToString());
                        MarkDownHelper.m_LogList.Add("");
                    }
                }

                GetDirectory(path1, AppDomain.CurrentDomain.BaseDirectory);

                string fileNames = "'" + string.Join("','", m_fileList) + "'";

                File.WriteAllText("files.txt", fileNames);

                txtFileNames.Text = fileNames;

                MarkDownHelper.m_LogList.Add("------------生成完成---------------");
            }

            listBox1.Items.Clear();
            foreach (var item in MarkDownHelper.m_LogList)
            {
                listBox1.Items.Add(item);
            }


        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            AnimateWindow(this.Handle, 5000, 0);
        }


        public static int GetDirectory(string srcPath, string roopath)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(srcPath);
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //获取目录下（不包含子目录）的文件和子目录
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo)     //判断是否文件夹
                    {
                        //GetDirectory(i.FullName, roopath);    //递归调用复制子文件夹

                        DirectoryInfo dir1 = new DirectoryInfo(i.FullName);
                        FileSystemInfo[] fileinfo1 = dir1.GetFiles();

                        foreach (FileSystemInfo files in fileinfo1)
                        {
                            m_fileList.Add("/" + files.FullName.Replace(roopath, "").Replace("\\", "/"));
                        }
                    }
                    else
                    {
                        m_fileList.Add("/" + i.FullName.Replace(roopath, "").Replace("\\", "/"));
                    }
                }

                return 0;
            }
            catch (Exception e)
            {
                return -1;
            }
        }
    }
}
