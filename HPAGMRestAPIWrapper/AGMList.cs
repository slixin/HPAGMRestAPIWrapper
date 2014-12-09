using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace HPAGMRestAPIWrapper
{
    /// <summary>
    /// List class
    /// </summary>
    public class AGMList
    {
        /// <summary>
        /// Type Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Type name
        /// </summary>
        public string Name { get; set; }

        public Dictionary<string, string> Items { get; set; }

        /// <summary>
        /// Get all lists
        /// </summary>
        /// <param name="conn">connection</param>
        /// <returns>list of types</returns>
        public static List<AGMList> GetLists(AGMConnection conn, string entityName)
        {
            var lists = new List<AGMList>();

            var url = string.Format("{0}/agm/rest/domains/{1}/projects/{2}/customization/entities/{3}/lists", conn.ServerName, conn.Domain, conn.Project, entityName);

            try
            {
                conn.Http.Method = "GET";
                conn.Http.ContentType = "application/xml";
                var status = conn.Http.Request(url);
                if (status >= 200 && status <= 204)
                {
                    if (!string.IsNullOrEmpty(conn.Http.Response))
                    {
                        var doc = new XmlDocument();
                        doc.LoadXml(conn.Http.Response);
                        var list = doc.SelectNodes(@"Lists/List");
                        foreach (XmlNode node in list)
                        {
                            Dictionary<string, string> listItems = new Dictionary<string, string>();
                            foreach (XmlNode itemNode in node.SelectNodes("Items/Item"))
                            {
                                listItems.Add(itemNode.Attributes["Id"].Value, itemNode.Attributes["value"].Value);
                            }

                            lists.Add(new AGMList
                            {
                                Id = Convert.ToInt32(node.SelectSingleNode("Id").InnerText),
                                Name = node.SelectSingleNode("Name").InnerText,
                                Items = listItems,
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return lists;
        }
    }
}
