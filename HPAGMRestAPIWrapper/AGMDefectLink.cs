using System;
using System.Collections.Generic;

namespace HPAGMRestAPIWrapper
{
    /// <summary>
    /// defect link collection class
    /// </summary>
    public class AGMDefectLinks : AGMEntityCollection<AGMDefectLink>
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="connection">connection</param>
        public AGMDefectLinks(AGMConnection connection)
            : base(connection)
        {
        }

        #region public methods
        /// <summary>
        /// Get defect link
        /// </summary>
        /// <param name="firstId">First endpoint  Id</param>
        /// <param name="secondId">Second endpoint Id</param>
        /// <param name="secondType">Second endpoint type</param>
        /// <returns>Defect Link</returns>
        public AGMDefectLink Get(int firstId, int secondId, string secondType)
        {
            var queryFields = new List<AGMField>();

            queryFields.Add(new AGMField { Name = "first-endpoint-id", Value = firstId.ToString() });
            queryFields.Add(new AGMField { Name = "second-endpoint-id", Value = secondId.ToString() });
            queryFields.Add(new AGMField { Name = "second-endpoint-type", Value = secondType });

            var dls = base.GetCollection(queryFields, null, null);

            if (dls.Count == 0) throw new Exception("Cannot get the defect link.");
            if (dls.Count > 1) throw new Exception("Get Multiple defect links.");

            var bugLink = dls[0];

            return bugLink;
        }

        /// <summary>
        /// Create defect link
        /// </summary>
        /// <param name="firstId">First endpoint  Id</param>
        /// <param name="secondId">Second endpoint Id</param>
        /// <param name="secondType">Second endpoint type</param>
        /// <returns>Defect Link</returns>
        public AGMDefectLink Create(int firstId, int secondId, string secondType)
        {
            var entityFields = new List<AGMField>();

            entityFields.Add(new AGMField{ Name="first-endpoint-id", Value=firstId.ToString()});
            entityFields.Add(new AGMField{ Name="second-endpoint-id", Value=secondId.ToString()});
            entityFields.Add(new AGMField{ Name="second-endpoint-type", Value=secondType});

            var buglink = base.Create(entityFields);

            return buglink;
        }        
        #endregion

        #region private methods
        #endregion
    }

    /// <summary>
    /// Defect link class
    /// </summary>
    public class AGMDefectLink: AGMEntity
    {
        #region private members
        #endregion

        #region public Properties
        #endregion

        #region constructor
        #endregion

        #region public methods   
        #endregion
    }
}
