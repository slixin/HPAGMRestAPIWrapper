using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace HPAGMRestAPIWrapper
{
    /// <summary>
    /// Type class
    /// </summary>
    public class AGMType
    {
        /// <summary>
        /// Type Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Type name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Get all types in the project
        /// </summary>
        /// <param name="conn">connection</param>
        /// <returns>list of types</returns>
        public static List<AGMType> GetTypes(AGMConnection conn, string entityType)
        {
            var types = new List<AGMType>();

            var url = string.Format("{0}/agm/rest/domains/{1}/projects/{2}/customization/entities/{3}/types", conn.ServerName, conn.Domain, conn.Project, entityType);

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
                        var list = doc.SelectNodes(@"types/type");
                        foreach (XmlNode node in list)
                        {
                            types.Add(new AGMType
                            {
                                Id = node.Attributes["id"].Value,
                                Name = node.Attributes["name"].Value,
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.IndexOf("does not support sub-types") < 0)
                    throw new Exception(ex.Message);
            }

            return types;
        }
    }
}
