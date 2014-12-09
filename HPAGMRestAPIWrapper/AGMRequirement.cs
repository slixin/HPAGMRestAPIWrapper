using System;
using System.Collections.Generic;
using System.Linq;

namespace HPAGMRestAPIWrapper
{
    /// <summary>
    /// requirment collection class
    /// </summary>
    public class AGMRequirements : AGMEntityCollection<AGMRequirement>
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="connection">connection</param>
        public AGMRequirements(AGMConnection connection)
            : base(connection)
        {
        }

        #region public methods
        /// <summary>
        /// Create requirement
        /// </summary>
        /// <param name="parentId">parent id</param>
        /// <param name="reqType">req type</param>
        /// <param name="name">req name</param>
        /// <param name="reqFields">req fields</param>
        /// <returns></returns>
        public AGMRequirement Create(int parentId, string reqType, string name, List<AGMField> fields)
        {
            if (string.IsNullOrEmpty(reqType))
                throw new Exception("Type cannot be empty.");

            if (string.IsNullOrEmpty(name))
                throw new Exception("Requirement name cannot be empty.");

            #region Add requirement entity
            AGMRequirements reqs = new AGMRequirements(Connection);
            if (reqs.Types.Where(o => o.Name.ToLower().Equals(reqType.ToLower())).Count() == 0)
                throw new Exception(string.Format("Cannot find the requirement type: {0}, please double check", reqType));
            string typeid = (reqs.Types.Where(o => o.Name.ToLower().Equals(reqType.ToLower())).Single() as AGMType).Id;

            Common.SetDictionaryValue(ref fields, "parent-id", parentId.ToString());
            Common.SetDictionaryValue(ref fields, "type-id", typeid);
            Common.SetDictionaryValue(ref fields, "owner", Connection.UserName);
            Common.SetDictionaryValue(ref fields, "name", name);

            AGMRequirement newReq = base.Create(fields);
            #endregion

            #region Add release backlog item for the requirement
            List<AGMField> backlogItemNewFields = new List<AGMField>();  
            Common.SetDictionaryValue(ref backlogItemNewFields, "entity-id", newReq.Id.ToString());
            Common.SetDictionaryValue(ref backlogItemNewFields, "entity-name", (newReq.Fields.Where(o => o.Name.Equals("name")).Single() as AGMField).Value);
            Common.SetDictionaryValue(ref backlogItemNewFields, "entity-type", "requirement");
            Common.SetDictionaryValue(ref backlogItemNewFields, "status", "New");
            Common.SetDictionaryValue(ref backlogItemNewFields, "owner", Connection.UserName);
            AGMReleaseBacklogItem newBacklogItem = new AGMReleaseBacklogItems(Connection).Create(backlogItemNewFields);
            #endregion

            newReq.BacklogItem = newBacklogItem;

            return newReq;
        }

        public List<AGMRequirement> Search(List<AGMField> queryFields, List<AGMField> returnFields, List<AGMField> sortFields)
        {
            List<AGMRequirement> reqList = new List<AGMRequirement>();
            List<AGMField> backlogItemEntityFields = new AGMReleaseBacklogItems(Connection).EntityFields;

            List<AGMField> normalizedQueryFields = NormalizedFields(queryFields, backlogItemEntityFields);
            List<AGMField> normalizedReturnFields = NormalizedFields(returnFields, backlogItemEntityFields);
            List<AGMField> normalizedSortFields = NormalizedFields(sortFields, backlogItemEntityFields);

            reqList = GetCollection(normalizedQueryFields, normalizedReturnFields, normalizedSortFields);

            HandleBacklogItem(reqList, backlogItemEntityFields);
            
            return reqList;
        }
        #endregion

