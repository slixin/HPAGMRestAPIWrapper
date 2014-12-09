using System;
using System.Collections.Generic;
using System.Linq;

namespace HPAGMRestAPIWrapper
{
    /// <summary>
    /// Test set Folder collection class
    /// </summary>
    public class AGMTestSetFolders : AGMEntityCollection<AGMTestSetFolder>
    {
        /// <summary>
        /// constructor
        /// </summary>
        public AGMTestSetFolders(AGMConnection connection)
            : base(connection)
        {
        }

        #region public methods
        /// <summary>
        /// Create test set folder
        /// </summary>
        /// <param name="parentId">parent id</param>
        /// <param name="folderName">folder name</param>
        /// <returns>test folder</returns>
        public AGMTestSetFolder Create(int parentId, string folderName)
        {
            if (string.IsNullOrEmpty(folderName)) throw new Exception("Folder name of testset cannot be empty.");

            var fields = new List<AGMField>();
            
            fields.Add(new AGMField{ Name="name", Value=folderName});
            fields.Add(new AGMField{ Name="parent-id", Value=parentId.ToString()});

            return base.Create(fields);
        }
        #endregion

        #region private methods
        #endregion
    }

    /// <summary>
    /// Test set folder class
    /// </summary>
    public class AGMTestSetFolder : AGMEntity
    {
        #region private members
        #endregion

        #region public Properties
        #endregion

        #region constructor
        #endregion

        #region public methods 
        /// <summary>
        /// Find test under the test set folder
        /// </summary>
        /// <param name="testSetName">test set name</param>
        /// <returns>test set</returns>
        public AGMTestSet FindTestSet(string testSetName)
        {
            var queries = new List<AGMField>();
            
            queries.Add(new AGMField{ Name="parent-id", Value=Id.ToString()});

            var testsets = new AGMTestSets(Connection);
            var testSetList = testsets.GetCollection(queries, null, null);
            if (testSetList.Count == 0) throw new Exception(string.Format("No test set be found under test set folder: {0}.", GetField("name").Value));

            var foundcount = testSetList.Where(o => o.GetField("name").Value.Equals(testSetName)).Count();

            if (foundcount == 0) throw new Exception(string.Format("Cannot find the test set: {0} ", testSetName));
            if (foundcount > 1) throw new Exception(string.Format("Found multiple test set: {0} ", testSetName));

            var testSet = testSetList.Where(o => o.GetField("name").Value.Equals(testSetName)).First();

            return testSet;
        }

        /// <summary>
        /// Create test folder under the test folder
        /// </summary>
        /// <param name="folderName">folder name</param>
        /// <returns>Test folder</returns>
        public AGMTestSetFolder CreateFolder(string folderName)
        {
            var testsetfolders = new AGMTestSetFolders(Connection);
            return testsetfolders.Create(Id.Value, folderName);
        }

        /// <summary>
        /// Add Test under the test folder
        /// </summary>
        /// <param name="testSetName">new test set name</param>
        /// <returns>Test Set</returns>
        public AGMTestSet AddTestSet(string testSetName)
        {
            var testSets = new AGMTestSets(Connection);
            return testSets.Create(Id.Value, testSetName);
        }

        /// <summary>
        /// Update Test set folder
        /// </summary>
        /// <returns></returns>
        public AGMTestSetFolder Update()
        {
            if (Id <= 0) throw new Exception("Test Set Folder Id cannot be empty.");

            return new AGMTestSetFolders(Connection).Update(Id.Value, GetFieldsDictionary());
        }
        #endregion

        #region private methods
        #endregion
    }
}
