using System;
using System.Collections.Generic;
using System.Linq;

namespace HPAGMRestAPIWrapper
{
    /// <summary>
    /// Test set collection class
    /// </summary>
    public class AGMTestSets : AGMEntityCollection<AGMTestSet>
    {
        /// <summary>
        /// constructor
        /// </summary>
        public AGMTestSets(AGMConnection connection) : base(connection)
        {
        }

        #region public methods
        /// <summary>
        /// Create Test Set
        /// </summary>
        /// <param name="testSetFolderId">test set folder Id</param>
        /// <param name="name">Test Name</param>
        /// <returns>Test</returns>
        public AGMTestSet Create(int testSetFolderId, string name)
        {
            if (string.IsNullOrEmpty(name)) throw new Exception("Test name cannot be empty.");

            List<AGMField> fields = new List<AGMField>();

            fields.Add(new AGMField { Name = "name", Value = name });
            fields.Add(new AGMField { Name = "parent-id", Value = testSetFolderId.ToString() });
            fields.Add(new AGMField { Name = "subtype-id", Value = "hp.qc.test-set.default" });
            fields.Add(new AGMField { Name = "status", Value = "Open" });

            return base.Create(fields);
        }
        #endregion
    }

    /// <summary>
    /// Test set class
    /// </summary>
    public class AGMTestSet : AGMEntity
    {
        #region private members
        #endregion

        #region public Properties
        #endregion

        #region constructor
        #endregion

        #region public methods
        /// <summary>
        /// Update Test SET
        /// </summary>
        /// <returns></returns>
        public AGMTestSet Update()
        {
            if (Id <= 0) throw new Exception("Test Set Id cannot be empty.");

            return new AGMTestSets(Connection).Update(Id.Value, GetFieldsDictionary());
        }

        /// <summary>
        /// Add tests under the test set
        /// </summary>
        /// <param name="tests">test list</param>
        public void AddTests(List<AGMTest> tests)
        {
            if (tests.Count == 0) throw new Exception("Tests cannot be empty.");

            var testinstances = new AGMTestInstances(Connection);
            var i = 1;
            foreach (var test in tests)
            {
                testinstances.Create(test.Id.Value, Id.Value, i, test.GetField("subtype-id").Value);
                i++;
            }
        }

        /// <summary>
        /// Get test instance list
        /// </summary>
        /// <returns>test instance list</returns>
        public List<AGMTestInstance> GetTestInstances()
        {
            var testinstances = new AGMTestInstances(Connection);
            var queryFields = new List<AGMField>();
            queryFields.Add(new AGMField { Name = "cycle-id", Value = Id.Value.ToString() });
            return testinstances.GetCollection(queryFields, null, null);
        }

        /// <summary>
        /// Get test instance
        /// </summary>
        /// <param name="testId">test id</param>
        /// <param name="testInstance">test instance</param>
        /// <returns>test instance</returns>
        public AGMTestInstance GetTestInstance(int testId, int testInstance)
        {
            var testinstances = new AGMTestInstances(Connection);
            var queryFields = new List<AGMField>();
            queryFields.Add(new AGMField { Name = "cycle-id", Value = Id.Value.ToString() });
            queryFields.Add(new AGMField { Name = "test-id", Value = testId.ToString() });
            queryFields.Add(new AGMField { Name = "test-instance", Value = testInstance.ToString() });
            var testInstanceList = testinstances.GetCollection(queryFields,null,null);
            if (testInstanceList.Count == 1)
                return testInstanceList[0];
            throw new Exception(string.Format("Found multiple/none test instance with TestSet ID={0}, Test ID={1}, Test Instance={2}", Id.Value, testId, testInstance));
        }

        /// <summary>
        /// Run all tests under the test set
        /// </summary>
        /// <returns>run list</returns>
        public List<AGMRun> RunTests()
        {
            var testInstances = GetTestInstances();
            var runs = new AGMRuns(Connection);

            return testInstances.Select(ati => runs.Create(ati)).ToList();
        }

        /// <summary>
        /// Get last runs for the test set
        /// </summary>
        /// <returns>run list</returns>
        public List<AGMRun> GetTestLastRuns()
        {
            var testInstances = GetTestInstances();

            return testInstances.Select(testins => testins.GetLastRun()).ToList();
        }
        #endregion
    }
}
