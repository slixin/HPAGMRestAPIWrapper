using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Threading.Tasks;
using System.ComponentModel;

namespace HPAGMRestAPIWrapper
{
    /// <summary>
    /// Entity Field class
    /// </summary>
    public class AGMField : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;      

        public enum FieldType {UsersList, Memo, Date, String, LookupList, Number, DateTime, Reference, Float}

        #region public properties
        /// <summary>
        /// Is to update field
        /// </summary>
        public bool IsToUpdate { get; set; }

        /// <summary>
        /// Field Label name
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Field value
        /// </summary>
        public string Value { get; set; }

        public int Size { get; set; }
        public bool History { get; set; }
        public bool Required { get; set; }
        public bool System { get; set; }
        public FieldType Type { get; set; }
        public bool Verify { get; set; }
        public bool Virtual { get; set; }
        public bool Active { get; set; }
        public bool Editable { get; set; }
        public bool Filterable { get; set; }
        public bool Groupable { get; set; }
        public bool SupportsMultivalue { get; set; }
        public bool Visible { get; set; }
        public bool MemoContainsHtml { get; set; }
        public bool VersionControlled { get; set; }
        public int ListId { get; set; }
        public string ReferenceType { get; set; }
        public string OrderBy { get; set; }
        public AGMConnection Connection { get; set; }

        public Dictionary<string, string> Options { get; set; }
        public string Entity
        {
            get
            {
                return _entity;
            }
            set
            {
                if (value != _entity)
                {
                    _entity = value;
                    OnPropertyChanged("Entity");
                }
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value != _name)
                {
                    _name = value;
                  OnPropertyChanged("Name");
                }
            }
        }
        #endregion

        #region private members
        private string _entity;
        private string _name;
        #endregion

        public AGMField()
        {

        }

        public AGMField DeepCopy()
        {
            AGMField clonedField = (AGMField)this.MemberwiseClone();

            return clonedField;
        }

        /// <summary>
        /// Get field for specified entity type.
        /// </summary>
        /// <param name="conn">connection</param>
        /// <param name="entityType">entity type</param>
        /// <returns>list of fields</returns>
        public List<AGMField> GetFields(AGMConnection conn, string entityName, int? entityTypeId)
        {
            var fields = new List<AGMField>();

            string url = null;
            
            if (entityTypeId != null)
                url = string.Format("{0}/agm/rest/domains/{1}/projects/{2}/customization/entities/{3}/types/{4}/fields", conn.ServerName, conn.Domain, conn.Project, entityName, entityTypeId.Value.ToString());
            else
                url = string.Format("{0}/agm/rest/domains/{1}/projects/{2}/customization/entities/{3}/fields", conn.ServerName, conn.Domain, conn.Project, entityName);

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
                        XmlNodeList fieldlist = doc.SelectNodes(@"Fields/Field");

                        foreach(XmlNode field in fieldlist)
                        {
                            List<AGMField> fieldProperteis = new List<AGMField>();
                            foreach (XmlNode propNode in field.ChildNodes)
                            {
                                fieldProperteis.Add(new AGMField { Name = propNode.Name, Value = propNode.InnerText });
                            }
                            var newfield = new AGMField
                                {
                                    Connection = conn,
                                    Entity = entityName,
                                    IsToUpdate = false,
                                    Name = field.Attributes["Name"].Value,
                                    Label = field.Attributes["Label"] == null ? string.Empty : field.Attributes["Label"].Value,
                                    Size = Convert.ToInt32(field.SelectSingleNode("Size").InnerText),
                                    History = Convert.ToBoolean(field.SelectSingleNode("History").InnerText),
                                    Active = Convert.ToBoolean(field.SelectSingleNode("Active").InnerText),
                                    Editable = Convert.ToBoolean(field.SelectSingleNode("Editable").InnerText),
                                    Filterable = Convert.ToBoolean(field.SelectSingleNode("Filterable").InnerText),
                                    Groupable = Convert.ToBoolean(field.SelectSingleNode("Groupable").InnerText),
                                    MemoContainsHtml = Convert.ToBoolean(field.SelectSingleNode("MemoContainsHtml").InnerText),
                                    Required = Convert.ToBoolean(field.SelectSingleNode("Required").InnerText),
                                    SupportsMultivalue = Convert.ToBoolean(field.SelectSingleNode("SupportsMultivalue").InnerText),
                                    System = Convert.ToBoolean(field.SelectSingleNode("System").InnerText),
                                    Verify = Convert.ToBoolean(field.SelectSingleNode("Verify").InnerText),
                                    VersionControlled = Convert.ToBoolean(field.SelectSingleNode("VersionControlled").InnerText),
                                    Visible = Convert.ToBoolean(field.SelectSingleNode("Visible").InnerText),
                                    Type = (FieldType)Enum.Parse(typeof(FieldType), field.SelectSingleNode("Type").InnerText),
                                    Virtual = Convert.ToBoolean(field.SelectSingleNode("Virtual").InnerText),
                                    ListId = field.SelectSingleNode("List-Id") == null ? 0 : Convert.ToInt32(field.SelectSingleNode("List-Id").InnerText),
                                    ReferenceType = field.SelectSingleNode("References") == null ? string.Empty: field.SelectSingleNode("References/RelationReference").Attributes["ReferencedEntityType"].Value,
                                };
                            fields.Add(newfield);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return fields;
        }

        public Dictionary<string, string> GetOptions()
        {
            return GetOptions(new List<AGMField>());
        }

        public Dictionary<string, string> GetOptions(List<AGMField> filter)
        {
            if (Connection == null)
                throw new Exception("Does not connected yet.");

            Dictionary<string, string> optionals = new Dictionary<string, string>();            

            #region Get all optionals for specified field
            if (Type == FieldType.LookupList)
            {
                List<AGMList> lists = AGMList.GetLists(Connection, Entity);
                if (lists.Where(o => o.Id == ListId).Count() > 0)
                {
                    foreach (KeyValuePair<string, string> kv in (lists.Where(o => o.Id == ListId).Single() as AGMList).Items)
                    {
                        optionals.Add(kv.Value, kv.Value);
                    }
                }
            }

            if (!string.IsNullOrEmpty(ReferenceType))
            {
                switch (ReferenceType)
                {
                    case "release":
                        optionals = new AGMReleases(Connection).GetOptionals(filter);
                        break;
                    case "release-cycle":
                        optionals = new AGMReleaseCycles(Connection).GetOptionals(filter);
                        break;                    
                    case "changeset":
                        optionals = new AGMChangesets(Connection).GetOptionals(filter);
                        break;
                    case "build-instance":
                        optionals = new AGMBuildInstances(Connection).GetOptionals(filter);
                        break;                    
                    case "product":
                        optionals = new AGMProducts(Connection).GetOptionals(filter);
                        break;
                    case "team":
                        optionals = new AGMTeams(Connection).GetOptionals(filter);
                        break;
                    //case "requirement":
                    //    optionals = new AGMRequirements(Connection).GetOptionals();
                    //    break;
                    //case "defect":
                    //    optionals = new AGMDefects(Connection).GetOptionals();
                    //    break;
                    //case "release-backlog-item":
                    //    optionals = new AGMReleaseBacklogItems(Connection).GetOptionals();
                    //    break;
                    default:
                        break;
                }
            }
            #endregion

            Dictionary<string, string> sortedOptionals = new Dictionary<string, string>();

            sortedOptionals = optionals.OrderBy(o => o.Value).ToDictionary(r => r.Key, r => r.Value);

            return sortedOptionals;
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
