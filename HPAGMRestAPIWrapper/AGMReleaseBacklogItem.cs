using System;
using System.Collections.Generic;
using System.Linq;

namespace HPAGMRestAPIWrapper
{
    /// <summary>
    ///  Release Backlog Item collection class
    /// </summary>
    public class AGMReleaseBacklogItems : AGMEntityCollection<AGMReleaseBacklogItem>
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="connection">connection</param>
        public AGMReleaseBacklogItems(AGMConnection connection)
            : base(connection)
        {            
        }

        #region public methods
        //public override List<AGMReleaseBacklogItem> GetCollection(List<AGMField> queryFields, List<AGMField> returnFields, List<AGMField> sortFields)
        //{
        //    var backlogItems = base.GetCollection(queryFields, returnFields, sortFields);
        //    foreach(AGMReleaseBacklogItem backlogItem in backlogItems)
        //    {
        //        var rel = backlogItem.GetField("release-id").Value;
        //        if (rel != null)
        //        {
        //            backlogItem.Release = base.Releases.Where(o => o.Id == Convert.ToInt32(rel)).Single() as AGMRelease;
        //        }
        //        var relcycle = backlogItem.GetField("sprint-id").Value;
        //        if (relcycle != null)
        //        {
        //            backlogItem.ReleaseCycle = base.ReleaseCycles.Where(o => o.Id == Convert.ToInt32(relcycle)).Single() as AGMReleaseCycle;
        //        }
        //        var product = backlogItem.GetField("product-id").Value;
        //        if (product != null)
        //        {
        //            backlogItem.Application = base.Applications.Where(o => o.Id == Convert.ToInt32(product)).Single() as AGMProduct;
        //        }
        //    }
        //    return backlogItems;
        //}
        #endregion
    }

    /// <summary>
    /// requirement class
    /// </summary>
    public class AGMReleaseBacklogItem : AGMEntity
    {
        #region private members
        #endregion       

        #region public Properties
        /// <summary>
        /// Release of the entity
        /// </summary>
        public AGMRelease Release { get; set; }
        /// <summary>
        /// Sprint of the entity
        /// </summary>
        public AGMReleaseCycle ReleaseCycle { get; set; }
        /// <summary>
        /// Product
        /// </summary>
        public AGMProduct Application { get; set; }
        #endregion

        #region constructor
        #endregion

        #region public methods
        public void GetReferenceField(string referenceFieldName, string fieldValue)
        {
            AGMReleaseBacklogItems releaseBacklogItems = new AGMReleaseBacklogItems(Connection);

            if(referenceFieldName.Equals("release"))
                Release = releaseBacklogItems.Releases.Where(o => o.Id == Convert.ToInt32(fieldValue)).Single() as AGMRelease;

            if (referenceFieldName.Equals("sprint"))
                ReleaseCycle = releaseBacklogItems.ReleaseCycles.Where(o => o.Id == Convert.ToInt32(fieldValue)).Single() as AGMReleaseCycle;

            if (referenceFieldName.Equals("application"))
                Application = releaseBacklogItems.Applications.Where(o => o.Id == Convert.ToInt32(fieldValue)).Single() as AGMProduct;

        }
        /// <summary>
        /// Update Release backlog item
        /// </summary>
        /// <returns></returns>
        public AGMReleaseBacklogItem Update()
        {
            if (Id <= 0) throw new Exception("Release backlog item Id cannot be empty.");

            return new AGMReleaseBacklogItems(Connection).Update(Id.Value, GetFieldsDictionary());
        }


        #region Task related
        public List<AGMProjectTask> GetAllTasks()
        {
            List<AGMProjectTask> tasks = new List<AGMProjectTask>();
            AGMProjectTasks pTasks = new AGMProjectTasks(Connection);
            List<AGMField> queryFields = new List<AGMField>();
            queryFields.Add(new AGMField { Name = "release-backlog-item-id", Value = Id.ToString() });
            tasks = pTasks.GetCollection(queryFields, null, null);

            return tasks;
        }
        #endregion
        #endregion

        #region private methods
        #endregion
    }
}
