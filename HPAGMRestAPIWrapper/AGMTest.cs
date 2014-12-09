using System;
using System.Collections.Generic;

namespace HPAGMRestAPIWrapper
{
    /// <summary>
    /// Test Collection class
    /// </summary>
    public class AGMTests : AGMEntityCollection<AGMTest>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connection">connection</param>
        public AGMTests(AGMConnection connection)
            : base(connection)
        {
        }

        #region public methods
        /// <summary>
        /// Create Test
        /// </summary>
        /// <param name="testFolderId">test folder Id</param>
        /// <param name="name">Test Name</param>
        /// <returns>Test</returns>
        public AGMTest Create(int testFolderId, string name)
        {
            if (string.IsNullOrEmpty(name)) throw new Exception("Test name cannot be empty.");

            var fields = new List<AGMField>();

            fields.Add(new AGMField{Name="name", Value=name});
            fields.Add(new AGMField{Name="parent-id", Value=testFolderId.ToString()});
            fields.Add(new AGMField{Name="subtype-id", Value="MANUAL"});
            fields.Add(new AGMField{Name="status", Value="Ready"});

            return base.Create(fields);
        }
        #endregion
    }

    /// <summary>
    /// Test class
    /// </summary>
    public class AGMTest : AGMEntity
    {
        #region private members
        #endregion

        #region public Properties
        #endregion

        #region constructor
        #endregion

        #region public methods
        /// <summary>
        /// Update Test
        /// </summary>
        /// <returns></returns>
        public AGMTest Update()
        {
            if (Id <= 0) throw new Exception("Test Id cannot be empty.");

            return new AGMTests(Connection).Update(Id.Value, GetFieldsDictionary());
        }

        /// <summary>
        /// Get steps for the test
        /// </summary>
        /// <returns></returns>
        public List<AGMTestStep> GetSteps()
        {
            if (Id <= 0) throw new Exception("Test Id cannot be empty.");

            var queryFields = new List<AGMField>();
            queryFields.Add(new AGMField { Name = "parent-id", Value = Id.ToString() });

            var teststeps = new AGMTestSteps(Connection);
            return teststeps.GetCollection(queryFields, null, null);
        }

        /// <summary>
        /// Get steps for the test
        /// </summary>
        /// <returns></returns>
        public AGMTestStep GetStep(int order)
        {
            if (Id <= 0) throw new Exception("Test Id cannot be empty.");

            var queryFields = new List<AGMField>();
            queryFields.Add(new AGMField { Name = "parent-id", Value = Id.ToString() });
            queryFields.Add(new AGMField { Name = "step-order", Value = order.ToString() });

            var teststeps = new AGMTestSteps(Connection);
            var teststepcollection = teststeps.GetCollection(queryFields, null, null);

            if (teststepcollection == null)
                throw new Exception(string.Format("Cannot find the step '{0}' in this test", order.ToString()));
            if (teststepcollection.Count == 0)
                throw new Exception(string.Format("Cannot find the step '{0}' in this test", order.ToString()));

            return teststepcollection[0];
        }

        /// <summary>
        /// add a new step for the test
        /// </summary>
        /// <param name="stepName"></param>
        /// <param name="description"></param>
        /// <param name="expected"></param>
        /// <returns></returns>
        public AGMTestStep AddStep(string stepName, string description, string expected)
        {
            if (Id <= 0) throw new Exception("Test Id cannot be empty.");

            var teststeps = new AGMTestSteps(Connection);
            var newteststep = teststeps.Create(Id.Value, stepName, description, expected);

            return newteststep;
        }
        #endregion

    }
}
