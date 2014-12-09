using System;
using System.Collections.Generic;

namespace HPAGMRestAPIWrapper
{
    /// <summary>
    /// Test run collection class
    /// </summary>
    public class AGMRuns : AGMEntityCollection<AGMRun>
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="connection">connection</param>
        public AGMRuns(AGMConnection connection)
            : base(connection)
        { }

        #region Public methods
        /// <summary>
        /// Create Test Run
        /// </summary>
        /// <param name="instance">test instance</param>
        /// <returns>Test Instance</returns>
        public AGMRun Create(AGMTestInstance instance)
        {
            var now = DateTime.UtcNow;

            var fields = new List<AGMField>();

            fields.Add(new AGMField{Name="test-id", Value=instance.GetField("test-id").Value});
            fields.Add(new AGMField{Name="cycle-id", Value=instance.GetField("cycle-id").Value});
            fields.Add(new AGMField{Name="testcycl-id", Value=instance.GetField("id").Value});
            fields.Add(new AGMField{Name="status", Value="No Run"});
            fields.Add(new AGMField{Name="name", Value=string.Format("Run_{0}-{1}_{2}-{3}-{4}", now.Month, now.Day, now.Hour, now.Minute, now.Second)});
            fields.Add(new AGMField{Name="owner", Value=Connection.UserName});

            var instType =instance.GetField("subtype-id").Value;
            fields.Add(new AGMField { Name = "subtype-id", Value = string.Format("hp.qc.run.{0}", instType.Substring(instType.LastIndexOf(".") + 1, instType.Length - instType.LastIndexOf(".") - 1)) });

            return base.Create(fields);
        }
        #endregion
    }

    /// <summary>
    /// Run class
    /// </summary>
    public class AGMRun : AGMEntity
    {
        #region private members
        #endregion

        #region public Properties
        #endregion

        #region constructor
        #endregion

        #region public methods
        /// <summary>
        /// Update Test Rim
        /// </summary>
        /// <returns></returns>
        public AGMRun Update()
        {
            if (Id <= 0) throw new Exception("Test Run Id cannot be empty.");

            return new AGMRuns(Connection).Update(Id.Value, GetFieldsDictionary());
        }

        public AGMRun Pass()
        {
            if (Id <= 0) throw new Exception("Test Run Id cannot be empty.");

            SetUpdateField("status", "Passed");
            return Update();
        }

        public AGMRun Fail()
        {
            if (Id <= 0) throw new Exception("Test Run Id cannot be empty.");

            SetUpdateField("status", "Failed");
            return Update();
        }

        public AGMRun NotComplete()
        {
            if (Id <= 0) throw new Exception("Test Run Id cannot be empty.");

            SetUpdateField("status", "Not Completed");
            return Update();
        }

        public AGMRun Block()
        {
            if (Id <= 0) throw new Exception("Test Run Id cannot be empty.");

            SetUpdateField("status", "Blocked");
            return Update();
        }

        public AGMRun Warn()
        {
            if (Id <= 0) throw new Exception("Test Run Id cannot be empty.");

            SetUpdateField("status", "Warning");
            return Update();
        }

        public AGMRun ReSet()
        {
            if (Id <= 0) throw new Exception("Test Run Id cannot be empty.");

            SetUpdateField("status", "No Run");
            return Update();
        }
        #endregion


    }
}
