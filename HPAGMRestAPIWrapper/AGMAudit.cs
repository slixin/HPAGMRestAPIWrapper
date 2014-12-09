using HPAGMRestAPIWrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml;

namespace HPAGMRestAPIWrapper
{
    public class AGMAudits : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public AGMConnection Connection { get; set; }

        public AGMAudits(AGMConnection connection)
        {
            Connection = connection;
        }

        public List<AGMAudit> GetAudits(int entityId, string entityName)
        {
            var audits = new List<AGMAudit>();

            string url = null;

            url = string.Format("{0}/agm/rest/domains/{1}/projects/{2}/{3}/{4}/audits", Connection.ServerName, Connection.Domain, Connection.Project, entityName, entityId);

            try
            {
                Connection.Http.Method = "GET";
                Connection.Http.ContentType = "application/xml";
                var status = Connection.Http.Request(url);
                if (status >= 200 && status <= 204)
                {
                    if (!string.IsNullOrEmpty(Connection.Http.Response))
                    {
                        var doc = new XmlDocument();
                        doc.LoadXml(Connection.Http.Response);
                        XmlNodeList auditlist = doc.SelectNodes(@"Audits/Audit");

                        foreach (XmlNode audit in auditlist)
                        {
                            List<AGMAudit.AGMAuditProperty> auditProperteis = new List<AGMAudit.AGMAuditProperty>();
                            foreach (XmlNode propNode in audit.SelectNodes("Properties/Property"))
                            {
                                auditProperteis.Add(new AGMAudit.AGMAuditProperty
                                {
                                    Name = propNode.Attributes["Name"].Value,
                                    Label = propNode.Attributes["Label"].Value,
                                    NewValue = propNode.SelectSingleNode("NewValue").InnerText,
                                    OldValue = propNode.SelectSingleNode("OldValue").InnerText,
                                });
                            }
                            var newAudit = new AGMAudit
                            {
                                Id = Convert.ToInt32(audit.SelectSingleNode("Id").InnerText),
                                Action = audit.SelectSingleNode("Action").InnerText,
                                ParentId = Convert.ToInt32(audit.SelectSingleNode("ParentId").InnerText),
                                ParentType = audit.SelectSingleNode("ParentType").InnerText,
                                Time = Convert.ToDateTime(audit.SelectSingleNode("Time").InnerText),
                                User = audit.SelectSingleNode("User").InnerText,
                                Properties = auditProperteis,
                            };
                            audits.Add(newAudit);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return audits;
        }

    }
    /// <summary>
    /// Entity Audit class
    /// </summary>
    public class AGMAudit : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public class AGMAuditProperty
        {
            public string Name {get;set;}
            public string Label {get;set;}
            public string NewValue {get;set;}
            public string OldValue {get;set;}
        }

        #region public properties
        public int Id { get; set; }
        public string Action { get; set; }
        public int ParentId { get; set; }
        public string ParentType { get; set; }
        public DateTime Time { get; set; }
        public string User { get; set; }
        public List<AGMAuditProperty> Properties { get; set; }        
        #endregion

        #region private members
        #endregion

        public AGMAudit()
        {

        }

        #region INotifyPropertyChanged Members
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
