using System;
using System.Collections.Generic;
using System.Linq;

namespace HPAGMRestAPIWrapper
{
    /// <summary>
    /// project task collection class
    /// </summary>
    public class AGMProjectTasks : AGMEntityCollection<AGMProjectTask>
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="connection">connection</param>
        public AGMProjectTasks(AGMConnection connection)
            : base(connection)
        {
        }

        #region public methods
        /// <summary>
        /// Create project task
        /// </summary>
        /// <param name="reqId">requirement id</param>
        /// <param name="assignedTo">assign to </param>
        /// <param name="description">description of the task</param>
        /// <param name="fields">fields</param>
        /// <returns></returns>
        public AGMProjectTask Create(int reqId, string assignedTo, string description, int estimatedhour, List<AGMField> fields)
        {
            if (string.IsNullOrEmpty(description))
                throw new Exception("Description cannot be empty.");

            if (reqId == 0)
                throw new Exception("Requirement Id cannot be empty.");

            #region get backlog item id
            AGMReleaseBacklogItems backlogItems = new AGMReleaseBacklogItems(Connection);
            List<AGMField> queryFields = new List<AGMField>();
            queryFields.Add(new AGMField { Name = "entity-id", Value = reqId.ToString() });
            var items = backlogItems.GetCollection(queryFields, null, null);
            if (items.Count == 0)
                throw new Exception(string.Format("Cannot get the release back log item with requirement id {0}.", reqId));

            int backlogItemId = Convert.ToInt32(items[0].Id);
            #endregion

            #region Add task for the release back log item
            List<AGMField> newTaskFields = new List<AGMField>();
            Common.SetDictionaryValue(ref newTaskFields, "release-backlog-item-id", backlogItemId.ToString());
            Common.SetDictionaryValue(ref newTaskFields, "description", description);
            Common.SetDictionaryValue(ref newTaskFields, "status", "New");
            Common.SetDictionaryValue(ref newTaskFields, "assigned-to", assignedTo);
            Common.SetDictionaryValue(ref newTaskFields, "estimated", estimatedhour.ToString());
            AGMProjectTask newTask = new AGMProjectTasks(Connection).Create(newTaskFields);
            #endregion

            return newTask;
        }

        /// <summary>
        /// Search tasks
        /// </summary>
        /// <param name="filterCondition">search condition</param>
        /// <returns></returns>
        public List<AGMProjectTask> Search(List<AGMField> queryFields, List<AGMField> returnFields)
        {
            List<AGMProjectTask> taskList = new List<AGMProjectTask>();

            if (returnFields == null)
                returnFields = new List<AGMField>();

            if (returnFields.Where(o => o.Name.Equals("id", StringComparison.InvariantCultureIgnoreCase)).Count() == 0) returnFields.Add(new AGMField { Name = "id", Entity = "project-task" });
            if (returnFields.Where(o => o.Name.Equals("description", StringComparison.InvariantCultureIgnoreCase)).Count() == 0) returnFields.Add(new AGMField { Name = "description", Entity = "project-task" });


            taskList = base.GetCollection(queryFields, returnFields, null);
            return taskList;
        }
        #endregion
    }

    /// <summary>
    /// project task class
    /// </summary>
    public class AGMProjectTask : AGMEntity
    {
        #region private members
        #endregion       

        #region public Properties
        #endregion

        #region constructor
        #endregion

        #region public methods
        /// <summary>
        /// Set task to be In Progress
        /// </summary>
        /// <returns></returns>
        public AGMProjectTask Start()
        {
            if (Id <= 0) throw new Exception("Task Id cannot be empty.");

            SetUpdateField("status", "In Progress");
            return Update();
        }

        /// <summary>
        /// Set task to be Completed
        /// </summary>
        /// <returns></returns>
        public AGMProjectTask Complete()
        {
            if (Id <= 0) throw new Exception("Task Id cannot be empty.");

            SetUpdateField("status", "Completed");
            return Update();
        }

        /// <summary>
        /// change assignment
        /// </summary>
        /// <param name="newOwner"></param>
        /// <returns></returns>
        public AGMProjectTask AssignTo(string newOwner)
        {
            if (Id <= 0) throw new Exception("Task Id cannot be empty.");
            
            SetUpdateField("assigned-to", newOwner);
            return Update();            
        }

        /// <summary>
        /// update task
        /// </summary>
        /// <returns></returns>
        public AGMProjectTask Update()
        {
            if (Id <= 0) throw new Exception("Task Id cannot be empty.");

            AGMProjectTasks tasks = new AGMProjectTasks(Connection);
            AGMProjectTask updatedTask = tasks.Update(Id.Value, GetFieldsDictionary());

            return updatedTask;
        }

        /// <summary>
        /// Change task description
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public AGMProjectTask Change(string description)
        {
            if (Id <= 0) throw new Exception("Task Id cannot be empty.");

            SetUpdateField("description", description);
            return Update();            
        }
        #endregion

        #region private methods
        #endregion
    }
}
