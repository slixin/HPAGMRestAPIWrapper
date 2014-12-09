using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HPAGMRestAPIWrapper;

namespace HPAGMRestAPIWrapper
{
    /// <summary>
    /// Test Steps Collection class
    /// </summary>
    public class AGMTestSteps : AGMEntityCollection<AGMTestStep>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connection">connection</param>
        public AGMTestSteps(AGMConnection connection)
            : base(connection)
        {
        }

        #region public methods
        /// <summary>
        /// Create Test Step
        /// </summary>
        /// <param name="testId">test id</param>
        /// <param name="stepName">step name</param>
        /// <param name="description">description of step</param>
        /// <param name="expected">expected result of step</param>
        /// <returns></returns>
        public AGMTestStep Create(int testId, string stepName, string description, string expected)
        {
            if (testId == 0) throw new Exception("Test id cannot be empty.");

            var fields = new List<AGMField>();

            fields.Add(new AGMField{ Name="name", Value=stepName});
            fields.Add(new AGMField{ Name="parent-id", Value=testId.ToString()});
            fields.Add(new AGMField{ Name="description", Value=description});
            fields.Add(new AGMField{ Name="expected", Value=expected});

            return base.Create(fields);
        }
        #endregion
    }

    /// <summary>
    /// Test step class
    /// </summary>
    public class AGMTestStep : AGMEntity
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
        public AGMTestStep Update()
        {
            if (Id <= 0) throw new Exception("Test Step Id cannot be empty.");

            return new AGMTestSteps(Connection).Update(Id.Value, GetFieldsDictionary());
        }

        /// <summary>
        /// Update Test
        /// </summary>
        /// <returns></returns>
        public AGMTestStep Update(string stepName, string description, string expected)
        {
            if (Id <= 0) throw new Exception("Test Step Id cannot be empty.");

            if (!string.IsNullOrEmpty(stepName)) SetUpdateField("name", stepName);
            if (!string.IsNullOrEmpty(description)) SetUpdateField("description", description);
            if (!string.IsNullOrEmpty(expected)) SetUpdateField("expected", expected);

            return this.Update();
        }


        public AGMTestStep MoveUp()
        {
            if (Id <= 0) throw new Exception("Test Step Id cannot be empty.");

            int currentStepOrder = Convert.ToInt32(GetField("step-order").Value);
            int newStepOrder = currentStepOrder-1;
            if ( currentStepOrder == 1)
                throw new Exception("The step is first step, cannot move up.");

            int testId = Convert.ToInt32(GetField("parent-id").Value);
            var test = new AGMTests(Connection).Get(testId, null, null);

            var switchStep = test.GetStep(newStepOrder);
            switchStep.SetUpdateField("step-order", currentStepOrder.ToString());
            switchStep.Update();

            SetUpdateField("step-order", newStepOrder.ToString());
            return this.Update();
        }

        public AGMTestStep MoveDown()
        {
            if (Id <= 0) throw new Exception("Test Step Id cannot be empty.");

            int currentStepOrder = Convert.ToInt32(GetField("step-order").Value);
            int newStepOrder = currentStepOrder + 1;

            int testId = Convert.ToInt32(GetField("parent-id").Value);
            var test = new AGMTests(Connection).Get(testId, null, null);
            int stepcount = test.GetSteps().Count;

            if (currentStepOrder == stepcount-1)
                throw new Exception("The step is last step, cannot move down.");

            var switchStep = test.GetStep(newStepOrder);
            switchStep.SetUpdateField("step-order", currentStepOrder.ToString());
            switchStep.Update();

            SetUpdateField("step-order", newStepOrder.ToString());
            return this.Update();
        }
        #endregion

        #region private methods
        #endregion

    }
}
