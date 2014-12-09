using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace HPAGMRestAPIWrapper
{
    /// <summary>
    ///  ReleaseCycle collection class
    /// </summary>
    public class AGMReleaseCycles : AGMEntityCollection<AGMReleaseCycle>
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="connection">connection</param>
        public AGMReleaseCycles(AGMConnection connection)
            : base(connection)
        {            
        }

        #region public methods
        public static List<AGMReleaseCycle> GetReleaseCycles(AGMConnection conn)
        {
            var releaseCycles = new List<AGMReleaseCycle>();

            string url = string.Format("{0}/agm/rest/domains/{1}/projects/{2}/release-cycles", conn.ServerName, conn.Domain, conn.Project);

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
                        XmlNodeList releaseCyclelist = doc.SelectNodes(@"Entities/Entity");

                        foreach (XmlNode releasecycle in releaseCyclelist)
                        {
                            var newReleaseCycle = new AGMReleaseCycle
                            {
                                Connection = conn,
                                Id = Convert.ToInt32(releasecycle.SelectSingleNode("./Fields/Field[@Name='id']").SelectSingleNode("Value").InnerText),
                                Name = releasecycle.SelectSingleNode("./Fields/Field[@Name='name']").SelectSingleNode("Value").InnerText,
                                StartDate = releasecycle.SelectSingleNode("./Fields/Field[@Name='start-date']").SelectSingleNode("Value").InnerText,
                                EndDate = releasecycle.SelectSingleNode("./Fields/Field[@Name='end-date']").SelectSingleNode("Value").InnerText,
                                ParentId = Convert.ToInt32(releasecycle.SelectSingleNode("./Fields/Field[@Name='parent-id']").SelectSingleNode("Value").InnerText),
                            };
                            releaseCycles.Add(newReleaseCycle);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return releaseCycles;
        }
        #endregion
    }

    /// <summary>
    /// requirement class
    /// </summary>
    public class AGMReleaseCycle : AGMEntity
    {
        #region private members
        #endregion       

        #region public Properties
        public string EndDate { get; set; }
        public string StartDate { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; }
        #endregion

        #region constructor
        #endregion

        #region public methods        
        #endregion

        #region private methods
        #endregion
    }
}
