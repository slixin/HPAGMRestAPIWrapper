using System;
using System.Collections.Generic;

namespace HPAGMRestAPIWrapper
{
    /// <summary>
    /// Test folder collection class
    /// </summary>
    public class AGMTestFolders : AGMEntityCollection<AGMTestFolder>
    {
         /// <summary>
        /// constructor
        /// </summary>
        public AGMTestFolders(AGMConnection connection)
            : base(connection)
        {
            
        }

        #region public methods
        /// <summary>
        /// Create test folder
        /// </summary>
        /// <param name="parentId">parent id</param>
        /// <param name="folderName">folder name</param>
        /// <returns>test folder</returns>
        public AGMTestFolder Create(int parentId, string folderName)
        {
            if (string.IsNullOrEmpty(folderName)) throw new Exception("Folder name of test cannot be empty.");

            var fields = new List<AGMField>();

            fields.Add(new AGMField { Name = "name", Value = folderName });
            fields.Add(new AGMField { Name = "parent-id", Value = parentId.ToString() });

            return base.Create(fields);
        }
        #endregion

        #region private methods
        #endregion

    }

    /// <summary>
    /// Test folder class
    /// </summary>
    public class AGMTestFolder : AGMEntity
    {
        #region private members
        #endregion

        #region public Properties
        #endregion

        #region constructor
        #endregion

        #region public methods        
        /// <summary>
        /// Find test under the test folder
        /// </summary>
        /// <param name="testName">test name</param>
        /// <returns>test</returns>
        public AGMTest FindTest(string testName)
        {
            var queryFields = new List<AGMField>();
            queryFields.Add(new AGMField { Name = "parent-id", Value = Id.ToString() });
            queryFields.Add(new AGMField { Name = "name", Value = testName });

            var tests = new AGMTests(Connection);
            var testList = tests.GetCollection(queryFields, null, null);
            return testList.Count == 0 ? null : testList[0];
        }

        /// <summary>
        /// Create test folder under the test folder
        /// </summary>
        /// <param name="folderName">folder name</param>
        /// <returns>Test folder</returns>
        public AGMTestFolder CreateFolder(string folderName)
        {
            var testfolders = new AGMTestFolders(Connection);
            return testfolders.Create(Id.Value, folderName);
        }

        /// <summary>
        /// Add Test under the test folder
        /// </summary>
        /// <param name="testName">test name</param>
        /// <returns>Test</returns>
        public AGMTest AddTest(string testName)
        {
            var tests = new AGMTests(Connection);
            return tests.Create(Id.Value, testName);
        }

        /// <summary>
        /// Update Test folder
        /// </summary>
        /// <returns></returns>
        public AGMTestFolder Update()
        {
            if (Id <= 0) throw new Exception("Test Folder Id cannot be empty.");

            return new AGMTestFolders(Connection).Update(Id.Value, GetFieldsDictionary());
        }
        #endregion

        #region private methods
        #endregion
    }
}
