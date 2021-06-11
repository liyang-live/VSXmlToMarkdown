using System;
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
                        MarkDownHelper.m_LogList.Add("发生异常，异常XML："+ item);
                        MarkDownHelper.m_LogList.Add("异常信息：" + ex.ToString());
                        MarkDownHelper.m_LogList.Add("");
                    }
                }
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
    }
}
