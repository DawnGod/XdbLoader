using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Engine;
using Game;
using XmlUtilities;

namespace XdbLoader
{
    /// <summary>
    /// XDB Mod加载器
    /// </summary>
    /// <remarks>
    /// 负责加载和合并所有Mod中的.netxdb文件到游戏的Database.xml中
    /// </remarks>
    public class XdbModLoad : ModLoader
    {
        /// <summary>
        /// Mod初始化
        /// </summary>
        public override void __ModInitialize()
        {
            ModsManager.RegisterHook("OnXdbLoad", this);
        }

        /// <summary>
        /// XDB加载钩子
        /// </summary>
        /// <param name="xElement">Database.xml的根元素</param>
        public override void OnXdbLoad(XElement xElement)
        {
            // 不能使用Equals判断，会出现总为False
            if (ModsManager.ModList.Last() != Entity)
            {
                ModsManager.ModList.Add(Entity);
                return;
            }

            var element = ContentManager.Get<XElement>("Database");

            XdbLoaderUtil.Load(element);
            foreach (var modEntity in ModsManager.ModList.Where(entity => !entity.IsSystemMod).ToList())
            {
                var modName = modEntity.modInfo?.Name ?? "未知Mod";

                // 获取Mod中的所有xdb
                modEntity.GetFiles(".xdb", delegate(string filename, Stream stream)
                {
                    try
                    {
                        // 加载XDB文件
                        var xdb = XmlUtils.LoadXmlFromStream(stream, Encoding.UTF8, true);
                        // 使用XDB加载器合并文件
                        XdbLoaderUtil.LoadXdb(xdb);
                        Console.WriteLine($"已加载XDB文件: {filename} (来自Mod: {modName})");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"加载XDB文件 {filename} 失败: {ex.Message}");
                        Console.WriteLine(ex.StackTrace);
                    }
                });
                
                modEntity.GetFiles(".netxdb", delegate(string filename, Stream stream)
                {
                    try
                    {
                        // 加载XDB文件
                        var xdb = XmlUtils.LoadXmlFromStream(stream, Encoding.UTF8, true);
                        // 使用XDB加载器合并文件
                        XdbLoaderUtil.LoadXdb(xdb);
                        Console.WriteLine($"已加载NET XDB文件: {filename} (来自Mod: {modName})");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"加载NET XDB文件 {filename} 失败: {ex.Message}");
                        Console.WriteLine(ex.StackTrace);
                    }
                });
            }

            XdbLoaderUtil.Process();

            xElement.Element("DatabaseObjects")?.ReplaceWith(element.Element("DatabaseObjects"));
        }
    }
}