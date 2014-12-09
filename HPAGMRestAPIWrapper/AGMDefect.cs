using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HPAGMRestAPIWrapper
{
    /// <summary>
    /// Defect collection class
    /// </summary>
    public class AGMDefects : AGMEntityCollection<AGMDefect>
    {
        #region Constructor
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="connection">connection</param>
        public AGMDefects(AGMConnection connection)
            : base(connection)
        {
        }
        #endregion

        #region public methods
        /// <summary>
        /// Create Defect
        /// </summary>
        /// <param name="defectFields">Defect fields</param>
        /// <returns>Defect</returns>
        public override AGMDefect Create(List<AGMField> newDefectFields)
        {
            Common.SetDictionaryValue(ref newDefectFields, "detected-by", Connection.UserName);
            Common.SetDictionaryValue(ref newDefectFields, "creation-time", DateTime.UtcNow.ToString("yyyy-MM-dd"));
            Common.SetDictionaryValue(ref newDefectFields, "status", "New");

            AGMDefect newDefect = base.Create(newDefectFields);

            List<AGMField> backlogItemNewFields = new List<AGMField>();  
            Common.SetDictionaryValue(ref backlogItemNewFields, "entity-id", newDefect.Id.ToString());
            Common.SetDictionaryValue(ref backlogItemNewFields, "entity-name", (newDefect.Fields.Where(o => o.Name.Equals("name")).Single() as AGMField).Value);
            Common.SetDictionaryValue(ref backlogItemNewFields, "entity-type", "defect");
            Common.SetDictionaryValue(ref backlogItemNewFields, "status", "New");
            Common.SetDictionaryValue(ref backlogItemNewFields, "owner", Connection.UserName);            
            AGMReleaseBacklogItem newBacklogItem = new AGMReleaseBacklogItems(Connection).Create(backlogItemNewFields);

            newDefect.BacklogItem = newBacklogItem;

            return newDefect;
        }

        /// <summary>
        /// Search defects
        /// </summary>
        /// <param name="filterCondition">search condition</param>
        /// <returns></returns>
        public List<AGMDefect> Search(List<AGMField> queryFields, List<AGMField> returnFields, List<AGMField> sortFields)
        {
            List<AGMDefect> bugList = new List<AGMDefect>();
            List<AGMField> backlogItemEntityFields = new AGMReleaseBacklogItems(Connection).EntityFields;

            List<AGMField> normalizedQueryFields = NormalizedFields(queryFields, backlogItemEntityFields);
            List<AGMField> normalizedReturnFields = NormalizedFields(returnFields, backlogItemEntityFields);
            List<AGMField> normalizedSortFields = NormalizedFields(sortFields, backlogItemEntityFields);

            bugList = GetCollection(normalizedQueryFields, normalizedReturnFields, normalizedSortFields);

            HandleBacklogItem(bugList, backlogItemEntityFields);

            return bugList;
        }

        #endregion

        #region private methods
        private void HandleBacklogItem(List<AGMDefect> bugs, List<AGMField> backlogitemEntityFields)
        {
            foreach(AGMDefect bug in bugs)
            {
                if (bug.RelatedEntities == null)
                    continue;

                if (bug.RelatedEntities.ContainsKey("release-backlog-item"))
                {                    
                    bug.BacklogItem = new AGMReleaseBacklogItem();
                    bug.BacklogItem.Connection = Connection;
                    bug.BacklogItem.Fields = new List<AGMField>();

                    foreach (AGMField blItemfield in backlogitemEntityFields)
                    {
                        bug.BacklogItem.Fields.Add(blItemfield.DeepCopy());
                    }

                    var fields = bug.RelatedEntities["release-backlog-item"];
                    foreach(AGMField field in fields.Where(o=>!string.IsNullOrEmpty(o.Value)))
                    {
                        bug.BacklogItem.Fields.Where(o => o.Name.Equals(field.Name, StringComparison.InvariantCultureIgnoreCase)).Single().Value = field.Value;
                        if (field.Name.Equals("release-id", StringComparison.InvariantCultureIgnoreCase))
                            bug.BacklogItem.GetReferenceField("release", field.Value);
                        if (field.Name.Equals("product-id", StringComparison.InvariantCultureIgnoreCase))
                            bug.BacklogItem.GetReferenceField("application", field.Value);
                        if (field.Name.Equals("sprint-id", StringComparison.InvariantCultureIgnoreCase))
                            bug.BacklogItem.GetReferenceField("sprint", field.Value);
                    }
                    
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// Defect class
    /// </summary>
    public class AGMDefect: AGMEntity
    {
        #region private members
        #endregion

        #region public Properties
        public AGMReleaseBacklogItem BacklogItem { get; set; }
        #endregion

        #region constructor

        #endregion

        #region public methods
        public AGMField GetField(string Name)
        {
            try
            {
                return base.GetField(Name);
            }
            catch
            {
                // Get Field from it's backlog item
                if (BacklogItem.GetField(Name).Value == null) GetReleaseBacklogItem(null);
                return BacklogItem.GetField(Name);
            }
        }

        /// <summary>
        /// Update defect
        /// </summary>
        /// <returns></returns>
        public AGMDefect Update()
        {
            if (Id <= 0) throw new Exception("Bug Id cannot be empty.");

            AGMDefects defects = new AGMDefects(Connection);
            AGMDefect updatedDefect = defects.Update(Id.Value, GetFieldsDictionary());
            updatedDefect.BacklogItem.Update();

            return updatedDefect;
        }

        /// <summary>
        /// Open defect
        /// </summary>
        /// <returns></returns>
        public AGMDefect Open()
        {
            SetUpdateField("status", "Open");
            BacklogItem.SetUpdateField("status", "In Progress");

            return Update();
        }

        /// <summary>
        /// Fix defect
        /// </summary>
        /// <returns></returns>
        public AGMDefect Fix()
        {
            SetUpdateField("status", "Fixed");
            BacklogItem.SetUpdateField("status", "In Testing");
            return Update();
        }

        /// <summary>
        /// Close defect
        /// </summary>
        /// <returns></returns>
        public AGMDefect Close()
        {
            SetUpdateField("status", "Closed");
            BacklogItem.SetUpdateField("status", "Done");
            return Update();
        }

        /// <summary>
        /// Proposeclose defect
        /// </summary>
        /// <returns></returns>
        public AGMDefect ProposeClose()
        {
            SetUpdateField("status", "Propose");
            BacklogItem.SetUpdateField("status", "Done");
            return Update();
        }

        #region Links in defect
        /// <summary>
        /// link to defects
        /// </summary>
        /// <param name="defectIds">defect ids</param>
        public void LinkToDefects(int[] defectIds)
        {
            if (defectIds == null)
                throw new Exception("Defect Ids are empty.");

            var defectlinks = new AGMDefectLinks(Connection);

            foreach (var defectId in defectIds)
            {
                defectlinks.Create(defectId, Id.Value, "defect");
            }
        }

        /// <summary>
        /// link to requirements
        /// </summary>
        /// <param name="reqIds">requirement Ids</param>
        public void LinkToRequirements(int[] reqIds)
        {
            if (reqIds == null)
                throw new Exception("Requirements Ids are empty.");

            var defectlinks = new AGMDefectLinks(Connection);

            foreach (var reqId in reqIds)
            {
                defectlinks.Create(Id.Value, reqId, "requirement");
            }
        }

        /// <summary>
        /// Get linked defects
        /// </summary>
        /// <returns>defect list</returns>
        public List<AGMDefect> GetLinkedDefects()
        {
            var linkeddefects = new List<AGMDefect>();

            var defectlinks = new AGMDefectLinks(Connection);
            var queryFields = new List<AGMField>();

            queryFields.Add(new AGMField { Name = "second-endpoint-id", Value = Id.Value.ToString() });
            queryFields.Add(new AGMField { Name = "second-endpoint-type", Value = "defect" });

            var dllist = defectlinks.GetCollection(queryFields, null, null);

            if (dllist != null)
            {
                var defects = new AGMDefects(Connection);
                linkeddefects.AddRange(dllist.Select(dl => defects.Get(Convert.ToInt32(dl.GetField("second-endpoint-id").Value), null, null)).Where(d => d != null));
            }

            return linkeddefects;
        }

        /// <summary>
        /// Get linked requirements
        /// </summary>
        /// <returns>requirement list</returns>
        public List<AGMRequirement> GetLinkedRequirements()
        {
            var linkedrequirements = new List<AGMRequirement>();

            var defectlinks = new AGMDefectLinks(Connection);
            var queryFields = new List<AGMField>();

            queryFields.Add(new AGMField { Name = "first-endpoint-id", Value = Id.Value.ToString() });
            queryFields.Add(new AGMField { Name = "second-endpoint-type", Value = "requirement" });

            var dllist = defectlinks.GetCollection(queryFields,null,null);

            if (dllist != null)
            {
                var reqs = new AGMRequirements(Connection);
                linkedrequirements.AddRange(dllist.Select(dl => reqs.Get(Convert.ToInt32(dl.GetField("second-endpoint-id").Value),null,null)).Where(d => d != null));
            }

            return linkedrequirements;
        }

        /// <summary>
        /// Delete defect links
        /// </summary>
        /// <param name="defectIds">defect ids</param>
        public void DeleteDefectLinks(int[] defectIds)
        {
            if (defectIds == null)
                throw new Exception("Defect Ids are empty.");

            var defectlinks = new AGMDefectLinks(Connection);

            foreach (var dl in
                defectIds.Select(defectId => defectlinks.Get(defectId, Id.Value, "defect")).Where(dl => dl != null))
            {
                dl.Delete();
            }
        }

        /// <summary>
        /// delete requirement links
        /// </summary>
        /// <param name="reqIds">requirement ids</param>
        public void DeleteRequirementLinks(int[] reqIds)
        {
            if (reqIds == null)
                throw new Exception("Requirement Ids are empty.");

            var defectlinks = new AGMDefectLinks(Connection);

            foreach (var dl in
                reqIds.Select(reqId => defectlinks.Get(Id.Value, reqId, "requirement")).Where(dl => dl != null))
            {
                dl.Delete();
            }
        }
        #endregion

        public void GetReleaseBacklogItem(List<AGMField> returnFields)
        {
            AGMReleaseBacklogItems backlogItems = new AGMReleaseBacklogItems(Connection);
            List<AGMField> queryFields = new List<AGMField>();
            queryFields.Add(new AGMField { Name = "entity-id", Value = Id.Value.ToString() });
            queryFields.Add(new AGMField { Name = "entity-type", Value = "defect" });
            List<AGMReleaseBacklogItem> items = backlogItems.GetCollection(queryFields, returnFields, null);
            if (items != null)
            {
                BacklogItem = items[0];                
            }
        }

        #endregion
    }
}
