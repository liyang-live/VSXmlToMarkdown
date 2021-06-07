using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System;
using System.Runtime.InteropServices;

namespace VSXmlToMarkdown
{
    public static class MarkDownHelper
    {

        public static List<string> m_LogList = new List<string>();
        public static List<SeeasoLink> m_UrlList = new List<SeeasoLink>();

        public static Doc m_Doc = new Doc();

        /// <summary>
        /// Generates the header.
        /// </summary>
        /// <returns></returns>
        public static void Generate(string path)
        {
            //path = @"Lenovo.HIS.Common.xml";

            // path = @"ConsoleApp2.xml";

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(File.ReadAllText(path));

            var doc = XmlSerializeHelper.Deserialize<Doc>(ConvertToMarkdown(File.ReadAllText(path), false));

            GenerateCatalog(doc);

            //foreach (var item in doc.Members.Member)
            //{
            //    Console.WriteLine(item.Name);
            //}
        }

        /// <summary>
        /// Generates the header.
        /// </summary>
        /// <returns></returns>
        public static void GenerateCatalog(Doc doc)
        {
            m_Doc = doc;


            StringBuilder builder = new StringBuilder();

            string title = GenerateTitle(doc);

            //生成文档名
            builder.AppendLine(title);

            //生成标题
            //builder.AppendLine($" - {title}");

            List<Catelog> m_Catelog = new List<Catelog>();

            foreach (var item in doc.Members.Member)
            {

                string filename = "";

                if (item.Name.AsString().Contains("T:"))
                {

                    //生成目录
                    string catelog = item.Name.AsString().Replace($"T:{doc.Assembly.Name}.", "").Replace("`1", "&#96;1").Replace("`2", "&#96;2");



                    string obsolete = ""; //弃用
                    string obsoleteText = ""; //弃用
                    string group = "";                    //组
                    string intr = "";//介绍
                    if (item.Summary != null && item.Summary.Text != null)
                    {
                        intr = string.Join(",", item.Summary.Text);

                        if (item.Summary.Obsolete.AsString() != "")
                        {
                            obsolete = "⚡<font size=3 color=red>【弃用】</font>";
                            obsoleteText = "⚡<font size=3 color=red>【弃用说明】</font>" + item.Summary.Obsolete.AsString();
                        }
                        if (item.Summary.Group.AsString() != "")
                        {
                            group = item.Summary.Group.AsString();
                        }
                    }

                    //文件名
                    filename = $"/{doc.Assembly.Name}/{catelog}.md".Replace("&#96;1", "").Replace("&#96;2", "").Replace("<T>", "");

                    m_UrlList.Add(new SeeasoLink { FileName = $"{catelog}.md", Ulr = filename });

                    //注释目录生成
                    // builder.AppendLine($" - [{catelog}]({filename}) &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;**{intr.Trim()}**");

                    m_Catelog.Add(new Catelog
                    {
                        catelog = catelog,
                        filename = filename,
                        Intr = intr,
                        Obsolete = obsolete,
                        ObsoleteText = obsoleteText,
                        Group = group
                    });

                    string _Directory = "doc/" + doc.Assembly.Name;
                    if (!Directory.Exists(_Directory))
                    {
                        Directory.CreateDirectory(_Directory);
                    }

                    StringBuilder builderContentTitle = new StringBuilder();

                    //文件标题
                    builderContentTitle.AppendLine($"# {catelog}");
                    builderContentTitle.AppendLine($"by [liyang](https://www.liyang.love/)");
                    builderContentTitle.AppendLine("");
                    builderContentTitle.AppendLine($"**最新版本: v3.0.{DateTime.Now.ToString("MM.dd")} &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; {DateTime.Now.ToString("yyyy年MM月dd日")}**  ");
                    builderContentTitle.AppendLine("<br/>");
                    builderContentTitle.AppendLine("## 说明");//
                    builderContentTitle.AppendLine("```C#");
                    builderContentTitle.AppendLine($"{intr.Trim()}");
                    builderContentTitle.AppendLine("```");
                    builderContentTitle.AppendLine("");


                    //处理子信息

                    builderContentTitle.AppendLine(GenerateBody(doc.Members.Member, item.Name.AsString().Replace("T:", ""), catelog, doc.Assembly.Name, filename.Replace(".md", "")));

                    File.WriteAllText("doc/" + filename, builderContentTitle.ToString().Replace("<T>", "&lt;T&gt;"));
                }


            }


            //生成目录
            var catelogGroup = m_Catelog.GroupBy(s => s.Group).ToList();

            foreach (var item in catelogGroup)
            {
                builder.AppendLine($" - ### **<font size=3 face=\"幼圆\" color=#FF0080>{item.Key}</font>** ");

                int sort = 0;
                foreach (var itemCatelog in item.Select(s => s).ToList())
                {

                    //builder.AppendLine($"   - [{itemCatelog.Obsolete}{itemCatelog.catelog}]({itemCatelog.filename}) <br/>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; **<font size=2 face=\"幼圆\" color=#FF0080>```C# {itemCatelog.Intr.Trim()} ```</font>** ");
                    builder.AppendLine($"   - #### **<font size=3 face=\"幼圆\" color=#FF0080>{sort}、</font>[{itemCatelog.Obsolete}{itemCatelog.catelog}](../doc{itemCatelog.filename}#{itemCatelog.catelog.Replace(".", "")})**   ");
                    builder.AppendLine($"        ```c#  ");
                    builder.AppendLine($"      {itemCatelog.Intr.Trim()}");

                    if (!string.IsNullOrEmpty(itemCatelog.ObsoleteText))
                    {
                        builder.AppendLine($"      {itemCatelog.ObsoleteText.Trim()}");
                    }

                    builder.AppendLine($"        ``` ");

                    sort++;
                }
            }

            m_UrlList.Add(new SeeasoLink { FileName = $"{doc.Assembly.Name}.md", Ulr = $"doc/{doc.Assembly.Name}.md" });

            //生成根目录
            File.WriteAllText($"doc/{doc.Assembly.Name}.md", builder.ToString());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="members"></param>
        /// <param name="name"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static string GenerateBody(List<Member> members, string name, string methodName, string assemblyName, string filename)
        {
            StringBuilder builderBody = new StringBuilder();

            var mesmList = members.FindAll(s => s.Name.Contains(":" + name + ".") && !s.Name.Contains("T:"));


            #region 构造函数
            builderBody.AppendLine($" ## 构造函数");

            builderBody.AppendLine($"|  构造函数    |   参数   |   说明   |");
            builderBody.AppendLine($"| ---- | ---- | ---- |");
            foreach (var item in mesmList)
            {

                //方法、构造函数
                if (item.Name.Contains("M:"))
                {

                    //构造函数  M:Lenovo.HIS.Common.DataField.#ctor(System.String)
                    if (item.Name.Contains("#ctor")|| item.Name.Contains("#cctor"))
                    {
                        try
                        {

                            //M:Lenovo.HIS.Common.TComparer`1.#ctor(System.Func{`0,`0,System.Boolean})
                            //var paras = item.Name.AsString().Replace("M:" + name + ".#ctor", "").Replace(")", "").Split('(').ToList();

                            //paras.Remove("");

                            var paras = GetParamTypes(item.Name.AsString());

                            if (paras != null && paras.Count > 0)
                            {

                                for (int i = 0; i < paras.Count; i++)
                                {
                                    paras[i] = "[" + ConvertToMarkdown(paras[i]).Replace("{", "&lt;").Replace("}", "&gt;") + $"]({GetTypeUrl(paras[i], methodName, item.Name)})" + "&nbsp;&nbsp;" + item.Param[i].Name;
                                }
                            }

                            builderBody.AppendLine($" | {methodName}({string.Join(",<br />", paras).Trim()}) | {string.Join("", item.Param.Select(s => s.Name + " : " + s.Text.AsString().Trim() + " <br />")).Trim()} | {Escape(item.Summary.Text.AsString().Trim())} | ");

                        }
                        catch (Exception ex)
                        {
                            m_LogList.Add("");
                            string msg = $"错误数据:{item.Name.AsString()}  错误信息:{ex.ToString()}";
                            m_LogList.Add(msg);
                            File.AppendAllText($"{DateTime.Now.ToString("yyyy-MM-dd")}.log", msg);
                            //Console.WriteLine($"错误数据:{item.Name.AsString()}  错误信息:{ex.ToString()}");
                        }
                    }
                }

            }
            #endregion

            #region 方法

            builderBody.AppendLine($" ## 方法");

            builderBody.AppendLine($"|  方法    |   参数   |   说明   |   返回值   |");
            builderBody.AppendLine($"| ---- | ---- | ---- | ---- |");
            //Field — 方法
            foreach (var item in mesmList)
            {

                if (item.Name.Contains("Execute(System.Collections.Generic.Dictionary{System.String,System.Object})"))
                {
                }

                //M:Lenovo.HIS.Common.FuncAgeHelper.GetAgeByBirthday(System.Nullable{System.DateTime},System.Int32,System.Int32,System.Int32,System.Int32)
                //M:Lenovo.HIS.Common.FuncDefaultHttpHelper.HttpRequest2(Lenovo.HIS.Common.EnumHttpMethodType2,Lenovo.HIS.Common.EnumContextTypes2,System.Collections.Generic.Dictionary{System.String,System.String},System.Collections.Generic.Dictionary{System.String,System.Object})
                //方法
                if (item.Name.Contains("M:") && !(item.Name.Contains("#ctor")|| item.Name.Contains("#cctor")))
                {
                    try
                    {

                        //方法名
                        string methodName1 = item.Name.AsString().Replace("M:" + name + ".", "").Split('(')[0];

                        //var paras = item.Name.AsString().Replace("M:" + name + ".", "").Replace(")", "").Split('(').ToList();

                        //paras.Remove("");

                        if (methodName1 == "ReadCard")
                        {

                        }

                        //参数
                        var paras = GetParamTypes(item.Name.AsString());
                        // paras = paras[1].Split(',').ToList();
                        if (paras != null && paras.Count > 0)
                        {
                            for (int i = 0; i < paras.Count; i++)
                            {
                                paras[i] = " [" + ConvertToMarkdown(paras[i]).Replace("{", "&lt;").Replace("}", "&gt;") + $"]({GetTypeUrl(paras[i], methodName1, item.Name.AsString())})" + " &nbsp;&nbsp;" + item.Param[i].Name;
                            }
                        }

                        string obsolete = ""; //弃用
                        string obsoleteText = ""; //弃用
                        string group = "";                    //组
                        string intr = "";//介绍
                        if (item.Summary != null && item.Summary.Text != null)
                        {
                            intr = string.Join(",", item.Summary.Text);

                            if (item.Summary.Obsolete.AsString() != "")
                            {
                                obsolete = "⚡<font size=3 color=red>【弃用】</font>";
                                obsoleteText = "⚡<font size=3 color=red>【弃用说明】</font>" + item.Summary.Obsolete.AsString();
                            }
                            if (item.Summary.Group.AsString() != "")
                            {
                                group = item.Summary.Group.AsString();
                            }
                        }
                        string str = "";
                        if (paras != null && paras.Count > 0)
                        {
                            str = "<br />";
                        }

                        //参数组合
                        builderBody.AppendLine($" | [{obsolete}{methodName1}](../../doc{filename}/{methodName1.Replace("<T>", "&lt;T&gt;")}.md#{methodName1.Replace("<T>", "&lt;T&gt;")})({str}{ConvertToMarkdown(string.Join(",<br />", paras).Trim())}{str}) | {string.Join("", item.Param.Select(s => s.Name + " : " + EscapeNoN(s.Text.AsString().Trim()) + " <br />")).Trim()} | {Escape(item.Summary.Text.AsString().Trim())} <br />{obsoleteText} | {Escape(item.Returns?.Text)} |");

                        m_UrlList.Add(new SeeasoLink { FileName = $"{filename}/{methodName1.Replace("<T>", "&lt;T&gt;")}.md", Ulr = $"../../doc{filename}/{methodName1.Replace("<T>", "&lt;T&gt;")}.md#{methodName1.Replace("<T>", "&lt;T&gt;")}" });

                        //生成方法说明  $"{methodName.Replace("<T>", "&lt;T&gt;")}.md"
                        GenerateExample(item, name, methodName1, assemblyName, filename);

                    }
                    catch (Exception ex)
                    {
                        m_LogList.Add("");
                        string msg = $"错误数据:{item.Name.AsString()}  错误信息:{ex.ToString()}";
                        m_LogList.Add(msg);
                        File.AppendAllText($"{DateTime.Now.ToString("yyyy-MM-dd")}.log", msg);
                        //Console.WriteLine($"错误数据:{item.Name.AsString()}  错误信息:{ex.ToString()}");
                    }
                }
            }
            #endregion

            #region 变量

            builderBody.AppendLine($" ## 变量");

            builderBody.AppendLine($"|  名称    |   类型   |   说明   |");
            builderBody.AppendLine($"| ---- | ---- | ---- |");


            //Field — 字段
            foreach (var item in mesmList)
            {

                //Field — 字段
                if (item.Name.Contains("F:"))
                {

                    string filed = item.Name.AsString().Replace("F:" + name + ".", "");

                    builderBody.AppendLine($" | {filed}|  | {Escape(item.Summary.Text.AsString().Trim())} | ");
                }
            }
            #endregion

            #region 属性

            builderBody.AppendLine($" ## 属性");

            builderBody.AppendLine($"|  名称    |   类型   |   说明   |");
            builderBody.AppendLine($"| ---- | ---- | ---- |");

            //包括索引器或其他索引属性。
            foreach (var item in mesmList)
            {
                //包括索引器或其他索引属性。
                if (item.Name.Contains("P:"))
                {
                    string filed = item.Name.AsString().Replace("P:" + name + ".", "");

                    builderBody.AppendLine($" | {filed}|  | {Escape(item.Summary.Text.AsString().Trim())} | ");
                }
            }
            #endregion

            #region 事件
            builderBody.AppendLine($" ## 事件");

            builderBody.AppendLine($"|  名称    |   类型   |   说明   |");
            builderBody.AppendLine($"| ---- | ---- | ---- |");
            //事件。
            foreach (var item in mesmList)
            {
                //事件
                if (item.Name.Contains("E:"))
                {
                    string filed = item.Name.AsString().Replace("E:" + name + ".", "");

                    builderBody.AppendLine($" | {filed}|  | {Escape(item.Summary.Text.AsString().Trim())} | ");
                }
            }
            #endregion

            #region 错误字符串

            if (mesmList != null && mesmList.Exists(s => s.Name.Contains("!:")))
            {

                builderBody.AppendLine($" ## 属性");

                builderBody.AppendLine($"|  名称    |   类型   |   说明   |");
                builderBody.AppendLine($"| ---- | ---- | ---- |");
                //错误字符串。
                foreach (var item in mesmList)
                {
                    //错误字符串	字符串的其余部分提供有关错误的信息。 C# 编译器将生成无法解析的链接的错误信息。
                    if (item.Name.Contains("!:"))
                    {
                        string filed = item.Name.AsString().Replace("!:" + name + ".", "");

                        builderBody.AppendLine($" | {filed}|  | {Escape(item.Summary.Text.AsString().Trim())} | ");
                    }
                }
            }
            #endregion

            return builderBody.ToString();
        }

        /// <summary>
        /// 生成例子
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static void GenerateExample(Member member, string name, string methodName, string assemblyName, string filename)
        {

            StringBuilder builderContentTitle = new StringBuilder();

            //文件标题
            builderContentTitle.AppendLine($"# {methodName}");
            builderContentTitle.AppendLine($"by [liyang](https://www.liyang.love/)");
            builderContentTitle.AppendLine("");
            builderContentTitle.AppendLine($"**最新版本: v3.0.{DateTime.Now.ToString("MM.dd")} &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; {DateTime.Now.ToString("yyyy年MM月dd日")}**  ");
            builderContentTitle.AppendLine("<br/>");
            builderContentTitle.AppendLine("#### 说明");//
            builderContentTitle.AppendLine("```C#");
            builderContentTitle.AppendLine($"{member.Summary.Text.AsString().Trim()}");
            builderContentTitle.AppendLine("```");
            builderContentTitle.AppendLine("");


            #region 参数
            //参数
            builderContentTitle.AppendLine($" #### 参数");

            var paras = GetParamTypes(member.Name.AsString());

            if (paras != null && paras.Count > 0)
            {

                builderContentTitle.AppendLine($"|  名称    |   类型   |   说明   |   参见   |");
                builderContentTitle.AppendLine($"| ---- | ---- | ---- | ---- |");

                // paras = paras[1].Split(',').ToList();
                if (paras != null && paras.Count > 0)
                {
                    for (int i = 0; i < paras.Count; i++)
                    {
                        //paras[i] = " [" + ConvertToMarkdown(paras[i]).Replace("{", "&lt;").Replace("}", "&gt;") + $"]({GetTypeUrl(paras[i])})" + " &nbsp;&nbsp;" + member.Param[i].Name;

                        //参数组合
                        if (member?.Param[i]?.Seealso != null)
                        {
                            var _SeeAlso = member?.Param[i]?.Seealso?.Cref == null ? "" : $"参见:[{Escape(member?.Param[i]?.Seealso?.Cref.AsString().Replace("T:", ""))}](" + GetTypeUrl(member?.Param[i]?.Seealso?.Cref, member?.Param[i]?.Seealso?.Cref.AsString(), member?.Param[i]?.Seealso?.Cref.AsString()) + ")";

                            builderContentTitle.AppendLine($" | {member.Param[i].Name.Trim()}" +
                                $"| {paras[i]} | {ConvertToMarkdown(member.Param[i].Text.Trim()).Replace("{", "&lt;").Replace("}", "&gt;")} " +
                                $"| {member?.Param[i]?.Seealso?.Text}{_SeeAlso} | ");
                        }
                        else
                        {
                            var _SeeAlso = member?.Param[i]?.See?.Cref == null ? "" : $"参见:[{Escape(member?.Param[i]?.See?.Cref.AsString().Replace("T:", ""))}](" + GetTypeUrl(member?.Param[i]?.See?.Cref, member?.Param[i]?.See?.Cref.AsString(), member?.Param[i]?.See?.Cref.AsString()) + ")";

                            builderContentTitle.AppendLine($" | {member.Param[i].Name.Trim()}" +
                            $"| {paras[i]} | {ConvertToMarkdown(member.Param[i].Text.AsString().Trim()).Replace("{", "&lt;").Replace("}", "&gt;")} " +
                            $"| {member?.Param[i]?.See?.Text}<br/>{_SeeAlso} | ");
                        }
                    }
                }
            }
            else
            {
                builderContentTitle.AppendLine("`无相关信息`");
            }
            builderContentTitle.AppendLine();
            #endregion

            #region 返回值
            //返回值
            builderContentTitle.AppendLine($" #### 返回值");

            bool _IsReturns = true;

            if (member?.Returns?.Text != null)
            {
                builderContentTitle.AppendLine($"  `{Escape(member?.Returns?.Text?.Trim())}` ");

                builderContentTitle.AppendLine();
                _IsReturns = false;
            }
            //builderContentTitle.AppendLine("<br/>");
            //builderContentTitle.AppendLine(" ");
            if ((member?.Returns?.Seealso != null && member?.Returns?.Seealso.Count > 0) || (member?.Returns?.See != null && member?.Returns?.See.Count > 0))
            {

                builderContentTitle.AppendLine($"|  名称    |   类型   |   说明   |   参见   |");
                builderContentTitle.AppendLine($"| ---- | ---- | ---- | ---- |");

                if (member?.Returns?.Seealso != null)
                {

                    foreach (var item in member?.Returns?.Seealso)
                    {
                        var _SeeAlso = item.Cref == null ? "" : $"参见:[{Escape(item.Cref.AsString().Replace("T:", ""))}](" + GetTypeUrl(Escape(item.Cref), item.Cref.AsString(), item.Cref.AsString()) + ")";
                        //var _SeeAlso = item.Cref == null ? "" : "参见:" + GetTypeUrl(Escape(item.Cref), item.Cref.AsString(), item.Cref.AsString());

                        builderContentTitle.AppendLine($"| {GetTypeUrl(Escape(item.Cref), item.Cref.AsString(), item.Cref.AsString())} |  | {Escape(item.Text)} | {_SeeAlso} |");
                        _IsReturns = false;
                    }
                }
                if (member?.Returns?.See != null)
                {

                    foreach (var item in member?.Returns?.See)
                    {
                        var _SeeAlso = item.Cref == null ? "" : $"参见:[{Escape(item.Cref.AsString().Replace("T:", ""))}](" + GetTypeUrl(Escape(item.Cref), item.Cref.AsString(), item.Cref.AsString()) + ")";

                        builderContentTitle.AppendLine($"| {GetTypeUrl(Escape(item.Cref), item.Cref.AsString(), item.Cref.AsString())} |  | {Escape(item.Text)} | {_SeeAlso} |");
                        _IsReturns = false;
                    }
                }
            }
            if (_IsReturns)
            {
                builderContentTitle.AppendLine("`无相关信息`");
            }
            #endregion


            //异常信息
            #region 异常信息

            builderContentTitle.AppendLine($" #### 异常信息");

            bool _IsException = true;

            if (member?.Exception?.Text != null)
            {
                builderContentTitle.AppendLine($"  `{Escape(member?.Exception?.Text?.Trim())}` ");
                _IsException = false;
            }
            builderContentTitle.AppendLine();

            if (member?.Exception != null || member?.Exception?.Seealso != null || member?.Exception?.See != null)
            {


                builderContentTitle.AppendLine($"|  名称    |   类型   |   说明   |   参见   |");
                builderContentTitle.AppendLine($"| ---- | ---- | ---- | ---- |");

                if (member?.Exception != null)
                {
                    var _SeeAlso = member?.Exception.Cref == null ? "" : $"参见:[{Escape(member?.Exception.Cref.AsString().Replace("T:", ""))}](" + GetTypeUrl(Escape(member?.Exception.Cref.AsString().Replace("T:", "")), member?.Exception.Cref.AsString(), member?.Exception.Cref.AsString()) + ")";

                    builderContentTitle.AppendLine($"| {GetTypeUrl(Escape(member?.Exception.Cref.AsString().Replace("T:", "")), member?.Exception.Cref.AsString(), member?.Exception.Cref.AsString())} |  | {Escape(member?.Exception.Text)} | {_SeeAlso} |");
                    _IsException = false;
                }

                if (member?.Exception?.Seealso != null)
                {

                    foreach (var item in member?.Exception?.Seealso)
                    {
                        var _SeeAlso = item.Cref == null ? "" : $"参见:[{Escape(item.Cref.AsString().Replace("T:", ""))}](" + GetTypeUrl(Escape(item.Cref.AsString().Replace("T:", "")), item.Cref.AsString(), item.Cref.AsString()) + ")";

                        builderContentTitle.AppendLine($"| {GetTypeUrl(Escape(item.Cref.AsString().Replace("T:", "")), item.Cref.AsString(), item.Cref.AsString())} |  | {Escape(item.Text)} | {_SeeAlso} |");
                        _IsException = false;
                    }
                }
                if (member?.Exception?.See != null)
                {

                    foreach (var item in member?.Exception?.See)
                    {
                        var _SeeAlso = item.Cref == null ? "" : $"参见:[{Escape(item.Cref.AsString().Replace("T:", ""))}](" + GetTypeUrl(Escape(item.Cref.AsString().Replace("T:", "")), item.Cref.AsString(), item.Cref.AsString()) + ")";

                        builderContentTitle.AppendLine($"| {GetTypeUrl(Escape(item.Cref.AsString().Replace("T:", "")), item.Cref.AsString(), item.Cref.AsString())} |  | {Escape(item.Text)} | {_SeeAlso} |");
                        _IsException = false;
                    }
                }

                builderContentTitle.AppendLine();
                builderContentTitle.AppendLine("<br/>");
            }
            if (_IsException)
            {
                builderContentTitle.AppendLine("`无相关信息`");
            }
            builderContentTitle.AppendLine();
            #endregion


            //备注
            #region 备注
            builderContentTitle.AppendLine($" #### 备注");
            bool _IsRemarks = true;
            if (member?.Remarks != null && member?.Remarks.Text != null)
            {
                builderContentTitle.AppendLine($"  <b>{Escape(string.Join(",", member?.Remarks?.Text))}</b> ");
                _IsRemarks = false;
            }

            if (member?.Remarks?.Seealso != null || member?.Remarks?.See != null)
            {

                builderContentTitle.AppendLine(" ");
                builderContentTitle.AppendLine($"|  名称    |   类型   |   说明   |   参见   |");
                builderContentTitle.AppendLine($"| ---- | ---- | ---- | ---- |");

                if (member?.Remarks?.Seealso != null)
                {

                    foreach (var item in member?.Remarks?.Seealso)
                    {
                        var _SeeAlso = item.Cref == null ? "" : $"参见:[{Escape(item.Cref.AsString().Replace("T:", ""))}](" + GetTypeUrl(Escape(item.Cref.AsString().Replace("T:", "")), item.Cref.AsString(), item.Cref.AsString()) + ")";

                        builderContentTitle.AppendLine($"| {GetTypeUrl(Escape(item.Cref.AsString().Replace("T:", "")), item.Cref.AsString(), item.Cref.AsString())} |  | {Escape(item.Text)} | {_SeeAlso} |");
                        _IsRemarks = false;
                    }
                }
                if (member?.Remarks?.See != null)
                {

                    foreach (var item in member?.Remarks?.See)
                    {
                        var _SeeAlso = item.Cref == null ? "" : $"参见:[{Escape(item.Cref.AsString().Replace("T:", ""))}](" + GetTypeUrl(Escape(item.Cref.AsString().Replace("T:", "")), item.Cref.AsString(), item.Cref.AsString()) + ")";

                        builderContentTitle.AppendLine($"| {GetTypeUrl(Escape(item.Cref.AsString().Replace("T:", "")), item.Cref.AsString(), item.Cref.AsString())} |  | {Escape(item.Text)} | {_SeeAlso} |");
                        _IsRemarks = false;
                    }
                }
            }
            if (_IsRemarks)
            {
                builderContentTitle.AppendLine("`无相关信息`");
            }
            #endregion

            //示例
            #region 示例
            builderContentTitle.AppendLine($" #### 示例");
            bool _IsExample = true;
            if (member?.Example != null && member?.Example.Text != null)
            {
                builderContentTitle.AppendLine($"  **{EscapeNoN(member?.Example.Text.Trim())}** ");
                _IsExample = false;
            }

            builderContentTitle.AppendLine("```C#");
            if (member?.Example != null && member?.Example?.Code != null)
            {
                builderContentTitle.AppendLine($"{member?.Example.Code}");
                _IsExample = false;
            }
            if (_IsExample)
            {
                builderContentTitle.AppendLine("无相关信息");
            }
            builderContentTitle.AppendLine("```");

            #endregion
            // string fileName = $"doc/{assemblyName}/{filename}/{methodName}.md";

            WriteFile("doc" + filename, $"{methodName.Replace("<T>", "&lt;T&gt;")}.md", builderContentTitle.ToString());

            m_UrlList.Add(new SeeasoLink { FileName = $"{methodName.Replace("<T>", "&lt;T&gt;")}.md", Ulr = "doc" + filename });

            //File.WriteAllText(fileName, builderContentTitle.ToString());
            // }
        }


        #region Common Method
        public static List<string> GetParamTypes(string name)
        {
            if (!name.Contains("(")) { return new List<string>(); }

            var paramString = name.Split('(').Last().Trim(')');

            var delta = 0;
            var list = new List<StringBuilder>()
            {
                new StringBuilder(""),
            };

            foreach (var character in paramString)
            {
                if (character == '{')
                {
                    delta++;
                }
                else if (character == '}')
                {
                    delta--;
                }
                else if (character == ',' && delta == 0)
                {
                    list.Add(new StringBuilder(""));
                }

                if (character != ',' || delta != 0)
                {
                    list.Last().Append(character);
                }
            }

            return list.Select(x => x.ToString()).ToList();
        }


        public static string GenerateTitle(Doc doc)
        {
            StringBuilder builderContentTitle = new StringBuilder();

            //文件标题
            builderContentTitle.AppendLine($"# {doc.Assembly.Name}");
            builderContentTitle.AppendLine($"by [liyang](https://www.liyang.love/)");
            builderContentTitle.AppendLine("");
            builderContentTitle.AppendLine($"**最新版本: v3.0.{DateTime.Now.ToString("MM.dd")} &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; {DateTime.Now.ToString("yyyy年MM月dd日")}**  ");
            builderContentTitle.AppendLine("```C#");
            builderContentTitle.AppendLine($" 本章节为{doc.Assembly.Name}目录介绍章节");
            builderContentTitle.AppendLine("```");
            builderContentTitle.AppendLine("---");
            builderContentTitle.AppendLine("### 目录 ");//
            builderContentTitle.AppendLine("---");
            builderContentTitle.AppendLine("");

            return builderContentTitle.ToString();
        }

        public static string AsString(this string str)
        {
            return !string.IsNullOrWhiteSpace(str) ? str : "";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="methodName"></param>
        /// <param name="FillName"></param>
        /// <returns></returns>
        public static string GetTypeUrl(string str, string methodName, string FillName)
        {
            if (str.Contains("System.") && !str.Contains(".HIS."))
            {
                return $"https://docs.microsoft.com/zh-cn/dotnet/api/" + str.ToLower().Replace("[]", "");
            }

            if (str.Contains(".HIS."))
            {
                //M:方法   M:+#ctor  构造函数    F:变量  P:属性  E:事件

                //构造函数
                if (str.Contains("M:") && str.Contains("#ctor"))
                {

                }

                //方法
                if (str.Contains("M:") && !str.Contains("#ctor"))
                {

                }

                //变量
                if (str.Contains("F:"))
                {

                }

                //类型
                if (str.Contains("T:"))
                {
                    var FillInfo = GetCatelog("T:", FillName);

                    return $"../../{FillInfo.Item1}/{FillInfo.Item2}.md".Replace("&#96;1", "").Replace("&#96;2", "").Replace("<T>", "");
                }

                //属性
                if (str.Contains("P:"))
                {

                }

                //事件
                if (str.Contains("E:"))
                {

                }

                //.Entities.
                //类型
                if (str.Contains(".Entities."))
                {
                    var FillInfo = GetCatelog(".Entities.", str);

                    return $"../../{FillInfo.Item1}/{FillInfo.Item2}.md".Replace("&#96;1", "").Replace("&#96;2", "").Replace("<T>", "");
                }

                return "#";

                //if (!str.Contains(" P:"))
                //{
                //    var link = str.Split(new char[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries);

                //    var url = link[link.Length - 1].Split('.').ToList();

                //    var urlList = m_UrlList.Find(s => s.FileName == url[url.Count - 1] + ".md");
                //    if (urlList == null)
                //    {
                //        return "#";
                //    }

                //    var strUlr= m_UrlList.Find(s=>s.FileName== url[url.Count-1]+".md").Ulr;

                //    return strUlr;
                //    //string linklabel = "";
                //    //int i = 0;
                //    //url.ForEach(f =>
                //    //{
                //    //    if (i != url.Count - 1)
                //    //    {
                //    //        linklabel += f;
                //    //    }
                //    //    i++;
                //    //});

                //    //return "../doc/" + linklabel + "/" + url[url.Count - 1] + ".md#" + url[url.Count - 1];
                //}
                //return "#";
            }
            return "#";
        }

        public static Tuple<string, string> GetCatelog(string str, string path)
        {
            str = path.Replace(str, "");

            string catelog = str.Substring(0, str.LastIndexOf("."));

            string fileName = str.Substring(str.LastIndexOf(".") + 1, str.Length - str.LastIndexOf(".") - 1);

            return new Tuple<string, string>(catelog, fileName);

        }


        public static string ConvertToMarkdown(string str, bool isInit = true)
        {
            var result = str.Replace("``0", "&lt;T&gt;").Replace("``1", "&lt;T&gt;")
                      .Replace("``2", "&lt;T&gt;").Replace("``2", "&lt;T&gt;").Replace("``3", "&lt;T&gt;").Replace("``4", "&lt;T&gt;").Replace("``5", "&lt;T&gt;")
                      .Replace("``6", "&lt;T&gt;").Replace("``7", "&lt;T&gt;").Replace("``8", "&lt;T&gt;").Replace("``9", "&lt;T&gt;")
                      .Replace("`0,", "&lt;T&gt;，").Replace("`1,", "&lt;T&gt;，").Replace("`2,", "&lt;T&gt;，").Replace("`3,", "&lt;T&gt;，")
                      .Replace("`4,", "&lt;T&gt;，").Replace("`5,", "&lt;T&gt;，").Replace("`6,", "&lt;T&gt;，").Replace("`7,", "&lt;T&gt;，")
                      .Replace("`8,", "&lt;T&gt;，").Replace("`9,", "&lt;T&gt;，").Replace("`0,", "&lt;T&gt;，")
                      .Replace("`0", "&lt;T&gt;").Replace("`1", "&lt;T&gt;").Replace("`2", "&lt;T&gt;").Replace("`3", "&lt;T&gt;")
                      .Replace("`4", "&lt;T&gt;").Replace("`5", "&lt;T&gt;").Replace("`6", "&lt;T&gt;").Replace("`7", "&lt;T&gt;")
                      .Replace("`8", "&lt;T&gt;").Replace("`9", "&lt;T&gt;");

            return result;
        }

        private static string Escape(string sInput)
        {
            return sInput.AsString().Replace("\n", " ").Replace("'", " &#39;")
                        .Replace("\"", " &quot;")
                        //.Replace("<", "&lt;")
                        //.Replace(">", "&gt;")
                        .Replace(" ", " &nbsp;")
                        .Replace("©", " &copy;")
                        .Replace("®", " &reg;")
                        .Replace("™", " &#8482;");//.Replace("\n", "  "); 
        }

        private static string EscapeNoN(string sInput)
        {
            return sInput.Replace("\n", " ");
        }

        private static void WriteFile(string directory, string fileName, string info)
        {
            string path = Path.Combine(directory, fileName);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);

            }
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                byte[] bty = Encoding.UTF8.GetBytes(info);
                fs.Write(bty, 0, bty.Length);

                fs.Flush();
                fs.Close();
                fs.Dispose();
            }
        }
        #endregion
    }

    public class SeeasoLink
    {
        public string Ulr { get; set; }

        public string FileName { get; set; }
    }
}
