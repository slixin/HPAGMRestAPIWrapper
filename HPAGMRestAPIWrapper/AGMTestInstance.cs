using System.Collections.Generic;
using System.Linq;

namespace HPAGMRestAPIWrapper
{
    /// <summary>
    /// Test instance collection class
    /// </summary>
    public class AGMTestInstances : AGMEntityCollection<AGMTestInstance>
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="connection">connection</param>
        public AGMTestInstances(AGMConnection connection)
            : base(connection)
        {            
        }

        #region Public methods
        /// <summary>
        /// Create Test Instance
        /// </summary>
        /// <param name="testId">test Id</param>
        /// <param name="testSetId">Test Set Id</param>
        /// <param name="order">test Set order number</param>
        /// <param name="type">Test type, normally is MANUAL</param>
        /// <returns>Test Instance</returns>
        public AGMTestInstance Create(int testId, int testSetId, int order, string type)
        {
            var fields = new List<AGMField>();

            fields.Add(new AGMField{Name="test-id", Value=testId.ToString()});
            fields.Add(new AGMField{Name="cycle-id", Value=testSetId.ToString()});
            fields.Add(new AGMField{Name="test-order", Value=order.ToString()});
            fields.Add(new AGMField{Name="subtype-id", Value=string.Format("hp.qc.test-instance.{0}", type)});

            return base.Create(fields);
        }
        #endregion
    }

    /// <summary>
    /// test instance class
    /// </summary>
    public class AGMTestInstance : AGMEntity
    {
        #region private members
        #endregion

        #region public Properties
        #endregion

        #region constructor
        #endregion

        #region public methods 
        /// <summary>
        /// Get Last test run
        /// </summary>
        /// <returns>Test run</returns>
        public AGMRun GetLastRun()
        {
            var queryFields = new List<AGMField>();

            queryFields.Add(new AGMField { Name = "cycle-id", Value = GetField("cycle-id").Value });
            queryFields.Add(new AGMField { Name = "test-instance", Value = GetField("test-instance").Value });
            queryFields.Add(new AGMField { Name = "test-id", Value = GetField("test-id").Value });

            var runs = new AGMRuns(Connection);
            var runlist = runs.GetCollection(queryFields, null, null);
            runlist.Sort(delegate(AGMRun r1, AGMRun r2)
            {
                var r1Id = (r1.Id == null ? 0 : r1.Id.Value);
                var r2Id = (r2.Id == null ? 0 : r2.Id.Value);
                return r2Id.CompareTo(r1Id);
            });
            var lastrun = runlist.First();

            return lastrun;
        }
        #endregion
    }
}
