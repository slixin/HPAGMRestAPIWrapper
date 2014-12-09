using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace HPAGMRestAPIWrapper
{
    /// <summary>
    /// User class
    /// </summary>
    public class AGMUser
    {
        /// <summary>
        /// List name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Full name
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Phone
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Get users for project
        /// </summary>
        /// <param name="conn">connection</param>
        /// <returns>list of user</returns>
        public static List<AGMUser> GetUsers(AGMConnection conn)
        {
            var users = new List<AGMUser>();

            var url = string.Format("{0}/agm/rest/domains/{1}/projects/{2}/customization/users", conn.ServerName, conn.Domain, conn.Project);

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
                        var userNodes = doc.SelectNodes(@"Users/User");
                        foreach (XmlNode userNode in userNodes)
                        {
                            users.Add(new AGMUser
                            {
                                Name = userNode.Attributes["Name"].Value,
                                FullName = userNode.Attributes["FullName"].Value,
                                Email = userNode.SelectSingleNode("email").InnerText,
                                Phone = userNode.SelectSingleNode("phone").InnerText,
                                IsActive = Convert.ToBoolean(userNode.SelectSingleNode("UserActive").InnerText),
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return users;
        }
    }
}
