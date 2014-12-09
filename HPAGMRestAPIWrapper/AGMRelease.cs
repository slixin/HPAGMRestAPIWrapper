using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace HPAGMRestAPIWrapper
{
    /// <summary>
    ///  Release collection class
    /// </summary>
    public class AGMReleases : AGMEntityCollection<AGMRelease>
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="connection">connection</param>
        public AGMReleases(AGMConnection connection)
            : base(connection)
        {            
        }

        #region public methods
        public static List<AGMRelease> GetReleases(AGMConnection conn)
        {
            var releases = new List<AGMRelease>();

            string url = string.Format("{0}/agm/rest/domains/{1}/projects/{2}/releases", conn.ServerName, conn.Domain, conn.Project);

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
                        XmlNodeList releaselist = doc.SelectNodes(@"Entities/Entity");

                        foreach (XmlNode release in releaselist)
                        {
                            var newRelease = new AGMRelease
                            {
                                Connection = conn,
                                Id = Convert.ToInt32(release.SelectSingleNode("./Fields/Field[@Name='id']").SelectSingleNode("Value").InnerText),
                                Name = release.SelectSingleNode("./Fields/Field[@Name='name']").SelectSingleNode("Value").InnerText,
                                StartDate = release.SelectSingleNode("./Fields/Field[@Name='start-date']").SelectSingleNode("Value").InnerText,
                                EndDate = release.SelectSingleNode("./Fields/Field[@Name='end-date']").InnerText,
                                SprintDuration = Convert.ToInt32(release.SelectSingleNode("./Fields/Field[@Name='sprint-duration']").SelectSingleNode("Value").InnerText),
                                SprintDurationUnits = release.SelectSingleNode("./Fields/Field[@Name='sprint-duration-units']").SelectSingleNode("Value").InnerText,
                                SprintsCount = Convert.ToInt32(release.SelectSingleNode("./Fields/Field[@Name='sprints-count']").SelectSingleNode("Value").InnerText),
                            };
                            releases.Add(newRelease);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return releases;
        }
        #endregion
    }

    /// <summary>
    /// requirement class
    /// </summary>
    public class AGMRelease : AGMEntity
    {
        #region private members
        #endregion       

        #region public Properties
        public string EndDate { get; set; }
        public string StartDate { get; set; }
        public string SprintDurationUnits { get; set; }
        public int SprintDuration { get; set; }
        public int SprintsCount { get; set; }
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
