using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSXmlToMarkdown
{
    /// <summary>
    /// 标准注释,,需要安装GhostDoc工具
    /// <group>Orm帮助类</group>
    /// <obsolete>是否弃用</obsolete>
    /// </summary>
    public class 标准类注释
    {

        /// <summary>
        /// Tests the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <example>
        ///    <code>
        ///    </code>
        /// </example>
        /// <exception cref="System.Exception">
        /// </exception>
        public void Test(string id)
        {
        }

        /// <summary>
        ///   <group></group>
        ///    <obsolete>于XXX被弃用</obsolete>
        /// </summary>
        /// <param name="NullCreate"></param>
        /// <param name="showTab"></param>
        /// <returns></returns>
        /// <example>
        ///    <code>
        ///    </code>
        /// </example>
        /// <exception cref="System.Exception">
        /// </exception>
        public int ReadCard(bool NullCreate, int showTab)
        {
            return 0;
        }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        /// <example>
        ///    <code>
        ///    </code>
        /// </example>
        /// <exception cref="System.Exception">
        /// </exception>
        public string Name { get; set; }
    }
}