        #region private methods
        private void HandleBacklogItem(List<AGMRequirement> reqs, List<AGMField> backlogitemEntityFields)
        {
            foreach (AGMRequirement req in reqs)
            {
                if (req.RelatedEntities == null)
                    continue;

                if (req.RelatedEntities.ContainsKey("release-backlog-item"))
                {                    
                    req.BacklogItem = new AGMReleaseBacklogItem();
                    req.BacklogItem.Connection = Connection;
                    req.BacklogItem.Fields = new List<AGMField>();

                    foreach (AGMField blItemfield in backlogitemEntityFields)
                    {
                        req.BacklogItem.Fields.Add(blItemfield.DeepCopy());
                    }

                    var fields = req.RelatedEntities["release-backlog-item"];
                    foreach (AGMField field in fields.Where(o => !string.IsNullOrEmpty(o.Value)))
                    {
                        req.BacklogItem.Fields.Where(o => o.Name.Equals(field.Name, StringComparison.InvariantCultureIgnoreCase)).Single().Value = field.Value;
                        if (field.Name.Equals("release-id", StringComparison.InvariantCultureIgnoreCase))
                            req.BacklogItem.GetReferenceField("release", field.Value);
                        if (field.Name.Equals("product-id", StringComparison.InvariantCultureIgnoreCase))
                            req.BacklogItem.GetReferenceField("application", field.Value);
                        if (field.Name.Equals("sprint-id", StringComparison.InvariantCultureIgnoreCase))
                            req.BacklogItem.GetReferenceField("sprint", field.Value);
                    }
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// requirement class
    /// </summary>
    public class AGMRequirement : AGMEntity
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
        /// Add sub requirement
        /// </summary>
        /// <param name="reqType">type of requirement</param>
        /// <param name="name">name of requirement</param>
        /// <param name="requirementFields"></param>
        /// <returns></returns>
        public AGMRequirement AddSubRequirement(string reqType, string name, List<AGMField> reqFields)
        {
            if (Id <= 0) throw new Exception("Requirement Id cannot be empty.");

            return new AGMRequirements(Connection).Create(Id.Value, reqType, name, reqFields);
        }

        /// <summary>
        /// Update Requirement
        /// </summary>
        /// <returns></returns>
        public AGMRequirement Update()
        {
            if (Id <= 0) throw new Exception("Requirement Id cannot be empty.");
            
            AGMRequirements reqs = new AGMRequirements(Connection);
            AGMRequirement updatedReq = reqs.Update(Id.Value, GetFieldsDictionary());
            updatedReq.BacklogItem.Update();

            return updatedReq;
        }

        public AGMRequirement AssignTo(string newOwner)
        {
            if (Id <= 0) throw new Exception("Requirement Id cannot be empty.");
            
            SetUpdateField("owner", newOwner);
            BacklogItem.SetUpdateField("owner", newOwner);
            return Update();            
        }

        public AGMRequirement AssignBackLogItemTo(string newOwner)
        {
            if (Id <= 0) throw new Exception("Requirement Id cannot be empty.");
            
            BacklogItem.SetUpdateField("owner", newOwner);
            return Update();            
        }

        #region Links in defect
        /// <summary>
        /// Link defects to this requirements
        /// </summary>
        /// <param name="defectIds">defect ids</param>
        public void LinkToDefects(int[] defectIds)
        {
            if (defectIds == null)
                throw new Exception("Defect Ids are empty.");

            var defectlinks = new AGMDefectLinks(Connection);

            foreach (var defectId in defectIds)
            {
                defectlinks.Create(defectId, Id.Value, "requirement");
            }
        }

        /// <summary>
        /// Get all linked defects under the requirements
        /// </summary>
        /// <returns>list of defects</returns>
        public List<AGMDefect> GetLinkedDefects()
        {
            var linkeddefects = new List<AGMDefect>();

            var defectlinks = new AGMDefectLinks(Connection);
            var queryFields = new List<AGMField>();
            queryFields.Add(new AGMField { Name = "second-endpoint-id", Value = Id.Value.ToString() });
            queryFields.Add(new AGMField { Name = "second-endpoint-type", Value = "requirement" });

            var dllist = defectlinks.GetCollection(queryFields, null, null);

            if (dllist != null)
            {
                var defects = new AGMDefects(Connection);
                linkeddefects.AddRange(dllist.Select(dl => defects.Get(Convert.ToInt32(dl.GetField("first-endpoint-id").Value),null, null)).Where(d => d != null));
            }

            return linkeddefects;
        }

        /// <summary>
        /// delete defects under the requirement
        /// </summary>
        /// <param name="defectIds">defect ids</param>
        public void DeleteLinkedDefects(int[] defectIds)
        {
            if (defectIds == null)
                throw new Exception("Defect Ids are empty.");

            var defectlinks = new AGMDefectLinks(Connection);

            foreach (var dl in
                defectIds.Select(defectId => defectlinks.Get(defectId, Id.Value, "requirement")).Where(dl => dl != null))
            {
                dl.Delete();
            }
        }
        #endregion

        #region Link to Release backlog Item
        public void GetReleaseBacklogItem(List<AGMField> returnFields)
        {
            AGMReleaseBacklogItems backlogItems = new AGMReleaseBacklogItems(Connection);
            List<AGMField> queryFields = new List<AGMField>();
            queryFields.Add(new AGMField { Name = "entity-id", Value = Id.Value.ToString() });
            queryFields.Add(new AGMField { Name = "entity-type", Value = "requirement" });
            List<AGMReleaseBacklogItem> items = backlogItems.GetCollection(queryFields, returnFields, null);
            if (items != null)
            {
                BacklogItem = items[0];
            }
        }
        

        //Not implemented yet
        //public void CreateReleaseBacklogItem(string release, string sprint)
        //{
        //    AGMReleaseBacklogItems items = new AGMReleaseBacklogItems(Connection);
        //    var fields = new List<AGMField>
        //                      {
        //                          {"owner", Connection.UserName},
        //                          {"entity-name", Fields.Where(o=>o.Name.Equals("name")).Single().Value},
        //                          {"status", "New"},
        //                          {"release-id", "New"},
        //                          {"theme-id", "New"},
        //                          {"team-id", "New"},
        //                          {"feature-id", "New"},
        //                      };
        //    //items.Create(Id.Value, "Requirement", fields);
        //}
        #endregion

        #endregion

        #region private methods
        #endregion
    }
}
