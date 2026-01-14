using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace XdbLoader
{
    public static class XdbLoaderUtil
    {
        private static readonly Dictionary<string, XElement> ElementCache = new Dictionary<string, XElement>();

        private static readonly HashSet<string> CustomXdbCache = new HashSet<string>();

        private static XElement _rootElement;

        private static XElement _targetElement;

        private static string _target;

        private static int _count;

        public static void Load(XElement databaseRootElement)
        {
            _rootElement = databaseRootElement;
            _targetElement = databaseRootElement.Element("DatabaseObjects");
        }

        public static void LoadXdb(XElement source)
        {
            if (_targetElement == null || source == null) return;
            CustomXdbCache.Clear();
            source = ProcessXdb(source);
            CustomXdbCache.Clear();
            foreach (var sourceChild in source.Elements())
            {
                MergeElement(sourceChild);
            }
        }


        private static void MergeElement(XElement sourceElement)
        {
            var sourceGuid = sourceElement?.Attribute("Guid")?.Value;

            if (string.IsNullOrEmpty(sourceGuid))
            {
                return; // 跳过处理没有Guid的标签
            }

            var parentGuid = sourceElement.Parent?.Attribute("Guid")?.Value;

            var element = FindElement(sourceGuid, parentGuid, _targetElement);

            if (element == null)
            {
                // 没有找到对应标签
                if (parentGuid == null) _targetElement.Add(sourceElement);
                else ElementCache[parentGuid].Add(sourceElement);
            }
            else
            {
                // 找到Guid相同的标签
                if (element.Name == sourceElement.Name)
                {
                    if (element.Name == "Parameter")
                    {
                        // 如果是参数标签，则直接替换
                        element.ReplaceWith(sourceElement);
                        return;
                    }

                    // 标签也相同，则继续往下匹配
                    foreach (var sourceChild in sourceElement.Elements())
                    {
                        MergeElement(sourceChild);
                    }
                }
                else
                {
                    // 错误的标签，替换Guid为占位符
                    sourceElement.SetAttributeValue("Guid", $"${{{sourceGuid}}}");
                    element.Parent?.Add(sourceElement);
                }
            }
        }

        private static XElement FindElement(string guid, string parentGuid, XElement target)
        {
            if (ElementCache.TryGetValue(guid, out var element)) return element;
            element = FindElementByGuid(guid, parentGuid == null ? target : ElementCache[parentGuid]);
            if (element != null) ElementCache[guid] = element;
            return element;
        }

        private static XElement FindElementByGuid(string guid, XElement target)
        {
            if (target == null) return null;
            foreach (var xElement in target.Elements())
            {
                var eleGuid = xElement.Attribute("Guid")?.Value;
                if (eleGuid == null) continue;
                if (eleGuid == guid) return xElement;
                ElementCache[eleGuid] = xElement;
                var element = FindElementByGuid(guid, xElement);
                if (element != null) return element;
            }

            return null;
        }

        public static void Process()
        {
            if (_targetElement == null) return;
            if (_count > 0)
            {
                var guids = new HashSet<string>(_count);

                do guids.Add(Guid.NewGuid().ToString());
                while (guids.Count < _count);

                var guidArray = guids.ToArray();

                var content = _targetElement.ToString();
                for (var i = 0; i < _count; i++)
                {
                    content = content.Replace("${guid_" + i + "}", guidArray[i]);
                }

                _rootElement.Element("DatabaseObjects")?.ReplaceWith(XElement.Parse(content));
                _count = 0;
            }

            ClearCache();
        }

        private static void ClearCache()
        {
            ElementCache.Clear();
        }

        private static XElement ProcessXdb(XElement xdb)
        {
            _target = _targetElement.ToString();
            foreach (var element in xdb.Elements()) GenerateCustomXdbCache(element);
            _target = null;
            var content = xdb.ToString();
            foreach (var guid in CustomXdbCache)
            {
                content = content.Replace($"{guid}", "${guid_" + _count + "}");
                _count++;
            }

            return XElement.Parse(content);
        }

        private static void GenerateCustomXdbCache(XElement xdb)
        {
            var guid = xdb?.Attribute("Guid")?.Value;
            if (guid == null) return;
            if (!_target.Contains(guid)) CustomXdbCache.Add(guid);
            foreach (var element in xdb.Elements()) GenerateCustomXdbCache(element);
        }
    }
}