using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace HPAGMRestAPIWrapper
{
    public class Common
    {
        public static string ParseHeaders(string str, string sessionname)
        {
            var startstr = sessionname;

            if (str.IndexOf(startstr) < 0)
                return null;

            var startpos = str.IndexOf(startstr) + startstr.Length + 1;
            return str.Substring(startpos, str.Length - startpos);
        }

        public static string BuildQueryFieldsString(List<AGMField> queryFields)
        {
            var queryStr = "query={";
            string str = null;

            if (queryFields == null)
                return null;

            foreach (var queryField in queryFields)
            {
                if (string.IsNullOrEmpty(str))
                    if (queryField.Entity.Equals("release-backlog-item"))
                        str += string.Format("release-backlog-item.{0}[{1}]", queryField.Name, queryField.Value);
                    else
                        str += string.Format("{0}[{1}]", queryField.Name, queryField.Value);
                else
                    if (queryField.Entity.Equals("release-backlog-item"))
                        str += string.Format(";release-backlog-item.{0}[{1}]", queryField.Name, queryField.Value);
                    else
                        str += string.Format(";{0}[{1}]", queryField.Name, queryField.Value);
            }

            queryStr += str + "}";

            return queryStr;
        }

        public static string BuildReturnFieldsString(List<AGMField> returnfields)
        {
            string returnStr = "fields=";
            string str = null;

            if (returnfields == null)
                return null;

            foreach (var returnField in returnfields)
            {
                if (string.IsNullOrEmpty(str))
                    if (returnField.Entity.Equals("release-backlog-item"))
                        str += string.Format("release-backlog-item.{0}", returnField.Name);
                    else
                        str += string.Format("{0}", returnField.Name);
                else
                    if (returnField.Entity.Equals("release-backlog-item"))
                        str += string.Format(",release-backlog-item.{0}", returnField.Name);
                    else
                        str += string.Format(",{0}", returnField.Name);
            }

            returnStr += str;

            return returnStr;
        }

        public static string BuildSortFieldsString(List<AGMField> sortfields)
        {
            string sortFieldStr = "order-by={";

            string str = null;

            if (sortfields == null)
                return null;

            foreach (var sortfield in sortfields)
            {
                if (string.IsNullOrEmpty(str))
                    if (sortfield.Entity.Equals("release-backlog-item"))
                        str += string.Format("release-backlog-item.{0}[{1}]", sortfield.Name, sortfield.OrderBy.ToUpper());
                    else
                        str += string.Format("{0}[{1}]", sortfield.Name, sortfield.OrderBy.ToUpper());
                else
                    if (sortfield.Entity.Equals("release-backlog-item"))
                        str += string.Format(";release-backlog-item.{0}[{1}]", sortfield.Name, sortfield.OrderBy.ToUpper());
                    else
                        str += string.Format(";{0}[{1}]", sortfield.Name, sortfield.OrderBy.ToUpper());
            }

            sortFieldStr += str + "}";

            return sortFieldStr;
        }

        public static string GetEntityType(string classname)
        {
            string name = null;
            switch (classname)
            {
                case "AGMTest":
                    name = "tests";
                    break;
                case "AGMTestSet":
                    name = "test-sets";
                    break;
                case "AGMDefect":
                    name = "defects";
                    break;
                case "AGMRequirement":
                    name = "requirements";
                    break;
                case "AGMRun":
                    name = "runs";
                    break;
                case "AGMTestFolder":
                    name = "test-folders";
                    break;
                case "AGMTestSetFolder":
                    name = "test-set-folders";
                    break;
                case "AGMTestInstance":
                    name = "test-instances";
                    break;
                case "AGMDefectLink":
                    name = "defect-links";
                    break;
                case "AGMTestStep":
                    name = "design-steps";
                    break;
                case "AGMReleaseBacklogItem":
                    name = "release-backlog-items";
                    break;
                case "AGMProjectTask":
                    name = "project-tasks";
                    break;
                case "AGMAttachment":
                    name = "attachments";
                    break;
                case "AGMRelease":
                    name = "releases";
                    break;
                case "AGMReleaseCycle":
                    name = "release-cycles";
                    break;
                case "AGMChangeset":
                    name = "changesets";
                    break;
                case "AGMBuildInstance":
                    name = "build-instances";
                    break;
                case "AGMProduct":
                    name = "products";
                    break;
                case "AGMTeam":
                    name = "teams";
                    break;
            }

            return name;
        }

        public static string GetEntityId(XmlNode node)
        {
            string Value = null;
            try
            {
                var idNode = node.SelectSingleNode(string.Format("Field[@Name='id']"));
                if (idNode != null)
                {
                    var valueNode = idNode.SelectSingleNode("Value");
                    if (valueNode != null)
                        Value = valueNode.InnerText;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return Value;
        }

        public static string GetQCRestException(string response)
        {
            string exceptionTitle;

            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(response);

                exceptionTitle = doc.SelectSingleNode(@"QCRestException/Title").InnerText;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
            return exceptionTitle;
        }

        public static string BuildEntityXML(List<AGMField> fields, string entityType)
        {
            var doc = CreateXmlDocument("Entity");
            var rootNode = doc.SelectSingleNode("Entity");
            CreateAttribute(rootNode, "Type", entityType);

            var fieldsNode = CreateChildNode(rootNode, "Fields");

            foreach (var field in fields)
            {
                var fieldNode = CreateChildNode(fieldsNode, "Field", "Name", field.Name);
                if (!string.IsNullOrEmpty(field.Value))
                    CreateChildNodeValue(fieldNode, "Value", field.Value);
            }

            return doc.InnerXml;            
        }

        public static void SetDictionaryValue(ref List<AGMField> dic, string key, string value)
        {
            if (dic.Where(o=>o.Name.Equals(key)).Count() > 0)
                (dic.Where(o => o.Name.Equals(key)) as AGMField).Value = value;
            else
                dic.Add(new AGMField {Name = key, Value = value });
        }

        #region private methods        
        private static XmlAttribute CreateAttribute(XmlNode node, string attributeName, string value)
        {
            var doc = node.OwnerDocument;
            // create new attribute
            var attr = doc.CreateAttribute(attributeName);
            attr.Value = value;
            // link attribute to node
            node.Attributes.SetNamedItem(attr);
            return attr;
        }

        private static XmlDocument CreateXmlDocument(string rootName)
        {
            var doc = new XmlDocument();
            //var decl = doc.CreateXmlDeclaration("1.0", "utf-8", "");
            //doc.InsertBefore(decl, doc.DocumentElement);
            XmlNode newNode = doc.CreateElement(rootName);
            doc.AppendChild(newNode);
            return doc;
        }

        private static XmlNode CreateChildNode(XmlNode rootNode, string name)
        {
            var doc = rootNode.OwnerDocument;
            XmlNode newNode = doc.CreateElement(name);
            rootNode.AppendChild(newNode);

            return newNode;
        }

        private static XmlNode CreateChildNode(XmlNode rootNode, string nodeName, string attributeName, string attributeValue)
        {
            var doc = rootNode.OwnerDocument;
            XmlNode newNode = doc.CreateElement(nodeName);
            CreateAttribute(newNode, attributeName, attributeValue);
            rootNode.AppendChild(newNode);

            return newNode;
        }

        private static XmlNode CreateChildNodeValue(XmlNode rootNode, string name, string value)
        {
            var doc = rootNode.OwnerDocument;
            XmlNode newNode = doc.CreateElement(name);
            newNode.InnerText = value;
            rootNode.AppendChild(newNode);

            return newNode;
        }
        #endregion
    }
}
